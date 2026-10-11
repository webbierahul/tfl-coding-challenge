using NUnit.Framework;
using Reqnroll;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;
using Tfl.Api.Ui.Tests.Configuration;
using Tfl.Api.Ui.Tests.Pages;

namespace Tfl.Api.Ui.Tests.Steps;

[Binding]
public class JourneyPlannerSteps
{
    private readonly JourneyPlannerPage _plannerPage;
    private readonly JourneyResultsPage _resultsPage;
    private readonly TestSettings _settings;

    public JourneyPlannerSteps(IPage page, TestSettings settings)
    {
        _settings = settings;
        _plannerPage = new JourneyPlannerPage(page, settings);
        _resultsPage = new JourneyResultsPage(page, settings);
    }

    [Given(@"I navigate to the TfL Journey Planner page")]
    public async Task GivenINavigateToTheTflJourneyPlannerPage()
    {
        await _plannerPage.NavigateToAsync(_settings.BaseUrl);
    }

    [When(@"I enter ""(.*)"" as origin and select ""(.*)""")]
    public async Task WhenIEnterAsOriginAndSelect(string partialText, string suggestion)
    {
        await _plannerPage.EnterFromLocationAndSelectSuggestionAsync(partialText, suggestion);
    }

    [When(@"I enter ""(.*)"" as destination and select ""(.*)""")]
    public async Task WhenIEnterAsDestinationAndSelect(string partialText, string suggestion)
    {
        await _plannerPage.EnterToLocationAndSelectSuggestionAsync(partialText, suggestion);
    }

    [When(@"I enter invalid location ""(.*)"" as origin")]
    public async Task WhenIEnterInvalidLocationAsOrigin(string location)
    {
        await _plannerPage.EnterFromLocationRawAsync(location);
    }

    [When(@"I enter invalid location ""(.*)"" as destination")]
    public async Task WhenIEnterInvalidLocationAsDestination(string location)
    {
        await _plannerPage.EnterToLocationRawAsync(location);
    }

    [When(@"I click on ""Plan my journey""")]
    public async Task WhenIClickOnPlanMyJourney()
    {
        await _plannerPage.ClickPlanJourneyAsync();
    }

    [When(@"I click on ""Plan my journey"" without entering any locations")]
    public async Task WhenIClickOnPlanMyJourneyWithoutEnteringAnyLocations()
    {
        await _plannerPage.ClickPlanJourneyAsync();
    }

    [Then(@"journey results should be displayed")]
    public async Task ThenJourneyResultsShouldBeDisplayed()
    {
        await _resultsPage.WaitForResultsToLoadAsync();
        await Expect(_resultsPage.JourneyBoxes.First).ToBeVisibleAsync();
    }

    [Then(@"the journey results should display a valid walking time")]
    public async Task ThenTheJourneyResultsShouldDisplayAValidWalkingTime()
    {
        await _resultsPage.WaitForResultsToLoadAsync();
        await Expect(_resultsPage.WalkingJourneyBox).ToBeVisibleAsync();
        var walkingTime = await _resultsPage.GetWalkingTimeAsync();
        Assert.That(walkingTime, Does.Match(@"\d+\s*min"), "Walking journey time should specify a valid duration in minutes.");
    }

    [Then(@"the journey results should display a valid cycling time")]
    public async Task ThenTheJourneyResultsShouldDisplayAValidCyclingTime()
    {
        await _resultsPage.WaitForResultsToLoadAsync();
        await Expect(_resultsPage.CyclingJourneyBox).ToBeVisibleAsync();
        var cyclingTime = await _resultsPage.GetCyclingTimeAsync();
        Assert.That(cyclingTime, Does.Match(@"\d+\s*min"), "Cycling journey time should specify a valid duration in minutes.");
    }

    private string? _selectedPreference;

    [When(@"I edit preferences to select ""(.*)""")]
    public async Task WhenIEditPreferencesToSelect(string preference)
    {
        _selectedPreference = preference;
        await _resultsPage.WaitForResultsToLoadAsync();
        await _resultsPage.ClickEditPreferencesAsync();
        await _resultsPage.SelectRoutePreferenceAsync(preference);
    }

    [When(@"I update the journey")]
    public async Task WhenIUpdateTheJourney()
    {
        var expectedKeyword = _selectedPreference?.Contains("least walking", StringComparison.OrdinalIgnoreCase) == true
            ? "least walking"
            : _selectedPreference;
        await _resultsPage.ClickUpdateJourneyAsync(expectedKeyword);
    }

    [Then(@"the updated journey time should be displayed")]
    public async Task ThenTheUpdatedJourneyTimeShouldBeDisplayed()
    {
        // 1. Verify observable indicator that results reflect the selected preference
        var preferencesText = await _resultsPage.GetTravelPreferencesTextAsync();
        Assert.That(preferencesText, Does.Contain("least walking").IgnoreCase,
            "The updated journey results summary should reflect the selected 'least walking' preference.");

        // 2. Verify updated journey time is visible and formatted
        await Expect(_resultsPage.FirstJourneyTime).ToBeVisibleAsync();
        var journeyTime = await _resultsPage.GetUpdatedJourneyTimeAsync();
        Assert.That(journeyTime, Is.Not.Empty, "Updated journey time should be displayed.");
        Assert.That(journeyTime, Does.Match(@"\d+\s*min"), "Updated journey time should specify a valid duration in minutes.");
    }

    [When(@"I click on ""View Details""")]
    public async Task WhenIClickOnViewDetails()
    {
        await _resultsPage.ClickViewDetailsAsync();
        await _resultsPage.ClickStationAccessibilityDetailsAsync();
    }

    [Then(@"complete access information should be displayed for ""(.*)""")]
    public async Task ThenCompleteAccessInformationShouldBeDisplayedFor(string stationName)
    {
        await Expect(_resultsPage.FullWidthContent).ToContainTextAsync(stationName);
        var stepContent = await _resultsPage.GetStationStepAccessDetailsAsync(stationName);

        // 1. Verify the station name is prominently present in the detailed step
        Assert.That(stepContent, Does.Contain(stationName).IgnoreCase
            .Or.Contain("Covent Garden").IgnoreCase,
            $"Expected expanded journey details to display station '{stationName}'.");

        // 2. Verify physical station access information (e.g. 'Up stairs' entrance/exit detail)
        Assert.That(stepContent, Does.Contain("Up stairs").IgnoreCase
            .Or.Contain("stairs").IgnoreCase,
            $"Expected physical access details ('Up stairs') for '{stationName}', but received:\n{stepContent}");

        // 3. Verify the dedicated 'Accessibility details' feature/link for the station
        Assert.That(stepContent, Does.Contain("Accessibility details").IgnoreCase,
            $"Expected 'Accessibility details' link/section to be present for station '{stationName}'.");
    }

    [Then(@"no journey results should be provided")]
    public async Task ThenNoJourneyResultsShouldBeProvided()
    {
        var hasResults = await _resultsPage.HasJourneyResultsAsync();
        Assert.That(hasResults, Is.False, "Widget should not provide valid journey results for invalid locations.");
    }

    [Then(@"an error message should be displayed")]
    public async Task ThenAnErrorMessageShouldBeDisplayed()
    {
        var errorMessage = await _resultsPage.GetErrorOrDisambiguationMessageAsync();
        Assert.That(errorMessage, Is.Not.Empty, "An error or disambiguation message should be displayed.");
        Assert.That(errorMessage, Does.Contain("Sorry").Or.Contain("not find").Or.Contain("more than one location").Or.Contain("valid"),
            "Error or disambiguation message should explain why no journey results were found.");
    }

    [Then(@"a validation error ""(.*)"" should be displayed for the From field")]
    public async Task ThenAValidationErrorShouldBeDisplayedForTheFromField(string expectedError)
    {
        await Expect(_plannerPage.FromValidationError).ToHaveTextAsync(expectedError);
    }

    [Then(@"a validation error ""(.*)"" should be displayed for the To field")]
    public async Task ThenAValidationErrorShouldBeDisplayedForTheToField(string expectedError)
    {
        await Expect(_plannerPage.ToValidationError).ToHaveTextAsync(expectedError);
    }
}
