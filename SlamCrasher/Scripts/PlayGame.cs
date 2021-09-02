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
        public decimal nextBet, lastBet, nextTarget, lastTarget, streakWin, streakLoss, startingBalance, originalWinProfit;

        [SetUp]
        public void GameSetup()
        {
            NewBrowserSetup();
            _slamCrash = new SlamCrash(driver);
            _slamCrash.Goto(gameUrl);
            _slamCrash.ClearServerConnect("Inital Load");
            _slamCrash.wait.Until(readyToLogin => _slamCrash.ReadyForLogin);
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
            startingBalance = _slamCrash.GetBalance(token);
            originalWinProfit = (nextBet * nextTarget) - nextBet;

            _slamCrash.InitializeTarget();
            _history = new History(driver);
        }
        [TearDown]
        public void GameTearDown()
        {
            Console.WriteLine("Success. Won " + winsSoFar + " games:");
            Console.WriteLine("Starting balance was " + startingBalance + token);
            Console.WriteLine("Final balance is " + _slamCrash.GetBalance(token) + token);
            TearDown();
        }

        public void PlayGame(Action WeLost, Action WeWon, Action PreFirstRoll)
        {
            PreFirstRoll();

            while (winsSoFar < winsPerRun)
            {
                decimal balance = _slamCrash.GetBalance(token);
                Console.WriteLine("Balance: " + balance);

                SetBet(nextBet, lastBet, nextTarget, lastTarget, balance);

                while (_slamCrash.WinIndicator || _slamCrash.LossIndicator)
                {
                    _slamCrash.CustomTimeout(100);
                }
                lastBet = nextBet;
                lastTarget = nextTarget;
                _history.Update();

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
                    lossStreak = 0;
                    streakLoss = 0;
                    winStreak++;
                    decimal profit = (nextBet * nextTarget) - nextBet;
                    streakWin += profit;
                    WeWon();
                }
                else
                {
                    lossStreak++;
                    streakLoss += nextBet;
                    winStreak = 0;
                    streakWin = 0;
                    WeLost();
                }
                if (nextBet < tokenMinBet) { nextBet = tokenMinBet; }
                if (nextBet % tokenMinBet != 0)
                {
                    nextBet -= tokenMinBet / 10;
                    nextBet = decimal.Round(nextBet, tokenNormal.ToString().ToCharArray().Count(c => c == '0'));
                }
                Console.WriteLine("Win: " + weDidWin + ". " + nextBet + token);
            }
        }

        public void Simulate(bool wins, Action method)
        {
            decimal balance = 100.00m;
            for (int i = 0; i < 50; i++)
            {
                if (wins)
                {
                    winsSoFar++;
                    lossStreak = 0;
                    streakLoss = 0;
                    winStreak++;
                    decimal profit = (nextBet * nextTarget) - nextBet;
                    streakWin += profit;
                    //
                    method();
                }
                else
                {
                    lossStreak++;
                    streakLoss += nextBet;
                    balance -= nextBet;
                    winStreak = 0;
                    streakWin = 0;
                    //
                    method();
                }
                if (nextBet < tokenMinBet) { nextBet = tokenMinBet; }
                if (nextBet % tokenMinBet != 0)
                {
                    nextBet -= tokenMinBet / 10;
                    nextBet = decimal.Round(nextBet, tokenNormal.ToString().ToCharArray().Count(c => c == '0'));
                }
                Console.WriteLine("Win: false. " + nextBet + token + ". Balance: " + balance + ". Win: " + (balance + (nextBet * nextTarget)));
            }
        }

        public void SkipGames(int n)
        {
            int currentGame = _history.games[_history.games.Count - 1].number;
            int endGame = currentGame + n;
            while(currentGame < endGame)
            {
                _slamCrash.CustomTimeout(1000);
                _history.Update();
                currentGame = _history.games[_history.games.Count - 1].number;
            }
        }

        public void SetBet(decimal nextBet, decimal lastBet, decimal nextTarget, decimal lastTarget, decimal balance)
        {
            int maxClicks = 70;
            List<int> diffs = new List<int>{
                Convert.ToInt32(tokenNormal * (nextBet - lastBet)), //diffFromLast 0
                Convert.ToInt32(tokenNormal * (nextBet - tokenMinBet)), //diffFromMin 1
                Convert.ToInt32(tokenNormal * (nextBet - balance)), //diffFromMax 2
                Convert.ToInt32(tokenNormal * (nextBet - (balance / 4))), //diffFrom25 3
                Convert.ToInt32(tokenNormal * (nextBet - (balance / 2))), //diffFrom50 4
                Convert.ToInt32(tokenNormal * (nextBet - (3 * (balance / 4)))) //diffFrom75 5
            };
            int minClicks = diffs.Min(x => Math.Abs(x));
            
            if(Math.Abs(minClicks) < maxClicks)
            {
                if (Math.Abs(diffs[0]) < Math.Abs(minClicks) + 20 )
                {
                    //Always better to imcrement from last bet than to use the range slider
                    _slamCrash.IncrementButtons(diffs[0], true);
                    //Console.WriteLine("Bet set from last bet: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[1]))
                {
                    _slamCrash.SetBetToMin();
                    _slamCrash.IncrementButtons(diffs[1], true);
                    //Console.WriteLine("Bet set from maximum: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[2]))
                {
                    _slamCrash.SetBetToMax();
                    _slamCrash.IncrementButtons(diffs[2], true);
                    //Console.WriteLine("Bet set from maximum: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[3]))
                {
                    _slamCrash.SetBetTo25();
                    _slamCrash.IncrementButtons(diffs[3], true);
                    //Console.WriteLine("Bet set from 25%: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[4]))
                {
                    _slamCrash.SetBetTo50();
                    _slamCrash.IncrementButtons(diffs[4], true);
                    //Console.WriteLine("Bet set from 50%: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[5]))
                {
                    _slamCrash.SetBetTo75();
                    _slamCrash.IncrementButtons(diffs[5], true);
                    //Console.WriteLine("Bet set from 75%: " + nextBet);
                }
            }
            else
            {
                _slamCrash.SetBetCloseEnough(nextBet, token);
                //Console.WriteLine("Bet set with slider: " + nextBet);
            }
            int targetClicks = Convert.ToInt32((nextTarget - lastTarget) * targetNormal);
            _slamCrash.IncrementButtons(targetClicks, false);
            //Console.WriteLine("Auto-Cashout set to: " + nextTarget);
        }
    }
}