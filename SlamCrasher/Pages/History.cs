﻿using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pages
{
    public class History : BasePage
    {
        public List<PreviousGame> games;
        public int oldestGameNumber;

        public History(IWebDriver driver) : base(driver)
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
                    catch(StaleElementReferenceException)
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
            int nextGameNumber = games[games.Count - 1].number + 1;
            while (ElementExists(By.XPath($"//div[contains(@onclick,'{nextGameNumber}')]")))
            {
                string? crashString = null;
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
                Console.WriteLine("Added game: " + nextGame.number);
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
                if (lastGameNumber != games[games.Count -1].number) {
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
        public bool LastGamesLoss(int numberOfGames, decimal target)
        {
            List<bool> lost = new List<bool>();
            for (int i=1; i < numberOfGames+1; i++)
            {
                decimal gameCrash = games[games.Count - i].crash;
                int gameNumber = games[games.Count - i].number;
                lost.Add(gameCrash < target);
            }
            Console.WriteLine(lost.All(x => x == true));
            return lost.All(x => x == true);
        }
        public decimal LastFewWinRatio(int few, decimal target)
        {
            List<PreviousGame> lastFew = new List<PreviousGame>();
            if (few != 100)
            {
                for (int i = 1; i < few + 1; i++)
                {
                    lastFew.Add(games[games.Count - i]);
                }
            }
            else
            {
                lastFew = games;
            }
            decimal ratio = (decimal)lastFew.Count(x => x.crash > target) / lastFew.Count;
            Console.WriteLine("Ratio: " + ratio + " for last " + few + " games.");
            return ratio;
        }
    }
}
