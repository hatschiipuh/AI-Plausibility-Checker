using System.Text;
using BusinessService.Api.Clients.ValidationScoringService;
using BusinessService.Api.Managers;
using BusinessService.Infrastructure;
using BusinessService.Api.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Business") ?? "Data Source=business.db";

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// add swagger with auth scheme JWT
builder.Services.AddSwaggerGen(options =>
 {
     options.SwaggerDoc("v1", new OpenApiInfo
     {
         Title = "BusinessService API",
         Version = "v1"
     });
 
     options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
     {
         Name = "Authorization",
         Type = SecuritySchemeType.Http,
         Scheme = "bearer",
         BearerFormat = "JWT",
         In = ParameterLocation.Header,
         Description = "Enter a valid JWT bearer token."
     });
 
     options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
     {
         [new OpenApiSecuritySchemeReference("Bearer", document, null)] = new List<string>()
     });
 });

builder.Services
     .AddOptions<JwtSettings>()
     .Bind(builder.Configuration.GetSection("Jwt"))
     .ValidateDataAnnotations()
     .ValidateOnStart();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Missing config: Jwt");

builder.Services
     .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(options =>
     {
         options.TokenValidationParameters = new TokenValidationParameters
         {
             ValidateIssuer = true,
             ValidateAudience = true,
             ValidateLifetime = true,
             ValidateIssuerSigningKey = true,
             ValidIssuer = jwtSettings.Issuer,
             ValidAudience = jwtSettings.Audience,
             IssuerSigningKey = new SymmetricSecurityKey(
                 Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
             ClockSkew = TimeSpan.Zero
         };
     });
 
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthManager, AuthManager>();

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

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
