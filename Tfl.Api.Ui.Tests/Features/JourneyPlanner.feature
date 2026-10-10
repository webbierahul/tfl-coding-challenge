@journeyPlanner
Feature: Journey Planner
  As a TfL commuter
  I want to plan journeys using the Journey Planner widget
  So that I can find valid routes, travel times, and accessibility information

  Background:
    Given I navigate to the TfL Journey Planner page

  @smoke
  Scenario: Plan a valid journey using autocomplete suggestions
    When I enter "Leicester Square" as origin and select "Leicester Square Underground Station"
    And I enter "Covent Garden" as destination and select "Covent Garden Underground Station"
    And I click on "Plan my journey"
    Then journey results should be displayed

  Scenario: Validate walking and cycling times for a planned journey
    When I enter "Leicester Square" as origin and select "Leicester Square Underground Station"
    And I enter "Covent Garden" as destination and select "Covent Garden Underground Station"
    And I click on "Plan my journey"
    Then the journey results should display a valid walking time
    And the journey results should display a valid cycling time

  Scenario: Edit preferences to choose routes with least walking
    When I enter "Leicester Square" as origin and select "Leicester Square Underground Station"
    And I enter "Covent Garden" as destination and select "Covent Garden Underground Station"
    And I click on "Plan my journey"
    And I edit preferences to select "Routes with least walking"
    And I update the journey
    Then the updated journey time should be displayed

  Scenario: View journey details and verify station accessibility information
    When I enter "Leicester Square" as origin and select "Leicester Square Underground Station"
    And I enter "Covent Garden" as destination and select "Covent Garden Underground Station"
    And I click on "Plan my journey"
    And I edit preferences to select "Routes with least walking"
    And I update the journey
    And I click on "View Details"
    Then complete access information should be displayed for "Covent Garden Underground Station"

  Scenario: Verify widget does not provide results for an invalid journey
    When I enter invalid location "ZZZZ99999InvalidLocation" as origin
    And I enter invalid location "YYYY88888InvalidLocation" as destination
    And I click on "Plan my journey"
    Then no journey results should be provided
    And an error message should be displayed

  Scenario: Verify widget is unable to plan a journey when no locations are entered
    When I click on "Plan my journey" without entering any locations
    Then a validation error "The From field is required." should be displayed for the From field
    And a validation error "The To field is required." should be displayed for the To field
