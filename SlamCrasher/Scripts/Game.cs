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
        public decimal nextBet, lastBet, nextTarget, lastTarget, streakWin, streakLoss, startingBalance, totalProfit;

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
            totalProfit = 0.00m;
            nextBet = startingBet;
            lastBet = tokenStart;
            nextTarget = cashout;
            lastTarget = targetDefault;
            winStreak = 0;
            streakWin = 0.00m;
            lossStreak = 0;
            streakLoss = 0.00m;
            startingBalance = _slamCrash.GetBalance(token);

            _slamCrash.InitializeTarget();
        }

        [TearDown]
        public void GameTearDown()
        {
            Console.WriteLine("Success. Won " + winsSoFar + " games:");
            Console.WriteLine("Starting balance was " + startingBalance + token);
            Console.WriteLine("Final balance is " + _slamCrash.GetBalance(token) + token);
            TearDown();
        }

        public void PlayGame(Action WeLost, Action WeWon, Action BeforeFirstBet, Action BeforeBet)
        {
            //
            BeforeFirstBet();
            //
            while (winsSoFar < winsPerRun)
            {
                decimal balance = _slamCrash.GetBalance(token);
                Console.WriteLine("Balance: " + balance);
                totalProfit = balance - startingBalance;

                SetBet(nextBet, lastBet, nextTarget, lastTarget, balance);

                while (_slamCrash.WinIndicator || _slamCrash.LossIndicator)
                {
                    _slamCrash.CustomTimeout(100);
                }
                lastBet = nextBet;
                lastTarget = nextTarget;
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
                    Console.WriteLine("Profit: " + profit + ". Streak profit: " + streakWin);
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
                    Console.WriteLine("Loss: " + nextBet + ". Streak loss: " + streakLoss);
                    //
                    WeLost();
                    //
                }
                if (nextBet < tokenMinBet) { nextBet = tokenMinBet; }
                if (nextBet % tokenMinBet != 0)
                {
                    nextBet -= tokenMinBet / 10;
                    nextBet = decimal.Round(nextBet, tokenNormal.ToString().ToCharArray().Count(c => c == '0'));
                }
                if (nextBet > (startingBalance / 5)) { EndGame("Critical strategic failure"); }
                Console.WriteLine("Next bet: " + nextBet);
            }
        }

        public void Simulate(bool weDidWin, Action Method)
        {
            for (int i = 0; i < 50; i++)
            {
                //
                totalProfit = (startingBalance + streakWin - streakLoss) - startingBalance;
                Action WeWon = Method;
                Action WeLost = Method;
                //
                if (weDidWin)
                {
                    winsSoFar++;
                    winStreak++;
                    decimal profit = (nextBet * nextTarget) - nextBet;
                    streakWin += profit;
                    lossStreak = 0;
                    streakLoss = 0.00m;
                    Console.WriteLine("Profit: " + profit + ". Streak profit: " + streakWin);
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
                    Console.WriteLine("Loss: " + nextBet + ". Streak loss: " + streakLoss);
                    //
                    WeLost();
                    //
                }
                if (nextBet < tokenMinBet) { nextBet = tokenMinBet; }
                if (nextBet % tokenMinBet != 0)
                {
                    nextBet -= tokenMinBet / 10;
                    nextBet = decimal.Round(nextBet, tokenNormal.ToString().ToCharArray().Count(c => c == '0'));
                }
                //
                Console.WriteLine("Streak: " + i + ". Next bet: " + nextBet + ". Balance: " + (startingBalance + streakWin - streakLoss));
                if (!weDidWin)
                {
                    decimal wonBalance = (startingBalance + streakWin - streakLoss) + (nextBet * nextTarget);
                    Console.WriteLine("If we'd won: " + wonBalance);
                }
                //
            }
        }

        public void SkipGames(int n)
        {
            if(_history == null) { EndGame("History not initialized"); }
            int currentGame = _history.games[^1].number;
            int endGame = currentGame + n;
            while(currentGame < endGame)
            {
                _slamCrash.CustomTimeout(1000);
                _history.Update();
                currentGame = _history.games[^1].number;
            }
        }

        public void SetBet(decimal nextBet, decimal lastBet, decimal nextTarget, decimal lastTarget, decimal balance)
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
            if(Math.Abs(minClicks) < maxClicks)
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

        public void WaitForLosses()
        {
            if (!_history.LastGamesLoss(Convert.ToInt32(nextTarget) - 1, nextTarget))
            {
                Console.WriteLine("Skipping game: WaitForLosses");
                SkipGames(1);
                WaitForLosses();
            }
        }

        public void EndGame(string message)
        {
            Console.WriteLine(message);
            winsSoFar = winsPerRun;
        }
    }
}