using NUnit.Framework;

namespace Scripts
{
    public class Lossless : GameScript
    {
        private void PreFirstRoll()
        {
            void CheckTwoLosses()
            {
                if (!_history.LastGamesLoss(2, nextTarget))
                {
                    SkipGames(1);
                    CheckTwoLosses();
                }
            }
            //CheckTwoLosses();
        }
        private void WeWon()
        {
            nextBet = startingBet;
        }
        private void WeLost()
        {
            nextBet = (streakLoss + (originalWinProfit / 2)) / (nextTarget - 1);
            if (nextBet > startingBet * 1000)
            {
                _slamCrash.CrashOut("Wtf");
            }
        }
        
        [Test]
        public void LosslessStrategy()
        {
            PlayGame(WeLost, WeWon, PreFirstRoll);
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
