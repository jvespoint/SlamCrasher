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
        }
        private void WeLost()
        {
            BetFromStreakProfit(OriginalWinProfit());
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
            Simulate(false, WeWon, BeforeFirstBet, BeforeBet);
        }

    }
}
