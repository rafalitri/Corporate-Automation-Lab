
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CorporateAutomation.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CorporateAutomation.Tests;

public class EditarTareaTests
{
    [Theory]
    [InlineData("Pendiente", 1, 0)]
    [InlineData("Completada", 0, 1)]
    public async Task EditarTarea_ConservaEstadoYActualizaDatos(
        string estadoInicial,
        int pendientesEsperados,
        int completadasEsperadas)
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

        // Crear una tarea con prioridad Media.
        using var respuestaCreacion = await cliente.PostAsJsonAsync(
            "/api/tareas",
            new
            {
                nombre = "Revisar servidores",
                prioridad = "Media"
            }
        );

        Assert.Equal(
            HttpStatusCode.Created,
            respuestaCreacion.StatusCode
        );

        var tareaCreada = await respuestaCreacion.Content
            .ReadFromJsonAsync<JsonElement>();

        int id = tareaCreada.GetProperty("id").GetInt32();

        // Preparar el caso de una tarea completada.
        if (estadoInicial == "Completada")
        {
            using var respuestaCompletar = await cliente.PutAsJsonAsync(
                $"/api/tareas/{id}",
                new
                {
                    nombre = "Revisar servidores",
                    estado = "Completada",
                    prioridad = "Media"
                }
            );

            Assert.Equal(
                HttpStatusCode.OK,
                respuestaCompletar.StatusCode
            );
        }

        // Simular Guardar cambios desde el dashboard.
        using var respuestaEdicion = await cliente.PutAsJsonAsync(
            $"/api/tareas/{id}",
            new
            {
                nombre = "Revisar servidores actualizados",
                estado = estadoInicial,
                prioridad = "Alta"
            }
        );

        Assert.Equal(
            HttpStatusCode.OK,
            respuestaEdicion.StatusCode
        );

        // Consultar la tarea nuevamente para comprobar
        // que los cambios se han guardado.
        using var respuestaConsulta = await cliente.GetAsync(
            $"/api/tareas/{id}"
        );

        Assert.Equal(
            HttpStatusCode.OK,
            respuestaConsulta.StatusCode
        );

        var tareaGuardada = await respuestaConsulta.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Revisar servidores actualizados",
            tareaGuardada.GetProperty("nombre").GetString()
        );

        Assert.Equal(
            "Alta",
            tareaGuardada.GetProperty("prioridad").GetString()
        );

        // El estado debe conservarse después de editar.
        Assert.Equal(
            estadoInicial,
            tareaGuardada.GetProperty("estado").GetString()
        );

        // Comprobar las estadísticas actualizadas.
        using var respuestaResumen = await cliente.GetAsync(
            "/api/tareas/resumen"
        );

        Assert.Equal(
            HttpStatusCode.OK,
            respuestaResumen.StatusCode
        );

        var resumen = await respuestaResumen.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            1,
            resumen.GetProperty("total").GetInt32()
        );

        Assert.Equal(
            pendientesEsperados,
            resumen.GetProperty("pendientes").GetInt32()
        );

        Assert.Equal(
            completadasEsperadas,
            resumen.GetProperty("completadas").GetInt32()
        );

        Assert.Equal(
            1,
            resumen.GetProperty("prioridadAlta").GetInt32()
        );

        Assert.Equal(
            0,
            resumen.GetProperty("prioridadMedia").GetInt32()
        );
    }
}
