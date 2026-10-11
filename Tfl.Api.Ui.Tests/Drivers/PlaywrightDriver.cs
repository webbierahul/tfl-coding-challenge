using Microsoft.Playwright;
using Tfl.Api.Ui.Tests.Configuration;

namespace Tfl.Api.Ui.Tests.Drivers;

public class PlaywrightDriver : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public async Task<IBrowser> GetBrowserAsync(TestSettings settings)
    {
        if (_browser != null)
            return _browser;

        _playwright = await Playwright.CreateAsync();

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = settings.Headless,
            SlowMo = settings.SlowMoMs,
            Args = new[]
            {
                "--disable-blink-features=AutomationControlled",
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-infobars",
                "--window-size=1280,800"
            }
        };

        if (string.Equals(settings.Browser, "chrome", StringComparison.OrdinalIgnoreCase))
        {
            launchOptions.Channel = "chrome";
        }

        _browser = await _playwright.Chromium.LaunchAsync(launchOptions);
        return _browser;
    }

    public async Task<IBrowserContext> CreateContextAsync(IBrowser browser, TestSettings settings)
    {
        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 },
            Locale = "en-GB",
            TimezoneId = "Europe/London",
            UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36"
        };

        if (settings.RecordVideo)
        {
            var videosDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Videos");
            Directory.CreateDirectory(videosDir);
            contextOptions.RecordVideoDir = videosDir;
            contextOptions.RecordVideoSize = new RecordVideoSize { Width = 1280, Height = 800 };
        }

        var context = await browser.NewContextAsync(contextOptions);

        // Mask automation flags for Cloudflare bot verification
        await context.AddInitScriptAsync(@"
            Object.defineProperty(navigator, 'webdriver', {
                get: () => undefined
            });
            window.chrome = {
                runtime: {}
            };
            Object.defineProperty(navigator, 'plugins', {
                get: () => [1, 2, 3, 4, 5]
            });
            Object.defineProperty(navigator, 'languages', {
                get: () => ['en-GB', 'en']
            });
        ");

        context.SetDefaultTimeout(settings.DefaultTimeoutMs);
        return context;
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.DisposeAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;
    }
}
