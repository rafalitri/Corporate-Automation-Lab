using CorporateAutomation.App;

namespace CorporateAutomation.Tests;

public class TareaServiceTests
{
    [Fact]
    public void BuscarTareaPorId_DevuelveTarea_CuandoExiste()
    {
        TareaService servicio = new();

        List<Tarea> tareas = new()
        {
            new Tarea
            {
                Id = 1,
                Nombre = "Revisar servidor",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Alta
            }
        };

        Tarea? resultado = servicio.BuscarTareaPorId(tareas, 1);

        Assert.NotNull(resultado);
        Assert.Equal("Revisar servidor", resultado.Nombre);
    }


    [Fact]
    public void BuscarTareaPorId_DevuelveNull_CuandoNoExiste()
    {
        TareaService servicio = new();

        List<Tarea> tareas = new()
        {
            new Tarea
            {
                Id = 1,
                Nombre = "Revisar servidor",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Alta
            }
        };

        Tarea? resultado = servicio.BuscarTareaPorId(tareas, 99);

        Assert.Null(resultado);
    }


    [Fact]
    public void ContarTareasPendientes_DevuelveCantidadCorrecta()
    {
        TareaService servicio = new();

        List<Tarea> tareas = new()
        {
            new Tarea
            {
                Id = 1,
                Nombre = "Tarea 1",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Alta
            },

            new Tarea
            {
                Id = 2,
                Nombre = "Tarea 2",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Media
            },

            new Tarea
            {
                Id = 3,
                Nombre = "Tarea 3",
                Estado = EstadoTarea.Completada,
                Prioridad = PrioridadTarea.Baja
            }
        };

        int resultado = servicio.ContarTareasPendientes(tareas);

        Assert.Equal(2, resultado);
    }


    [Fact]
    public void CompletarTarea_CambiaEstado_CuandoExiste()
    {
        TareaService servicio = new();

        List<Tarea> tareas = new()
        {
            new Tarea
            {
                Id = 1,
                Nombre = "Revisar servidor",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Alta
            }
        };

        bool resultado = servicio.CompletarTarea(tareas, 1);

        Assert.True(resultado);
        Assert.Equal(
            EstadoTarea.Completada,
            tareas[0].Estado
        );
    }


    [Fact]
    public void CompletarTarea_DevuelveFalse_CuandoNoExiste()
    {
        TareaService servicio = new();

        List<Tarea> tareas = new()
        {
            new Tarea
            {
                Id = 1,
                Nombre = "Revisar servidor",
                Estado = EstadoTarea.Pendiente,
                Prioridad = PrioridadTarea.Alta
            }
        };

        bool resultado = servicio.CompletarTarea(tareas, 99);

        Assert.False(resultado);
        Assert.Equal(
            EstadoTarea.Pendiente,
            tareas[0].Estado
        );
    }
}