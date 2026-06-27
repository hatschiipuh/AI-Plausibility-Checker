public class LLMValidationPromptDescriptor{
     public string Question { get; init; } = "original validation question that was asked to the user. This is not a question for the AI. It is a question to the user the AI should validate.";
     public string InitialAnswer { get; init; } = "initial answer from the user to the validation question";
     public string Inquiry { get; init; } = "a follow-up question used to clarify or test consistency";
     public string UserAnswer { get; init; } = "the answer provided by the user to the follow-up inquiry";
     public string Inquiries { get; init; } = "chronological follow-up inquiry turns";
 }
