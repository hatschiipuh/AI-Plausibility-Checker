
using System.Text.Json;

public static class PerplexityResponseFormatFactory
{
    public static PerplexityResponseFormat CreateValidationResult()
    {
        var schema = JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new
            {
                score = new
                {
                    type = "number",
                    minimum = 0.0,
                    maximum = 1.0
                },
                explanation = new
                {
                    type = "string"
                },
                isFinal = new
                {
                    type = "boolean"
                },
                nextInquiry = new
                {
                    anyOf = new object[]
                    {
                        new { type = "string" },
                        new { type = "null" }
                    }
                }
            },
            required = new[]
            {
                "score",
                "explanation",
                "isFinal",
                "nextInquiry"
            },
            additionalProperties = false
        });

        return new PerplexityResponseFormat
        {
            JsonSchema = new PerplexityJsonSchema
            {
                SchemaIdentifier = "ValidationResult",
                Schema = schema
            }
        };
    }
}
