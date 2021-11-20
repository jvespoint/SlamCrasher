using NUnit.Framework;
using System;

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
                bet = (loss + tokenMinBet) / (cashout - 1m); // same as BetFromStreakLoss()
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
            int maxLosses = _history.FindMaxLossStreakForTarget(new decimal[] { cashout })[0];
            waitFor = (maxLosses - CanLose(balance, nextBet));
            Console.WriteLine($"CalcSafe Start-up Complete: Wait for {waitFor} losses before betting at an auto-cashout of {cashout}.");
        }
        private void BeforeBet()
        {
            _history.WaitForLosses(waitFor, cashout);
        }
        private void SimBeforeBet()
        {

        }
        private void WeWon()
        {
            nextBet = startingBet;
        }
        private void WeLost()
        {
            BetFromStreakProfit(tokenMinBet); //see CanLose()
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
