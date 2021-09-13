using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Configuration;

namespace Scripts
{
    public class BaseScript
    {
        public IWebDriver driver;
        public bool headless;
        public string gameUrl;
        public bool demo;
        public string token;
        public decimal tokenStart, tokenMinBet, startingBet, cashout, targetDefault;
        public int winsPerRun, tokenNormal, targetNormal;
        
        private void LoadConfigs()
        {
            headless = bool.Parse(ConfigurationManager.AppSettings["headless"]);
            gameUrl = ConfigurationManager.AppSettings["Slamcrash"];
            demo = bool.Parse(ConfigurationManager.AppSettings["demoMode"]);

            targetDefault = decimal.Parse(ConfigurationManager.AppSettings["targetDefault"]);
            targetNormal = Int32.Parse(ConfigurationManager.AppSettings["targetNormal"]);
            
            token = demo ? token = "slam" : token = ConfigurationManager.AppSettings["token"];
            tokenStart = decimal.Parse(ConfigurationManager.AppSettings[token + "Start"]);
            tokenMinBet = decimal.Parse(ConfigurationManager.AppSettings[token + "Minbet"]);
            tokenNormal = Int32.Parse(ConfigurationManager.AppSettings[token + "Normal"]);
            
            winsPerRun = Convert.ToInt32(ConfigurationManager.AppSettings["winsPerRun"]);
            startingBet = decimal.Parse(ConfigurationManager.AppSettings["startingBet"]);
            cashout = decimal.Parse(ConfigurationManager.AppSettings["cashout"]);
            
            if (startingBet < tokenMinBet)
            {
                startingBet = tokenMinBet;
            }
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
    }
}