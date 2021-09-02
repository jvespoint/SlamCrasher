using NUnit.Framework;
using Pages;
using System;
using System.Linq;

namespace Scripts
{
    public class Tests : GameScript
    {
        [Test]
        public void TestSetBetScript()
        {
            SetBet(0.02m, 1.00m, 2.00m, 2.00m, 100.00m);
            SkipGames(1);
            SetBet(0.04m, 0.02m, 2.00m, 2.00m, 100.00m);
            SkipGames(1);
            SetBet(0.08m, 0.04m, 2.00m, 2.00m, 100.00m);
            SkipGames(1);
            SetBet(0.16m, 0.08m, 2.00m, 2.00m, 100.00m);
            SkipGames(1);
            SetBet(0.32m, 0.16m, 2.00m, 2.00m, 100.00m);
            SkipGames(1);
            SetBet(0.64m, 0.32m, 2.00m, 2.00m, 100.00m);
            SkipGames(1);
            SetBet(1.28m, 0.64m, 2.00m, 2.00m, 100.00m);
            SkipGames(1);
            SetBet(2.56m, 1.25m, 2.00m, 2.00m, 100.00m);
            SkipGames(1);
            SetBet(5.12m, 2.56m, 2.00m, 2.00m, 100.00m);
            SkipGames(1);
            SetBet(10.24m, 5.12m, 2.00m, 2.00m, 100.00m);
            SkipGames(1);
            SetBet(20.48m, 10.24m, 2.00m, 2.00m, 100.00m);
        }
    }
}
