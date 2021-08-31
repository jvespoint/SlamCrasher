using NUnit.Framework;
using Pages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts
{
    public class PlayGame : BaseScript
    {
        public SlamCrash _slamCrash;
        public List<PreviousGame> history;

        [SetUp]
        public void PlayGameSetup()
        {
            NewBrowserSetup();
            _slamCrash = new SlamCrash(driver);
            history = new List<PreviousGame>();
        }
        [OneTimeTearDown]
        public void PlayGameTearDown() => TearDown();

        
        public void SetBet(decimal nextBet, decimal lastBet, decimal nextTarget, decimal lastTarget, decimal balance)
        {
            int maxClicks = 80;
            List<int> diffs = new List<int>{
                Convert.ToInt32(tokenNormal * (nextBet - lastBet)), //diffFromLast 0
                Convert.ToInt32(tokenNormal * (nextBet - tokenStart)), //diffFromStart 1
                Convert.ToInt32(tokenNormal * (nextBet - tokenMinBet)), //diffFromMin 2
                Convert.ToInt32(tokenNormal * (nextBet - balance)), //diffFromMax 3
                Convert.ToInt32(tokenNormal * (nextBet - (balance / 4))), //diffFrom25 4
                Convert.ToInt32(tokenNormal * (nextBet - (balance / 2))), //diffFrom50 5
                Convert.ToInt32(tokenNormal * (nextBet - (3 * (balance / 4)))) //diffFrom75 6
            };
            int minClicks = diffs.Min(x => Math.Abs(x));
            if (minClicks == Math.Abs(diffs[2]) && (Math.Abs(diffs[2]) + 50) < Math.Abs(diffs[0]))
            {
                _slamCrash.SetBetToMin();
                _slamCrash.IncrementButtons(diffs[2], true);
                //Console.WriteLine("Bet set from minimum: " + nextBet);
            }
            else if (Math.Abs(diffs[0]) > maxClicks && minClicks != Math.Abs(diffs[0]))
            {
                if (minClicks == Math.Abs(diffs[1]) && Math.Abs(diffs[1]) < maxClicks)
                {
                    _slamCrash.RefreshPage();
                    _slamCrash.ClearServerConnect("After refresh for start bet");
                    _slamCrash.wait.Until(ready => _slamCrash.AllButtonsReady);
                    _slamCrash.IncrementButtons(diffs[1], true);
                    _slamCrash.InitializeTarget(); //has to be set again after page-load
                    //Console.WriteLine("Bet set from start: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[3]) && Math.Abs(diffs[3]) < maxClicks)
                {
                    _slamCrash.SetBetToMax();
                    _slamCrash.IncrementButtons(diffs[3], true);
                    //Console.WriteLine("Bet set from maximum: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[4]) && Math.Abs(diffs[4]) < maxClicks)
                {
                    _slamCrash.SetBetTo25();
                    _slamCrash.IncrementButtons(diffs[4], true);
                    //Console.WriteLine("Bet set from 25%: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[5]) && Math.Abs(diffs[5]) < maxClicks)
                {
                    _slamCrash.SetBetTo50();
                    _slamCrash.IncrementButtons(diffs[5], true);
                    //Console.WriteLine("Bet set from 50%: " + nextBet);
                }
                else if (minClicks == Math.Abs(diffs[6]) && Math.Abs(diffs[6]) < maxClicks)
                {
                    _slamCrash.SetBetTo75();
                    _slamCrash.IncrementButtons(diffs[6], true);
                    //Console.WriteLine("Bet set from 75%: " + nextBet);
                }
                else
                {
                    _slamCrash.SetBetCloseEnough(nextBet, token);
                    //Console.WriteLine("Bet set with slider: " + nextBet);
                }
            }
            else
            {
                //Always better to imcrement from last bet than to use the range slider
                _slamCrash.IncrementButtons(diffs[0], true);
                //Console.WriteLine("Bet set from last bet: " + nextBet);
            }
            //Set Auto-Cashout
            int targetClicks = Convert.ToInt32((nextTarget - lastTarget) * targetNormal);
            _slamCrash.IncrementButtons(targetClicks, false);
            //Console.WriteLine("Auto-Cashout set to: " + nextTarget);
        }
    }
}