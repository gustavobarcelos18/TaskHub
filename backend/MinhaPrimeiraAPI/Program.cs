using MinhaPrimeiraAPI.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationLogging();

builder.Services
    .AddApplicationDatabase(builder.Configuration)
    .AddApplicationServices()
    .AddApplicationSwagger()
    .AddApplicationCors(builder.Configuration);

var app = builder.Build();

app.UseApplicationSwagger();
app.UseApplicationRequestLogging();
app.UseApplicationExceptionHandler();
app.UseHttpsRedirection();
app.UseApplicationCors();

app.MapControllers();

try
{
    Log.Information("Iniciando a aplicação MinhaPrimeiraAPI");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação foi encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
