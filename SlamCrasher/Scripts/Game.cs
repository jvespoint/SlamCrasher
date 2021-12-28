using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
namespace Scripts
{
    public class GameScript : BaseScript
    {
        public SlamCrash _slamCrash;
        public History _history;
        public int winsSoFar, winStreak, lossStreak, highestLossStreak;
        public decimal nextBet, lastBet, nextTarget, lastTarget, streakWin, streakLoss, startingBalance, balance, firstLossBet;
        private DateTime startTime;
        public bool endSimulation;
        List<decimal> profits;
        public decimal PotentialWin() => nextBet * nextTarget;
        public decimal PotentialProfit() => PotentialWin() - nextBet;
        public decimal OriginalWinProfit() => startingBet * cashout - startingBet;
        public decimal TotalProfit() => balance - startingBalance;
        public decimal ExpectedAverageWinRatio() => (1 / nextTarget) - houseEdge;
        [SetUp]
        public void GameSetup()
        {
            LoadConfigs();
            winsSoFar = 0;
            lastBet = tokenStart;
            nextTarget = cashout;
            lastTarget = targetDefault;
            winStreak = 0;
            streakWin = 0.00m;
            lossStreak = 0;
            highestLossStreak = 0;
            streakLoss = 0.00m;
            startTime = DateTime.Now;
            profits = new List<decimal>();
            endSimulation = false;
        }
        [TearDown]
        public void GameTearDown()
        {
            Console.WriteLine("~~~~~ Run Complete ~~~~~");
            Console.WriteLine("Won " + winsSoFar + " games. Longest loss streak: " + highestLossStreak);
            Console.WriteLine("Starting balance: " + startingBalance + token);
            Console.WriteLine("Final balance: " + balance + token);
            Console.WriteLine("Total Profit: " + TotalProfit() + token);
            DateTime endTime = DateTime.Now;
            TimeSpan timeElapsed = endTime.Subtract(startTime);
            Console.WriteLine("Elapsed Time: " + timeElapsed);
            string strSeconds = timeElapsed.TotalSeconds.ToString();
            decimal decMinutes = decimal.Parse(strSeconds) / 60.00m;
            decimal profitHour = decimal.Round(((TotalProfit() / decMinutes) * 60), 10);
            Console.WriteLine("Profit / Hour: " + profitHour + " " + token);
            using (var client = new HttpClient())
            {
                string tokenId = token == "bnb" ? "binancecoin" : "slam-token";
                var response = client.GetAsync($"https://api.coingecko.com/api/v3/simple/price?ids={tokenId}&vs_currencies=usd").Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    string responseString = responseContent.ReadAsStringAsync().Result.Replace("{\"" + tokenId + "\":{\"usd\":", "").Replace("}}", "");
                    double tokenPriceDouble = double.Parse(responseString);
                    decimal tokenPrice = (decimal)tokenPriceDouble;
                    if (token == "slam")
                    {
                        tokenPrice *= 1000000;
                    }
                    Console.WriteLine("$ / Hour: " + (profitHour * tokenPrice) + " USD");
                }
            }
            if (endSimulation == false)
            {
                _history.WriteHistoryFile();
                TearDown();
            }
        }
        public void PlayGame(Action WeLost, Action WeWon, Action BeforeFirstBet, Action BeforeBet)
        {
            NewBrowserSetup();
            _slamCrash = new SlamCrash(driver);
            _slamCrash.Goto(gameUrl);
            _slamCrash.ClearServerConnect("Inital Load");
            _slamCrash.wait.Until(readyToLogin => _slamCrash.ReadyForLogin);
            _slamCrash.Login(demo, token);
            _history = new History(driver, historyFile);

            balance = _slamCrash.GetBalance(token);
            startingBalance = balance;
            nextBet = startingBet;
            ValidateBet();
            startingBet = nextBet;
            firstLossBet = startingBet;
            bool weDidWin;
            //
            BeforeFirstBet();
            //
            _slamCrash.InitializeTarget();
            while (!CheckForEnd())
            {
                Console.WriteLine("Balance: " + balance);

                SetBet(nextBet, nextTarget, balance);

                lastBet = _slamCrash.GetBet(betInputPath);
                Assert.IsTrue(nextBet == lastBet);
                lastTarget = nextTarget;

                while (_slamCrash.WinIndicator || _slamCrash.LossIndicator)
                {
                    _slamCrash.CustomTimeout(100);
                }
                _slamCrash.CustomTimeout(100);
                //
                _history.Update();
                BeforeBet();
                //
                _slamCrash.CustomTimeout(100);
                while (!_slamCrash.BetPlaced)
                {
                    _slamCrash.Click(_slamCrash.betButtonLocator);
                }
                Assert.IsTrue(_slamCrash.BetPlaced);

                try
                {
                    weDidWin = _slamCrash.CheckForWin();
                }
                catch(WebDriverTimeoutException)
                {
                    ResetGame();
                    weDidWin = _slamCrash.CheckForWin();
                }
                if (weDidWin)
                {
                    Console.Beep(220,1000);
                    winsSoFar++;
                    winStreak++;
                    decimal profit = (nextBet * nextTarget) - nextBet;
                    streakWin += profit;
                    string toLog = winStreak + ": Profit: " + profit;
                    if (winStreak == 1)
                    {
                        decimal streakProfit = profit - streakLoss;
                        toLog += ". Recovery profit: " + streakProfit;
                        profits.Add(streakProfit);
                    }
                    lossStreak = 0;
                    streakLoss = 0.00m;
                    Console.WriteLine(toLog);
                    //
                    WeWon();
                    //
                }
                else
                {
                    Console.Beep(440, 300);
                    lossStreak++;
                    streakLoss += nextBet;
                    winStreak = 0;
                    streakWin = 0.00m;
                    if (lossStreak == 1)
                    {
                        firstLossBet = nextBet;
                    }
                    if (lossStreak > highestLossStreak)
                    {
                        highestLossStreak = lossStreak;
                    }
                    Console.WriteLine(lossStreak + ": Loss: " + nextBet + ". Streak loss: " + streakLoss);
                    //
                    WeLost();
                    //
                }
                balance = _slamCrash.GetBalance(token);
                ValidateBet();
                Console.WriteLine("Next bet: " + nextBet);
                if (nextBet > balance)
                {
                    StrategyFailure();
                    break;
                }
            }
        }
        public void ResetGame()
        {
            _slamCrash.RefreshPage();
            _slamCrash.ClearServerConnect("Game Reset");
            lastBet = tokenStart;
            lastTarget = targetDefault;
            _slamCrash.InitializeTarget();
            SetBet(nextBet, nextTarget, balance);
        }
        public void ValidateBet()
        {
            if (nextBet < tokenMinBet) { nextBet = tokenMinBet; }
            if (nextBet % tokenMinBet != 0)
            {
                nextBet += tokenMinBet / 10;
                nextBet = decimal.Round(nextBet, tokenNormal.ToString().ToCharArray().Count(c => c == '0'));
            }
            while (PotentialProfit() % tokenMinBet != 0)
            {
                nextBet += tokenMinBet;
            }
        }
        public bool CheckForEnd()
        {
            TimeSpan timeElapsed = DateTime.Now.Subtract(startTime);
            //winsSoFar
            if(winStreak > 0)
            {
                if (winsSoFar >= winsPerRun)
                {
                    Console.WriteLine("End condition met: Number of Wins");
                    return true;
                }
                if (timeElapsed >= minRunTime)
                {
                    Console.WriteLine("End condition met: Runtime");
                    return true;
                }
                if ((balance - startingBalance) >= profitTarget)
                {
                    Console.WriteLine("End condition met: Profit Target");
                    return true;
                }
            }
            return false;
        }
        public void BetFromProfit(decimal profit)
        {
            nextBet = startingBet;
            while (PotentialProfit() < profit)
            {
                nextBet += tokenMinBet;
                ValidateBet();
            }
        }
        public void BetFromStreakProfit(decimal streakProfit)
        {
            BetFromProfit(streakLoss + streakProfit);
        }
        public void SetBet(decimal nextBet, decimal nextTarget, decimal balance)
        {
            decimal currentBet = _slamCrash.GetBet(betInputPath);
            if (nextBet != currentBet)
            {
                int maxClicks = 50;
                List<int> diffs = new List<int>{
                    Convert.ToInt32(tokenNormal * (nextBet - currentBet)),         //0: Last
                    Convert.ToInt32(tokenNormal * (nextBet - tokenMinBet)),        //1: Min
                    Convert.ToInt32(tokenNormal * (nextBet - balance)),            //2: Max
                    Convert.ToInt32(tokenNormal * (nextBet - (balance / 4))),      //3: 25%
                    Convert.ToInt32(tokenNormal * (nextBet - (balance / 2))),      //4: 50%
                    Convert.ToInt32(tokenNormal * (nextBet - (3 * (balance / 4)))) //5: 75%
                };
                int minClicks = diffs.Min(x => Math.Abs(x));
                if (Math.Abs(minClicks) < maxClicks)
                {
                    if (Math.Abs(diffs[0]) < (maxClicks + 30)) //Always better to increment from last bet
                    {
                        _slamCrash.IncrementButtons(diffs[0], true);
                        //Console.WriteLine("Bet set from last bet: " + nextBet);
                    }
                    else
                    {
                        if (minClicks == Math.Abs(diffs[1]))
                        {
                            _slamCrash.SetBetToMin();
                            _slamCrash.IncrementButtons(Convert.ToInt32(tokenNormal * (nextBet - _slamCrash.GetBet(betInputPath))), true);
                            Console.WriteLine("Bet set from minimum: " + nextBet);
                        }
                        else if (minClicks == Math.Abs(diffs[2]))
                        {
                            _slamCrash.SetBetToMax();
                            _slamCrash.IncrementButtons(Convert.ToInt32(tokenNormal * (nextBet - _slamCrash.GetBet(betInputPath))), true);
                            Console.WriteLine("Bet set from maximum: " + nextBet);
                        }
                        else if (minClicks == Math.Abs(diffs[3]))
                        {
                            _slamCrash.SetBetTo25();
                            _slamCrash.IncrementButtons(Convert.ToInt32(tokenNormal * (nextBet - _slamCrash.GetBet(betInputPath))), true);
                            Console.WriteLine("Bet set from 25%: " + nextBet);
                        }
                        else if (minClicks == Math.Abs(diffs[4]))
                        {
                            _slamCrash.SetBetTo50();
                            _slamCrash.IncrementButtons(Convert.ToInt32(tokenNormal * (nextBet - _slamCrash.GetBet(betInputPath))), true);
                            Console.WriteLine("Bet set from 50%: " + nextBet);
                        }
                        else if (minClicks == Math.Abs(diffs[5]))
                        {
                            _slamCrash.SetBetTo75();
                            _slamCrash.IncrementButtons(Convert.ToInt32(tokenNormal * (nextBet - _slamCrash.GetBet(betInputPath))), true);
                            Console.WriteLine("Bet set from 75%: " + nextBet);
                        }
                    }
                }
                else
                {
                    _slamCrash.SetBetCloseEnough(nextBet + (tokenMinBet * 5), token);
                    _slamCrash.IncrementButtons(Convert.ToInt32(tokenNormal * (nextBet - _slamCrash.GetBet(betInputPath))), true);
                    Console.WriteLine("Bet set with slider: " + nextBet);
                }
            }

            _slamCrash.InitializeTarget();
            decimal currentTarget = Decimal.Parse(_slamCrash.Find(By.XPath($"(//input)[{cashoutInputPath}]")).GetAttribute("value").Replace("x",""));
            if (nextTarget != currentTarget)
            {
                if (nextTarget % 0.10m != 0 || Math.Abs(nextTarget - currentTarget) > 10.00m || currentTarget % 0.10m != 0)
                {
                    var elem = _slamCrash.Find(By.XPath($"(//input)[{cashoutInputPath}]"));
                    elem.Click();
                    string targetStripped = nextTarget.ToString().Replace(".", "");
                    elem.SendKeys(targetStripped);
                    elem.SendKeys(Keys.Left + Keys.Left + "." + Keys.Enter);
                }
                else
                {
                    int targetClicks = Convert.ToInt32((nextTarget - currentTarget) * targetNormal);
                    _slamCrash.IncrementButtons(targetClicks, false);
                }
            }
        }
        public void Simulate(bool weDidWin, Action Method, Action BeforeFirstBet, Action BeforeBet)
        {
            startingBalance = demo ? 100.00m : 2.1830m;
            balance = startingBalance;
            nextBet = startingBet;
            ValidateBet();
            startingBet = nextBet;
            lastBet = demo ? tokenStart : 0.01m;
            
            firstLossBet = startingBet;
            //
            BeforeFirstBet();
            void WeWon()
            {
                balance += (nextBet * nextTarget) - nextBet;
                Method();
            }
            void WeLost()
            {
                balance -= nextBet;
                Method();
            }
            //
            
            while (endSimulation == false)
            {
                lastBet = nextBet;
                lastTarget = nextTarget;
                //
                BeforeBet();
                //
                
                if (weDidWin)
                {
                    winsSoFar++;
                    winStreak++;
                    decimal profit = (nextBet * nextTarget) - nextBet;
                    streakWin += profit;
                    string toLog = winStreak + ": Profit: " + profit;
                    if (winStreak == 1)
                    {
                        decimal streakProfit = profit - streakLoss;
                        toLog += ". Recovery profit: " + streakProfit;
                        profits.Add(streakProfit);
                    }
                    lossStreak = 0;
                    streakLoss = 0.00m;
                    Console.WriteLine(toLog);
                    //
                    WeWon();
                    //
                }
                else
                {
                    lossStreak++;
                    streakLoss += nextBet;
                    winStreak = 0;
                    streakWin = 0.00m;
                    if (lossStreak == 1)
                    {
                        firstLossBet = nextBet;
                    }
                    if (lossStreak > highestLossStreak)
                    {
                        highestLossStreak = lossStreak;
                    }
                    Console.WriteLine(lossStreak + ": Loss: " + nextBet + ". Streak loss: " + streakLoss);
                    //
                    WeLost();
                    //
                }
                //
                ValidateBet();
                if (nextBet > balance)
                {
                    StrategyFailure();
                    break;
                }
                string roundOverMessage = " Balance: " + balance;
                if(!weDidWin)
                {
                    decimal ifWedWon = decimal.Round(balance + (lastBet * lastTarget),4);
                    profits.Add(ifWedWon - startingBalance);
                    roundOverMessage += ". If we'd won: " + ifWedWon;
                }
                else
                {
                    decimal ifWedLost = balance - lastBet - ((lastBet * lastTarget)-lastBet);
                    roundOverMessage += ". If we'd lost: " + ifWedLost;
                    if (CheckForEnd())
                    {
                        endSimulation = true;
                    }
                }
                Console.WriteLine( roundOverMessage );
            }
            endSimulation = true;
        }
        public void StrategyFailure()
        {
            Console.WriteLine("~~~~~Critical Strategy Failure~~~~~");
            decimal chanceOfOneLoss = (1 - ExpectedAverageWinRatio());
            decimal chanceOfOneWin = ExpectedAverageWinRatio();
            decimal chanceOfThisLoss = chanceOfOneLoss;
            decimal chanceOfThisWin = chanceOfOneWin;
            for (int i = lossStreak; i > 1; i--)
            {
                chanceOfThisLoss *= chanceOfOneLoss;
                chanceOfThisWin += chanceOfOneWin;
            }
            decimal oneInHowMany;
            try
            {
                oneInHowMany = 1.00m / chanceOfThisLoss;
            }
            catch(Exception)
            {
                oneInHowMany = 100000000000000m;
            }
            Console.WriteLine("Chance of this loss: " + decimal.Round(chanceOfThisLoss * 100m, 10) + "% or 1 in " + decimal.Round(oneInHowMany, 6));
        }
    }
}