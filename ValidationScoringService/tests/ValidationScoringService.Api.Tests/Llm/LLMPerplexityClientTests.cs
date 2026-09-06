using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using ValidationScoringService.Api.Tests.TestData;

namespace ValidationScoringService.Api.Tests.Llm;

public sealed class LLMPerplexityClientTests
{
    [Fact]
    public async Task EvaluateStepAsync_PostsToAgentEndpointAndParsesResponse()
    {
        const string responseBody = """
            {
              "output_text": "{\"score\":0.8,\"explanation\":\"The answer is detailed.\",\"isFinal\":true,\"nextInquiry\":null}"
            }
            """;

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    responseBody,
                    Encoding.UTF8,
                    "application/json")
            });

        using var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.perplexity.ai")
        };

        var settings = Options.Create(new PerplexitySettings
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.perplexity.ai",
            Model = "perplexity/sonar"
        });

        var client = new LLMPerplexityClient(httpClient, settings);

        var result = await client.EvaluateStepAsync(
            AiScoreStepRequestBuilder.CreateValidRequest(),
            CancellationToken.None);

        Assert.Equal(0.8, result.Score);
        Assert.Equal("The answer is detailed.", result.Explanation);
        Assert.True(result.IsFinal);
        Assert.Null(result.NextInquiry);

        handler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Post
                    && request.RequestUri == new Uri("https://api.perplexity.ai/v1/agent")),
                ItExpr.IsAny<CancellationToken>());
    }
}
