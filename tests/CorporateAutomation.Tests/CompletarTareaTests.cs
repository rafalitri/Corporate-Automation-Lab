
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CorporateAutomation.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CorporateAutomation.Tests;

public class CompletarTareaTests
{
    [Fact]
    public async Task CompletarTarea_ActualizaEstadoYEstadisticas()
    {
        using var factory = new TareasApiFactory();
        using var cliente = factory.CreateClient();

        // Preparar una base de datos SQLite aislada.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            db.Database.EnsureCreated();
        }

        // Crear una tarea pendiente mediante la API.
        using var respuestaCreacion = await cliente.PostAsJsonAsync(
            "/api/tareas",
            new
            {
                nombre = "Comprobar Docker",
                prioridad = "Alta"
            }
        );

        Assert.Equal(
            HttpStatusCode.Created,
            respuestaCreacion.StatusCode
        );

        var tareaCreada = await respuestaCreacion.Content
            .ReadFromJsonAsync<JsonElement>();

        int id = tareaCreada.GetProperty("id").GetInt32();

        Assert.Equal(
            "Pendiente",
            tareaCreada.GetProperty("estado").GetString()
        );

        // Simular la petición enviada por el botón Completar.
        using var respuestaActualizacion = await cliente.PutAsJsonAsync(
            $"/api/tareas/{id}",
            new
            {
                nombre = "Comprobar Docker",
                estado = "Completada",
                prioridad = "Alta"
            }
        );

        Assert.Equal(
            HttpStatusCode.OK,
            respuestaActualizacion.StatusCode
        );

        var tareaActualizada = await respuestaActualizacion.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Completada",
            tareaActualizada.GetProperty("estado").GetString()
        );

        Assert.Equal(
            "Comprobar Docker",
            tareaActualizada.GetProperty("nombre").GetString()
        );

        Assert.Equal(
            "Alta",
            tareaActualizada.GetProperty("prioridad").GetString()
        );

        // Comprobar que las estadísticas se han actualizado.
        using var respuestaResumen = await cliente.GetAsync(
            "/api/tareas/resumen"
        );

        Assert.Equal(
            HttpStatusCode.OK,
            respuestaResumen.StatusCode
        );

        var resumen = await respuestaResumen.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(1, resumen.GetProperty("total").GetInt32());
        Assert.Equal(0, resumen.GetProperty("pendientes").GetInt32());
        Assert.Equal(1, resumen.GetProperty("completadas").GetInt32());
        Assert.Equal(1, resumen.GetProperty("prioridadAlta").GetInt32());
    }
}
