namespace CorporateAutomation.App;

public class Tarea
{
    public int Id { get; set; }

    public string Nombre { get; set; } = "";

    public EstadoTarea Estado { get; set; }

    public PrioridadTarea Prioridad { get; set; }
}
