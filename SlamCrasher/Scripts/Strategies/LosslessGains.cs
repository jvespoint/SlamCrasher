using NUnit.Framework;
using Pages;

namespace Scripts
{
    public class LosslessGains : GameScript
    {
        private void BeforeFirstBet()
        {
            _history = new History(driver);
            SkipGames(1);
            WaitForLosses();
        }
        private void BeforeBet()
        {
            _history.Update();
        }
        private void WeWon()
        {
            nextBet = startingBet + (totalProfit / cashout);
        }
        private void WeLost()
        {
            nextBet = (streakLoss + (((nextTarget * startingBet) - startingBet) / 2)) / (nextTarget - 1);
        }

        [Test]
        public void LosslessGainsStrategy()
        {
            PlayGame(WeLost, WeWon, BeforeFirstBet, BeforeBet);
        }
        [Test]
        public void SimulateLosses()
        {
            Simulate(false, WeLost);
        }
        [Test]
        public void SimulateWins()
        {
            Simulate(true, WeWon);
        }

    }
}
