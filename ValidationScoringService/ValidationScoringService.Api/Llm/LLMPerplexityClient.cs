using System.Net.Http.Headers;
using System.Text;
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

            LLMPerplexityRequestType.Agent =>
                throw new NotSupportedException($"requestType Agent is currently not Supported"),

            _ => throw new NotSupportedException($"Unsupported request type: {requestType}")
        };
      
    }

    private static LLMPerplexityRequestType ResolveRequestType(PerplexitySettings settings)
    {
        switch (settings.Model.ToLowerInvariant())
        {
            case "sonar" or  "sonar-pro" or "sonar-deep-research" or "sonar-reasoning-pro":
                return LLMPerplexityRequestType.Sonar;
            default :
                return LLMPerplexityRequestType.Agent;
        }
    }

     private async Task<LLMResultModel> EvaluateWithSonarAsync(
        VSScoreStepRequest request,
        CancellationToken cancellationToken)
    {
        var sonarPrompt = new SonarPromptRequest(this.settings.Model, request);

        using var response = await this.httpClient.PostAsJsonAsync(
            "chat/completions",
            sonarPrompt.ToRequest(),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<SonarResponse>(
            cancellationToken: cancellationToken);

        if (payload is null)
            throw new InvalidOperationException("Perplexity Sonar returned an empty response.");

        return MapSonarResponse(payload);
    }

    private static LLMResultModel MapSonarResponse(SonarResponse response)
    {
        var content = response.choices?.FirstOrDefault()?.message?.content?.Trim();

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Perplexity Sonar returned no content.");

        content = RemoveCodeFences(content);

        var result = JsonSerializer.Deserialize<LLMResultModel>(
            content,
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
