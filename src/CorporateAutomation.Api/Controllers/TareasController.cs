using CorporateAutomation.App;
using Microsoft.AspNetCore.Mvc;

namespace CorporateAutomation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TareasController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Tarea>> Get()
    {
        using CorporateAutomationContext db = new();

        List<Tarea> tareas = db.Tareas.ToList();

        return Ok(tareas);
    }


    [HttpPost]
    public ActionResult<Tarea> Post(CrearTareaRequest request)
    {
        using CorporateAutomationContext db = new();

        Tarea nuevaTarea = new()
        {
            Nombre = request.Nombre,
            Estado = EstadoTarea.Pendiente,
            Prioridad = request.Prioridad
        };

        db.Tareas.Add(nuevaTarea);
        db.SaveChanges();

        return Ok(nuevaTarea);
    }
}