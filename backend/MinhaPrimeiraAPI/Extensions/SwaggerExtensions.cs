using System.Reflection;
using Microsoft.OpenApi;

namespace ProjetoTarefas.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddApplicationSwagger(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ProjetoTarefas",
                Version = "v1",
                Description =
                    "Web API para gerenciamento de tarefas, com criação, " +
                    "consulta, atualização, exclusão lógica e exclusão permanente."
            });

            var nomeArquivoXml =
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

            var caminhoArquivoXml = Path.Combine(
                AppContext.BaseDirectory,
                nomeArquivoXml
            );

            if (File.Exists(caminhoArquivoXml))
            {
                options.IncludeXmlComments(caminhoArquivoXml);
            }
        });

        return services;
    }

    public static WebApplication UseApplicationSwagger(
        this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "ProjetoTarefas v1"
            );

            options.DocumentTitle =
                "ProjetoTarefas - Documentação";
        });

        return app;
    }
}
