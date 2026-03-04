using System;
using ValidationScoringService.Api.Contracts;

namespace ValidationScoringService.Api.Tests.TestData;

internal static class AiScoreStepRequestBuilder
{
    public static AiScoreStepRequest CreateValidRequest()
    {
        return new AiScoreStepRequest
        {
            ValidationId = Guid.NewGuid(),
            Question = "How do you ensure data privacy?",
            InitialAnswer = "We encrypt everything.",
            Inquiry = "Can you elaborate on access controls?",
            UserAnswer = "Only admins can view decrypted data."
        };
    }
}
