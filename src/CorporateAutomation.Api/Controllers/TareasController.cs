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


    [HttpGet("{id:int}")]
    public ActionResult<Tarea> GetById(int id)
    {
        using CorporateAutomationContext db = new();

        Tarea? tarea = db.Tareas.Find(id);

        if (tarea is null)
        {
            return NotFound();
        }

        return Ok(tarea);
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
    [HttpPut("{id:int}")]
    public ActionResult<Tarea> Put(
    int id,
    ActualizarTareaRequest request
)
    {
        using CorporateAutomationContext db = new();

        Tarea? tarea = db.Tareas.Find(id);

        if (tarea is null)
        {
            return NotFound();
        }

        tarea.Nombre = request.Nombre;
        tarea.Estado = request.Estado;
        tarea.Prioridad = request.Prioridad;

        db.SaveChanges();

        return Ok(tarea);
    }
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        using CorporateAutomationContext db = new();

        Tarea? tarea = db.Tareas.Find(id);

        if (tarea is null)
        {
            return NotFound();
        }

        db.Tareas.Remove(tarea);
        db.SaveChanges();

        return NoContent();
    }
}