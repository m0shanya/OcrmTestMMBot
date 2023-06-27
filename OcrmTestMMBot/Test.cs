using OcrmTestMMBot.Controllers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace OcrmTestMMBot;

public class Test
{
    private IWebDriver _driver { get; set; }
    private WebDriverWait _wait { get; set; }
    private static Sender? _sender { get; set; }
    private static Test? _instance { get; set; }
    public static bool isRunning { get; private set; }
    private static ILogger<MmBotTestController> _logger { get; set; }

    private Test()
    {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit!);
        isRunning = false;
    }

    public static Test Instance(ILogger<MmBotTestController> logger, string chatID)
    {
        if (isRunning)
        {
            return _instance!;
        }

        _logger = logger;

        if (_instance == null)
        {
            _instance = new Test();
        }

        _sender = new Sender(_logger, chatID);
        return _instance;
    }

    private void CreateDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--ignore-certificate-errors");
        var remoteWebDriverUrl = @"http://10.128.217.206:4444/wd/hub";
        _driver = new RemoteWebDriver(new Uri(remoteWebDriverUrl), options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
    }

    private byte[] TakeScreenshot(string address, IWebDriver _driver)
    {
        Screenshot ss = ((ITakesScreenshot)_driver).GetScreenshot();
        // var path = $"screenshots/{address.Split("/")[2]}.png";
        // ss.SaveAsFile(path, ScreenshotImageFormat.Png);
        var array = ss.AsByteArray;
        return ss.AsByteArray;
    }

    private async Task CheckLogin(string ip)
    {
        var output = $"{ip.Split("/")[2]}: ";
        try
        {
            Func<IWebDriver, bool> first = new Func<IWebDriver, bool>((IWebDriver d) =>
            {
                return d.FindElements(By.CssSelector("#t-comp0-wrap")).Count > 0;
            });

            Func<IWebDriver, bool> second = new Func<IWebDriver, bool>((IWebDriver d) =>
            {
                return d.FindElements(By.CssSelector("#header-right-image-container")).Count > 0;
            });

            Func<IWebDriver, bool> condition = new Func<IWebDriver, bool>((IWebDriver d) =>
            {
                return first(d) || second(d);
            });

            _wait.Until(condition);

            if (first(_driver))
            {
                output += "Invalid login parameters";
            }
            else if (second(_driver))
            {
                output += "Success";
            }
        }
        catch (Exception e)
        {
            output += e.Message;
        }
        finally
        {
            await _sender!.SendMessage(output, TakeScreenshot(ip, _driver));
        }
    }

    private List<string> GetAddresses(string url)
    {
        using var client = new HttpClient();
        var task = Task.Run(async () => await client.GetStringAsync(url));
        List<string> addresses = task.Result.Split("\n").ToList();

        return addresses;
    }

    public async Task TestAddresses(string source)
    {
        isRunning = true;

        CreateDriver();
        var addresses = GetAddresses(source);
        foreach (var address in addresses)
        {
            try
            {
                await Login(address);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"[{DateTime.Now}] Message sent exception\n{e.Message}");
                break;
            }
            catch (OpenQA.Selenium.WebDriverException e)
            {
                _logger.LogError($"[{DateTime.Now}] WebDriverException exception\n{e.Message}");
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        _driver.Dispose();
        isRunning = false;
    }

    public async Task Login(string address)
    {
        _driver.Navigate().GoToUrl(address);
        _wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector("#loginContainer")));

        IWebElement login = _driver.FindElement(By.CssSelector("#loginEdit-el"));
        IWebElement password = _driver.FindElement(By.CssSelector("#passwordEdit-el"));
        login.Clear();
        login.SendKeys("webtester");
        password.Clear();
        password.SendKeys("webtester");
        _driver.FindElement(By.CssSelector("#t-comp14-textEl, #t-comp16-textEl, #t-comp4-textEl")).Click();

        await CheckLogin(address);
    }

    private void OnProcessExit(object sender, EventArgs e)
    {
        if (_driver != null)
        {
            _driver.Quit();
        }
    }
}