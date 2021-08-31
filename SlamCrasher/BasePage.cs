using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;


namespace Pages
{
    public class BasePage
    {
        protected IWebDriver _driver;
        public WebDriverWait wait;

        public BasePage(IWebDriver driver)
        {
            _driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
        }
        public string GetTitle => _driver.Title;
        public IWebDriver GetDriver => _driver;
        public IWebElement Find(By locator)
        {
            wait.Until(drv => drv.FindElement(locator));
            IWebElement Element = null;
            for(int i = 3; i > 0; i++)
            {
                try
                {
                    Element = _driver.FindElement(locator);
                    wait.Until(ready => Element.Displayed && Element.Enabled);
                    break;
                }
                catch (StaleElementReferenceException)
                {
                    
                }
            }
            return Element;
        }
        public bool ElementExists(By Locator)
        {
            try
            {
                IWebElement Element = GetDriver.FindElement(Locator);
            }
            catch ( NoSuchElementException )
            {
                return false;
            }
            return true;
        }
        public void CustomTimeout(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }
        public void Goto(string url)
        {
            _driver.Url = url;
            CustomTimeout(1000);
        }
        public void RefreshPage()
        {
            _driver.Navigate().Refresh();
            CustomTimeout(1000);
        }
        public void Type(string toSend, By Locator) => Find(Locator).SendKeys(toSend);
        public void SetText(string Text, By Locator)
        {
            Find(Locator).Clear();
            Type(Text, Locator);
        }
        public void Click(By locator)
        {
            try
            {
                Find(locator).Click();
            }
            catch ( ElementClickInterceptedException )
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", Find(locator));
            }
            catch ( StaleElementReferenceException )
            {
                System.Threading.Thread.Sleep(500);
                Click(locator);
            }
        }
        
        public bool GetCheckbox(By CheckBoxLocator)
        {
            bool Checkbox = false;
            try
            {
                Checkbox = bool.Parse(_driver.FindElement(CheckBoxLocator).GetAttribute("checked"));
            }
            catch ( NullReferenceException ) { }
            catch ( ArgumentNullException ) { }
            return Checkbox;
        }
        public void SetCheckbox(By CheckboxLocator, bool Checked)
        {
            if ( GetCheckbox(CheckboxLocator) != Checked )
            {
                Click(CheckboxLocator);
            }
        }
        public List<string> GetAllSelected(By Locator)
        {
            List<string> selectedValues = new List<string>();
            foreach ( IWebElement Value in Find(Locator).FindElements(By.XPath("./option[@selected]")) )
            {
                selectedValues.Add(Value.Text);
            }
            return selectedValues;
        }
        public string GetFirstSelected(By Locator, bool JustSet)
        {
            IWebElement SelectElement = Find(Locator);
            if ( JustSet == false )
            {
                return SelectElement.FindElements(By.XPath("./option[@selected='selected']"))[0].Text;
            }
            else
            {
                foreach ( IWebElement Option in SelectElement.FindElements(By.XPath("./option")) )
                {
                    if ( Option.Selected == true )
                    {
                        return Option.Text;
                    }
                }
                return null;
            }
        }
        public void SimulateHover(IWebElement Element)
        {
            Actions action = new Actions(this._driver);
            action.MoveToElement(Element);
            action.Build().Perform();
            System.Threading.Thread.Sleep(100);
        }
    }
}