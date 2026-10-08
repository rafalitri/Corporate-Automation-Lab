namespace CorporateAutomation.App;

public class TareaService
{
    public Tarea? BuscarTareaPorId(List<Tarea> tareas, int id)
    {
        foreach (Tarea tarea in tareas)
        {
            if (tarea.Id == id)
            {
                return tarea;
            }
        }

        return null;
    }

    public int ContarTareasPendientes(List<Tarea> tareas)
    {
        int cantidad = 0;

        foreach (Tarea tarea in tareas)
        {
            if (tarea.Estado == EstadoTarea.Pendiente)
            {
                cantidad++;
            }
        }

        return cantidad;
    }

    public bool CompletarTarea(List<Tarea> tareas, int id)
    {
        Tarea? tarea = BuscarTareaPorId(tareas, id);

        if (tarea is null)
        {
            return false;
        }

        tarea.Estado = EstadoTarea.Completada;

        return true;
    }
}