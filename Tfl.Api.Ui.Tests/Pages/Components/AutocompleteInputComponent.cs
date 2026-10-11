using Microsoft.Playwright;

namespace Tfl.Api.Ui.Tests.Pages.Components;

public class AutocompleteInputComponent
{
    private readonly ILocator _input;
    private readonly ILocator _suggestions;

    public ILocator Input => _input;

    public AutocompleteInputComponent(ILocator input, ILocator suggestions)
    {
        _input = input;
        _suggestions = suggestions;
    }

    public async Task EnterAndSelectSuggestionAsync(string partialText, string suggestionText)
    {
        await _input.ClickAsync();
        await _input.FillAsync(partialText);

        // Wait for autocomplete typeahead dropdown to render
        await _suggestions.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 8000 });

        var option = _suggestions.Filter(new() { HasText = suggestionText }).First;
        await option.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await option.ClickAsync();
    }

    public async Task EnterRawAsync(string text)
    {
        await _input.ClickAsync();
        await _input.FillAsync(text);
    }
}
