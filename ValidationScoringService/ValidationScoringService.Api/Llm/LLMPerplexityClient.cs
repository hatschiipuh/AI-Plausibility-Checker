using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ValidationScoringService.Api.Contracts;
using ValidationScoringService.Api.Llm;

public sealed class LLMPerplexityClient : ILLMClient
{ 
    private readonly HttpClient httpClient;
    private readonly PerplexitySettings settings;

    public LLMPerplexityClient(HttpClient httpClient, IOptions<PerplexitySettings> options)
    {
        this.httpClient = httpClient;
        this.settings   = options.Value;

        httpClient.BaseAddress ??= new Uri(this.settings.BaseUrl);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", this.settings.ApiKey);
    }

    public Task<LLMResultModel> EvaluateStepAsync(VSScoreStepRequest request, CancellationToken cancellationToken)
    {
        var apiKey = this.settings.ApiKey
            ?? throw new InvalidOperationException("Perplexity:ApiKey not configured.");

        this.httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var requestType = ResolveRequestType(this.settings);

        return requestType switch
        {
            LLMPerplexityRequestType.Sonar =>
                EvaluateWithSonarAsync(request, cancellationToken),
            _ => throw new InvalidOperationException(
                $"Perplexity:Model '{this.settings.Model}' is not supported.")
        };
    }

    private static LLMPerplexityRequestType ResolveRequestType(PerplexitySettings settings)
    {
        if (PerplexitySettings.IsSupportedModel(settings.Model))
        {
            return LLMPerplexityRequestType.Sonar;
        }

        throw new InvalidOperationException(
            $"Perplexity:Model '{settings.Model}' is not supported.");
    }

     private async Task<LLMResultModel> EvaluateWithSonarAsync(
        VSScoreStepRequest request,
        CancellationToken cancellationToken)
    {
        var sonarPrompt = LLMValidationPerplexitySonarPromptBuilder.Build(request);
        var clientrequest = LLMValidationPerplexitySonarPromptToRequestMapper.Map(
            this.settings.Model,
            sonarPrompt);

        using var response = await this.httpClient.PostAsJsonAsync(
            "chat/completions",
            clientrequest,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<SonarResponse>(
            cancellationToken: cancellationToken);

        if (payload is null)
            throw new InvalidOperationException("Perplexity Sonar returned an empty response.");

        return ParseResultModel(payload);
    }

    private static LLMResultModel ParseResultModel(SonarResponse response)
    {
        var content = response.choices?.FirstOrDefault()?.message?.content?.Trim();

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Perplexity Sonar returned no content.");

        content = RemoveCodeFences(content);

        var normalizedContent = content.Trim();
        
        if (normalizedContent.StartsWith('"'))
        {
            normalizedContent = JsonSerializer.Deserialize<string>(normalizedContent)
                ?? throw new InvalidOperationException("Content is an invalid JSON string.");
        }
        
        var result = JsonSerializer.Deserialize<LLMResultModel>(
            normalizedContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return result ?? throw new InvalidOperationException(
            "Perplexity Sonar response could not be parsed into LLMResultModel.");
    }

    private static string RemoveCodeFences(string content)
    {
        if (content.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewLine = content.IndexOf('\n');
            if (firstNewLine >= 0)
                content = content[(firstNewLine + 1)..];

            var lastFence = content.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence >= 0)
                content = content[..lastFence];
        }

        return content.Trim();
    }
}
