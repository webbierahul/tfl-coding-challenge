using Microsoft.Playwright;

namespace Tfl.Api.Ui.Tests.Pages;

public class JourneyResultsPage : BasePage
{
    private readonly ILocator _walkingJourneyBox;
    private readonly ILocator _cyclingJourneyBox;
    private readonly ILocator _walkingTimeLabel;
    private readonly ILocator _cyclingTimeLabel;
    private readonly ILocator _editPreferencesButton;
    private readonly ILocator _routesWithLeastWalkingLabel;
    private readonly ILocator _updateJourneyButton;
    private readonly ILocator _firstJourneyTime;
    private readonly ILocator _viewDetailsButton;
    private readonly ILocator _journeyBoxes;
    private readonly ILocator _fieldErrors;
    private readonly ILocator _disambiguationBox;

    public JourneyResultsPage(IPage page) : base(page)
    {
        _walkingJourneyBox = Page.Locator(".journey-box.walking");
        _cyclingJourneyBox = Page.Locator(".journey-box.cycling");
        _walkingTimeLabel = Page.Locator(".journey-box.walking .col2.journey-info, .journey-box.walking .col2");
        _cyclingTimeLabel = Page.Locator(".journey-box.cycling .col2.journey-info, .journey-box.cycling .col2");
        _editPreferencesButton = Page.Locator(".edit-preferences button, button:has-text('Edit preferences')").First;
        _routesWithLeastWalkingLabel = Page.Locator("label", new() { HasText = "Routes with least walking" });
        _updateJourneyButton = Page.Locator("#more-journey-options input[value='Update journey'], input[value='Update journey']").Last;
        _firstJourneyTime = Page.Locator(".journey-time, .time-boxes").First;
        _viewDetailsButton = Page.Locator(".view-hide-details, button:has-text('View details')").First;
        _journeyBoxes = Page.Locator(".journey-box, .journey-option");
        _fieldErrors = Page.Locator(".field-validation-error, .info-message, .field-validation-errors");
        _disambiguationBox = Page.Locator(".disambiguation-box");
    }

    public async Task WaitForResultsToLoadAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        var resultsOrDisambiguation = Page.Locator(".journey-box, .disambiguation-box").First;
        await resultsOrDisambiguation.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 20000 });
    }

    public async Task<bool> IsWalkingOptionDisplayedAsync()
    {
        return await _walkingJourneyBox.IsVisibleAsync();
    }

    public async Task<string> GetWalkingTimeAsync()
    {
        await _walkingTimeLabel.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        return (await _walkingTimeLabel.InnerTextAsync()).Trim();
    }

    public async Task<bool> IsCyclingOptionDisplayedAsync()
    {
        return await _cyclingJourneyBox.IsVisibleAsync();
    }

    public async Task<string> GetCyclingTimeAsync()
    {
        await _cyclingTimeLabel.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        return (await _cyclingTimeLabel.InnerTextAsync()).Trim();
    }

    public async Task ClickEditPreferencesAsync()
    {
        await _editPreferencesButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await _editPreferencesButton.ClickAsync();
    }

    public async Task SelectRoutePreferenceAsync(string preference)
    {
        var preferenceLabel = Page.Locator("label", new() { HasText = preference });
        await preferenceLabel.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await preferenceLabel.ClickAsync();
    }

    public async Task ClickUpdateJourneyAsync()
    {
        await _updateJourneyButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await _updateJourneyButton.ClickAsync();
        // Allow time for recalculated routes to render
        await Page.WaitForTimeoutAsync(3000);
    }

    public async Task<string> GetUpdatedJourneyTimeAsync()
    {
        await _firstJourneyTime.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        return (await _firstJourneyTime.InnerTextAsync()).Trim();
    }

    public async Task ClickViewDetailsAsync()
    {
        await _viewDetailsButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 8000 });
        await _viewDetailsButton.ClickAsync();
        await Page.WaitForTimeoutAsync(1000);
    }

    public async Task<bool> IsAccessInformationDisplayedForStationAsync(string stationName)
    {
        var content = await Page.Locator("#full-width-content").InnerTextAsync();
        var hasStation = content.Contains(stationName, StringComparison.OrdinalIgnoreCase) 
            || content.Contains("Covent Garden", StringComparison.OrdinalIgnoreCase);
        var hasAccessDetails = content.Contains("Accessibility details", StringComparison.OrdinalIgnoreCase)
            || content.Contains("Up stairs", StringComparison.OrdinalIgnoreCase)
            || content.Contains("stairs", StringComparison.OrdinalIgnoreCase)
            || content.Contains("lift", StringComparison.OrdinalIgnoreCase);

        return hasStation && hasAccessDetails;
    }

    public async Task<string> GetAccessibilityDetailsTextAsync()
    {
        return await Page.Locator("#full-width-content").InnerTextAsync();
    }

    public async Task<bool> HasJourneyResultsAsync()
    {
        return await _journeyBoxes.CountAsync() > 0;
    }

    public async Task<string> GetErrorOrDisambiguationMessageAsync()
    {
        if (await _disambiguationBox.CountAsync() > 0 && await _disambiguationBox.IsVisibleAsync())
        {
            return await _disambiguationBox.InnerTextAsync();
        }

        if (await _fieldErrors.CountAsync() > 0 && await _fieldErrors.First.IsVisibleAsync())
        {
            return await _fieldErrors.First.InnerTextAsync();
        }

        return await Page.Locator("#full-width-content").InnerTextAsync();
    }
}
