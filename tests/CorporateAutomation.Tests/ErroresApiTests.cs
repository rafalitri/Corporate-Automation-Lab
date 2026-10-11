
using System.Net;
using System.Text.Json;
using CorporateAutomation.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateAutomation.Tests;

public class ErroresApiTests
{
    [Fact]
    public async Task ErrorDeBaseDeDatos_Devuelve500SinDetallesInternos()
    {
        // Crear una API de pruebas con SQLite en memoria.
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        // Preparar la base de datos de pruebas.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            db.Database.EnsureCreated();

            // Provocar un error solo en SQLite de pruebas.
            db.Database.ExecuteSqlRaw("DROP TABLE Tareas");
        }

        // La API intenta consultar una tabla que ya no existe.
        var response = await client.GetAsync("/api/tareas");

        // Comprobar el código HTTP.
        Assert.Equal(
            HttpStatusCode.InternalServerError,
            response.StatusCode
        );

        // Comprobar el formato de la respuesta.
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType
        );

        string contenido = await response.Content.ReadAsStringAsync();

        using var documento = JsonDocument.Parse(contenido);

        Assert.Equal(
            500,
            documento.RootElement.GetProperty("status").GetInt32()
        );

        // No revelar información interna de la base de datos.
        Assert.DoesNotContain(
            "SQLite",
            contenido,
            StringComparison.OrdinalIgnoreCase
        );

        Assert.DoesNotContain(
            "no such table",
            contenido,
            StringComparison.OrdinalIgnoreCase
        );
    }
}
