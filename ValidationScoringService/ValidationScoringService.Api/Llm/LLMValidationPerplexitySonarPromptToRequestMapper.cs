 using System.Text.Json;
 
 public static class LLMValidationPerplexitySonarPromptToRequestMapper
 {
     public static PerplexitySonarRequest Map(
         string model,
         LLMValidationPerplexitySonarPrompt prompt)
     {
         ArgumentException.ThrowIfNullOrWhiteSpace(model);
         ArgumentNullException.ThrowIfNull(prompt);
 
        var hints = LLMValidationPerplexitySonarPrompt.HINTS.Select(x => 
                new PerplexitySonarMessage(LLMValidationPerplexitySonarPrompt.SYSTEMROLE, x)).ToArray();

        var messages = new[]
                {
                    new PerplexitySonarMessage(
                        LLMValidationPerplexitySonarPrompt.SYSTEMROLE,
                        LLMValidationPerplexitySonarPrompt.SYSTEMTASK),
                    new PerplexitySonarMessage(
                        LLMValidationPerplexitySonarPrompt.USERROLE,
                        JsonSerializer.Serialize(prompt.ValidationPrompt))
                }
                .Concat(hints)
                .ToArray();

         return new PerplexitySonarRequest(
             model: model,
             stream: false,
             max_tokens: 256,
             temperature: 0.0,
             messages: messages
         );
     }
 }