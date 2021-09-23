using NUnit.Framework;
using Pages;

namespace Scripts
{
    public class Tests : GameScript
    {
        [Test]
        public void TestSetBetScript()
        {
            _history = new History(driver);
            _slamCrash.Login(true, token);
            token = "slam";

            SetBet(0.64m, 1.00m, 2.00m, 2.00m, 100.00m);
            _history.SkipGames(1);
            SetBet(1.28m, 0.64m, 2.00m, 2.00m, 100.00m);
            _history.SkipGames(1);
            SetBet(2.56m, 1.25m, 2.00m, 2.00m, 100.00m);
            _history.SkipGames(1);
            SetBet(5.12m, 2.56m, 2.00m, 2.00m, 100.00m);
            _history.SkipGames(1);
            SetBet(10.24m, 5.12m, 2.00m, 2.00m, 100.00m);
            _history.SkipGames(1);
            SetBet(20.48m, 10.24m, 2.00m, 2.00m, 100.00m);
        }
    }
}
