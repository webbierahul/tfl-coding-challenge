using System.Text.Json;

namespace Tfl.Api.Ui.Tests.Configuration;

public class TestSettings
{
    public string BaseUrl { get; set; } = "https://tfl.gov.uk/plan-a-journey/?cid=plan-a-journey";
    public string Browser { get; set; } = "chromium";
    public bool Headless { get; set; } = true;
    public float DefaultTimeoutMs { get; set; } = 30000;
    public float SlowMoMs { get; set; } = 0;

    public static TestSettings Load()
    {
        var settings = new TestSettings();
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        if (File.Exists(configPath))
        {
            try
            {
                var json = File.ReadAllText(configPath);
                var deserialized = JsonSerializer.Deserialize<TestSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (deserialized != null)
                {
                    settings = deserialized;
                }
            }
            catch
            {
                // Fall back to defaults
            }
        }

        // Allow CI or local overrides via environment variables
        var envBaseUrl = Environment.GetEnvironmentVariable("BASE_URL");
        if (!string.IsNullOrWhiteSpace(envBaseUrl))
        {
            settings.BaseUrl = envBaseUrl;
        }

        var envHeadless = Environment.GetEnvironmentVariable("HEADLESS");
        if (bool.TryParse(envHeadless, out var headlessParsed))
        {
            settings.Headless = headlessParsed;
        }

        var envBrowser = Environment.GetEnvironmentVariable("BROWSER");
        if (!string.IsNullOrWhiteSpace(envBrowser))
        {
            settings.Browser = envBrowser;
        }

        return settings;
    }
}
