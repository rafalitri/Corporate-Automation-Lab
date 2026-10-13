
namespace CorporateAutomation.Api.Models;

public record ResumenTareasResponse(
    int Total,
    int Pendientes,
    int Completadas,
    int PrioridadBaja,
    int PrioridadMedia,
    int PrioridadAlta
);
