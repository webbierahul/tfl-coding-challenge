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
            var acceptBtn = Page.GetByRole(AriaRole.Button, new() { Name = "Accept all cookies" })
                .Or(Page.Locator("#cb-cookie-consent-accept-all")).First;

            await acceptBtn.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            await acceptBtn.ClickAsync();
        }
        catch
        {
            // Banner not present or already accepted
        }

        try
        {
            await Page.EvaluateAsync(@"() => {
                const overlay = document.getElementById('cb-cookieoverlay');
                if (overlay) {
                    overlay.remove();
                }
            }");
        }
        catch
        {
        }
    }

    public async Task NavigateToAsync(string url)
    {
        await Page.GotoAsync(url, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await AcceptCookiesIfPresentAsync();
    }
}
