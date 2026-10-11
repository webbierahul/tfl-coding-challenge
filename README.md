# TfL Journey Planner Automation Framework

[![CI Tests](https://github.com/webbierahul/tfl-coding-challenge/actions/workflows/ci.yml/badge.svg)](https://github.com/webbierahul/tfl-coding-challenge/actions/workflows/ci.yml)

> **Continuous Integration**: Automated test execution runs on GitHub Actions. Reviewers can inspect the 100% green test execution, step logs, in-browser scenario summary, full-motion video recordings, and HTML test report directly at: [**Latest Successful CI Run (Run #5)**](https://github.com/webbierahul/tfl-coding-challenge/actions/runs/38129255152) or view the full history at [**GitHub Actions CI Runs**](https://github.com/webbierahul/tfl-coding-challenge/actions/workflows/ci.yml).

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

### 2. Playwright with Chromium (CDP) vs. Legacy ChromeDriver
While the brief mentions Playwright and ChromeDriver, this implementation standardises on **Microsoft Playwright**:
- **Native Chrome DevTools Protocol (CDP)**: Controls Chromium directly without requiring an intermediate `chromedriver` binary or matching local Chrome browser versions.
- **Auto-Waiting & Web-First Assertions**: Playwright's `Expect(locator)` natively polls and retries until DOM elements satisfy actionable states (visible, enabled, stable), avoiding arbitrary sleeps.
- **Fast and Resilient**: Executes tests in isolated browser contexts in headless or headed mode with minimal overhead.

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

### 6. Cloudflare Bot Protection & CI Considerations
- **Why Tests Pass Locally vs. CI**:
  - **IP Reputation**: Local runs originate from trusted residential/office ISPs in the UK. In contrast, public CI runners (GitHub Actions hosted on Microsoft Azure) originate from data center IP ranges (ASN 8075), which Cloudflare's Bot Management flags as elevated risk, triggering Turnstile ("Verify you are human").
  - **Automation Signatures**: Standard headless Chromium advertises `navigator.webdriver = true` and lacks full desktop rendering signals.
- **Applied Mitigations**:
  - Launched Chromium with `--disable-blink-features=AutomationControlled` and masked `navigator.webdriver` via `AddInitScriptAsync`.
  - Configured matching UK locale (`en-GB`) and London timezone (`Europe/London`).
  - Configured GitHub Actions with `xvfb-run` to execute in headed mode on a virtual 1280x800 display, eliminating headless canvas/font discrepancies.
- **Enterprise Best Practice**: In enterprise environments, synthetic automation suites against Cloudflare-protected domains utilize Cloudflare WAF bypass rules (such as a secure CI header or IP whitelisting) or run against staging environments where bot challenges are disabled.

---

## Tech Stack

| Technology | Purpose |
|---|---|
| **C# / .NET 10** | Core programming language and runtime framework |
| **Microsoft.Playwright (v1.52.0)** | Browser automation engine automating Chromium natively |
| **Reqnroll (v2.4.1)** | BDD / Gherkin test execution engine (.NET successor to SpecFlow) |
| **Reqnroll.NUnit & NUnit (v4.3.2)** | Test runner and assertion library |
| **Microsoft.Extensions.Configuration** | Strongly typed settings from `appsettings.json` |

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

This test automation framework was developed with the assistance of the official **Microsoft Playwright documentation** and **Google Gemini** for initial boilerplate scaffolding, research into .NET 10 Reqnroll compatibility, and reviewing Playwright .NET best practices. All architectural decisions, component patterns, Gherkin scenario structures, element locators, and framework refactorings were reviewed, refined, and validated by the author.

---

## Additional Test Scenarios (Functional & Non-Functional)

Below is an overview of additional scenarios recommended for testing the public-facing TfL Journey Planner component across both functional and non-functional quality gates.

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

2. **Performance & Peak Load Concurrency**
   - **Rush Hour Load**: Simulate high concurrent search traffic (e.g., 5,000+ requests per second) representative of Monday morning peak commuting hours and major transit strike events.
   - **Response Latency**: Benchmark API response times for route calculation, ensuring 95th percentile ($p95$) latency stays below 1.5 seconds.
   - **Client-Side Rendering Performance**: Validate Core Web Vitals (Largest Contentful Paint < 2.5s, Cumulative Layout Shift < 0.1) on the Journey Results page.

3. **Mobile Responsiveness & Cross-Device Compatibility**
   - Validate widget layout, touch tap-targets (minimum 44x44px), and expandable preference drawers across varying viewport sizes:
     - Mobile (360px – 414px, iOS Safari and Android Chrome)
     - Tablet (768px – 1024px, iPadOS Safari)
     - Desktop (1280px – 1920px)

4. **Security & Input Sanitization**
   - **Injection Testing**: Input SQL payloads (`' OR 1=1 --`) and cross-site scripting strings (`<script>alert(1)</script>`) into origin and destination fields to verify proper input sanitization and encoding.
   - **Rate Limiting**: Verify rate-limiting safeguards prevent automated denial-of-service abuse against the public autocomplete and journey planning endpoints.

5. **Network Resilience & Graceful Degradation**
   - **Slow 3G Network**: Simulate network throttling and verify appropriate loading indicators ("Fetching results") inform the user without timing out abruptly.
   - **API Outage / 503 Fallback**: Simulate downstream journey planning API downtime; verify users receive a friendly, actionable error message rather than unhandled script exceptions or blank pages.
