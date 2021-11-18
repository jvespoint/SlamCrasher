using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Configuration;

namespace Scripts
{
    public class BaseScript
    {
        public IWebDriver driver;
        public bool headless, demo;
        public string gameUrl, token, historyFile;
        public decimal tokenStart, tokenMinBet, startingBet, cashout, targetDefault, houseEdge, profitTarget;
        public int winsPerRun, tokenNormal, targetNormal, betInputPath, cashoutInputPath;
        public TimeSpan minRunTime;
        
        public void LoadConfigs()
        {
            headless = bool.Parse(ConfigurationManager.AppSettings["headless"]);
            gameUrl = ConfigurationManager.AppSettings["Slamcrash"];
            historyFile = ConfigurationManager.AppSettings["historyFile"];
            demo = bool.Parse(ConfigurationManager.AppSettings["demoMode"]);

            targetDefault = decimal.Parse(ConfigurationManager.AppSettings["targetDefault"]);
            targetNormal = Int32.Parse(ConfigurationManager.AppSettings["targetNormal"]);
            
            token = demo ? token = "slam" : token = ConfigurationManager.AppSettings["token"];
            betInputPath = demo ? 1 : 2;
            cashoutInputPath = demo ? 2 : 3;
            tokenStart = decimal.Parse(ConfigurationManager.AppSettings[token + "Start"]);
            tokenMinBet = decimal.Parse(ConfigurationManager.AppSettings[token + "Minbet"]);
            tokenNormal = Int32.Parse(ConfigurationManager.AppSettings[token + "Normal"]);
            startingBet = decimal.Parse(ConfigurationManager.AppSettings["startingBet"]);
            if (startingBet < tokenMinBet)
            {
                startingBet = tokenMinBet;
            }
            cashout = decimal.Parse(ConfigurationManager.AppSettings["cashout"]);
            houseEdge = decimal.Parse(ConfigurationManager.AppSettings["houseEdge"]);

            try
            {
                winsPerRun = Int32.Parse(ConfigurationManager.AppSettings["winsPerRun"]);
            }
            catch(Exception)
            {
                winsPerRun = (int)(200 / cashout);
            }
            try
            {
                minRunTime = TimeSpan.Parse(ConfigurationManager.AppSettings["minRunTime"]);
            }
            catch(Exception)
            {
                minRunTime = TimeSpan.Parse("01:00:00");
            }
            try
            {
                profitTarget = decimal.Parse(ConfigurationManager.AppSettings["profitTarget"]);
            }
            catch(Exception)
            {
                profitTarget = startingBet * 100;
            }

            
        }

        public virtual void NewBrowserSetup()
        {
            ChromeOptions options = new ChromeOptions();
            if ( headless ) { options.AddArgument("--headless"); }
            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
        }

        public void TearDown() => driver.Quit();
    }
}