using ProjetoTarefas.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationLogging();

try
{
    builder.Services
        .AddApplicationDatabase(builder.Configuration, builder.Environment)
        .AddApplicationServices(builder.Environment)
        .AddApplicationSwagger()
        .AddApplicationHealthChecks();

    var app = builder.Build();

    app.UseApplicationSwagger();
    app.UseApplicationRequestLogging();
    app.UseApplicationExceptionHandler();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Iniciando a aplicação ProjetoTarefas");
    app.Run();
}
catch (HostAbortedException)
{
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação foi encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
