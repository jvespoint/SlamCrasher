using NUnit.Framework;

namespace Scripts
{
    public class SafeySafe : GameScript
    {
        private void BeforeFirstBet()
        {
            
        }
        private void BeforeBet()
        {
            _history.WaitForLosses(2, 1.50m);
        }
        private void SimBeforeBet()
        {
            //_history.WaitForLosses(2, 1.50m);
        }
        private void WeWon()
        {
            nextBet = startingBet;
        }
        private void WeLost()
        {
            BetFromStreakProfit(tokenMinBet);
        }
        [Test]
        public void SafeySafeStrategy()
        {
            PlayGame(WeLost, WeWon, BeforeFirstBet, BeforeBet);
        }
        //Simulation
        [Test]
        public void SimulateLosses()
        {
            Simulate(false, WeLost, BeforeFirstBet, SimBeforeBet);
        }
        [Test]
        public void SimulateWins()
        {
            Simulate(true, WeWon, BeforeFirstBet, BeforeBet);
        }

    }
}
