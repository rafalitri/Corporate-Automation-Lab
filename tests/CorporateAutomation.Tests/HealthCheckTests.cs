
using System.Data.Common;
using System.Net;
using CorporateAutomation.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CorporateAutomation.Tests;

public class HealthCheckTests
{
    [Fact]
    public async Task Health_BaseDeDatosDisponible_Devuelve200()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        // Preparamos SQLite en memoria.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            db.Database.EnsureCreated();
        }

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string contenido = await response.Content.ReadAsStringAsync();

        Assert.Equal("Healthy", contenido);
    }

    [Fact]
    public async Task Health_BaseDeDatosNoDisponible_Devuelve503()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        // Obtenemos la conexión SQLite exclusiva de esta prueba.
        var conexion = factory.Services
            .GetRequiredService<DbConnection>();

        conexion.Close();

        // Simulamos una base de datos que no existe.
        // Mode=ReadOnly impide que SQLite la cree.
        string rutaInexistente = Path.Combine(
            Path.GetTempPath(),
            $"health-inexistente-{Guid.NewGuid():N}.db"
        );

        conexion.ConnectionString =
            $"Data Source={rutaInexistente};Mode=ReadOnly";

        // Consultamos el endpoint de salud.
        var response = await client.GetAsync("/health");

        Assert.Equal(
            HttpStatusCode.ServiceUnavailable,
            response.StatusCode
        );

        string contenido = await response.Content.ReadAsStringAsync();

        Assert.Equal("Unhealthy", contenido);
    }
}
