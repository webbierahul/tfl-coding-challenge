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

    public async Task CheckForEnvironmentalBlockersAsync()
    {
        var title = await Page.TitleAsync();
        if (title.Contains("Verification", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("Just a moment", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Access blocked: TfL endpoint presented a Cloudflare Bot / Turnstile verification challenge. " +
                "Automated execution against the live public edge requires execution from a trusted IP network, " +
                "an authorized WAF bypass token, or execution in an isolated staging environment.");
        }
    }

    public async Task NavigateToAsync(string url)
    {
        await Page.GotoAsync(url, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await CheckForEnvironmentalBlockersAsync();
        await AcceptCookiesIfPresentAsync();
    }
}
