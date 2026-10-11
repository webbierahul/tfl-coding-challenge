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

    public async Task NavigateToAsync(string url)
    {
        await Page.GotoAsync(url, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await AcceptCookiesIfPresentAsync();
    }
}
