using Microsoft.Playwright;

namespace Tfl.Api.Ui.Tests.Pages;

public abstract class BasePage
{
    protected readonly IPage Page;

    protected BasePage(IPage page)
    {
        Page = page;
    }

    public async Task AcceptCookiesIfPresentAsync()
    {
        try
        {
            var acceptBtn = Page.Locator("#cb-cookie-consent-accept-all, button:has-text('Accept all cookies')").First;
            if (await acceptBtn.CountAsync() > 0 && await acceptBtn.IsVisibleAsync())
            {
                await acceptBtn.ClickAsync();
            }
        }
        catch
        {
            // Banner not present or already accepted
        }
    }

    public async Task NavigateToAsync(string url)
    {
        await Page.GotoAsync(url, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await AcceptCookiesIfPresentAsync();
    }
}
