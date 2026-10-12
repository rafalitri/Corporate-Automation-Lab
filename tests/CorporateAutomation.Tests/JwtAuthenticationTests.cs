
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace CorporateAutomation.Tests;

public class JwtAuthenticationTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task DeleteTarea_TokenInvalido_Devuelve401(
        bool expirado,
        bool firmaIncorrecta)
    {
        using var factory = new TareasApiFactory();
        using var client = factory.CreateClient();

        // Generamos un token caducado o con firma incorrecta.
        string token = factory.CrearTokenDePrueba(
            expirado,
            firmaIncorrecta
        );

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token
            );

        var response = await client.DeleteAsync("/api/tareas/999");

        // La API debe rechazar ambos tipos de token.
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode
        );
    }
}
