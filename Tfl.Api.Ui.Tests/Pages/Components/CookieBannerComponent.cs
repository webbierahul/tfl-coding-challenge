using Microsoft.Playwright;

namespace Tfl.Api.Ui.Tests.Pages.Components;

public class CookieBannerComponent
{
    private readonly IPage _page;
    private readonly ILocator _acceptButton;

    public CookieBannerComponent(IPage page)
    {
        _page = page;
        _acceptButton = page.GetByRole(AriaRole.Button, new() { Name = "Accept all cookies" })
            .Or(page.Locator("#cb-cookie-consent-accept-all")).First;
    }

    public async Task AcceptIfPresentAsync()
    {
        try
        {
            await _acceptButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            await _acceptButton.ClickAsync();
        }
        catch
        {
            // Banner not present or already accepted
        }

        try
        {
            await _page.EvaluateAsync("() => document.getElementById('cb-cookieoverlay')?.remove()");
        }
        catch
        {
        }
    }
}
