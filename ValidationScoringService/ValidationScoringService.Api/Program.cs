using ValidationScoringService.Api.Llm;
using ValidationScoringService.Api.Managers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IValidationScoringManager, ValidationScoringManager>();
//builder.Services.AddSingleton<ILLMClient, LLMClientMock>();

builder.Services.AddHttpClient<ILLMClient, LLMPerplexityClient>();

builder.Services.Configure<PerplexitySettings>(
    builder.Configuration.GetSection("Perplexity"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
