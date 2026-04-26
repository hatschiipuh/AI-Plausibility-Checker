using System;
using ValidationScoringService.Api.Contracts;

namespace ValidationScoringService.Api.Tests.TestData;

internal static class AiScoreStepRequestBuilder
{
    public static VSScoreStepRequest CreateValidRequest()
    {
        return new VSScoreStepRequest
        {
            ValidationId = Guid.NewGuid(),
            Question = "How do you ensure data privacy?",
            InitialAnswer = "We encrypt everything.",
            Inquiries =
            [
                new VSScoreStepRequestInquery
                {
                    Inquiry = "Can you elaborate on access controls?",
                    UserAnswer = "Only admins can view decrypted data."
                }
            ]
        };
    }
}
