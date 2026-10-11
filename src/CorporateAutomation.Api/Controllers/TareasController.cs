using CorporateAutomation.App;
using Microsoft.AspNetCore.Mvc;

namespace CorporateAutomation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TareasController : ControllerBase
{
    private readonly CorporateAutomationContext _db;

    public TareasController(CorporateAutomationContext db)
    {
        _db = db;
    }


    [HttpGet]
    public ActionResult<List<Tarea>> Get()
    {
        List<Tarea> tareas = _db.Tareas.ToList();

        return Ok(tareas);
    }


    [HttpGet("{id:int}")]
    public ActionResult<Tarea> GetById(int id)
    {
        Tarea? tarea = _db.Tareas.Find(id);

        if (tarea is null)
        {
            return NotFound();
        }

        return Ok(tarea);
    }


    [HttpPost]
    public ActionResult<Tarea> Post(CrearTareaRequest request)
    {
        Tarea nuevaTarea = new()
        {
            Nombre = request.Nombre,
            Estado = EstadoTarea.Pendiente,
            Prioridad = request.Prioridad
        };

        _db.Tareas.Add(nuevaTarea);
        _db.SaveChanges();

        return Ok(nuevaTarea);
    }


    [HttpPut("{id:int}")]
    public ActionResult<Tarea> Put(
        int id,
        ActualizarTareaRequest request
    )
    {
        Tarea? tarea = _db.Tareas.Find(id);

        if (tarea is null)
        {
            return NotFound();
        }

        tarea.Nombre = request.Nombre;
        tarea.Estado = request.Estado;
        tarea.Prioridad = request.Prioridad;

        _db.SaveChanges();

        return Ok(tarea);
    }


    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        Tarea? tarea = _db.Tareas.Find(id);

        if (tarea is null)
        {
            return NotFound();
        }

        _db.Tareas.Remove(tarea);
        _db.SaveChanges();

        return NoContent();
    }
}