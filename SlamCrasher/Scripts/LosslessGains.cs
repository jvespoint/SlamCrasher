using NUnit.Framework;
using System;
using System.Linq;

namespace Scripts
{
    public class LosslessGains : PlayGame
    {
        [Test]
        public void LosslessGainsStrategy()
        {
            _slamCrash.Goto(gameUrl);
            _slamCrash.ClearServerConnect("Inital Load");
            _slamCrash.wait.Until(readyToLogin => _slamCrash.ReadyForLogin);
            _slamCrash.Login(demo, token);

            int winsSoFar = 0;
            decimal nextBet = startingBet;
            decimal lastBet = tokenStart;
            decimal nextTarget = cashout;
            decimal lastTarget = targetDefault;
            int winStreak = 0;
            decimal streakWin = 0;
            int lossStreak = 0;
            decimal streakLoss = 0.00m;
            decimal startingBalance = _slamCrash.GetBalance(token);
            decimal originalWinProfit = (nextBet * nextTarget) - nextBet;

            _slamCrash.InitializeTarget();

            while (winsSoFar < winsPerRun)
            {
                decimal balance = _slamCrash.GetBalance(token);
                Console.WriteLine("Balance: " + balance);

                SetBet(nextBet, lastBet, nextTarget, lastTarget, balance);

                while (_slamCrash.WinIndicator || _slamCrash.LossIndicator)
                {
                    _slamCrash.CustomTimeout(100);
                }

                lastBet = nextBet;
                lastTarget = nextTarget;

                _slamCrash.Click(_slamCrash.betButtonLocator);
                _slamCrash.CustomTimeout(50);
                if (!_slamCrash.BetPlaced)
                {
                    _slamCrash.Click(_slamCrash.betButtonLocator);
                }

                bool weDidWin = _slamCrash.CheckForWin();
                if (weDidWin)
                {
                    lossStreak = 0;
                    streakLoss = 0;
                    winStreak++;
                    decimal profit = (nextBet * nextTarget) - nextBet;
                    streakWin += profit;
                    //
                    if (winStreak == 1)
                    {
                        streakWin = originalWinProfit;
                    }
                    nextBet = startingBet + (streakWin / 2);
                }
                else
                {
                    lossStreak++;
                    streakLoss += nextBet;
                    winStreak = 0;
                    streakWin = 0;
                    //
                    if (lossStreak == 1)
                    {
                        streakLoss = startingBet;
                    }
                    nextBet = (streakLoss) / (nextTarget - 1);
                }
                if (nextBet < tokenMinBet) { nextBet = tokenMinBet; }
                if (nextBet % tokenMinBet != 0)
                {
                    nextBet -= tokenMinBet / 10;
                    nextBet = decimal.Round(nextBet, tokenNormal.ToString().ToCharArray().Count(c => c == '0'));
                }
                Console.WriteLine("Win: " + weDidWin + ". " + nextBet + token);
            }
            Console.WriteLine("Success. Won " + winsSoFar + " games:");
            Console.WriteLine("Starting balance was " + startingBalance + token);
            Console.WriteLine("Final balance is " + _slamCrash.GetBalance(token) + token);
        }
    }
}
