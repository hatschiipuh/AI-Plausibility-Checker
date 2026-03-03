using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessService.Api.Clients;

public interface IAiServiceClient
{
    Task<AiScoreStepResponse> ScoreStepAsync(AiScoreStepRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Simplified representation of an NSwag-generated client for the AiService.
/// </summary>
public class AiServiceClient : IAiServiceClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public AiServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AiScoreStepResponse> ScoreStepAsync(AiScoreStepRequest request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/ai/score-step")
        {
            Content = new StringContent(JsonSerializer.Serialize(request, SerializerOptions), Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<AiScoreStepResponse>(responseStream, SerializerOptions, cancellationToken);

        return payload ?? throw new InvalidOperationException("AI service response was empty.");
    }
}

public class AiScoreStepRequest
{
    public Guid ValidationId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string InitialAnswer { get; set; } = string.Empty;
    public string Inquiry { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
}

public class AiScoreStepResponse
{
    public double Score { get; set; }
    public string AiExplanation { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public string? NextInquiry { get; set; }
}
