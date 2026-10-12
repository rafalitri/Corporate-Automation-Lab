
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CorporateAutomation.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CorporateAutomation.Tests;

public class DevTokenTests
{
    [Fact]
    public async Task Token_ClaveIncorrecta_Devuelve401()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/dev/token",
            new { password = "clave-incorrecta" }
        );

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Token_ClaveCorrecta_PermiteAccederADelete()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        // Solicitamos un JWT con la credencial de pruebas.
        var response = await client.PostAsJsonAsync(
            "/api/dev/token",
            new
            {
                password =
                    TareasApiFactory.DevelopmentTokenTestPassword
            }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var documento = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync()
        );

        string token = documento.RootElement
            .GetProperty("access_token")
            .GetString()!;

        Assert.False(string.IsNullOrWhiteSpace(token));

        // Preparamos la base de datos SQLite de pruebas.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            db.Database.EnsureCreated();
        }

        // Utilizamos el token para acceder a DELETE.
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var deleteResponse = await client.DeleteAsync(
            "/api/tareas/999"
        );

        // 404 indica que hemos pasado la autenticación,
        // pero la tarea 999 no existe.
        Assert.Equal(
            HttpStatusCode.NotFound,
            deleteResponse.StatusCode
        );
    }

    [Fact]
    public async Task Token_EnProduccion_Devuelve404()
    {
        using var factory = new TareasApiFactory();

        // Simulamos la ejecución en Production.
        using var productionFactory = factory.WithWebHostBuilder(
            builder => builder.UseEnvironment("Production")
        );

        using var client = productionFactory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/dev/token",
            new
            {
                password =
                    TareasApiFactory.DevelopmentTokenTestPassword
            }
        );

        // El endpoint de desarrollo no debe existir
        // cuando la API se ejecuta en Production.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }
}
