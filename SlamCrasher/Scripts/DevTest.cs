using NUnit.Framework;
using Pages;
using System;

namespace Scripts
{
    public class DevTest : GameScript
    {
        [Test]
        public void Script()
        {
            NewBrowserSetup();
            _slamCrash = new SlamCrash(driver);
            _slamCrash.Goto(gameUrl);
            _slamCrash.ClearServerConnect("Inital Load");
            _slamCrash.wait.Until(readyToLogin => _slamCrash.ReadyForLogin);
            _history = new History(driver, historyFile);
            _slamCrash.Login(true, token);
            token = "slam";
            //_history.FindMaxLossStreakForTarget(new decimal[22] { 1.01m, 1.10m, 1.20m, 1.30m, 1.40m, 1.50m, 2.00m, 3.00m, 4.00m, 5.00m, 6.00m, 7.00m, 8.00m, 9.00m, 10.00m, 15.00m, 20.00m, 25.00m, 50.00m, 100.00m, 500.00m, 1000m });
            _history.FindMaxLossStreakForTarget(new decimal[1] { 10.00m });
        }
    }
}
