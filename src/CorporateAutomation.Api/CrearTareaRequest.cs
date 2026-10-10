using CorporateAutomation.App;

namespace CorporateAutomation.Api;

public class CrearTareaRequest
{
    public string Nombre { get; set; } = "";

    public PrioridadTarea Prioridad { get; set; }
}