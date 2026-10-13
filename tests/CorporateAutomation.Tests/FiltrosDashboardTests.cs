
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CorporateAutomation.Core;
using CorporateAutomation.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CorporateAutomation.Tests;

public class FiltrosDashboardTests
{
    [Theory]
    [InlineData("Pendiente", "Alta", "Pendiente alta")]
    [InlineData("Completada", "Media", "Completada media")]
    [InlineData("Pendiente", "Media", "")]
    public async Task GetTareas_CombinaFiltros_SinAlterarEstadisticas(
        string estado,
        string prioridad,
        string nombreEsperado)
    {
        using var factory = new TareasApiFactory();
        using var cliente = factory.CreateClient();

        // Preparar una base de datos aislada para cada prueba.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            await db.Database.EnsureCreatedAsync();

            db.Tareas.AddRange(
                new Tarea
                {
                    Nombre = "Pendiente alta",
                    Estado = EstadoTarea.Pendiente,
                    Prioridad = PrioridadTarea.Alta
                },
                new Tarea
                {
                    Nombre = "Pendiente baja",
                    Estado = EstadoTarea.Pendiente,
                    Prioridad = PrioridadTarea.Baja
                },
                new Tarea
                {
                    Nombre = "Completada media",
                    Estado = EstadoTarea.Completada,
                    Prioridad = PrioridadTarea.Media
                },
                new Tarea
                {
                    Nombre = "Completada alta",
                    Estado = EstadoTarea.Completada,
                    Prioridad = PrioridadTarea.Alta
                }
            );

            await db.SaveChangesAsync();
        }

        // Aplicar los filtros combinados mediante HTTP.
        var url = $"/api/tareas?estado={estado}&prioridad={prioridad}";

        using var respuesta = await cliente.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        var tareas = await respuesta.Content
            .ReadFromJsonAsync<List<JsonElement>>();

        Assert.NotNull(tareas);

        int cantidadEsperada = string.IsNullOrEmpty(nombreEsperado)
            ? 0
            : 1;

        // Verificar también el contador de resultados filtrados.
        string totalFiltrado = respuesta.Headers
            .GetValues("X-Total-Count")
            .Single();

        Assert.Equal(
            cantidadEsperada.ToString(),
            totalFiltrado
        );

        if (cantidadEsperada == 0)
        {
            Assert.Empty(tareas);
        }
        else
        {
            var tarea = Assert.Single(tareas);

            Assert.Equal(
                nombreEsperado,
                tarea.GetProperty("nombre").GetString()
            );

            Assert.Equal(
                estado,
                tarea.GetProperty("estado").GetString()
            );

            Assert.Equal(
                prioridad,
                tarea.GetProperty("prioridad").GetString()
            );
        }

        // Las estadísticas generales deben conservar las 4 tareas,
        // aunque la consulta anterior solo muestre 0 o 1.
        using var respuestaResumen = await cliente.GetAsync(
            "/api/tareas/resumen"
        );

        Assert.Equal(
            HttpStatusCode.OK,
            respuestaResumen.StatusCode
        );

        using var documento = JsonDocument.Parse(
            await respuestaResumen.Content.ReadAsStringAsync()
        );

        var resumen = documento.RootElement;

        Assert.Equal(4, resumen.GetProperty("total").GetInt32());
        Assert.Equal(2, resumen.GetProperty("pendientes").GetInt32());
        Assert.Equal(2, resumen.GetProperty("completadas").GetInt32());
        Assert.Equal(1, resumen.GetProperty("prioridadBaja").GetInt32());
        Assert.Equal(1, resumen.GetProperty("prioridadMedia").GetInt32());
        Assert.Equal(2, resumen.GetProperty("prioridadAlta").GetInt32());
    }
}
