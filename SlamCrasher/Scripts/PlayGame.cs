using NUnit.Framework;
using Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Scripts
{
    public class PlayGame : BaseScript
    {
        private SlamCrash _slamCrash;
        private List<PreviousGame> history;

        [SetUp]
        public void PlayGameSetup()
        {
            NewBrowserSetup();
            _slamCrash = new SlamCrash(driver);
            history = new List<PreviousGame>();
        }
        [OneTimeTearDown]
        public void PlayGameTearDown() => TearDown();
        [Test]
        public void SimulateLossesScript()
        {
            const bool didWeWin = false;
            int winsSoFar = 0;
            decimal nextBet = startingBet; //startingBet
            decimal nextTarget = cashout; //cashout
            int lossStreak = 0;
            decimal streakLoss = 0.00m;
            decimal balance = 0.1m;

            decimal originalWinProfit = (nextBet * nextTarget) - nextBet;

            for (int i = 10; i > 0; i--)
            {
                balance -= nextBet;

                Console.WriteLine(1+lossStreak);
                Console.WriteLine("Bet: " + nextBet);
                Console.WriteLine("Target: " + nextTarget);
                Console.WriteLine("Loss: " + (streakLoss + nextBet));
                Console.WriteLine("Balance:" + balance);
                Console.WriteLine("If-We'd-Won Balance:" + (balance + (nextBet*nextTarget)));
                Console.WriteLine("-------------------------");


                //Handle changes to bet/target according to strategy selected:
                switch (strategy)
                {
                    case "LosslessGains":
                        if (didWeWin)
                        {
                            nextBet = startingBet;
                            nextTarget = cashout;
                            winsSoFar++;
                            lossStreak = 0;
                            streakLoss = 0;
                        }
                        else
                        {
                            lossStreak++;
                            streakLoss += nextBet;
                            nextBet = (originalWinProfit + streakLoss) / (nextTarget - 1);
                        }
                        break;
                    case "KnowsBest":
                        if (didWeWin)
                        {
                            //lastRound = _slamCrash.GetLastGame().number;
                            nextBet = startingBet;
                            nextTarget = cashout;
                            winsSoFar++;
                            lossStreak = 0;
                            streakLoss = 0;
                        }
                        else
                        {
                            //lastRound = _slamCrash.GetLastGame().number;
                            lossStreak++;
                            streakLoss += nextBet;
                            nextBet = (originalWinProfit + streakLoss) / (nextTarget - 1);
                            if (!(Convert.ToInt32(nextBet) % 2 == 0))
                            {
                                nextBet += tokenMinBet;
                            }
                        }
                        break;
                    default:
                        //_slamCrash.CrashOut("Invalid strategy selected");
                        break;
                }
                //

            }
            
        }

        [Test]
        public void PlayGameScript()
        {
            _slamCrash.Goto(gameUrl);
            _slamCrash.ClearServerConnect("Inital Load");
            _slamCrash.wait.Until(readyToLogin => _slamCrash.ReadyForLogin);
            _slamCrash.Login(demo, token);
            history = _slamCrash.GetHistory();
            _slamCrash.InitializeTarget();

            //Wait to play first game until we would've lost the last game
            //CardCounting("would-have-lost");

            int winsSoFar = 0;
            decimal nextBet = startingBet;
            decimal lastBet = tokenStart;
            decimal nextTarget = cashout;
            decimal lastTarget = targetDefault;
            int winStreak = 0;
            decimal streakWin = 0;
            int lossStreak = 0;
            decimal streakLoss = 0.00m;
            decimal startingBalance = _slamCrash.GetBalance(token);

            decimal originalWinProfit = (nextBet * nextTarget) - nextBet;


            //
            //Wait until would have lost last two games
            //CardCounting("two-loss", 2.0m);

            //
            while (winsSoFar < winsPerRun) //Until we reach wins required
            {
                decimal balance = _slamCrash.GetBalance(token);
                Console.WriteLine("Balance: " + balance);

                SetBet(nextBet, lastBet, nextTarget, lastTarget, balance);

                while (_slamCrash.WinIndicator || _slamCrash.LossIndicator)
                {
                    _slamCrash.CustomTimeout(200);
                }
                history = _slamCrash.UpdateHistory(history); //Get this round into history

                

                if (winStreak > 2 || winStreak == 0)
                {
                    //Wait until last crash was below
                    //CardCounting("low-crash", 2.0m);

                    //Wait until would have lost last two games
                    //CardCounting("two-loss", 2.0m);

                    //Wait until would have lost last two games
                    //CardCounting("would-have-lost");

                    //Wait until would-have-won-ratio is below a certain level to continue playing
                    //CardCounting("win-ratio", 0);
                }

                lastBet = nextBet;
                lastTarget = nextTarget;
                //To-do: find a way to confirm bet is set correctly

                _slamCrash.CustomTimeout(100);
                _slamCrash.Click(_slamCrash.betButtonLocator);
                Assert.IsTrue(_slamCrash.BetPlaced);


                bool weDidWin = _slamCrash.CheckForWin();
                

                if (weDidWin)
                {

                    Console.WriteLine("Won a game. Profit: " + ((nextBet * nextTarget) - nextBet));
                    Console.WriteLine("Waiting for crash...");
                    winsSoFar++;
                    winStreak++;
                    streakWin += (nextBet * nextTarget) - nextBet;
                    lossStreak = 0;
                    streakLoss = 0;
                }
                else
                {
                    winStreak = 0;
                    streakWin = 0;
                    lossStreak++;
                    streakLoss += nextBet;
                    Console.WriteLine("Lost a game. Streak: " + lossStreak + ". Total lost in streak: " + streakLoss);
                }
                //Handle changes to bet/target according to strategy selected:
                switch (strategy)
                {
                    case "KnowsBest":
                        if (weDidWin)
                        {
                            nextBet = startingBet;
                            nextTarget = cashout;
                        }
                        else
                        {
                            nextBet = (originalWinProfit + streakLoss) / (cashout - 1);
                            if (lossStreak == 4)
                            {
                                nextTarget -= 0.10m;
                            }
                            if (!(Convert.ToInt32(nextBet) %2 == 0))
                            {
                                nextBet += tokenMinBet;
                            }
                        }
                        break;
                    case "lossless":
                        if (weDidWin)
                        {
                            nextBet = startingBet;
                            nextTarget = cashout;
                        }
                        else
                        {
                            nextBet = (streakLoss) / (cashout - 1);
                            if (lossStreak == 5)
                            {
                                nextTarget -= 0.5m;
                            }
                            if (!(Convert.ToInt32(nextBet) %2 == 0))
                            {
                                nextBet += tokenMinBet;
                            }
                        }
                        break;
                    default:
                        _slamCrash.CrashOut("Invalid strategy selected");
                        break;
                }
                if (nextBet < tokenMinBet) { nextBet = tokenMinBet; }
                
            }
            Console.WriteLine("Success. Won " + winsSoFar + " games:");
            Console.WriteLine("Starting balance was " + startingBalance + token);
            Console.WriteLine("Final balance is " + _slamCrash.GetBalance(token) + token);
            System.Threading.Thread.Sleep(5000); //wait awhile when finished
        }
        public void SetBet(decimal nextBet, decimal lastBet, decimal nextTarget, decimal lastTarget, decimal balance)
        {
            int maxClicks = 80;
            List<int> diffs = new List<int>{
                Convert.ToInt32(tokenNormal * (nextBet - lastBet)), //diffFromLast 0
                Convert.ToInt32(tokenNormal * (nextBet - tokenStart)), //diffFromStart 1
                Convert.ToInt32(tokenNormal * (nextBet - tokenMinBet)), //diffFromMin 2
                Convert.ToInt32(tokenNormal * (nextBet - balance)), //diffFromMax 3
                Convert.ToInt32(tokenNormal * (nextBet - (balance / 4))), //diffFrom25 4
                Convert.ToInt32(tokenNormal * (nextBet - (balance / 2))), //diffFrom50 5
                Convert.ToInt32(tokenNormal * (nextBet - (3 * (balance / 4)))) //diffFrom75 6
            };
            int minClicks = diffs.Min(x => Math.Abs(x));
            if (minClicks == Math.Abs(diffs[2]) && (Math.Abs(diffs[2]) + 20) < Math.Abs(diffs[0]))
            {
                _slamCrash.SetBetToMin();
                _slamCrash.IncrementButtons(diffs[2], true);
                Console.WriteLine("Bet set from minimum: " + nextBet);
            }
            else if(Math.Abs(diffs[0]) > maxClicks && minClicks != Math.Abs(diffs[0]))
            {
                if (minClicks == Math.Abs(diffs[1]) && Math.Abs(diffs[1]) < maxClicks)
                {
                    _slamCrash.RefreshPage();
                    _slamCrash.ClearServerConnect("After refresh for start bet");
                    _slamCrash.wait.Until(ready => _slamCrash.AllButtonsReady);
                    _slamCrash.IncrementButtons(diffs[1], true);
                    _slamCrash.InitializeTarget(); //has to be set again after page-load
                    Console.WriteLine("Bet set from start: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[3]) && Math.Abs(diffs[3]) < maxClicks)
                {
                    _slamCrash.SetBetToMax();
                    _slamCrash.IncrementButtons(diffs[3], true);
                    Console.WriteLine("Bet set from maximum: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[4]) && Math.Abs(diffs[4]) < maxClicks)
                {
                    _slamCrash.SetBetTo25();
                    _slamCrash.IncrementButtons(diffs[4], true);
                    Console.WriteLine("Bet set from 25%: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[5]) && Math.Abs(diffs[5]) < maxClicks)
                {
                    _slamCrash.SetBetTo50();
                    _slamCrash.IncrementButtons(diffs[5], true);
                    Console.WriteLine("Bet set from 50%: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[6]) && Math.Abs(diffs[6]) < maxClicks)
                {
                    _slamCrash.SetBetTo75();
                    _slamCrash.IncrementButtons(diffs[6], true);
                    Console.WriteLine("Bet set from 75%: " + nextBet);
                }
                else
                {
                    _slamCrash.SetBetCloseEnough(nextBet, token);
                    Console.WriteLine("Bet set with slider: " + nextBet);
                }
            }
            else
            {
                //Always better to imcrement from last bet than to use the range slider
                _slamCrash.IncrementButtons(diffs[0], true);
                Console.WriteLine("Bet set from last bet: " + nextBet);
            }
            //Set Auto-Cashout
            int targetClicks = Convert.ToInt32((nextTarget - lastTarget) * targetNormal);
            _slamCrash.IncrementButtons(targetClicks, false);
            Console.WriteLine("Auto-Cashout set to: " + nextTarget);
        }
        private void CardCounting(string type, decimal factor = 0)
        {
            bool violation = false;
            decimal lastRoundCrashed;
            switch (type)
            {
                case "would-have-lost": //Wait to play game until we would've lost the last game
                    
                    if (cashout < history[0].crash)
                    {
                        violation = true;
                        SkipRounds(1);
                    }
                    break;
                case "two-loss": //Wait to play until we would've lost the last two games
                    lastRoundCrashed = _slamCrash.GetLastGame().crash;
                    Console.WriteLine("Last round crashed at: " + lastRoundCrashed + "x");
                    if (!(history[0].crash < factor && history[1].crash < factor))
                    {
                        violation = true;
                        SkipRounds(1);
                    }
                    break;
                case "win-ratio": //compare ratio of wins/losses if we'd have been playing
                    decimal ratio = _slamCrash.WouldHaveWonRatio(history, cashout);
                    Console.WriteLine("Historical Win Ratio: " + ratio);
                    decimal maxRatio = (1 / cashout) - (factor / (100*cashout)); //To-do: factor in given house-edge
                    if (ratio > maxRatio)
                    {
                        violation = true;
                        SkipRounds(3);
                    }
                    break;
                case "low-crash":
                    lastRoundCrashed = history[0].crash;
                    Console.WriteLine("Last round crashed at: " + lastRoundCrashed + "x");
                    if (lastRoundCrashed > factor)
                    {
                        violation = true;
                        SkipRounds(1);
                    }
                    break;
            }
            if (violation)
            { 
                Console.WriteLine("Skipped round for Card-counting: " + type);
                CardCounting(type, factor);
            }
            
        }

        public void SkipRounds(int numberOfRoundsToSkip)
        {
            if (numberOfRoundsToSkip <= 0)
            {
                //Do nothing with zero or negative numbers
                return;
            }
            Console.WriteLine("Skipping " + numberOfRoundsToSkip + " round(s)...");
            int currentRound = _slamCrash.GetLastGame().number;
            int waitForRound = currentRound + numberOfRoundsToSkip;
            while (currentRound < waitForRound)
            {
                _slamCrash.CustomTimeout(1000);
                int lastGame = _slamCrash.GetLastGame().number;
                if ( lastGame != currentRound)
                {
                    currentRound = lastGame;
                    Console.WriteLine("Skipped a round: " + lastGame);
                    history = _slamCrash.UpdateHistory(history);
                }
            }
        }
    }
}