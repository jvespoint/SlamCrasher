using NUnit.Framework;
using Pages;
using System;

namespace Scripts
{
    public class DevTest : GameScript
    {
        [Test]
        public void FindMaxLossStreak()
        {
            NewBrowserSetup();
            _slamCrash = new SlamCrash(driver);
            _slamCrash.Goto(gameUrl);
            _slamCrash.ClearServerConnect("Inital Load");
            _slamCrash.wait.Until(readyToLogin => _slamCrash.ReadyForLogin);
            _history = new History(driver, historyFile);
            _slamCrash.Login(true, token);
            token = "slam";
            decimal[] targetsToCheck = new decimal[] { 2.00m, 3.00m, 4.00m, 5.00m, 6.00m, 7.00m, 8.00m, 9.00m };
            _history.FindMaxLossStreakForTarget(targetsToCheck);
        }
    }
}
