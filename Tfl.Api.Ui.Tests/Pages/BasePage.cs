using Microsoft.Playwright;
using Tfl.Api.Ui.Tests.Pages.Components;

namespace Tfl.Api.Ui.Tests.Pages;

public abstract class BasePage
{
    protected readonly IPage Page;
    public CookieBannerComponent CookieBanner { get; }

    protected BasePage(IPage page)
    {
        Page = page;
        CookieBanner = new CookieBannerComponent(page);
    }

    public async Task AcceptCookiesIfPresentAsync()
    {
        await CookieBanner.AcceptIfPresentAsync();
    }

    public async Task HandleCloudflareIfPresentAsync()
    {
        try
        {
            var title = await Page.TitleAsync();
            if (title.Contains("Verification", StringComparison.OrdinalIgnoreCase) ||
                title.Contains("Just a moment", StringComparison.OrdinalIgnoreCase))
            {
                var checkbox = Page.FrameLocator("iframe[src*='challenges.cloudflare.com']")
                    .Locator("input[type='checkbox'], .ctp-checkbox-label, #challenge-stage")
                    .First;

                await checkbox.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
                await checkbox.ClickAsync();
                await Page.WaitForTimeoutAsync(2500);
            }
        }
        catch
        {
            // Best effort - do not fail if challenge elements are absent
        }
    }

    public async Task NavigateToAsync(string url)
    {
        await Page.GotoAsync(url, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await HandleCloudflareIfPresentAsync();
        await AcceptCookiesIfPresentAsync();
    }
}
