using Microsoft.EntityFrameworkCore;

namespace CorporateAutomation.App;

public class CorporateAutomationContext : DbContext
{
    public DbSet<Tarea> Tareas { get; set; }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder
    )
    {
        optionsBuilder.UseSqlServer(
            @"Server=.\SQLEXPRESS;
            Database=CorporateAutomation;
            Trusted_Connection=True;
            TrustServerCertificate=True;"
        );
    }
}
