using NUnit.Framework;
using System;
using System.Linq;

namespace Scripts
{
    public class LosslessGains : GameScript
    {
        decimal winRatio;
        Random rnd;

        private void BeforeFirstBet()
        {
            _history = new Pages.History(driver);
        }
        private void BeforeBet()
        {
            _history.Update();
            winRatio = _history.LastFewWinRatio(100, nextTarget);
        }
        private void WeWon()
        {
            nextBet = startingBet;
        }
        private decimal Extra(decimal winRatio)
        {
            decimal targetFactor = startingBet / nextTarget;
            decimal lossStreakFactor = 2 * lossStreak * targetFactor;
            decimal ratioFactor = ExpectedAverageWinRatio() / winRatio;
            decimal extra = tokenMinBet + decimal.Round(ratioFactor * lossStreakFactor, tokenNormal.ToString().ToCharArray().Count(c => c == '0'));
            return extra;
        }
        private void WeLost()
        {
            BetFromStreakProfit(Extra(winRatio));
        }
        [Test]
        public void LosslessGainsStrategy()
        {
            PlayGame(WeLost, WeWon, BeforeFirstBet, BeforeBet);
        }

        //Simulation
        private void SimBeforeFirstBet()
        {

        }
        private void SimBeforeBet()
        {
            winRatio = 0.00m;
            for (int i = 0; i < rnd.Next(1, 10); i++)
            {
                winRatio += 0.05m;
            }
            Console.WriteLine(winRatio);
        }
        [Test]
        public void SimulateLosses()
        {
            rnd = new Random();
            Simulate(false, WeLost, SimBeforeFirstBet, SimBeforeBet);
        }
        [Test]
        public void SimulateWins()
        {
            Simulate(false, WeWon, SimBeforeFirstBet, SimBeforeBet);
        }

    }
}
