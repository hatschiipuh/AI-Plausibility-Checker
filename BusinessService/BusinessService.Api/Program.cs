using BusinessService.Api.Clients;
using BusinessService.Api.Managers;
using BusinessService.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Business") ?? "Data Source=business.db";

builder.Services.AddDbContext<BusinessDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddHttpClient<IAiServiceClient, AiServiceClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["AiService:BaseUrl"] ?? "http://localhost:5075";
    client.BaseAddress = new Uri(baseUrl);
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
