using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ValidationScoringService.Api.Tests.Llm;

public sealed class PerplexitySettingsValidationTests
{
    [Fact]
    public void Resolve_WhenModelIsSupported_ReturnsSettings()
    {
        var configuration = BuildConfiguration(
            apiKey: "test-key",
            baseUrl: "https://api.perplexity.ai",
            model: "sonar");

        using var serviceProvider = CreateServiceProvider(configuration);

        var settings = serviceProvider.GetRequiredService<IOptions<PerplexitySettings>>().Value;

        Assert.Equal("test-key", settings.ApiKey);
        Assert.Equal("https://api.perplexity.ai", settings.BaseUrl);
        Assert.Equal("sonar", settings.Model);
    }

    [Fact]
    public void Resolve_WhenModelIsUnsupported_ThrowsOptionsValidationException()
    {
        var configuration = BuildConfiguration(
            apiKey: "test-key",
            baseUrl: "https://api.perplexity.ai",
            model: "mistral-7b-instruct");

        using var serviceProvider = CreateServiceProvider(configuration);

        var exception = Assert.Throws<OptionsValidationException>(
            () => serviceProvider.GetRequiredService<IOptions<PerplexitySettings>>().Value);

        Assert.Contains("Perplexity:Model must be one of:", exception.Message);
    }

    private static IConfiguration BuildConfiguration(string apiKey, string baseUrl, string model)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Perplexity:ApiKey"] = apiKey,
                ["Perplexity:BaseUrl"] = baseUrl,
                ["Perplexity:Model"] = model
            })
            .Build();
    }

    private static ServiceProvider CreateServiceProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        services
            .AddOptions<PerplexitySettings>()
            .Bind(configuration.GetSection("Perplexity"))
            .ValidateDataAnnotations()
            .Validate(
                settings => PerplexitySettings.IsSupportedModel(settings.Model),
                $"Perplexity:Model must be one of: {string.Join(", ", PerplexitySettings.SupportedModels)}.");

        return services.BuildServiceProvider();
    }
}
