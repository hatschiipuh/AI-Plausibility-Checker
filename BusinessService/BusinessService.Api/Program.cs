using BusinessService.Api.Clients.ValidationScoringService;
using BusinessService.Api.Managers;
using BusinessService.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Business") ?? "Data Source=business.db";

builder.Services.AddDbContext<BusinessDbContext>(options =>
    options.UseSqlite(connectionString));

var validationScoringBaseUrl =
    builder.Configuration["ValidationScoringService:BaseUrl"]
    ?? throw new InvalidOperationException("Missing config: ValidationScoringService:BaseUrl");

builder.Services.AddHttpClient("ValidationScoringService", client =>
{
    client.BaseAddress = new Uri(validationScoringBaseUrl);
});

builder.Services.AddScoped<BusinessService.Api.Clients.ValidationScoringService.IValidationScoringServiceClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("ValidationScoringService");

    return new ValidationScoringServiceClient(
        httpClient);
});

builder.Services.AddScoped<IBusinessServiceManager, BusinessServiceManager>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BusinessDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
