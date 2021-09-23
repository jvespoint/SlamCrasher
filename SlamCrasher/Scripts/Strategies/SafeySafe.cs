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
            
        }
        private void WeWon()
        {
            nextBet = startingBet;
            ValidateBet();
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
            Simulate(false, WeLost, BeforeFirstBet, BeforeBet);
        }
        [Test]
        public void SimulateWins()
        {
            Simulate(true, WeWon, BeforeFirstBet, BeforeBet);
        }

    }
}
