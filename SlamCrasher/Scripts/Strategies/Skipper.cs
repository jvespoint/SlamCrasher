using NUnit.Framework;
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
                //The numberOfGames values have been pre-calculated. Please confirm these are still accurate!
                decimal targ = 0.00m;
                if (_history.LastGamesLoss(3, 1.50m))
                {
                    targ = 1.50m;
                }
                if (_history.LastGamesLoss(10, 2.00m))
                {
                    targ = 2.00m;
                }
                if (_history.LastGamesLoss(13, 3.00m))
                {
                    targ = 3.00m;
                }
                if (_history.LastGamesLoss(17, 4.00m))
                {
                    targ = 4.00m;
                }
                if (_history.LastGamesLoss(21, 5.00m))
                {
                    targ = 5.00m;
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
                    _history.SkipGames(1);
                    BeforeBet();
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
