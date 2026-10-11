
using System.Net;
using System.Net.Http.Json;
using CorporateAutomation.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Text.Json;

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

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.NotNull(response.Headers.Location);

        var consulta = await client.GetAsync(response.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);

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


    [Theory]
    [InlineData("estado=Pendiente", "Pendiente alta|Pendiente baja")]
    [InlineData("prioridad=Alta", "Pendiente alta|Completada alta")]
    [InlineData("estado=Pendiente&prioridad=Alta", "Pendiente alta")]
    public async Task GetTareas_FiltraPorEstadoYPrioridad(
        string filtro,
        string nombresEsperados)
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        // Creamos tres tareas para comprobar los filtros.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

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
                    Nombre = "Completada alta",
                    Estado = EstadoTarea.Completada,
                    Prioridad = PrioridadTarea.Alta
                }
            );

            db.SaveChanges();
        }

        // Consultamos la API utilizando los filtros.
        var response = await client.GetAsync(
            $"/api/tareas?{filtro}"
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var resultado = await response.Content
            .ReadFromJsonAsync<List<JsonElement>>();

        Assert.NotNull(resultado);

        // Comprobamos que devuelve exactamente las tareas esperadas.
        string[] obtenidos = resultado
            .Select(tarea => tarea.GetProperty("nombre").GetString()!)
            .OrderBy(nombre => nombre)
            .ToArray();

        string[] esperados = nombresEsperados
            .Split('|')
            .OrderBy(nombre => nombre)
            .ToArray();

        Assert.Equal(esperados, obtenidos);
    }


    [Theory]
    [InlineData(1, 2, "Tarea 1|Tarea 2")]
    [InlineData(2, 2, "Tarea 3|Tarea 4")]
    [InlineData(3, 2, "Tarea 5")]
    [InlineData(4, 2, "")]
    public async Task GetTareas_PaginacionCorrecta(
        int pagina,
        int tamanoPagina,
        string nombresEsperados)
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        // Guardamos cinco tareas.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CorporateAutomationContext>();

            for (int i = 1; i <= 5; i++)
            {
                db.Tareas.Add(new Tarea
                {
                    Nombre = $"Tarea {i}",
                    Estado = EstadoTarea.Pendiente,
                    Prioridad = PrioridadTarea.Media
                });
            }

            db.SaveChanges();
        }

        // Solicitamos la página indicada.
        var response = await client.GetAsync(
            $"/api/tareas?pagina={pagina}&tamanoPagina={tamanoPagina}"
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // El total debe seguir siendo cinco.
        string total = response.Headers
            .GetValues("X-Total-Count")
            .Single();

        Assert.Equal("5", total);

        // Comprobamos las tareas devueltas.
        var resultado = await response.Content
            .ReadFromJsonAsync<List<JsonElement>>();

        Assert.NotNull(resultado);

        string[] obtenidos = resultado
            .Select(t => t.GetProperty("nombre").GetString()!)
            .ToArray();

        string[] esperados = nombresEsperados.Split(
            '|',
            StringSplitOptions.RemoveEmptyEntries
        );

        Assert.Equal(esperados, obtenidos);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public async Task GetTareas_PaginacionInvalida_Devuelve400(
        int pagina,
        int tamanoPagina)
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        PrepararBaseDeDatos(factory);

        var response = await client.GetAsync(
            $"/api/tareas?pagina={pagina}&tamanoPagina={tamanoPagina}"
        );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }

}
