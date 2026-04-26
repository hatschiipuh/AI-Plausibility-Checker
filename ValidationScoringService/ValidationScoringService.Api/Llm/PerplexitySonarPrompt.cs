using System.Text;
using ValidationScoringService.Api.Contracts;

public sealed class SonarPromptRequest : IPerplexityRequest<PerplexitySonarRequest>
{
    private const string SYSTEMROLE = "system";
    private const string USERROLE = "user";
    private const string SYSTEMTASK = "You are an AI model that evaluates the quality of answers to validation questions.";
   
    public string Model { get; }
   
    public VSScoreStepRequest Input { get; }

    public SonarPromptRequest(string Model, VSScoreStepRequest request) 
    {
        this.Model = Model;
        this.Input = request;
    }
    public LLMPerplexityRequestType RequestType => LLMPerplexityRequestType.Sonar;

    public PerplexitySonarRequest ToRequest() =>
       this.BuildSonarRequestBody(this.Input);

    private PerplexitySonarRequest BuildSonarRequestBody(VSScoreStepRequest request)
    {
        return new PerplexitySonarRequest(
        model: this.Model,
        stream: false,
        max_tokens: 256,
        temperature: 0.0,
        messages: new[]
        {
            new PerplexitySonarMessage(
                SYSTEMROLE,
                SYSTEMTASK),
            new PerplexitySonarMessage(
                USERROLE,
                BuildPrompt(request))
        });
    }

    private static string BuildPrompt(VSScoreStepRequest request)
    {
         var prompt = new StringBuilder();
            prompt.Append(
           $@"
Question {request.Question}
Initiale Antwort: {request.InitialAnswer} {Environment.NewLine}
             ");
        foreach (var inquiry in request.Inquiries)
        {
            var inqueryPromtPart = 
            $@"
Nachfrage: {inquiry.Inquiry}
Antwort des Nutzers: {inquiry.UserAnswer} {Environment.NewLine}
              ";
            prompt.Append(inqueryPromtPart);
        }

        prompt.Append(@"
Evaluate the quality of the initial answer in the context of the question and the inquiries. Consider if the answer is correct, complete, and relevant.
Use the following JSON format to return the evaluation result:
{{
  ""score"": <Number inbetween 0 and 1>,
  ""explanation"": ""a short explaination of the score"",
  ""isFinal"": true/false,
  ""nextInquiry"": ""optional next inquiry or null""
}}");
    
    return prompt.ToString();
    }
}