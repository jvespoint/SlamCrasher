using NUnit.Framework;

namespace Scripts
{
    public class CalcSafe : GameScript
    {
        int waitFor;
        public int CanLose(decimal startBal, decimal startBet)
        {
            decimal bal = startBal;
            decimal loss = 0;
            decimal bet = startBet;
            int count = 0;
            while (true)
            {
                loss += bet;
                bal -= bet;
                bet = (loss + tokenMinBet) / (cashout - 1m);
                if ((bal - bet) <= 0)
                {
                    return count + 1;
                }
                else
                {
                    count++;
                }
            }
        }
        private void BeforeFirstBet()
        {
            waitFor = (_history.FindMaxLossStreakForTarget(new decimal[] { cashout })[0] - CanLose(balance, nextBet)) + 1;
        }
        private void BeforeBet()
        {
            _history.WaitForLosses(waitFor, 1.50m);
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
        public void CalcSafeStrategy()
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
