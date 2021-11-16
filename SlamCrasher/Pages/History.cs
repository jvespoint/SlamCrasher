using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using Newtonsoft.Json;
using System.IO;

namespace Pages
{
    public class PreviousGame
    {
        public decimal crash;
        public int number;
        
        public PreviousGame(int roundNumber, decimal crashPoint)
        {
            number = roundNumber;
            crash = crashPoint;
        }
        public string FileFormat()
        {
            return number.ToString() + " " + crash.ToString();
        }
    }
    class ItemEqualityComparer : IEqualityComparer<PreviousGame>
    {
        public bool Equals(PreviousGame x, PreviousGame y)
        {
            // Two items are equal if their keys are equal.
            return x.number == y.number;
        }

        public int GetHashCode(PreviousGame obj)
        {
            return obj.number.GetHashCode();
        }
    }
    public class History : BasePage
    {
        public List<PreviousGame> games;
        public string historyFile; //format: {round number} {crash-point}
        public int oldestGameNumber; //oldest game shown on history tab
        public int firstRoundEver = 1000000; //rounds before 1mil are invalid
        public string getRoundURL = "https://slamcrash.com/history/round/";
        public By RoundLoadedIndicator(int round) => By.XPath($"//div[text()='ROUND: #{round}']"); //when round info is diisplayed
        public By CrashPointLocator => By.XPath("//div[@class='item-after']/span[contains(text(),'x')]"); //shown when round crashes
        public History(IWebDriver driver, string filename) : base(driver) 
        {
            historyFile = filename;
            GetLast50();
            try
            {
                List<PreviousGame> gamesOnFile = ReadHistoryFile();
                games.AddRange(gamesOnFile);
            }
            catch (Exception)
            {
                Console.WriteLine("Warning: History file not found.");
            }
            finally
            {
                games = games.Distinct(new ItemEqualityComparer()).ToList();
                games = games.OrderBy(o => o.number).ToList();
            }
        }
        public void CloseRoundHistoryPopup() => Click(By.XPath("//a[text()='Close']"));
        public List<PreviousGame> ReadHistoryFile()
        {
            string[] lines = File.ReadAllLines(historyFile); //1000 1.25
            List<PreviousGame> linesList = new List<PreviousGame>();
            foreach(string line in lines)
            {
                string[] data = line.Split(" ");
                linesList.Add(new PreviousGame(Int32.Parse(data[0]), decimal.Parse(data[1])));
            }
            return linesList;
        }
        public void WriteHistoryFile()
        {
            games = games.OrderBy(o => o.number).ToList();
            PreviousGame[] gamesArray = games.ToArray();
            string[] gamesStringArray = new string[gamesArray.Length];
            for(int i = 0; i < gamesArray.Length; i++)
            {
                string gameString = gamesArray[i].FileFormat();
                gamesStringArray[i] = gameString;
            }
            File.WriteAllLines(historyFile, gamesStringArray);
        }
        public void GetLast50()
        {
            Click(historyButtonLocator);
            wait.Until(ready => HistoryPopupShown);
            games = new List<PreviousGame>();

            oldestGameNumber = Convert.ToInt32(Find(By.XPath("(//div[contains(@class,'gameslog-txt')])[50]")).GetAttribute("data-game-id"));
            PreviousGame OldestGame = new PreviousGame(oldestGameNumber, decimal.Parse(Find(By.XPath("(//div[contains(@class,'gameslog-txt')])[50]")).Text.Replace("x", "")));
            games.Add(OldestGame);

            int nextGameNumber = oldestGameNumber + 1;
            while (ElementExists(By.XPath($"//div[contains(@data-game-id,'{nextGameNumber}')]")))
            {
                PreviousGame nextGame = null;
                int attempts = 3;
                while (attempts > 0)
                {
                    try
                    {
                        nextGame = new PreviousGame(Convert.ToInt32(Find(By.XPath($"//div[contains(@data-game-id,'{nextGameNumber}')]")).GetAttribute("data-game-id")), decimal.Parse(Find(By.XPath($"//div[contains(@data-game-id,'{nextGameNumber}')]")).Text.Replace("x", "")));
                        attempts = 0;
                    }
                    catch (StaleElementReferenceException)
                    {
                        attempts--;
                        CustomTimeout(50);
                    }
                }
                if (nextGame == null)
                {
                    throw new StaleElementReferenceException();
                }
                games.Add(nextGame);
                nextGameNumber++;
            }
            Click(historyCloseLinkLocator);
            CustomTimeout(200); //for the splash screen to disappear
            Update();
        }
        public void Update()
        {
            int numberOfGames = games.Count;
            int nextGameNumber = games[^1].number + 1;
            while (ElementExists(By.XPath($"//div[contains(@onclick,'{nextGameNumber}')]")))
            {
                #nullable enable
                string? crashString = null;
                #nullable disable
                int attempts = 3;
                while (attempts > 0)
                {
                    try
                    {
                        crashString = Find(By.XPath($"//div[contains(@onclick,'{nextGameNumber}')]")).Text.Replace("x", "");
                        attempts = 0;
                    }
                    catch(StaleElementReferenceException)
                    {
                        attempts--;
                    }
                }
                if (crashString == null)
                {
                    throw new StaleElementReferenceException();
                }
                PreviousGame nextGame = new PreviousGame(nextGameNumber, decimal.Parse(crashString));
                games.Add(nextGame);
                Console.WriteLine("Added game: " + nextGame.number + ". Crashed: " + nextGame.crash);
                nextGameNumber++;
            }
            if (numberOfGames == games.Count)
            {
                //None of the last 6 games matched. We might be further than 6 behind.
                int attempts = 3;
                int? lastGameNumber = null;
                while (attempts > 0)
                {
                    try
                    {
                        lastGameNumber = Convert.ToInt32(Find(By.XPath("//div[@class='prev_games']/div[1]")).GetAttribute("onclick").Replace("Hexa.history.game_detail.view(", "").Replace(");", ""));
                        attempts = 0;
                    }
                    catch(StaleElementReferenceException)
                    {
                        CustomTimeout(100);
                        attempts--;
                    }
                }
                if (lastGameNumber == null)
                {
                    throw new StaleElementReferenceException();
                }
                if (lastGameNumber != games[^1].number) {
                    Click(historyButtonLocator);
                    wait.Until(ready => HistoryPopupShown);
                    while (ElementExists(By.XPath($"//div[contains(@data-game-id,'{nextGameNumber}')]")))
                    {
                        PreviousGame nextGame = new PreviousGame(Convert.ToInt32(Find(By.XPath($"//div[contains(@data-game-id,'{nextGameNumber}')]")).GetAttribute("data-game-id")), decimal.Parse(Find(By.XPath($"//div[contains(@data-game-id,'{nextGameNumber}')]")).Text.Replace("x", "")));
                        games.Add(nextGame);
                        nextGameNumber++;
                    }
                    Click(historyCloseLinkLocator);
                    CustomTimeout(200); //for the splash screen to disappear
                }
            }
        }
        public PreviousGame GetSpecificRound(int round)
        {
            if (round < firstRoundEver)
            {
                round = firstRoundEver;
            }
            Goto(getRoundURL + round);
            try
            {
                wait.Until(loaded => ElementExists(RoundLoadedIndicator(round)));
            }
            catch (UnhandledAlertException)
            {
                return new PreviousGame(round, 1.00m);
            }
            catch (WebDriverTimeoutException)
            {
                RefreshPage();
                wait.Until(loaded => ElementExists(RoundLoadedIndicator(round)));
            }
            CustomTimeout(500);
            decimal crashPoint = 0.00m;
            for(int tries = 0; tries < 5; tries++)
            {
                try
                {
                    crashPoint = decimal.Parse(Find(CrashPointLocator).Text.Replace("x", ""));
                    break;
                }
                catch (WebDriverTimeoutException)
                {

                }
            }
            if (crashPoint == 0.00m)
            {
                RefreshPage();
                CustomTimeout(1000);
                return new PreviousGame(round, 1.001m);
            }
            return new PreviousGame(round, crashPoint);
        }
        public void SkipGames(int n)
        {
            int currentGame = games[^1].number;
            int endGame = currentGame + n;
            while (currentGame < endGame)
            {
                CustomTimeout(1000);
                Update();
                currentGame = games[^1].number;
            }
        }
        public bool LastGamesLoss(int numberOfGames, decimal target)
        {
            if (numberOfGames < 1)
            {
                return true;
            }
            List<bool> lost = new List<bool>();
            for (int i=1; i < numberOfGames+1; i++)
            {
                decimal gameCrash = games[^i].crash;
                int gameNumber = games[^i].number;
                lost.Add(gameCrash < target);
            }
            return lost.All(x => x == true);
        }
        public void WaitForLosses(int n, decimal target)
        {
            if (!LastGamesLoss(n, target))
            {
                Console.WriteLine("Skipping game: WaitForLosses");
                SkipGames(1);
                WaitForLosses(n, target);
            }
        }
        public decimal WinRatio(int few, decimal target)
        {
            List<PreviousGame> lastFew = new List<PreviousGame>();
            if (few != 1000000)
            {
                for (int i = 1; i < few + 1; i++)
                {
                    lastFew.Add(games[^i]);
                }
            }
            else
            {
                lastFew = games;
            }
            decimal ratio = decimal.Round((decimal)lastFew.Count(x => x.crash > target) / lastFew.Count, 4);
            string toLog = "Ratio: " + ratio;
            toLog += few == 1000000 ? " for known history." : " for last " + few + " games.";
            Console.WriteLine(toLog);
            return ratio;
        }
        public int GameIdFromRoundNumber(int roundNumber) => games.FindIndex(r => r.number == roundNumber);
        public List<int> FindMissingGames()
        {
            List<int> missing = Enumerable.Range(games.First().number, games.Last().number - games.First().number + 1).Except(games.Select(x => x.number)).ToList();
            return missing;
        }
        public int FindFirstGap()
        {
            int gap = FindMissingGames()[0] - 1;
            Console.WriteLine($"Made it to round {gap} before a gap.");
            return gap;
        }
        public int FindFirstLossAfter(int roundNumber, decimal target)
        {
            int lossIndex = games.FindIndex(GameIdFromRoundNumber(roundNumber), x => x.crash < target);
            return games[lossIndex].number;
        }
        public int FindFirstWinAfter(int roundNumber, decimal target)
        {
            int lossIndex = games.FindIndex(GameIdFromRoundNumber(roundNumber), x => x.crash >= target);
            return games[lossIndex].number;
        }
        public int[] FindMaxLossStreakForTarget(decimal[] targets)
        {
            int firstGapNumber = FindFirstGap();
            int[] firstLossNumbers = new int[targets.Count()];
            int[] maxLossStreaks = new int[targets.Count()];
            int[] maxLossStreakIndexes = new int[targets.Count()];
            for (int i = 0; i < targets.Count(); i++)
            {
                firstLossNumbers[i] = FindFirstLossAfter(firstRoundEver, targets[i]);
                while (true)
                {
                    int firstWinAfterLossNumber = FindFirstWinAfter(firstLossNumbers[i], targets[i]);
                    if (firstWinAfterLossNumber > firstGapNumber) { break; }

                    int thisLossStreak = firstWinAfterLossNumber - firstLossNumbers[i];
                    if (thisLossStreak > maxLossStreaks[i])
                    {
                        maxLossStreaks[i] = thisLossStreak;
                        maxLossStreakIndexes[i] = firstLossNumbers[i];
                    }
                    firstLossNumbers[i] = FindFirstLossAfter(firstWinAfterLossNumber, targets[i]);
                }
                Console.WriteLine($"Max losses for {targets[i]}x is {maxLossStreaks[i]} @  #{maxLossStreakIndexes[i]} (S/T: {maxLossStreaks[i] / targets[i]})");
            }
            return maxLossStreaks;
        }
    }
}
