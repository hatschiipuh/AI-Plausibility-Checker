public interface IPerplexityRequest
{
    string Model { get; }
    LLMPerplexityRequestType RequestType { get; }
}

public interface IPerplexityRequest<out TRequest> : IPerplexityRequest
{
    TRequest ToRequest();
}