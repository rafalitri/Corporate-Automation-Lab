using CorporateAutomation.App;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CorporateAutomationContext>(
    (serviceProvider, options) =>
    {
        var configuration = serviceProvider
            .GetRequiredService<IConfiguration>();

        string connectionString =
            configuration.GetConnectionString("CorporateAutomation")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión."
            );

        options.UseSqlServer(connectionString);
    }
);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }