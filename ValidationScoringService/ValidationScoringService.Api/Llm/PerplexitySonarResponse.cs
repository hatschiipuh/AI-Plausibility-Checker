public sealed record SonarResponse(
    SonarChoice[] choices,
    SonarUsage? usage,
    string[]? citations);

public sealed record SonarChoice(
    SonarMessage message);

public sealed record SonarMessage(
    string content);

public sealed record SonarUsage(
    int prompt_tokens,
    int completion_tokens,
    int total_tokens);