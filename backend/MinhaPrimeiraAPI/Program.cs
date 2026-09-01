using ProjetoTarefas.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationLogging();
builder.AddTechnicalLogStorage();

try
{
    builder.Services
        .AddApplicationDatabase(builder.Configuration, builder.Environment)
        .AddApplicationServices()
        .AddApplicationSwagger()
        .AddApplicationHealthChecks();

    var app = builder.Build();

    app.UseApplicationSwagger();
    app.UseApplicationRequestLogging();
    app.UseApplicationExceptionHandler();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseTechnicalLogContext();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Iniciando a aplicação ProjetoTarefas");
    app.Run();
}
catch (HostAbortedException)
{
    // As ferramentas do EF Core interrompem o host depois de obter os serviços de design time.
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação foi encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
