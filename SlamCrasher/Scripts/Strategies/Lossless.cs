using NUnit.Framework;
using Pages;

namespace Scripts
{
    public class Lossless : GameScript
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
            nextBet = startingBet;
        }
        private void WeLost()
        {
            nextBet = (streakLoss + (((nextBet * nextTarget) - nextBet) / 2)) / (nextTarget - 1);
            if (nextBet > startingBet * 1000)
            {
                EndGame("Wtf");
            }
        }
        
        [Test]
        public void LosslessStrategy()
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
