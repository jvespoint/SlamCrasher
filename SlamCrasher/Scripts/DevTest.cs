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
            decimal[] targetsToCheck = new decimal[] { 1.50m };
            _history.FindMaxLossStreakForTarget(targetsToCheck);
        }
    }
}
