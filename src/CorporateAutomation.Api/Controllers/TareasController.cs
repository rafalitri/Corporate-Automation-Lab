
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

    // GET: api/tareas
    // Permite filtrar por estado y prioridad.

    [HttpGet]
    public async Task<ActionResult<List<Tarea>>> Get(
        [FromQuery] EstadoTarea? estado,
        [FromQuery] PrioridadTarea? prioridad,
        CancellationToken cancellationToken,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10)
    {
        // Validamos que los parámetros sean correctos.
        if (pagina < 1 || tamanoPagina < 1 || tamanoPagina > 100)
        {
            return BadRequest(
                "La página debe ser mayor que 0 y el tamaño debe estar entre 1 y 100."
            );
        }

        long salto = ((long)pagina - 1) * tamanoPagina;

        if (salto > int.MaxValue)
        {
            return BadRequest("El número de página es demasiado grande.");
        }

        IQueryable<Tarea> consulta = _db.Tareas.AsNoTracking();

        // Filtrar por estado.
        if (estado.HasValue)
        {
            consulta = consulta.Where(
                tarea => tarea.Estado == estado.Value
            );
        }

        // Filtrar por prioridad.
        if (prioridad.HasValue)
        {
            consulta = consulta.Where(
                tarea => tarea.Prioridad == prioridad.Value
            );
        }

        // Informamos del total de tareas que cumplen los filtros.
        int total = await consulta.CountAsync(cancellationToken);

        Response.Headers["X-Total-Count"] = total.ToString();

        // Ordenamos y obtenemos solamente la página solicitada.
        List<Tarea> tareas = await consulta
            .OrderBy(tarea => tarea.Id)
            .Skip((int)salto)
            .Take(tamanoPagina)
            .ToListAsync(cancellationToken);

        return Ok(tareas);
    }


    // GET: api/tareas/1
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
                "No se encontró la tarea {TareaId}",
                id
            );

            return NotFound();
        }

        return Ok(tarea);
    }

    // POST: api/tareas
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
            nuevaTarea.Id
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = nuevaTarea.Id },
            nuevaTarea
        );
    }

    // PUT: api/tareas/1
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
                id
            );

            return NotFound();
        }

        tarea.Nombre = request.Nombre;
        tarea.Estado = request.Estado;
        tarea.Prioridad = request.Prioridad;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Tarea {TareaId} actualizada correctamente",
            id
        );

        return Ok(tarea);
    }

    // DELETE: api/tareas/1
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
                id
            );

            return NotFound();
        }

        _db.Tareas.Remove(tarea);

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Tarea {TareaId} eliminada correctamente",
            id
        );

        return NoContent();
    }
}
