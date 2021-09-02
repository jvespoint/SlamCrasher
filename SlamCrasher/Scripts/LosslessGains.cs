using NUnit.Framework;
using Pages;
using System;
using System.Linq;

namespace Scripts
{
    public class LosslessGains : GameScript
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
            if (winStreak == 1)
            {
                if (_history.LastFewWinRatio(10, cashout) < 0.4m)
                {
                    streakWin = originalWinProfit * 2;
                }
                else
                {
                    streakWin = originalWinProfit;
                }
            }
            nextBet = startingBet + (streakWin / 2);
            if (nextBet > (startingBet * 10))
            {
                nextBet = lastBet;
            }
        }
        private void WeLost()
        {
            if (lossStreak == 1)
            {
                if (_history.LastFewWinRatio(20, cashout) < 0.7m)
                {
                    streakLoss = startingBet + originalWinProfit;
                }
                else
                {
                    streakLoss = startingBet;
                }
            }
            nextBet = (streakLoss) / (nextTarget - 1);
            if (nextBet > startingBet * 1000)
            {
                _slamCrash.CrashOut("Wtf");
            }
        }

        [Test]
        public void LosslessGainsStrategy()
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
