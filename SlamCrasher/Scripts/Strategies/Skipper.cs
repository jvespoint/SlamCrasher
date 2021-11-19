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
            cashout = 1.50m;
            nextTarget = cashout;
            ValidateBet();
            waitFor = 2;
            Console.WriteLine($"Skipper Strategy: {waitFor} losses before betting. Never change the bet amount.");
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
            
        }
        private void WeLost()
        {
            _history.SkipGames(2);
        }
        private void SimWeLost()
        {
            //_history.SkipGames(2);
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
            Simulate(false, SimWeLost, BeforeFirstBet, SimBeforeBet);
        }
        [Test]
        public void SimulateWins()
        {
            Simulate(true, WeWon, BeforeFirstBet, BeforeBet);
        }

    }
}
