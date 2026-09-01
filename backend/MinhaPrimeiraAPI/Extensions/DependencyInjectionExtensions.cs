using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjetoTarefas.Data;
using ProjetoTarefas.Models;
using ProjetoTarefas.Repositories;
using ProjetoTarefas.Services;

namespace ProjetoTarefas.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
        services.AddApplicationProblemDetails();
        services.AddHttpContextAccessor();

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__Host-taskhub-antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.Path = "/";
        });

        services.AddIdentity<Usuario, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "__Host-taskhub";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.Path = "/";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddAuthorization();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<HealthDiagnosticsService>();
        services.AddSingleton<LogService>();
        services.AddSingleton<ILogRepository>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var dataSource = configuration["TechnicalLogging:ConnectionString"] ?? "Data Source=Database/logs.db";
            if (dataSource.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
                dataSource = dataSource["Data Source=".Length..];
            if (!Path.IsPathFullyQualified(dataSource))
                dataSource = Path.Combine(provider.GetRequiredService<IHostEnvironment>().ContentRootPath, dataSource);
            return new LogRepository(new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = dataSource }.ToString());
        });

        services.AddScoped<ITarefaRepository, TarefaRepository>();
        services.AddScoped<ITarefaService, TarefaService>();
        services.AddScoped<IEtiquetaRepository, EtiquetaRepository>();
        services.AddScoped<IEtiquetaService, EtiquetaService>();
        services.AddScoped<IProjetoRepository, ProjetoRepository>();
        services.AddScoped<IProjetoService, ProjetoService>();
        services.AddScoped<IUsuarioAtual, UsuarioAtual>();

        return services;
    }
}
