using NUnit.Framework;
using Pages;
using System.Linq;

namespace Scripts
{
    public class BuildRoundHistory : GameScript
    {
        [Test]
        public void BuildRoundHistoryScript()
        {
            NewBrowserSetup();
            _slamCrash = new SlamCrash(driver);
            _slamCrash.Goto(gameUrl);
            _slamCrash.ClearServerConnect("Inital Load");
            _slamCrash.wait.Until(readyToLogin => _slamCrash.ReadyForLogin);
            _history = new History(driver, historyFile);

            int firstGapStart = _history.FindFirstGap();
            while (firstGapStart != _history.games.Count - 1) 
            {
                FillGap(firstGapStart);
                _history.WriteHistoryFile();
                firstGapStart = _history.FindFirstGap();
            }
        }
        public void FillGap(int gapStart)
        {
            int gapStartRoundNumber = _history.games[gapStart].number;
            int gapEndRoundNumber = _history.games[gapStart + 1].number;
            if (gapEndRoundNumber - gapStartRoundNumber > 600)
            {
                gapEndRoundNumber = gapStartRoundNumber + 600;
            }
            for (int i = gapStartRoundNumber; i < gapEndRoundNumber; i++)
            {
                PreviousGame missingRound = _history.GetSpecificRound(i+1);
                _history.games.Add(missingRound);
            }
        }
    }
}
