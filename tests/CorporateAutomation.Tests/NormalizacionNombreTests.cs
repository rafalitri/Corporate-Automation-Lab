
using System.Net;
using System.Net.Http.Json;
using CorporateAutomation.Core;
using CorporateAutomation.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CorporateAutomation.Tests;

public class NormalizacionNombreTests
{
    [Fact]
    public async Task PostTarea_EliminaEspaciosDelNombre()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            db.Database.EnsureCreated();
        }

        var nuevaTarea = new
        {
            Nombre = "   Revisar servidor   ",
            Prioridad = "Alta"
        };

        var response = await client.PostAsJsonAsync(
            "/api/tareas",
            nuevaTarea
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scopeVerificacion = factory.Services.CreateScope();

        var dbVerificacion = scopeVerificacion.ServiceProvider
            .GetRequiredService<CorporateAutomationContext>();

        var tarea = Assert.Single(dbVerificacion.Tareas);

        Assert.Equal("Revisar servidor", tarea.Nombre);
    }

    [Fact]
    public async Task PutTarea_EliminaEspaciosDelNombre()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        int id;

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            db.Database.EnsureCreated();

            var tarea = new Tarea
            {
                Nombre = "Nombre original",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Media
            };

            db.Tareas.Add(tarea);
            db.SaveChanges();

            id = tarea.Id;
        }

        var cambios = new
        {
            Nombre = "   Nombre actualizado   ",
            Estado = "Completada",
            Prioridad = "Alta"
        };

        var response = await client.PutAsJsonAsync(
            $"/api/tareas/{id}",
            cambios
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scopeVerificacion = factory.Services.CreateScope();

        var dbVerificacion = scopeVerificacion.ServiceProvider
            .GetRequiredService<CorporateAutomationContext>();

        var tareaGuardada = dbVerificacion.Tareas
            .AsNoTracking()
            .Single(t => t.Id == id);

        Assert.Equal("Nombre actualizado", tareaGuardada.Nombre);
    }
}
