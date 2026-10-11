
using CorporateAutomation.App;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<List<Tarea>>> Get(
        CancellationToken cancellationToken)
    {
        List<Tarea> tareas = await _db.Tareas
            .ToListAsync(cancellationToken);

        return Ok(tareas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Tarea>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        Tarea? tarea = await _db.Tareas.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (tarea is null)
        {
            return NotFound();
        }

        return Ok(tarea);
    }

    [HttpPost]
    public async Task<ActionResult<Tarea>> Post(
        CrearTareaRequest request,
        CancellationToken cancellationToken)
    {
        Tarea nuevaTarea = new()
        {
            Nombre = request.Nombre,
            Estado = EstadoTarea.Pendiente,
            Prioridad = request.Prioridad
        };

        _db.Tareas.Add(nuevaTarea);

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(nuevaTarea);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Tarea>> Put(
        int id,
        ActualizarTareaRequest request,
        CancellationToken cancellationToken)
    {
        Tarea? tarea = await _db.Tareas.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (tarea is null)
        {
            return NotFound();
        }

        tarea.Nombre = request.Nombre;
        tarea.Estado = request.Estado;
        tarea.Prioridad = request.Prioridad;

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(tarea);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        Tarea? tarea = await _db.Tareas.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (tarea is null)
        {
            return NotFound();
        }

        _db.Tareas.Remove(tarea);

        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
