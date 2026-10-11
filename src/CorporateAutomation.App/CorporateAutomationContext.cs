using Microsoft.EntityFrameworkCore;

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
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                @"Server=.\SQLEXPRESS;
                Database=CorporateAutomation;
                Trusted_Connection=True;
                TrustServerCertificate=True;"
            );
        }
    }
}