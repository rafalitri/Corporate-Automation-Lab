
using System.Net;

namespace CorporateAutomation.Tests;

public class DashboardTests
{
    [Theory]
    [InlineData("/", "Dashboard | Corporate Automation Lab")]
    [InlineData("/css/dashboard.css", "--background")]
    [InlineData("/js/dashboard.js", "/api/tareas/resumen")]
    public async Task ArchivosDelDashboardSeSirvenCorrectamente(
        string ruta,
        string textoEsperado)
    {
        using var factory = new TareasApiFactory();
        using var cliente = factory.CreateClient();

        using var respuesta = await cliente.GetAsync(ruta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        string contenido = await respuesta.Content.ReadAsStringAsync();

        Assert.Contains(textoEsperado, contenido);
    }
}
