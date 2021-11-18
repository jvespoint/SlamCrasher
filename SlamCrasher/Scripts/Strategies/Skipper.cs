using NUnit.Framework;
using System;

namespace Scripts
{
    public class Skipper : GameScript
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
            //int maxLosses = _history.FindMaxLossStreakForTarget(new decimal[] { cashout })[0];
            int maxLosses = 10;
            waitFor = (maxLosses - CanLose(balance, nextBet));
            Console.WriteLine($"waitFor: {waitFor} losses before betting.");
        }
        private void BeforeBet()
        {
            _history.WaitForLosses(waitFor, cashout);
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
            _history.SkipGames(2);
        }
        [Test]
        public void SkipperStrategy()
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
