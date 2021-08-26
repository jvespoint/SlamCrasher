using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace Scripts
{
    public class BaseScript
    {
        public IWebDriver driver;
        public bool headless;
        public string gameUrl;
        public string strategy;
        public bool demo;
        public string token;
        public decimal tokenStart;
        public decimal tokenMinBet;
        public decimal tokenNormal;
        public decimal winsPerRun;
        public decimal cashout;
        public decimal targetDefault;
        public decimal targetNormal;
        public decimal startingBet;

        private void LoadConfigs()
        {

            headless = bool.Parse(ConfigurationManager.AppSettings["headless"]);
            demo = bool.Parse(ConfigurationManager.AppSettings["demoMode"]);
            token = demo ? token = "slam" : token = ConfigurationManager.AppSettings["token"];
            gameUrl = ConfigurationManager.AppSettings["Slamcrash"];
            strategy = ConfigurationManager.AppSettings["strategy"];

            winsPerRun = Convert.ToInt32(ConfigurationManager.AppSettings["winsPerRun"]);
            decimal.TryParse(ConfigurationManager.AppSettings["startingBet"], out startingBet);
            cashout = decimal.Parse(ConfigurationManager.AppSettings["cashout"]);

            tokenStart = decimal.Parse(ConfigurationManager.AppSettings[token + "Start"]);
            decimal.TryParse(ConfigurationManager.AppSettings[token + "Minbet"], out tokenMinBet);
            decimal.TryParse(ConfigurationManager.AppSettings[token + "Normal"], out tokenNormal);
            decimal.TryParse(ConfigurationManager.AppSettings["targetDefault"], out targetDefault);
            decimal.TryParse(ConfigurationManager.AppSettings["targetNormal"], out targetNormal);
            
            
            if (demo)
            {
                decimal.TryParse(ConfigurationManager.AppSettings["slamMinbet"], out tokenMinBet);
                decimal.TryParse(ConfigurationManager.AppSettings["slamNormal"], out tokenNormal);
                decimal.TryParse(ConfigurationManager.AppSettings["slamStart"], out tokenStart);
                if (startingBet < tokenMinBet)
                {
                    startingBet = tokenMinBet;
                }
            }
            decimal.TryParse(ConfigurationManager.AppSettings[token + "Minbet"], out tokenMinBet);
        }

        public virtual void NewBrowserSetup()
        {
            LoadConfigs();
            ChromeOptions options = new ChromeOptions();
            if ( headless ) { options.AddArgument("--headless"); }
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
        }
        public void TearDown() => driver.Quit();
        
        //public List<string> ReadFileLines(string Filename)
        //{
        //    using var reader = new StreamReader(Filename);
        //    List<string> FileLines = new List<string>();
        //    while ( !reader.EndOfStream )
        //    {
        //        var line = reader.ReadLine().Replace("\0", "").Replace("\t", "");
        //        FileLines.Add(line);
        //    }
        //    reader.Close();
        //    File.Delete(Filename);
        //    return FileLines;
        //}
    }
}