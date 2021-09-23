using NUnit.Framework;

namespace Scripts
{
    public class Steady : GameScript
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
            BetFromStreakProfit(OriginalWinProfit() / 2.00m);
        }
        [Test]
        public void SteadyStrategy()
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
