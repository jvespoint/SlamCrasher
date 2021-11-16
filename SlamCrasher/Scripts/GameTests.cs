using NUnit.Framework;
using Pages;

namespace Scripts
{
    public class GameTests : GameScript
    {
        [Test]
        public void TestSetBetScript()
        {
            NewBrowserSetup();
            _slamCrash = new SlamCrash(driver);
            _slamCrash.Goto(gameUrl);
            _slamCrash.ClearServerConnect("Inital Load");
            _slamCrash.wait.Until(readyToLogin => _slamCrash.ReadyForLogin);
            _history = new History(driver, historyFile);
            _slamCrash.Login(true, token);
            token = "slam";
            _slamCrash.InitializeTarget();
            
            SetBet(0.64m, 1.00m, 1.05m, 2.00m, 100.00m);
            _history.SkipGames(1);
            SetBet(1.28m, 0.64m, 1.10m, 1.05m, 100.00m);
            _history.SkipGames(1);
            SetBet(2.56m, 1.00m, 2.00m, 1.10m, 100.00m);
            _history.SkipGames(1);
            SetBet(5.20m, 2.56m, 20.00m, 2.00m, 100.00m);
            //_history.SkipGames(1);
            //SetBet(50.20m, 5.00m, 2.00m, 2.00m, 100.00m);
            //_history.SkipGames(1);
            //SetBet(75.20m, 10.00m, 2.00m, 2.00m, 100.00m);
        }
    }
}
