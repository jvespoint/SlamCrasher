using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pages
{
    public class SlamCrash : BasePage
    {

        public SlamCrash(IWebDriver driver) : base(driver)
        {

        }

        //Locators
        private readonly By connectingToServerMessage = By.XPath("//p[text()='Connecting to server..']");
        private readonly By loginButtonLocator = By.XPath("//button[text()='Login To Play' and @data-lang-token='LoginToPlay']");
        private readonly By demoLoginButtonLocator = By.XPath("//button[text()='Free Demo' and @data-lang-token='SignUpToPlay']");
        private readonly By metamaskAlertShownLocator = By.XPath("//div[@class='dialog-text' and text()='Meta Mask / Trust Wallet is not installed!']");
        private readonly By alertDismissButtonLocator = By.XPath("//span[contains(@class,'dialog-button') and text()='OK']");
        private readonly By web3WalletsMenuShownLocator = By.XPath("//div[text()='Web3 Wallets']");
        private readonly By walletConnectButtonLocator = By.XPath("//div[@class='actions-button-text' and text()='Use Wallet Connect']");
        private readonly By walletConnectQRShownLocator = By.XPath("//p[text()='Scan QR code with a WalletConnect-compatible wallet']");
        public readonly By betButtonLocator = By.XPath("//a[text()='Place Bet!']");
        private readonly By cashierButtonLocator = By.XPath("//span[text()='CASHIER']");
        private readonly By walletBalanceLocator = By.XPath("//span[@class='balance' and @id='balance_bits']");
        private readonly By accountLocator = By.XPath("//span[@id='account-username']");
        private readonly string prevSixGamesLocator = "//div[@class='prev_games']/div";
        private readonly By rangeSliderLocator = By.XPath("//div[contains(@class,'range-slider')]");
        private readonly string plusButtonLocator = "(//div[@class='stepper-button-plus'])";
        private readonly string minusButtonLocator = "(//div[@class='stepper-button-minus'])";
        private readonly By sendingBetLocator = By.XPath("//span[text()='Sending Bet... ']");
        public readonly By autoCashoutLocator = By.XPath("(//input)[2]");
        private readonly By cashoutButtonLocator = By.XPath("//span[contains(text(),'Cash Out: ')]");
        private readonly By lossIndicatorLocator = By.XPath("//span[contains(text(),'You lost')]");
        private readonly By winIndicatorLocator = By.XPath("//span[text()='Cashed Out @  ']");
        private readonly By insufficientFundsLocator = By.XPath("//div[text()='Insufficient funds to play.']");
        private readonly string cashierPopupLoadedIndicatorLocator = "//button[contains(@onclick,'bnb')]";

        //Indicators
        public bool ConnectingToServer => ElementExists(connectingToServerMessage);
        public bool ReadyForLogin => ElementExists(loginButtonLocator);
        public bool MetamaskAlert => ElementExists(metamaskAlertShownLocator);
        public bool Web3WalletsMenuShown => ElementExists(web3WalletsMenuShownLocator);
        public bool WalletConnectQRShown => ElementExists(walletConnectQRShownLocator);
        public bool CashierPopupLoadedIndicator => ElementExists(By.XPath(cashierPopupLoadedIndicatorLocator));
        public bool BetButtonShown => ElementExists(betButtonLocator);
        public bool HistoryButtonShown => ElementExists(historyButtonLocator);
        public bool CashierButtonShown => ElementExists(cashierButtonLocator);
        public bool WalletBalanceShown => ElementExists(walletBalanceLocator);
        public bool AccountShown => ElementExists(accountLocator);
        public bool PrevSixGamesShown => ElementExists(By.XPath(prevSixGamesLocator));
        public bool AllButtonsReady => BetButtonShown && HistoryButtonShown && CashierButtonShown && WalletBalanceShown && AccountShown && PrevSixGamesShown;
        public bool SendingBet => ElementExists(sendingBetLocator);
        public bool Rolling => ElementExists(cashoutButtonLocator);
        public bool BetPlaced => SendingBet || Rolling;
        public bool WinIndicator => ElementExists(winIndicatorLocator);
        public bool LossIndicator => ElementExists(lossIndicatorLocator);
        public bool InsufficientFundsIndicator => ElementExists(insufficientFundsLocator);
        public void ClearServerConnect(string fromWhere)
        {
            int seconds = 0;
            CustomTimeout(2000); //wait here for pre-load to show indicator
            while (ConnectingToServer)
            {
                if (seconds > 3)
                {
                    RefreshPage();
                    seconds = 0;
                }
                CustomTimeout(1000);
                seconds++;
            }
            Console.WriteLine("Connected: " + fromWhere);
        }
        public void Login(bool demoMode, string token)
        {
            if (demoMode)
            {
                Click(demoLoginButtonLocator);
                Console.WriteLine("Login: Demo Mode");
            }
            else
            {
                Click(loginButtonLocator);
                wait.Until(alertShown => MetamaskAlert);
                Click(alertDismissButtonLocator);
                wait.Until(menuShown => Web3WalletsMenuShown);
                Click(walletConnectButtonLocator); //Opens a new tab: Wallectconnect QR code
                var tabs = _driver.WindowHandles;
                _driver.SwitchTo().Window(tabs[0]); //We need to close the old tab.
                _driver.Close();
                _driver.SwitchTo().Window(tabs[1]); //switch back to the QR code
                int seconds = 0;
                while (WalletConnectQRShown) //Still waiting on user to scan the QR
                {
                    if (seconds > 180) //if its longer than 3 minutes, something has gone wrong.
                    {
                        Console.WriteLine("Wallet connect took too long.");
                        _driver.Quit();
                    }
                    CustomTimeout(1000);
                    seconds++;
                }
                Console.WriteLine("Login: Live Mode");
            }
            ClearServerConnect("After Login");
            wait.Until(cashier => CashierButtonShown);
            CustomTimeout(2000);
            if (InsufficientFundsIndicator)
            {
                Click(cashierButtonLocator);
                wait.Until(cashierLoaded => CashierPopupLoadedIndicator);
                Click(By.XPath(cashierPopupLoadedIndicatorLocator.Replace("bnb", token)));
            }
            wait.Until(ready => AllButtonsReady);
        }
        public decimal GetBalance(string token)
        {
            string balanceString = Find(walletBalanceLocator).Text;
            if (token == "slam")
            {
                balanceString = balanceString.Replace("M SLAM", "");
            }
            else if (token == "bnb")
            {
                balanceString = balanceString.Replace("BNB", "");
            }
            return decimal.Parse(balanceString);
        }
        public decimal GetBet(int index)
        {
            decimal toReturn;
            string betString = Find(By.XPath($"(//input)[{index}]")).GetAttribute("value").ToString().Replace("SLAM", "").Replace("BNB", "").Replace("M", "").Trim();
            Decimal.TryParse(betString, out toReturn);
            return toReturn;
        }
        public void InitializeTarget()
        {
            IncrementButtons(1, false);
            IncrementButtons(-1, false);
            Console.WriteLine("Target Initialized: 2.00x");
        }
        public void IncrementButtons(int times, bool bet)
        {
            string locator = times > 0 ? plusButtonLocator : minusButtonLocator;
            locator += bet ? "[1]" : "[2]";
            for (int i = Math.Abs(times); i > 0; i--)
            {
                Click(By.XPath(locator));
            }
        }
        public void SetBetToMin()
        {
            IWebElement range = Find(rangeSliderLocator);
            int width = range.Size.Width;
            Actions act = new Actions(_driver);
            act.ClickAndHold(range).MoveByOffset(0, 0).Perform();
            CustomTimeout(10);
            act.MoveByOffset(-width, 0).Release().Perform();
        }
        public void SetBetToMax()
        {
            IWebElement range = Find(rangeSliderLocator);
            int width = range.Size.Width;
            Actions act = new Actions(_driver);
            act.ClickAndHold(range).MoveByOffset(0,0).Perform();
            CustomTimeout(10);
            act.MoveByOffset(width , 0).Release().Perform();
        }
        public void SetBetTo25()
        {
            IWebElement range = Find(rangeSliderLocator);
            int width = range.Size.Width;
            Actions act = new Actions(_driver);
            act.ClickAndHold(range).MoveByOffset(0,0).Perform();
            CustomTimeout(10);
            act.MoveByOffset(-width / 4 , 0).Release().Perform();
        }
        public void SetBetTo75()
        {
            IWebElement range = Find(rangeSliderLocator);
            int width = range.Size.Width;
            Actions act = new Actions(_driver);
            act.ClickAndHold(range).MoveByOffset(0,0).Perform();
            CustomTimeout(10);
            act.MoveByOffset(width / 4 , 0).Release().Perform();
        }
        public void SetBetTo50()
        {
            Actions act = new Actions(_driver);
            act.ClickAndHold(Find(rangeSliderLocator)).MoveByOffset(0,0).Perform();
            CustomTimeout(10);
            act.MoveByOffset(0, 0).Release().Perform();
        }
        public void SetBetCloseEnough(decimal bet, string token)
        {
            IWebElement range = Find(rangeSliderLocator);
            int widthHalf = range.Size.Width / 2;
            decimal halfBalance = GetBalance(token) / 2;
            Actions act = new Actions(_driver);
            act.ClickAndHold(range).MoveByOffset(0, 0).Perform();
            CustomTimeout(10);
            if (bet < halfBalance)
            {
                act.MoveByOffset(-Convert.ToInt32(widthHalf - (widthHalf * bet / halfBalance)), 0).Release().Perform();
            }
            else
            {
                act.MoveByOffset(Convert.ToInt32(widthHalf * (bet - halfBalance) / halfBalance), 0).Release().Perform();
            }
        }
        public bool CheckForWin()
        {
            while (!LossIndicator && !WinIndicator)
            {
                if (!BetPlaced)
                {
                    if(ElementExists(betButtonLocator))
                    {
                        Click(betButtonLocator);
                    }
                }
                CustomTimeout(200);
            }
            return WinIndicator;
        }
    }
}