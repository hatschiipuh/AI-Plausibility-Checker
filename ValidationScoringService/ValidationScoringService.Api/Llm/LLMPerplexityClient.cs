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

    public async Task<LLMResultModel> EvaluateStepAsync(
    VSScoreStepRequest request,
    CancellationToken cancellationToken)
    {
        var prompt = LLMValidationPerplexitySonarPromptBuilder.Build(request);

        var clientRequest = LLMValidationPerplexityPromptToAgentRequestMapper.Map(
            this.settings.Model,
            prompt);

        using var response = await this.httpClient.PostAsJsonAsync(
            "v1/agent",
            clientRequest,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PerplexityAgentResponse>(
            cancellationToken: cancellationToken);

        if (payload is null)
            throw new InvalidOperationException("Perplexity Agent API returned an empty response.");

        return ParseResultModel(payload);
    }

    private static LLMResultModel ParseResultModel(PerplexityAgentResponse response)
    {
        var responseText = string.Join(
        Environment.NewLine,
            response.Output?
                .Where(item =>
                    item.Type == "message" &&
                    item.Role == "assistant")
                .SelectMany(item => item.Content)
                .Where(item => item.Type == "output_text")
                .Select(item => item.Text?.Trim())
                .OfType<string>() ?? new List<string>());

        if (string.IsNullOrWhiteSpace(responseText))
            throw new InvalidOperationException("Perplexity Agent API returned no content.");

        responseText = RemoveCodeFences(responseText);

        var normalizedContent = responseText.Trim();
        
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
            
        if (result is null)
        {
            throw new InvalidOperationException(
                "Perplexity Agent API response could not be parsed into LLMResultModel.");
        }

        return LLMResultModelValidator.Validate(result);
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
