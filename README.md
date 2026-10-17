# TfL Journey Planner Automation Framework

[![CI Tests](https://github.com/webbierahul/tfl-coding-challenge/actions/workflows/ci.yml/badge.svg)](https://github.com/webbierahul/tfl-coding-challenge/actions/workflows/ci.yml)

> **Continuous Integration**: Automated test execution runs on GitHub Actions. Reviewers can inspect the 100% green test execution, step logs, in-browser scenario summary, full-motion video recordings, and HTML test report directly at: [**Latest Successful CI Run (Run #10)**](https://github.com/webbierahul/tfl-coding-challenge/actions/runs/38134958212) or view the full history at [**GitHub Actions CI Runs**](https://github.com/webbierahul/tfl-coding-challenge/actions/workflows/ci.yml).

An automated test framework built in C# and .NET 10 for the [Transport for London (TfL) Journey Planner](https://tfl.gov.uk/plan-a-journey). The suite automates and validates commuter journey planning, multi-modal travel options, route preference updates, accessibility information, and boundary error conditions.

---

## What the Framework Is About

This framework is built as part of the Senior Test Analyst technical evaluation for Transport for London. It targets the public-facing **Journey Planner** widget, delivering an automated Behaviour-Driven Development (BDD) test suite using Gherkin syntax.

The test scenarios verify:
- Finding valid routes using dynamic typeahead autocomplete suggestions (`Leicester Square` to `Covent Garden`).
- Validating estimated journey durations for both walking and cycling.
- Editing commuter preferences (e.g., selecting `"Routes with least walking"`) and verifying amended journey times.
- Viewing route details and verifying complete station accessibility information for Covent Garden Underground Station.
- Widget boundary and error handling when invalid or empty locations are submitted.

---

## Development Decisions & Architecture

### 1. Page Object Model (POM) & Component Object Model (COM)
Rather than writing large, monolithic page classes, the architecture separates responsibilities into distinct layers:
- **`BasePage`**: Houses core browser interaction and common navigation routines.
- **`JourneyPlannerPage` & `JourneyResultsPage`**: Model page-level interactions, exposing clear actions and `ILocator` handles for verification.
- **Reusable Component Objects**:
  - **`AutocompleteInputComponent`**: Encapsulates the Twitter Typeahead dropdown pattern (typing partial text, waiting for suggestions, and selecting matching items). Reused across both `FromInput` and `ToInput` fields, eliminating duplicate code and making the addition of future search fields (e.g., "Via") trivial.
  - **`CookieBannerComponent`**: Encapsulates cookie consent dialog detection, acceptance, and modal overlay dismissal, separating cross-cutting compliance popups from business test logic.

### 2. Addressing the Brief's "Playwright and Chromedriver" Requirement (Compliance Note)
The original challenge instructions state:
> *"Use 'Playwright' and 'Chromedriver' dotnet (.NET) libraries for UI Automation."*

**Current Implementation & Compliance Status**:
The submitted automation suite implements **Microsoft Playwright** exclusively. Neither `Selenium.WebDriver` nor `Selenium.WebDriver.ChromeDriver` is currently referenced in project dependencies to prevent dead-code bloat, redundant driver binaries, and dual-lifecycle complexity.

**Architectural Rationale & Trade-offs**:
In the .NET ecosystem, **Microsoft Playwright** communicates directly with Chromium via the native Chrome DevTools Protocol (CDP) and does not utilize `chromedriver` (which is part of the Selenium WebDriver ecosystem). We standardised on Microsoft Playwright driving Chromium because:
- **Native CDP Communication**: Controls Chromium directly without requiring an intermediate `chromedriver` binary or matching local Chrome browser versions.
- **Web-First Assertions & Auto-Waiting**: Playwright's `Expect(locator)` natively polls and retries until DOM elements satisfy actionable states (visible, enabled, stable), avoiding arbitrary sleeps.
- **Fast, Deterministic Execution**: Executes all 6 scenarios in isolated browser contexts in headless or headed mode in ~17-20 seconds.
- **Identified Gap & Extensible Driver Design**: While this solution does not implement ChromeDriver, if TfL intended a side-by-side framework comparison or strictly requires both libraries, an `IBrowserDriver` / `DriverFactory` abstraction could be introduced alongside a `Selenium.WebDriver` package reference. We highlight this architectural choice and recommend clarifying whether TfL intended Playwright automating Chromium, or an evaluation across both engines.

### 3. Reqnroll BDD & BoDi Dependency Injection
- Reqnroll (.NET 10 compatible successor to SpecFlow) provides living documentation in plain English Gherkin syntax.
- Lightweight constructor injection (`BoDi.IObjectContainer`) injects `IPage`, `TestSettings`, and page objects into step definitions without global static state.

### 4. Test Isolation, Video Recording & Reporting
- **Clean State per Scenario**: Each scenario executes in an isolated `IBrowserContext`, ensuring cookies, cache, and session storage do not bleed across tests.
- **Scenario Video Recordings**: Full-motion `.webm` videos are captured for each scenario, named after the scenario title (e.g. `Plan_a_valid_journey_using_autocomplete_suggestions_*.webm`), enabling visual verification of executions.
- **Standalone HTML Test Report**: A native HTML report (`test-report.html`) is produced with full step execution breakdowns, timings, and status summaries.
- **Automated Failure Screenshots**: The `[AfterScenario]` hook detects test failures, automatically takes full-page PNG screenshots into `Screenshots/`, and attaches them to the NUnit test context.
- **90-Day CI Artifact Retention**: All HTML reports, videos, screenshots, and TRX logs are automatically archived in GitHub Actions with the maximum 90-day retention period.

### 5. Configuration Management
- Configuration is loaded via `appsettings.json` with fallback support for CI/CD environment variable overrides (`BASE_URL`, `HEADLESS`, `BROWSER`).
- Targets the canonical Journey Planner URL (`https://tfl.gov.uk/plan-a-journey/?cid=plan-a-journey`), preventing premature navigation completion on HTTP 301 redirects.

### 6. Cloudflare Bot Protection & Environmental Limitations
- **Ethical Test Engineering & Zero Evasion**: In adherence to test automation standards, this framework does not employ stealth injection scripts, DOM monkey-patching (such as `navigator.webdriver` overrides), or automated CAPTCHA-clicking hacks.
- **Diagnostic Environmental Handling**: If an execution environment is challenged by Cloudflare Bot Management or Turnstile (which occurs when executing from cloud datacenter IP blocks such as Azure/AWS on public GitHub-hosted runners), `CheckForEnvironmentalBlockersAsync()` immediately halts the scenario with a clear diagnostic message rather than hanging or attempting unauthorized evasion.
- **CI Execution Architecture**: To validate tests reliably without datacenter WAF interception, the CI pipeline executes on a dedicated runner with a clean UK connection, executing all 6 scenarios in ~20 seconds with video recordings and HTML report artifacts.
- **Enterprise Best Practice**: In production engineering at organizations like TfL, synthetic automation suites against Cloudflare-protected domains utilize Cloudflare WAF bypass rules (such as an encrypted custom CI token header or IP allowlist) or execute against internal pre-production staging environments where bot challenges are disabled.

### 7. Engineering Trade-offs & Practical Considerations
Given this evaluation was automated against TfL's live public endpoint rather than an isolated test environment, several deliberate trade-offs were made:
- **Sequential vs. Concurrent Execution**: Scenarios execute sequentially in isolated browser contexts. While Playwright natively supports parallel execution, running concurrent sessions against TfL's public edge risks triggering rate limiting (HTTP 429) and WAF bot mitigations. In a dedicated staging environment, parallel threads would reduce run time further.
- **UI vs. API Layering (Test Pyramid)**: These UI scenarios validate critical commuter journeys and accessibility markup. In an enterprise pipeline, broad routing permutations (e.g. multi-leg interchanges, service disruption fallbacks) are best validated at the API layer against TfL's Unified API (`/Journey/JourneyResults/{from}/to/{to}`), reserving UI automation for high-impact end-to-end user journeys.
- **Composition over Inheritance**: Encapsulating autocomplete and cookie dialogues into dedicated component objects keeps page objects focused on business workflow orchestration, making additions (e.g. intermediate "Via" search fields) trivial without touching base classes.

---

## Tech Stack

| Technology | Package / Version | Purpose |
|---|---|---|
| **.NET SDK** | `10.0.401` (`net10.0`) | Core runtime and compilation framework |
| **Microsoft.Playwright.NUnit** | `v1.63.0` | Browser automation engine automating Chromium natively via CDP |
| **Reqnroll.NUnit** | `v3.3.4` | BDD / Gherkin test execution engine (.NET successor to SpecFlow) |
| **NUnit** | `v4.3.2` | Test execution and assertion library |
| **NUnit3TestAdapter** | `v5.0.0` | NUnit test adapter for `dotnet test` runner |
| **Microsoft.NET.Test.Sdk** | `v17.14.0` | MSBuild test execution infrastructure |
| **Microsoft.Extensions.Configuration** | `v10.0.x` | Strongly typed configuration loader from `appsettings.json` |

---

## Steps for Setup & Execution

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download) installed on your machine.
- PowerShell or terminal environment.

### 1. Clone the Repository
```bash
git clone <repository-url>
cd tfl-coding-challenge
```

### 2. Build the Solution
```bash
dotnet build
```

### 3. Install Playwright Browsers
On the first run, ensure Playwright's browser binaries are installed:
```bash
pwsh Tfl.Api.Ui.Tests/bin/Debug/net10.0/playwright.ps1 install chromium
```
*(Or on macOS/Linux using standard dotnet tool: `dotnet tool run playwright install chromium`)*

### 4. Run the Tests
Execute the full test suite:
```bash
dotnet test
```

### 5. Running with Custom Options
- **Run Smoke Tests only**:
  ```bash
  dotnet test --filter "TestCategory=smoke"
  ```
- **Run in Headed Mode** (watch browser interactions live):
  ```bash
  HEADLESS=false dotnet test
  ```
- **Run against Google Chrome channel**:
  ```bash
  BROWSER=chrome dotnet test
  ```

---

## Note on Use of AI

In accordance with TfL's instructions regarding responsible AI use, AI prompting (**Google Gemini** and **ChatGPT**) were used during the development of this challenge for initial boilerplate scaffolding, technical research (.NET 10 Reqnroll compatibility and Playwright .NET best practices), and code review. All architectural design choices, component object modeling, element locators, Gherkin scenarios, and test validations were engineered, reviewed, and verified by the author.

---

## Additional Test Scenarios (Functional & Non-Functional)

Below is an overview of additional scenarios recommended for testing the public-facing TfL Journey Planner component across both functional and non-functional quality gates. 

> [!NOTE]
> In accordance with the challenge brief, these scenarios are proposed for test strategy and coverage planning rather than automated execution. Active performance load testing and intrusive security penetration testing must only be conducted against an authorized, isolated pre-production/staging environment with explicit approval from TfL engineering, never against live public endpoints.

### Functional Test Scenarios

1. **Date and Time Travel Options**
   - Verify journey planning for *"Arriving by"* a specific future time rather than default *"Leaving now"*.
   - Verify scheduling a journey on weekends, bank holidays, and late-night services (Night Tube).

2. **Service Disruptions and Planned Engineering Works**
   - Plan a journey across a line with active severe delays, part-closures, or planned weekend maintenance.
   - Verify that prominent disruption warning banners, affected stations, and recommended alternative routes are displayed.

3. **Multi-Modal Preference Filters**
   - Under *Edit preferences*, select *"Bus only"* and verify no underground, rail, or DLR legs appear in results.
   - Select accessibility criteria such as *"Step-free access to platform"* or *"Step-free access to train"* and verify only accessible routes are offered.

4. **Location Disambiguation Handling**
   - Enter an ambiguous station name (e.g., *"Victoria"*) and verify the widget presents a disambiguation picker distinguishing between *Victoria Underground Station*, *Victoria Coach Station*, and *Victoria Rail Station*.

5. **Via / Intermediate Stop Routing**
   - Use the *"Add via/stop"* feature to plan a journey requiring an interchange at a specific transit point (e.g., from *Stratford* to *Heathrow* via *Green Park*).

6. **Origin & Destination Inversion (Reverse Journey)**
   - Click the swap/reverse locations icon on the results page and verify the origin and destination invert with recalculated route directions and timings.

7. **Recent Search History & Favourites**
   - Verify that recently searched origin/destination pairs appear under "Recent journeys" in subsequent browser sessions without requiring user authentication.

---

### Non-Functional Test Scenarios

1. **Accessibility (WCAG 2.1 AA Compliance)**
   - **Screen Reader Compatibility**: Verify dynamic typeahead autocomplete results and validation alerts announce properly using ARIA live regions (`aria-live="polite"`).
   - **Keyboard Navigability**: Ensure full end-to-end journey planning can be completed using only keyboard navigation (`Tab`, `Shift+Tab`, `Arrow keys`, `Enter`), with visible focus outlines throughout.
   - **Colour Contrast & High Contrast Mode**: Verify route icons, line identifiers, and alert banners meet minimum contrast ratios (4.5:1 for normal text).

2. **Performance & Peak Load Concurrency (Pre-Production Testing)**
   - **Peak Commuter Concurrency**: In an authorized performance test environment, simulate concurrent search volumes representative of Monday morning peak commuting hours and major transit strike events (with target concurrency defined in collaboration with TfL capacity planning).
   - **Response Latency Benchmarks**: Benchmark API response times for route calculation, measuring 95th percentile ($p95$) latency against agreed service-level objectives (SLOs) (e.g. proposed target $< 1.5$ seconds under standard load).
   - **Client-Side Rendering Performance**: Benchmark Core Web Vitals (Largest Contentful Paint $< 2.5$s, Cumulative Layout Shift $< 0.1$) on the Journey Results page across throttled network profiles.

3. **Mobile Responsiveness & Cross-Device Compatibility**
   - Validate widget layout, touch tap-targets (minimum 44x44px), and expandable preference drawers across varying viewport sizes:
     - Mobile (360px – 414px, iOS Safari and Android Chrome)
     - Tablet (768px – 1024px, iPadOS Safari)
     - Desktop (1280px – 1920px)

4. **Security & Input Sanitization (Pre-Production Testing)**
   - **Input Sanitization & Boundary Fuzzing**: Verify that origin and destination fields sanitize SQL payloads and script tags without triggering server-side unhandled exceptions or XSS execution.
   - **Rate Limiting Safeguards**: In a staging environment, verify rate-limiting safeguards throttle excessive automated queries per client IP to protect downstream route calculation microservices.

5. **Network Resilience & Graceful Degradation**
   - **Slow 3G Network**: Simulate network throttling and verify appropriate loading indicators ("Fetching results") inform the user without timing out abruptly.
   - **Downstream Service Outage Fallback**: Simulate downstream journey planning API 503 responses; verify users receive a friendly, actionable service advisory rather than unhandled script exceptions or broken markup.
