using NUnit.Framework;
using Pages;
using System;

namespace Scripts
{
    public class DevTest : GameScript
    {
        [Test]
        public void BeeperTest()
        {
            //Loss
            Console.Beep(440, 500);
            System.Threading.Thread.Sleep(1000);
            //Win
            Console.Beep(220, 1000);
        }
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
