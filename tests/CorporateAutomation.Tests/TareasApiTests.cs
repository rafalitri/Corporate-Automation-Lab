
using System.Net;
using System.Net.Http.Json;
using CorporateAutomation.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CorporateAutomation.Tests;

public class TareasApiTests
{
    private static void PrepararBaseDeDatos(TareasApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CorporateAutomationContext>();

        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetTareas_Devuelve200()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        var response = await client.GetAsync("/api/tareas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTareaInexistente_Devuelve404()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        var response = await client.GetAsync("/api/tareas/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostTarea_CreaTareaEnBaseDeDatos()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        var nuevaTarea = new
        {
            Nombre = "Revisar copias de seguridad",
            Prioridad = "Alta"
        };

        var response = await client.PostAsJsonAsync(
            "/api/tareas",
            nuevaTarea
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CorporateAutomationContext>();

        var tareaGuardada = Assert.Single(db.Tareas);

        Assert.Equal("Revisar copias de seguridad", tareaGuardada.Nombre);
        Assert.Equal(PrioridadTarea.Alta, tareaGuardada.Prioridad);
        Assert.Equal(EstadoTarea.Pendiente, tareaGuardada.Estado);
    }

    [Fact]
    public async Task PutTarea_ActualizaTareaEnBaseDeDatos()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        int id;

        // Insertamos una tarea inicial.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            var tarea = new Tarea
            {
                Nombre = "Tarea original",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Baja
            };

            db.Tareas.Add(tarea);
            db.SaveChanges();

            id = tarea.Id;
        }

        // Actualizamos la tarea mediante HTTP PUT.
        var cambios = new
        {
            Nombre = "Tarea actualizada",
            Estado = "Completada",
            Prioridad = "Alta"
        };

        var response = await client.PutAsJsonAsync(
            $"/api/tareas/{id}",
            cambios
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Comprobamos los datos guardados.
        using var verificationScope = factory.Services.CreateScope();

        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<CorporateAutomationContext>();

        var tareaActualizada = verificationDb.Tareas.Single(t => t.Id == id);

        Assert.Equal("Tarea actualizada", tareaActualizada.Nombre);
        Assert.Equal(EstadoTarea.Completada, tareaActualizada.Estado);
        Assert.Equal(PrioridadTarea.Alta, tareaActualizada.Prioridad);
    }

    [Fact]
    public async Task DeleteTarea_EliminaTareaDeBaseDeDatos()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        int id;

        // Creamos una tarea que después eliminaremos.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            var tarea = new Tarea
            {
                Nombre = "Tarea para eliminar",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Media
            };

            db.Tareas.Add(tarea);
            db.SaveChanges();

            id = tarea.Id;
        }

        // Eliminamos mediante HTTP DELETE.
        var response = await client.DeleteAsync($"/api/tareas/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Comprobamos que ya no existe.
        using var verificationScope = factory.Services.CreateScope();

        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<CorporateAutomationContext>();

        bool existe = verificationDb.Tareas.Any(t => t.Id == id);

        Assert.False(existe);
    }
}
