using System.Data.Common;
using CorporateAutomation.App;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CorporateAutomation.Tests;

public class TareasApiFactory : WebApplicationFactory<Program>
{
    public TareasApiFactory()
    {
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Quitamos la configuración de SQL Server de la API.
            services.RemoveAll<
                IDbContextOptionsConfiguration<CorporateAutomationContext>>();

            services.RemoveAll<
                DbContextOptions<CorporateAutomationContext>>();

            // Creamos una conexión SQLite temporal.
            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection("Data Source=:memory:");
                connection.Open();

                return connection;
            });

            // Los tests utilizarán SQLite en lugar de SQL Server.
            services.AddDbContext<CorporateAutomationContext>(
                (serviceProvider, options) =>
                {
                    var connection =
                        serviceProvider.GetRequiredService<DbConnection>();

                    options.UseSqlite(connection);
                });
        });
    }
}