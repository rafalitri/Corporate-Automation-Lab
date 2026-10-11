using CorporateAutomation.Core;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CorporateAutomation.Api;

public class CrearTareaRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = "";

    [JsonRequired]
    [EnumDataType(typeof(PrioridadTarea),
        ErrorMessage = "La prioridad no es valida.")]
    public PrioridadTarea Prioridad { get; set; }
}
