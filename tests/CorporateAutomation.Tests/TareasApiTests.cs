
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

    [Fact]
    public async Task PostTarea_NombreVacio_Devuelve400()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        // Intentamos crear una tarea sin nombre.
        var nuevaTarea = new
        {
            Nombre = "",
            Prioridad = "Alta"
        };

        var response = await client.PostAsJsonAsync(
            "/api/tareas",
            nuevaTarea
        );

        // La API debe rechazar la petición.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Comprobamos que no se ha guardado ninguna tarea.
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CorporateAutomationContext>();

        Assert.Empty(db.Tareas);
    }


    [Fact]
    public async Task PostTarea_SinPrioridad_Devuelve400()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        // Enviamos una tarea sin indicar su prioridad.
        var nuevaTarea = new
        {
            Nombre = "Revisar equipos"
        };

        var response = await client.PostAsJsonAsync(
            "/api/tareas",
            nuevaTarea
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // No debe haberse guardado ninguna tarea.
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CorporateAutomationContext>();

        Assert.Empty(db.Tareas);
    }


    [Fact]
    public async Task PutTarea_NombreVacio_Devuelve400()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        int id;

        // Creamos una tarea válida.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            var tarea = new Tarea
            {
                Nombre = "Actualizar servidores",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Alta
            };

            db.Tareas.Add(tarea);
            db.SaveChanges();

            id = tarea.Id;
        }

        // Intentamos actualizarla con un nombre vacío.
        var cambios = new
        {
            Nombre = "",
            Estado = "Completada",
            Prioridad = "Media"
        };

        var response = await client.PutAsJsonAsync(
            $"/api/tareas/{id}",
            cambios
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Comprobamos que la tarea original no ha cambiado.
        using var verificationScope = factory.Services.CreateScope();

        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<CorporateAutomationContext>();

        var tareaGuardada = verificationDb.Tareas.Single(t => t.Id == id);

        Assert.Equal("Actualizar servidores", tareaGuardada.Nombre);
        Assert.Equal(EstadoTarea.Pendiente, tareaGuardada.Estado);
        Assert.Equal(PrioridadTarea.Alta, tareaGuardada.Prioridad);
    }


    [Fact]
    public async Task PostTarea_PrioridadInvalida_Devuelve400()
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        // Intentamos crear una tarea con una prioridad inexistente.
        var nuevaTarea = new
        {
            Nombre = "Revisar servidores",
            Prioridad = 999
        };

        var response = await client.PostAsJsonAsync(
            "/api/tareas",
            nuevaTarea
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Comprobamos que no se ha guardado ninguna tarea.
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CorporateAutomationContext>();

        Assert.Empty(db.Tareas);
    }


    [Theory]
    [InlineData(999, 1)] // Estado inválido
    [InlineData(0, 999)] // Prioridad inválida
    public async Task PutTarea_EnumInvalido_Devuelve400(
        int estado,
        int prioridad)
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        int id;

        // Guardamos una tarea válida.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            var tarea = new Tarea
            {
                Nombre = "Revisar servidores",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Media
            };

            db.Tareas.Add(tarea);
            db.SaveChanges();

            id = tarea.Id;
        }

        // Intentamos actualizarla con valores incorrectos.
        var cambios = new
        {
            Nombre = "Tarea modificada",
            Estado = estado,
            Prioridad = prioridad
        };

        var response = await client.PutAsJsonAsync(
            $"/api/tareas/{id}",
            cambios
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Verificamos que los datos originales siguen intactos.
        using var scopeVerificacion = factory.Services.CreateScope();

        var dbVerificacion = scopeVerificacion.ServiceProvider
            .GetRequiredService<CorporateAutomationContext>();

        var tareaGuardada = dbVerificacion.Tareas.Single(t => t.Id == id);

        Assert.Equal("Revisar servidores", tareaGuardada.Nombre);
        Assert.Equal(EstadoTarea.Pendiente, tareaGuardada.Estado);
        Assert.Equal(PrioridadTarea.Media, tareaGuardada.Prioridad);
    }

}
