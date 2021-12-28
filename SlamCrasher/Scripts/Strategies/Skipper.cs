using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts
{
    public class SkipperTargets
    {
        public decimal[] crashPoints;
        public int[] requiredSkips;
        public int[] absorbableLosses;
        public double riskTolerance;
        public SkipperTargets(double ch)
        {
            riskTolerance = ch;
            //crashPoints = new decimal[] { 1.01m, 1.02m, 1.05m ,1.10m, 1.20m, 1.30m, 1.40m, 1.50m, 1.60m, 1.70m, 1.80m, 1.90m, 2.00m, 2.50m, 3.00m, 3.50m, 4.00m, 4.50m, 5.00m, 6.00m, 7.00m, 8.00m, 9.00m, 10.00m, 11.00m, 12.00m, 13.00m, 14.00m, 15.00m, 16.00m, 17.00m, 18.00m, 19.00m, 20.00m, 21.00m, 22.00m, 23.00m, 24.00m, 25.00m, 26.00m, 27.00m, 28.00m, 29.00m, 30.00m, 31.00m, 32.00m, 33.00m, 34.00m, 35.00m, 40.00m, 45.00m, 50.00m, 55.00m, 60.00m, 65.00m, 70.00m, 75.00m, 80.00m, 85.00m, 90.00m, 95.00m, 100.00m, 125.00m, 150.00m, 175.00m, 200.00m, 250.00m, 300.00m, 350.00m, 400.00m, 450.00m, 500.00m};
            int numTargets = 5080;
            crashPoints = new decimal[numTargets];
            for (int i = 0; i < numTargets; i++)
            {
                if (i < 100)
                {
                    crashPoints[i] = 1m + (0.01m * (i + 1));
                }
                else
                {
                    crashPoints[i] = 2m + (0.1m * ((i - 100) + 1));
                }
            }
            absorbableLosses = new int[crashPoints.Length];
            requiredSkips = new int[crashPoints.Length];
        }
        public void CalculateSkips()
        {
            Console.WriteLine($"Skipper Strategy: Wait for a setup between {crashPoints[0]}x-{crashPoints[^1]}x");
            Console.WriteLine($"Chance of losing everything is 0{riskTolerance * 100:.##########}% (1 out of {Math.Round(1 / riskTolerance, 0):N0})");
            for (int i = 0; i < crashPoints.Length; i++)
            {
                double chanceOfEachLoss = double.Parse((1 - (1 / crashPoints[i])).ToString());
                double calculatedMaxStreak = Math.Log(riskTolerance) / Math.Log(chanceOfEachLoss);
                int roundedMaxStreak = int.Parse(Math.Round(calculatedMaxStreak, 0).ToString());
                requiredSkips[i] = roundedMaxStreak - absorbableLosses[i];
                Console.WriteLine($"{crashPoints[i]}x: Max {roundedMaxStreak}, Absorb {absorbableLosses[i]}, Skip {requiredSkips[i]}");
            }
        }
    }
    public class Skipper : GameScript
    {
        bool streak;
        SkipperTargets targets;

        private void BeforeFirstBet()
        {
            targets = new SkipperTargets(risk);
            //calculate acceptable losses from our balance
            for (int i = 0; i < targets.crashPoints.Length; i++)
            {
                int streak = 0;
                streakLoss = 0.00m;
                //decimal bal = balance;
                decimal bal = 220.00m;
                nextTarget = targets.crashPoints[i];
                nextBet = tokenMinBet;
                ValidateBet();
                while (bal - nextBet > 0.00m)
                {
                    streak++;
                    bal -= nextBet;
                    streakLoss += nextBet;
                    WeLost();
                }
                targets.absorbableLosses[i] = streak - 1; // -1 because the last one would bankrupt us if we lost.
            }
            nextBet = tokenMinBet;
            streakLoss = 0.00m;
            targets.CalculateSkips();
            
            nextBet = startingBet;
            nextTarget = cashout;
            _history.GetRequiredHistory(targets.requiredSkips.Max());
        }
        private void BeforeBet()
        {
            if (!streak)
            {
                decimal targ = 0.00m;
                for (int i = 0; i < targets.crashPoints.Length; i++)
                {
                    if (_history.LastGamesLoss(targets.requiredSkips[i], targets.crashPoints[i]))
                    {
                        targ = targets.crashPoints[i];
                    }
                }
                if (targ != 0.00m)
                {
                    streak = true;
                    nextTarget = targ;
                    nextBet = tokenMinBet;
                    ValidateBet();
                    SetBet(nextBet, nextTarget, balance);
                }
                else
                {
                    try
                    {
                        _history.SkipGames(1);
                    }
                    catch (NoSuchElementException)
                    {

                    }
                    BeforeBet();
                }
            }
        }
        private void WeWon()
        {
            streak = false;
            nextBet = tokenMinBet;
            _history.SkipGames(1);
        }
        private void WeLost()
        {
            BetFromStreakProfit(tokenMinBet);
        }
        [Test]
        public void SkipperStrategy()
        {
            PlayGame(WeLost, WeWon, BeforeFirstBet, BeforeBet);
        }
    }
}
