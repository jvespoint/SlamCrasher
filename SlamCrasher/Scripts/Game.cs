using NUnit.Framework;
using Pages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts
{
    public class GameScript : BaseScript
    {
        public SlamCrash _slamCrash;
        public History _history;
        public int winsSoFar, winStreak, lossStreak;
        public decimal nextBet, lastBet, nextTarget, lastTarget, streakWin, streakLoss, startingBalance, balance, firstLossBet;
        private DateTime startTime;
        public decimal PotentialWin() => nextBet * nextTarget;
        public decimal PotentialProfit() => PotentialWin() - nextBet;
        public decimal OriginalWinProfit() => startingBet * cashout - startingBet;
        public decimal TotalProfit() => balance - startingBalance;
        public decimal ExpectedAverageWinRatio() => 1 / nextTarget;
        [SetUp]
        public void GameSetup()
        {
            NewBrowserSetup();
            _slamCrash = new SlamCrash(driver);
            _slamCrash.Goto(gameUrl);
            _slamCrash.ClearServerConnect("Inital Load");
            _slamCrash.wait.Until(readyToLogin => _slamCrash.ReadyForLogin);
            startTime = DateTime.Now;
        }
        [TearDown]
        public void GameTearDown()
        {
            TearDown();
        }
        public void PlayGame(Action WeLost, Action WeWon, Action BeforeFirstBet, Action BeforeBet)
        {
            _slamCrash.Login(demo, token);

            winsSoFar = 0;
            nextBet = startingBet;
            lastBet = tokenStart;
            nextTarget = cashout;
            lastTarget = targetDefault;
            winStreak = 0;
            streakWin = 0.00m;
            lossStreak = 0;
            streakLoss = 0.00m;
            firstLossBet = startingBet;
            startingBalance = _slamCrash.GetBalance(token);
            //
            BeforeFirstBet();
            //
            _slamCrash.InitializeTarget();
            while (winsSoFar < winsPerRun)
            {
                Console.WriteLine("Balance: " + balance);

                SetBet();
                lastBet = nextBet;
                lastTarget = nextTarget;

                while (_slamCrash.WinIndicator || _slamCrash.LossIndicator)
                {
                    _slamCrash.CustomTimeout(100);
                }
                //
                BeforeBet();
                //
                _slamCrash.CustomTimeout(50);
                while (!_slamCrash.BetPlaced)
                {
                    _slamCrash.Click(_slamCrash.betButtonLocator);
                }
                Assert.IsTrue(_slamCrash.BetPlaced);

                bool weDidWin = _slamCrash.CheckForWin();
                if (weDidWin)
                {
                    winsSoFar++;
                    winStreak++;
                    decimal profit = (nextBet * nextTarget) - nextBet;
                    streakWin += profit;
                    lossStreak = 0;
                    streakLoss = 0.00m;
                    Console.WriteLine(winStreak + ": Profit: " + profit + ". Streak profit: " + streakWin);
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
                    Console.WriteLine(lossStreak + ": Loss: " + nextBet + ". Streak loss: " + streakLoss);
                    //
                    WeLost();
                    //
                }
                ValidateBet();
                Console.WriteLine("Next bet: " + nextBet);
                balance = _slamCrash.GetBalance(token);
            }
            Console.WriteLine("Won " + winsSoFar + " games:");
            Console.WriteLine("Starting balance: " + startingBalance + token);
            Console.WriteLine("Final balance: " + balance + token);
            Console.WriteLine("Total Profit: " + TotalProfit() + token);
            DateTime endTime = DateTime.Now;
            TimeSpan timeElapsed = endTime.Subtract(startTime);
            Console.WriteLine("Elapsed Time: " + timeElapsed);
            string strSeconds = timeElapsed.TotalMinutes.ToString();
            decimal decSeconds = decimal.Parse(strSeconds);
            Console.WriteLine("Profit / Minute: " + (TotalProfit() / decSeconds) + token);
        }
        public void ValidateBet()
        {
            if (nextBet < tokenMinBet) { nextBet = tokenMinBet; }
            if (nextBet % tokenMinBet != 0)
            {
                nextBet -= tokenMinBet / 10;
                nextBet = decimal.Round(nextBet, tokenNormal.ToString().ToCharArray().Count(c => c == '0'));
            }
            if (nextBet > startingBalance / cashout) { EndGame("Critical strategic failure"); }
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
        public void SetBet()
        {
            int maxClicks = 50;
            List<int> diffs = new List<int>{
                Convert.ToInt32(tokenNormal * (nextBet - lastBet)),            //0: Last
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
                else if (minClicks == Math.Abs(diffs[1]))
                {
                    _slamCrash.SetBetToMin();
                    _slamCrash.IncrementButtons(diffs[1], true);
                    Console.WriteLine("Bet set from minimum: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[2]))
                {
                    _slamCrash.SetBetToMax();
                    _slamCrash.IncrementButtons(diffs[2], true);
                    Console.WriteLine("Bet set from maximum: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[3]))
                {
                    _slamCrash.SetBetTo25();
                    _slamCrash.IncrementButtons(diffs[3], true);
                    Console.WriteLine("Bet set from 25%: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[4]))
                {
                    _slamCrash.SetBetTo50();
                    _slamCrash.IncrementButtons(diffs[4], true);
                    Console.WriteLine("Bet set from 50%: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[5]))
                {
                    _slamCrash.SetBetTo75();
                    _slamCrash.IncrementButtons(diffs[5], true);
                    Console.WriteLine("Bet set from 75%: " + nextBet);
                }
            }
            else
            {
                _slamCrash.SetBetCloseEnough(nextBet, token);
                Console.WriteLine("Bet set with slider: " + nextBet);
            }
            int targetClicks = Convert.ToInt32((nextTarget - lastTarget) * targetNormal);
            _slamCrash.IncrementButtons(targetClicks, false);
        }
        public void Simulate(bool weDidWin, Action Method, Action BeforeFirstBet, Action BeforeBet)
        {
            winsSoFar = 0;
            nextBet = startingBet;
            lastBet = nextBet;
            nextTarget = cashout;
            lastTarget = nextTarget;
            winStreak = 0;
            streakWin = 0.00m;
            lossStreak = 0;
            streakLoss = 0.00m;
            firstLossBet = startingBet;
            startingBalance = demo ? 100.00m : 0.60m;
            balance = startingBalance;
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
            while (winsSoFar < winsPerRun)
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
                    decimal recoveryProfit = profit - streakLoss;
                    streakWin += profit;
                    lossStreak = 0;
                    streakLoss = 0.00m;
                    string winMessage = winStreak + ": Profit: " + profit + ". Streak profit: " + streakWin;
                    if (winStreak == 1)
                    {
                        winMessage += ". Actual Profit: " + recoveryProfit;
                    }
                    Console.WriteLine(winMessage);
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
                    Console.WriteLine(lossStreak + ": Loss: " + nextBet + ". Streak loss: " + streakLoss);
                    //
                    WeLost();
                    //
                }
                ValidateBet();
                //
                if (!weDidWin)
                {
                    winsSoFar++;
                }
                decimal wonBalance = balance + (lastBet * lastTarget);
                string roundOverMessage = "Balance: " + balance;
                if(!weDidWin)
                {
                    roundOverMessage += ". If we'd won: " + wonBalance;
                }
                Console.WriteLine( roundOverMessage );
            }
        }
        
        public void EndGame(string message)
        {
            Console.WriteLine(message);
            winsSoFar = winsPerRun;
        }
    }
}