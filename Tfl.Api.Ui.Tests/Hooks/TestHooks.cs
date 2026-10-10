using Microsoft.Playwright;
using Reqnroll;
using Reqnroll.BoDi;
using Tfl.Api.Ui.Tests.Configuration;
using Tfl.Api.Ui.Tests.Drivers;

namespace Tfl.Api.Ui.Tests.Hooks;

[Binding]
public class TestHooks
{
    private readonly IObjectContainer _container;
    private readonly ScenarioContext _scenarioContext;
    private static PlaywrightDriver? _driver;
    private static TestSettings? _settings;
    private IBrowserContext? _browserContext;
    private IPage? _page;

    public TestHooks(IObjectContainer container, ScenarioContext scenarioContext)
    {
        _container = container;
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        _settings = TestSettings.Load();
        _driver = new PlaywrightDriver();
    }

    [BeforeScenario]
    public async Task BeforeScenario()
    {
        var browser = await _driver!.GetBrowserAsync(_settings!);
        _browserContext = await _driver.CreateContextAsync(browser, _settings!);
        _page = await _browserContext.NewPageAsync();

        _container.RegisterInstanceAs(_settings!);
        _container.RegisterInstanceAs(_page);
        _container.RegisterInstanceAs(_browserContext);
    }

    [AfterScenario]
    public async Task AfterScenario()
    {
        if (_scenarioContext.TestError != null && _page != null)
        {
            try
            {
                var screenshotsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                Directory.CreateDirectory(screenshotsDir);

                var scenarioTitle = string.Join("_", _scenarioContext.ScenarioInfo.Title.Split(Path.GetInvalidFileNameChars()));
                var screenshotPath = Path.Combine(screenshotsDir, $"{scenarioTitle}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");

                await _page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = screenshotPath,
                    FullPage = true
                });

                TestContext.AddTestAttachment(screenshotPath, "Failure Screenshot");
                TestContext.Out.WriteLine($"Captured failure screenshot: {screenshotPath}");
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"Could not take failure screenshot: {ex.Message}");
            }
        }

        if (_page != null)
            await _page.CloseAsync();

        if (_browserContext != null)
            await _browserContext.DisposeAsync();
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (_driver != null)
            await _driver.DisposeAsync();
    }
}
