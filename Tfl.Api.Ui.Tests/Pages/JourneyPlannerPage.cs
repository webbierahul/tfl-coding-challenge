using Microsoft.Playwright;
using Tfl.Api.Ui.Tests.Pages.Components;

namespace Tfl.Api.Ui.Tests.Pages;

public class JourneyPlannerPage : BasePage
{
    private readonly ILocator _planJourneyButton;
    private readonly ILocator _fromValidationError;
    private readonly ILocator _toValidationError;

    public AutocompleteInputComponent FromInput { get; }
    public AutocompleteInputComponent ToInput { get; }
    public ILocator FromValidationError => _fromValidationError;
    public ILocator ToValidationError => _toValidationError;

    public JourneyPlannerPage(IPage page) : base(page)
    {
        var suggestions = page.Locator(".tt-suggestion, .stop-name");
        FromInput = new AutocompleteInputComponent(page.Locator("#InputFrom"), suggestions);
        ToInput = new AutocompleteInputComponent(page.Locator("#InputTo"), suggestions);

        _planJourneyButton = page.GetByRole(AriaRole.Button, new() { Name = "Plan my journey" })
            .Or(page.Locator("#plan-journey-button")).First;
        _fromValidationError = page.Locator("#InputFrom-error");
        _toValidationError = page.Locator("#InputTo-error");
    }

    public async Task EnterFromLocationAndSelectSuggestionAsync(string partialText, string suggestionToSelect)
    {
        await FromInput.EnterAndSelectSuggestionAsync(partialText, suggestionToSelect);
    }

    public async Task EnterToLocationAndSelectSuggestionAsync(string partialText, string suggestionToSelect)
    {
        await ToInput.EnterAndSelectSuggestionAsync(partialText, suggestionToSelect);
    }

    public async Task EnterFromLocationRawAsync(string location)
    {
        await FromInput.EnterRawAsync(location);
    }

    public async Task EnterToLocationRawAsync(string location)
    {
        await ToInput.EnterRawAsync(location);
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
}
