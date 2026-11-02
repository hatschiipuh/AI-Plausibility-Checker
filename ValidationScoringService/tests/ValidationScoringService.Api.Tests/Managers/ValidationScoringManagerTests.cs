using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using ValidationScoringService.Api.Contracts;
using ValidationScoringService.Api.Llm;
using ValidationScoringService.Api.Managers;
using ValidationScoringService.Api.Tests.TestData;

namespace ValidationScoringService.Api.Tests.Managers;

public sealed class ValidationScoringManagerTests
{
    [Fact]
    public async Task ScoreStepAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        var manager = new ValidationScoringManager(Mock.Of<ILLMClient>());

        await Assert.ThrowsAsync<ArgumentNullException>(() => manager.ScoreStepAsync(null!, CancellationToken.None));
    }

    public static IEnumerable<object[]> InvalidRequestMutations()
    {
        yield return new object[]
        {
            new Action<VSScoreStepRequest>(request => request.ValidationId = Guid.Empty),
            "ValidationId"
        };

        yield return new object[]
        {
            new Action<VSScoreStepRequest>(request => request.Question = " "),
            "Question"
        };

        yield return new object[]
        {
            new Action<VSScoreStepRequest>(request => request.InitialAnswer = string.Empty),
            "InitialAnswer"
        };

        yield return new object[]
        {
            new Action<VSScoreStepRequest>(request =>
                request.Question = new string('q', ValidationInputLimits.MaximumQuestionCharacters + 1)),
            "Question"
        };

        yield return new object[]
        {
            new Action<VSScoreStepRequest>(request =>
                request.Inquiries =
                [
                    .. Enumerable.Range(0, ValidationInputLimits.MaximumFollowUpQuestions + 1)
                        .Select(_ => new VSScoreStepRequestInquiry
                        {
                            Inquiry = "Follow-up question",
                            UserAnswer = "Follow-up answer"
                        })
                ]),
            "follow-up questions"
        };
    }

    [Theory]
    [MemberData(nameof(InvalidRequestMutations))]
    public async Task ScoreStepAsync_WithInvalidRequest_ThrowsArgumentException(Action<VSScoreStepRequest> mutate, string expectedFragment)
    {
        var llmClientMock = new Mock<ILLMClient>(MockBehavior.Strict);
        var manager = new ValidationScoringManager(llmClientMock.Object);
        var request = AiScoreStepRequestBuilder.CreateValidRequest();
        mutate(request);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => manager.ScoreStepAsync(request, CancellationToken.None));

        Assert.Contains(expectedFragment, exception.Message);
        llmClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ScoreStepAsync_WithValidRequest_MapsAiModelResult()
    {
        using var cts = new CancellationTokenSource();
        var request = AiScoreStepRequestBuilder.CreateValidRequest();
        var modelResult = new LLMResultModel
        {
            Score = 0.87,
            Explanation = "Detailed verdict",
            IsFinal = true,
            NextInquiry = "None"
        };

        var llmClientMock = new Mock<ILLMClient>(MockBehavior.Strict);
        llmClientMock
            .Setup(client => client.EvaluateStepAsync(request, cts.Token))
            .ReturnsAsync(modelResult);

        var manager = new ValidationScoringManager(llmClientMock.Object);

        var response = await manager.ScoreStepAsync(request, cts.Token);

        Assert.Equal(modelResult.Score, response.Score);
        Assert.Equal(modelResult.Explanation, response.AiExplanation);
        Assert.Equal(modelResult.IsFinal, response.IsFinal);
        Assert.Null(response.NextInquiry);
        llmClientMock.VerifyAll();
    }

    [Fact]
    public async Task ScoreStepAsync_AtFollowUpQuestionLimit_ForcesFinalResponse()
    {
        var request = AiScoreStepRequestBuilder.CreateValidRequest();
        request.Inquiries =
        [
            .. Enumerable.Range(0, ValidationInputLimits.MaximumFollowUpQuestions)
                .Select(index => new VSScoreStepRequestInquiry
                {
                    Inquiry = $"Follow-up question {index + 1}",
                    UserAnswer = "Follow-up answer"
                })
        ];

        var llmClientMock = new Mock<ILLMClient>(MockBehavior.Strict);
        llmClientMock
            .Setup(client => client.EvaluateStepAsync(request, CancellationToken.None))
            .ReturnsAsync(new LLMResultModel
            {
                Score = 0.6,
                Explanation = "The answer has been assessed.",
                IsFinal = false,
                NextInquiry = "This must not be returned."
            });

        var manager = new ValidationScoringManager(llmClientMock.Object);

        var response = await manager.ScoreStepAsync(request, CancellationToken.None);

        Assert.True(response.IsFinal);
        Assert.Null(response.NextInquiry);
        llmClientMock.VerifyAll();
    }

    [Fact]
    public async Task ScoreStepAsync_WhenPromptHistoryExceedsCharacterLimit_ThrowsBeforeCallingLlm()
    {
        var request = AiScoreStepRequestBuilder.CreateValidRequest();
        request.Inquiries =
        [
            .. Enumerable.Range(0, 4)
                .Select(_ => new VSScoreStepRequestInquiry
                {
                    Inquiry = new string('q', ValidationInputLimits.MaximumInquiryCharacters),
                    UserAnswer = new string('a', ValidationInputLimits.MaximumUserAnswerCharacters)
                })
        ];

        var llmClientMock = new Mock<ILLMClient>(MockBehavior.Strict);
        var manager = new ValidationScoringManager(llmClientMock.Object);

        await Assert.ThrowsAsync<ArgumentException>(
            () => manager.ScoreStepAsync(request, CancellationToken.None));

        llmClientMock.VerifyNoOtherCalls();
    }
}
