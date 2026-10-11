
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CorporateAutomation.App;

public class CorporateAutomationContext : DbContext
{
    public DbSet<Tarea> Tareas { get; set; }

    public CorporateAutomationContext()
    {
    }

    public CorporateAutomationContext(
        DbContextOptions<CorporateAutomationContext> options
    ) : base(options)
    {
    }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder
    )
    {
        // Si la API o los tests ya han configurado la base
        // de datos, respetamos esa configuración.
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        // La aplicación de consola obtiene su conexión
        // desde User Secrets.
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<CorporateAutomationContext>(optional: true)
            .Build();

        string connectionString =
            configuration.GetConnectionString("CorporateAutomation")
            ?? throw new InvalidOperationException(
                "No se encontró la conexión a SQL Server en User Secrets."
            );

        optionsBuilder.UseSqlServer(connectionString);
    }
}
