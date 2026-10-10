using Microsoft.Playwright;

namespace Tfl.Api.Ui.Tests.Pages;

public class JourneyPlannerPage : BasePage
{
    private readonly ILocator _fromInput;
    private readonly ILocator _toInput;
    private readonly ILocator _planJourneyButton;
    private readonly ILocator _fromValidationError;
    private readonly ILocator _toValidationError;
    private readonly ILocator _suggestions;

    public JourneyPlannerPage(IPage page) : base(page)
    {
        _fromInput = Page.Locator("#InputFrom");
        _toInput = Page.Locator("#InputTo");
        _planJourneyButton = Page.Locator("#plan-journey-button");
        _fromValidationError = Page.Locator("#InputFrom-error, span[data-valmsg-for='InputFrom']");
        _toValidationError = Page.Locator("#InputTo-error, span[data-valmsg-for='InputTo']");
        _suggestions = Page.Locator(".tt-suggestion, .stop-name");
    }

    public async Task EnterFromLocationAndSelectSuggestionAsync(string partialText, string suggestionToSelect)
    {
        await _fromInput.ClickAsync();
        await _fromInput.FillAsync(partialText);
        await SelectSuggestionAsync(suggestionToSelect);
    }

    public async Task EnterToLocationAndSelectSuggestionAsync(string partialText, string suggestionToSelect)
    {
        await _toInput.ClickAsync();
        await _toInput.FillAsync(partialText);
        await SelectSuggestionAsync(suggestionToSelect);
    }

    public async Task EnterFromLocationRawAsync(string location)
    {
        await _fromInput.ClickAsync();
        await _fromInput.FillAsync(location);
    }

    public async Task EnterToLocationRawAsync(string location)
    {
        await _toInput.ClickAsync();
        await _toInput.FillAsync(location);
    }

    public async Task ClickPlanJourneyAsync()
    {
        await _planJourneyButton.ClickAsync();
    }

    public async Task<string> GetFromValidationErrorAsync()
    {
        await _fromValidationError.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        return (await _fromValidationError.InnerTextAsync()).Trim();
    }

    public async Task<string> GetToValidationErrorAsync()
    {
        await _toValidationError.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        return (await _toValidationError.InnerTextAsync()).Trim();
    }

    private async Task SelectSuggestionAsync(string suggestionText)
    {
        // Wait for autocomplete typeahead list to render
        await _suggestions.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 8000 });

        var option = Page.Locator(".tt-suggestion, .stop-name", new() { HasText = suggestionText }).First;
        await option.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await option.ClickAsync();
    }
}
