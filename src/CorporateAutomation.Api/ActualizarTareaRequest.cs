using CorporateAutomation.App;

namespace CorporateAutomation.Api;

public class ActualizarTareaRequest
{
    public string Nombre { get; set; } = "";

    public EstadoTarea Estado { get; set; }

    public PrioridadTarea Prioridad { get; set; }
}