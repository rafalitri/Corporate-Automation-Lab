
using System.Net;
using System.Net.Http.Json;
using CorporateAutomation.Api.Models;
using CorporateAutomation.Core;
using CorporateAutomation.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CorporateAutomation.Tests;

public class TareasResumenTests
{
    [Fact]
    public async Task GetResumen_SinTareas_DevuelveContadoresACero()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        // Crear la base de datos SQLite temporal.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            db.Database.EnsureCreated();
        }

        // Consultar el nuevo endpoint.
        var response = await client.GetAsync("/api/tareas/resumen");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var resumen = await response.Content
            .ReadFromJsonAsync<ResumenTareasResponse>();

        Assert.NotNull(resumen);

        // Sin tareas, todos los contadores deben ser cero.
        Assert.Equal(0, resumen.Total);
        Assert.Equal(0, resumen.Pendientes);
        Assert.Equal(0, resumen.Completadas);
        Assert.Equal(0, resumen.PrioridadBaja);
        Assert.Equal(0, resumen.PrioridadMedia);
        Assert.Equal(0, resumen.PrioridadAlta);
    }

    [Fact]
    public async Task GetResumen_ConTareas_CalculaEstadisticasCorrectamente()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        // Preparar diferentes tareas de prueba.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            db.Database.EnsureCreated();

            db.Tareas.AddRange(
                new Tarea
                {
                    Nombre = "Revisar servidores",
                    Estado = EstadoTarea.Pendiente,
                    Prioridad = PrioridadTarea.Alta
                },
                new Tarea
                {
                    Nombre = "Actualizar equipos",
                    Estado = EstadoTarea.Pendiente,
                    Prioridad = PrioridadTarea.Media
                },
                new Tarea
                {
                    Nombre = "Comprobar backups",
                    Estado = EstadoTarea.Completada,
                    Prioridad = PrioridadTarea.Alta
                },
                new Tarea
                {
                    Nombre = "Inventario",
                    Estado = EstadoTarea.Pendiente,
                    Prioridad = PrioridadTarea.Baja
                },
                new Tarea
                {
                    Nombre = "Configurar red",
                    Estado = EstadoTarea.Completada,
                    Prioridad = PrioridadTarea.Media
                }
            );

            db.SaveChanges();
        }

        // Consultar las estadísticas mediante HTTP.
        var response = await client.GetAsync("/api/tareas/resumen");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var resumen = await response.Content
            .ReadFromJsonAsync<ResumenTareasResponse>();

        Assert.NotNull(resumen);

        // Verificar los resultados.
        Assert.Equal(5, resumen.Total);

        Assert.Equal(3, resumen.Pendientes);
        Assert.Equal(2, resumen.Completadas);

        Assert.Equal(1, resumen.PrioridadBaja);
        Assert.Equal(2, resumen.PrioridadMedia);
        Assert.Equal(2, resumen.PrioridadAlta);
    }
}
