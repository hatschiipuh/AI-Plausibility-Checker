using ValidationScoringService.Api.Llm;
using ValidationScoringService.Api.Managers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IValidationScoringManager, ValidationScoringManager>();
//builder.Services.AddSingleton<ILLMClient, LLMClientMock>();

builder.Services
    .AddOptions<PerplexitySettings>()
    .Bind(builder.Configuration.GetSection("Perplexity"))
    .ValidateDataAnnotations()
    .Validate(
        settings => PerplexitySettings.IsSupportedModel(settings.Model),
        $"Perplexity:Model must be one of: {string.Join(", ", PerplexitySettings.SupportedModels)}.")
    .ValidateOnStart();

builder.Services.AddHttpClient<ILLMClient, LLMPerplexityClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
