using NUnit.Framework;
using OpenQA.Selenium;
using System;

namespace Scripts
{
    public class Skipper : GameScript
    {
        bool streak;
        private void BeforeFirstBet()
        {
            Console.WriteLine($"Skipper Strategy: Wait for an assured win between 1.50x-5x. Only an unprecedented loss-streak can stop us!");
        }
        private void BeforeBet()
        {
            if (!streak)
            {
                //The numberOfGames values have been pre-calculated. Please confirm these are still safe!
                decimal targ = 0.00m;
                if (_history.LastGamesLoss(2, 1.10m))
                {
                    targ = 1.10m;
                }
                if (_history.LastGamesLoss(3, 1.20m))
                {
                    targ = 1.20m;
                }
                if (_history.LastGamesLoss(4, 1.50m))
                {
                    targ = 1.50m;
                }
                if (_history.LastGamesLoss(8, 2.00m))
                {
                    targ = 2.00m;
                }
                if (_history.LastGamesLoss(14, 3.00m))
                {
                    targ = 3.00m;
                }
                if (_history.LastGamesLoss(22, 4.00m))
                {
                    targ = 4.00m;
                }
                if (_history.LastGamesLoss(28, 5.00m))
                {
                    targ = 5.00m;
                }
                if (_history.LastGamesLoss(36, 6.00m))
                {
                    targ = 6.00m;
                }
                if (_history.LastGamesLoss(43, 7.00m))
                {
                    targ = 7.00m;
                }
                if (_history.LastGamesLoss(51, 8.00m))
                {
                    targ = 8.00m;
                }
                if (_history.LastGamesLoss(59, 9.00m))
                {
                    targ = 9.00m;
                }
                if (_history.LastGamesLoss(67, 10.00m))
                {
                    targ = 10.00m;
                }
                if (_history.LastGamesLoss(75, 11.00m))
                {
                    targ = 11.00m;
                }
                if (_history.LastGamesLoss(83, 12.00m))
                {
                    targ = 12.00m;
                }
                if (_history.LastGamesLoss(92, 13.00m))
                {
                    targ = 13.00m;
                }
                if (_history.LastGamesLoss(100, 14.00m))
                {
                    targ = 14.00m;
                }
                if (_history.LastGamesLoss(108, 15.00m))
                {
                    targ = 15.00m;
                }
                if (_history.LastGamesLoss(116, 16.00m))
                {
                    targ = 16.00m;
                }
                if (_history.LastGamesLoss(125, 17.00m))
                {
                    targ = 17.00m;
                }
                if (_history.LastGamesLoss(133, 18.00m))
                {
                    targ = 18.00m;
                }
                if (_history.LastGamesLoss(143, 19.00m))
                {
                    targ = 19.00m;
                }
                if (_history.LastGamesLoss(151, 20.00m))
                {
                    targ = 20.00m;
                }
                if (_history.LastGamesLoss(160, 21.00m))
                {
                    targ = 21.00m;
                }
                if (_history.LastGamesLoss(168, 22.00m))
                {
                    targ = 22.00m;
                }
                if (_history.LastGamesLoss(178, 23.00m))
                {
                    targ = 23.00m;
                }
                if (_history.LastGamesLoss(186, 24.00m))
                {
                    targ = 24.00m;
                }
                if (_history.LastGamesLoss(195, 25.00m))
                {
                    targ = 25.00m;
                }
                if (targ != 0.00m)
                {
                    streak = true;
                    nextTarget = targ;
                    nextBet = tokenMinBet;
                    ValidateBet();
                    SetBet(nextBet, nextTarget, balance);
                }
                else
                {
                    try
                    {
                        _history.SkipGames(1);
                    }
                    catch (NoSuchElementException)
                    {

                    }
                    finally
                    {
                        BeforeBet();
                    }
                }
            }
        }
        private void SimBeforeBet()
        {
            
        }
        private void WeWon()
        {
            streak = false;
            nextBet = tokenMinBet;
            _history.SkipGames(1);
        }
        private void WeLost()
        {
            BetFromStreakProfit(tokenMinBet);
        }
        private void SimWeLost()
        {
            
        }
        [Test]
        public void SkipperStrategy()
        {
            PlayGame(WeLost, WeWon, BeforeFirstBet, BeforeBet);
        }
        //Simulation
        [Test]
        public void SimulateLosses()
        {
            Simulate(false, SimWeLost, BeforeFirstBet, SimBeforeBet);
        }
        [Test]
        public void SimulateWins()
        {
            Simulate(true, WeWon, BeforeFirstBet, BeforeBet);
        }

    }
}
