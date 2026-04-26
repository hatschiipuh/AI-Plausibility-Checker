 public sealed record PerplexitySonarRequest(
    string model,
    bool stream,
    int max_tokens,
    double temperature,
    PerplexitySonarMessage[] messages);

public sealed record PerplexitySonarMessage(string role, string content);