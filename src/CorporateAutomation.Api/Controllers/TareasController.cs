
using CorporateAutomation.App;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CorporateAutomation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TareasController : ControllerBase
{
    private readonly CorporateAutomationContext _db;
    private readonly ILogger<TareasController> _logger;

    public TareasController(
        CorporateAutomationContext db,
        ILogger<TareasController> logger)
    {
        _db = db;
        _logger = logger;
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
            _logger.LogDebug(
                "No se encontró la tarea {TareaId}", id);

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

        _logger.LogInformation(
            "Tarea {TareaId} creada correctamente",
            nuevaTarea.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = nuevaTarea.Id },
            nuevaTarea
        );
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
            _logger.LogDebug(
                "No se encontró la tarea {TareaId} para actualizar",
                id);

            return NotFound();
        }

        tarea.Nombre = request.Nombre;
        tarea.Estado = request.Estado;
        tarea.Prioridad = request.Prioridad;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Tarea {TareaId} actualizada correctamente",
            id);

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
            _logger.LogDebug(
                "No se encontró la tarea {TareaId} para eliminar",
                id);

            return NotFound();
        }

        _db.Tareas.Remove(tarea);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Tarea {TareaId} eliminada correctamente",
            id);

        return NoContent();
    }
}
