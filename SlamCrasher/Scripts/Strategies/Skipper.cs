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
            int numTargets = 4991; // 1.01 + (1 + n*0.1); 4991=500.00x
            crashPoints = new decimal[numTargets];
            crashPoints[0] = 1.01m;
            for (int i = 1; i < numTargets; i++)
            {
                crashPoints[i] = 1m + (0.10m * i);
            }
            absorbableLosses = new int[numTargets];
            requiredSkips = new int[numTargets];
        }
        public void CalculateSkips()
        {
            Console.WriteLine($"Skipper Strategy: Wait for a setup between {crashPoints[0]}x-{crashPoints[^1]}x");
            Console.WriteLine($"Chance of losing everything is 0{riskTolerance * 100:.##########}% (1 in {Math.Round(1 / riskTolerance, 0):N0})");
            for (int i = 0; i < crashPoints.Length; i++)
            {
                double chanceOfEachLoss = double.Parse((1 - (1 / crashPoints[i])).ToString());
                double calculatedMaxStreak = Math.Log(riskTolerance) / Math.Log(chanceOfEachLoss);
                int roundedMaxStreak = int.Parse(Math.Round(calculatedMaxStreak - 0.49, 0).ToString());
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
                decimal bal = balance;
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
