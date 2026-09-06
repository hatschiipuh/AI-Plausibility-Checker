using ValidationScoringService.Api.Tests.TestData;

namespace ValidationScoringService.Api.Tests.Llm;

public sealed class LLMValidationPerplexityPromptToAgentRequestMapperTests
{
    [Fact]
    public void Map_CreatesAgentRequestWithStructuredResponseFormat()
    {
        var prompt = LLMValidationPerplexitySonarPromptBuilder.Build(
            AiScoreStepRequestBuilder.CreateValidRequest());

        var request = LLMValidationPerplexityPromptToAgentRequestMapper.Map(
            "perplexity/sonar",
            prompt);

        Assert.Equal("perplexity/sonar", request.Model);
        Assert.False(request.Stream);
        Assert.Contains("Return valid JSON only.", request.Instructions);

        var input = Assert.Single(request.Input);
        Assert.Equal("message", input.Type);
        Assert.Equal("user", input.Role);
        Assert.Contains("Question", input.Content);

        Assert.NotNull(request.ResponseFormat);
        Assert.Equal("json_schema", request.ResponseFormat!.Type);
        Assert.Equal(
            "ValidationResult",
            request.ResponseFormat.JsonSchema.SchemaIdentifier);
    }
}
