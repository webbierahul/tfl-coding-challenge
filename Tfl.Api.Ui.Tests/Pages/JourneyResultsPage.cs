using Microsoft.Playwright;
using Tfl.Api.Ui.Tests.Configuration;

namespace Tfl.Api.Ui.Tests.Pages;

public class JourneyResultsPage : BasePage
{
    private readonly ILocator _walkingJourneyBox;
    private readonly ILocator _cyclingJourneyBox;
    private readonly ILocator _walkingTimeLabel;
    private readonly ILocator _cyclingTimeLabel;
    private readonly ILocator _editPreferencesButton;
    private readonly ILocator _updateJourneyButton;
    private readonly ILocator _firstJourneyTime;
    private readonly ILocator _viewDetailsButton;
    private readonly ILocator _journeyBoxes;
    private readonly ILocator _fieldErrors;
    private readonly ILocator _disambiguationBox;

    public ILocator WalkingJourneyBox => _walkingJourneyBox;
    public ILocator CyclingJourneyBox => _cyclingJourneyBox;
    public ILocator WalkingTimeLabel => _walkingTimeLabel;
    public ILocator CyclingTimeLabel => _cyclingTimeLabel;
    public ILocator FirstJourneyTime => _firstJourneyTime;
    public ILocator JourneyBoxes => _journeyBoxes;
    public ILocator DisambiguationBox => _disambiguationBox;
    public ILocator FullWidthContent => Page.Locator("#full-width-content");

    public JourneyResultsPage(IPage page, TestSettings? settings = null) : base(page, settings)
    {
        _walkingJourneyBox = Page.Locator(".journey-box.walking");
        _cyclingJourneyBox = Page.Locator(".journey-box.cycling");
        _walkingTimeLabel = Page.Locator(".journey-box.walking .col2.journey-info, .journey-box.walking .col2");
        _cyclingTimeLabel = Page.Locator(".journey-box.cycling .col2.journey-info, .journey-box.cycling .col2");
        _editPreferencesButton = Page.GetByRole(AriaRole.Button, new() { Name = "Edit preferences" })
            .Or(Page.Locator(".edit-preferences button, button:has-text('Edit preferences')")).First;
        _updateJourneyButton = Page.Locator("#more-journey-options").GetByRole(AriaRole.Button, new() { Name = "Update journey" })
            .Or(Page.Locator("#more-journey-options input[value='Update journey'], input[value='Update journey']")).Last;
        _firstJourneyTime = Page.Locator(".journey-time, .time-boxes").First;
        _viewDetailsButton = Page.GetByRole(AriaRole.Button, new() { Name = "View details" })
            .Or(Page.Locator(".view-hide-details, button:has-text('View details')")).First;
        _journeyBoxes = Page.Locator(".journey-box, .journey-option");
        _fieldErrors = Page.Locator(".field-validation-error, .info-message, .field-validation-errors");
        _disambiguationBox = Page.Locator(".disambiguation-box");
    }

    public async Task WaitForResultsToLoadAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        var resultsOrDisambiguation = Page.Locator(".journey-box, .disambiguation-box").First;
        await resultsOrDisambiguation.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
    }

    public async Task<bool> IsWalkingOptionDisplayedAsync()
    {
        return await _walkingJourneyBox.IsVisibleAsync();
    }

    public async Task<string> GetWalkingTimeAsync()
    {
        await _walkingTimeLabel.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
        return (await _walkingTimeLabel.InnerTextAsync()).Trim();
    }

    public async Task<bool> IsCyclingOptionDisplayedAsync()
    {
        return await _cyclingJourneyBox.IsVisibleAsync();
    }

    public async Task<string> GetCyclingTimeAsync()
    {
        await _cyclingTimeLabel.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
        return (await _cyclingTimeLabel.InnerTextAsync()).Trim();
    }

    public async Task ClickEditPreferencesAsync()
    {
        await _editPreferencesButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
        await _editPreferencesButton.ClickAsync();
    }

    public async Task SelectRoutePreferenceAsync(string preference)
    {
        var preferenceLabel = Page.Locator("label", new() { HasText = preference });
        await preferenceLabel.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
        await preferenceLabel.ClickAsync();
    }

    public async Task<string> GetTravelPreferencesTextAsync()
    {
        var preferencesBox = Page.Locator(".travel-preferences, .summary-results, #full-width-content").First;
        await preferencesBox.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
        return (await preferencesBox.InnerTextAsync()).Trim();
    }

    public async Task ClickUpdateJourneyAsync(string? expectedPreferenceIndicator = "least walking")
    {
        await _updateJourneyButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
        await _updateJourneyButton.ClickAsync();

        // 1. If an active loader or "Fetching results" animation is attached, wait for it to clear
        var loader = Page.Locator(".loader, .waiting, .loading").Or(Page.GetByText("Fetching results")).First;
        if (await loader.CountAsync() > 0 && await loader.IsVisibleAsync())
        {
            await loader.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = DefaultTimeoutMs });
        }

        // 2. Condition-based wait for the observable travel preference indicator on the page
        if (!string.IsNullOrWhiteSpace(expectedPreferenceIndicator))
        {
            var preferenceIndicator = Page.Locator(".travel-preferences, .summary-results, #full-width-content")
                .Filter(new() { HasText = expectedPreferenceIndicator }).First;
            await preferenceIndicator.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
        }

        // 3. Wait for recalculated journey results to be rendered and visible
        var resultsOrDisambiguation = Page.Locator(".journey-box, .disambiguation-box, .journey-result-summary").First;
        await resultsOrDisambiguation.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
        await _firstJourneyTime.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
    }

    public async Task<string> GetUpdatedJourneyTimeAsync()
    {
        await _firstJourneyTime.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
        return (await _firstJourneyTime.InnerTextAsync()).Trim();
    }

    public async Task ClickViewDetailsAsync()
    {
        await _viewDetailsButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
        await _viewDetailsButton.ClickAsync();
        // Auto-wait for expanded details content
        await Page.Locator("#full-width-content").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
    }

    public async Task ClickStationAccessibilityDetailsAsync()
    {
        var accessibilityLink = Page.Locator("a:has-text('Accessibility details'), button:has-text('Accessibility details'), .accessibility-details").First;
        if (await accessibilityLink.CountAsync() > 0 && await accessibilityLink.IsVisibleAsync())
        {
            await accessibilityLink.ClickAsync();
        }
    }

    public async Task<string> GetStationStepAccessDetailsAsync(string stationName)
    {
        var stepContainer = Page.Locator(".step, .detailed-step, .journey-detail-step, #full-width-content")
            .Filter(new() { HasText = stationName }).First;
        await stepContainer.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs });
        return await stepContainer.InnerTextAsync();
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
