
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using CorporateAutomation.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace CorporateAutomation.Tests;

public class TareasApiFactory : WebApplicationFactory<Program>
{
    public TareasApiFactory()
    {
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    // Credencial ficticia utilizada exclusivamente en los tests.
    public const string DevelopmentTokenTestPassword =
        "credencial-ficticia-para-pruebas";

    // Clave aleatoria exclusiva para las pruebas automatizadas.
    public string JwtTestKey { get; } =
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    // Genera un JWT válido para probar endpoints protegidos.
    public string CrearTokenValido()
    {
        var clave = new SymmetricSecurityKey(
            Convert.FromBase64String(JwtTestKey)
        );

        var credenciales = new SigningCredentials(
            clave,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: "CorporateAutomation.Api.Tests",
            audience: "CorporateAutomation.Tests",
            claims: new[]
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    "usuario-pruebas"
                )
            },
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credenciales
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    public string CrearTokenDePrueba(
        bool expirado = false,
        bool firmaIncorrecta = false)
    {
        // Usamos otra clave si queremos simular una firma incorrecta.
        byte[] bytesClave = firmaIncorrecta
            ? RandomNumberGenerator.GetBytes(32)
            : Convert.FromBase64String(JwtTestKey);

        var clave = new SymmetricSecurityKey(bytesClave);

        var credenciales = new SigningCredentials(
            clave,
            SecurityAlgorithms.HmacSha256
        );

        DateTime ahora = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            issuer: "CorporateAutomation.Api.Tests",
            audience: "CorporateAutomation.Tests",
            claims: new[]
            {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                "usuario-pruebas"
            )
            },
            notBefore: expirado
                ? ahora.AddMinutes(-10)
                : ahora.AddMinutes(-1),
            expires: expirado
                ? ahora.AddMinutes(-5)
                : ahora.AddMinutes(15),
            signingCredentials: credenciales
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Configuración JWT utilizada únicamente durante los tests.
        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = JwtTestKey,
                    ["Jwt:Issuer"] = "CorporateAutomation.Api.Tests",
                    ["Jwt:Audience"] = "CorporateAutomation.Tests",
                    ["Auth:DevelopmentTokenPassword"] = DevelopmentTokenTestPassword
                }
            );
        });

        builder.ConfigureTestServices(services =>
        {
            // Eliminamos la configuración de SQL Server.
            services.RemoveAll<
                IDbContextOptionsConfiguration<CorporateAutomationContext>>();

            services.RemoveAll<
                DbContextOptions<CorporateAutomationContext>>();

            // Creamos una conexión SQLite temporal en memoria.
            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection(
                    "Data Source=:memory:"
                );

                connection.Open();

                return connection;
            });

            // Sustituimos SQL Server por SQLite en las pruebas.
            services.AddDbContext<CorporateAutomationContext>(
                (serviceProvider, options) =>
                {
                    var connection = serviceProvider
                        .GetRequiredService<DbConnection>();

                    options.UseSqlite(connection);
                }
            );
        });
    }
}
