using CorporateAutomation.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var configuration = builder.Configuration;

        string keyBase64 = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "No se ha configurado Jwt:Key."
            );

        string issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "No se ha configurado Jwt:Issuer."
            );

        string audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "No se ha configurado Jwt:Audience."
            );

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(keyBase64)
            ),

            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateLifetime = true,
            RequireExpirationTime = true,

            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },

            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<CorporateAutomationContext>(
    (serviceProvider, options) =>
    {
        var configuration = serviceProvider
            .GetRequiredService<IConfiguration>();

        string connectionString =
            configuration.GetConnectionString("CorporateAutomation")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión."
            );

        options.UseSqlServer(connectionString);
    }
);

// Comprobar la disponibilidad de la base de datos.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CorporateAutomationContext>("database");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });


builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Corporate Automation Lab API";

        document.Info.Description =
            "REST API for managing tasks using C# and .NET 10. " +
            "Supports CRUD operations, filtering, pagination, " +
            "SQL Server persistence, and automated testing.";

        return Task.CompletedTask;
    });
});


var app = builder.Build();

app.UseExceptionHandler();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Endpoint para obtener JWT únicamente en desarrollo.
    app.MapPost("/api/dev/token",
        (DevTokenRequest request, IConfiguration configuration) =>
        {
            string? passwordConfigurada =
                configuration["Auth:DevelopmentTokenPassword"];

            // Sin credencial configurada, no emitimos tokens.
            if (string.IsNullOrWhiteSpace(passwordConfigurada))
            {
                return Results.Problem(
                    "La autenticación de desarrollo no está configurada.",
                    statusCode: 503
                );
            }

            // Comparamos la credencial sin usar ==.
            byte[] recibida = Encoding.UTF8.GetBytes(
                request.Password ?? ""
            );

            byte[] esperada = Encoding.UTF8.GetBytes(
                passwordConfigurada
            );

            if (!CryptographicOperations.FixedTimeEquals(
                recibida, esperada))
            {
                return Results.Unauthorized();
            }

            // Recuperamos la configuración JWT.
            string keyBase64 = configuration["Jwt:Key"]!;

            string issuer = configuration["Jwt:Issuer"]!;
            string audience = configuration["Jwt:Audience"]!;

            var clave = new SymmetricSecurityKey(
                Convert.FromBase64String(keyBase64)
            );

            var credenciales = new SigningCredentials(
                clave,
                SecurityAlgorithms.HmacSha256
            );

            DateTime ahora = DateTime.UtcNow;
            DateTime caducidad = ahora.AddMinutes(15);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: new[]
                {
                    new Claim(
                        JwtRegisteredClaimNames.Sub,
                        "developer-local"
                    )
                },
                notBefore: ahora,
                expires: caducidad,
                signingCredentials: credenciales
            );

            string tokenGenerado =
                new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new
            {
                access_token = tokenGenerado,
                token_type = "Bearer",
                expires_in = 900
            });
        })
        .WithName("GenerateDevelopmentToken")
        .WithTags("Development")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status503ServiceUnavailable);
}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint público para comprobar el estado de la API y la base de datos.
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
public record DevTokenRequest(string Password);
