namespace ValidationScoringService.Api.Tests.Llm;

public sealed class PerplexityResponseFormatFactoryTests
{
    [Fact]
    public void CreateValidationResult_CreatesExpectedJsonSchema()
    {
        var responseFormat = PerplexityResponseFormatFactory.CreateValidationResult();
        var schema = responseFormat.JsonSchema.Schema;

        Assert.Equal("json_schema", responseFormat.Type);
        Assert.Equal("ValidationResult", responseFormat.JsonSchema.SchemaIdentifier);
        Assert.Equal("object", schema.GetProperty("type").GetString());
        Assert.False(schema.GetProperty("additionalProperties").GetBoolean());

        var requiredProperties = schema
            .GetProperty("required")
            .EnumerateArray()
            .Select(property => property.GetString()!)
            .ToArray();

        Assert.Equal(
            ["score", "explanation", "isFinal", "nextInquiry"],
            requiredProperties);

        Assert.Equal(
            "number",
            schema.GetProperty("properties").GetProperty("score").GetProperty("type").GetString());
        Assert.Equal(
            0,
            schema.GetProperty("properties").GetProperty("score").GetProperty("minimum").GetDouble());
        Assert.Equal(
            1,
            schema.GetProperty("properties").GetProperty("score").GetProperty("maximum").GetDouble());

        var nextInquiryTypes = schema
            .GetProperty("properties")
            .GetProperty("nextInquiry")
            .GetProperty("anyOf")
            .EnumerateArray()
            .Select(type => type.GetProperty("type").GetString()!)
            .ToArray();

        Assert.Equal(["string", "null"], nextInquiryTypes);
    }
}
