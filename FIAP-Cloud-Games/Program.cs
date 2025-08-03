using Application.Helper;
using Application.Mapping;
using Application.Services;
using Domain.Repository;
using FIAP_Cloud_Games.Configurations;
using FIAP_Cloud_Games.Endpoints;
using FIAP_Cloud_Games.Middleware;
using Infrastructure.Logging;
using Infrastructure.Middleware;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAutoMapper(typeof(MappingProfile));

#region injeção de dependência
builder.Services.AddScoped<IPessoaRepository, PessoaRepository>();
builder.Services.AddScoped<PessoaService>();
builder.Services.AddScoped<IJogoRepository, JogoRepository>();
builder.Services.AddScoped<JogoService>();
builder.Services.AddTransient<ICorrelationIdGenerator, CorrelationIdGenerator>();
builder.Services.AddTransient(typeof(IAppLogger<>), typeof(AppLogger<>));
#endregion

#region Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {seu_token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddSwaggerDocumentation();
builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
#endregion

#region JWT

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
});

#endregion

# region EF 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
#endregion

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

#region Map endpoints
app.MapPersonEndpoints();
app.MapGameEndpoints();
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });
#endregion

#region Middleware
app.UseCorrelationMiddleware();
app.UseGlobalErrorHandlingMiddleware();
#endregion

app.MapGet("/debug/datadog", () => new
{
    // Variáveis de ambiente do DataDog
    DD_API_KEY = Environment.GetEnvironmentVariable("DD_API_KEY")?.Length > 0 ? "***CONFIGURADO***" : "NÃO ENCONTRADO",
    DD_TRACE_ENABLED = Environment.GetEnvironmentVariable("DD_TRACE_ENABLED"),
    DD_SERVICE = Environment.GetEnvironmentVariable("DD_SERVICE"),
    DD_ENV = Environment.GetEnvironmentVariable("DD_ENV"),
    DD_SITE = Environment.GetEnvironmentVariable("DD_SITE"),

    // Status do .NET
    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    ProcessId = Environment.ProcessId,
    MachineName = Environment.MachineName,

    // DataDog Assembly Info
    DatadogAssemblyLoaded = AppDomain.CurrentDomain.GetAssemblies()
        .Any(a => a.FullName?.Contains("Datadog") == true),

    LoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => a.FullName?.Contains("Datadog") == true)
        .Select(a => a.FullName)
        .ToArray()
});

app.Run();
