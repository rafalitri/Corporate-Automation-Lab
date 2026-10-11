using CorporateAutomation.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddDbContext<CorporateAutomationContext>(
    (serviceProvider, options) =>
    {
        var configuration = serviceProvider
            .GetRequiredService<IConfiguration>();

        string connectionString =
            configuration.GetConnectionString("CorporateAutomation")
            ?? throw new InvalidOperationException(
                "No se encontrÃ³ la cadena de conexiÃ³n."
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


builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Corporate Automation Lab API";

        document.Info.Description =
            "REST API for managing tasks using C# and .NET 10. " +
            "Supports CRUD operations, filtering, pagination, " +
            "SQL Server persistence, and automated testing.";

        return Task.CompletedTask;
    });
});


var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }