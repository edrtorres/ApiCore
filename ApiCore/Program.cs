using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using System;
using ApiCore.Application.Interfaces;
using ApiCore.Infrastructure;
using ApiCore.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;


namespace ApiCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuración de puertos para Railway (Nube) y Local
            var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
            // Forzar la variable de entorno para que Kestrel no intente bindear a 127.0.0.1
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://0.0.0.0:{port}");

            // Configurar Kestrel para escuchar en Any IP usando el puerto proporcionado por el entorno.
            if (int.TryParse(port, out var p))
            {
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(p);
                });
            }
            else
            {
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
            }

            // Agregar servicios
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // --- API Versioning ---
            builder.Services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("x-api-version"),
                    new QueryStringApiVersionReader("api-version")
                );
            });

            // Expose versioned API explorer for Swagger
            builder.Services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // --- Configuración Swagger detallada ---
            // Incluir SHA corto en el título para verificar despliegues
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiCore (deploy: ecf77a8)", Version = "v1" });
            });

            // Configuración CORS (Importante para Railway)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Register HTTP client for Supabase admin interactions (Infrastructure)
            builder.Services.AddHttpClient<ApiCore.Application.Interfaces.ISupabaseClient, ApiCore.Infrastructure.SupabaseAdminClient>(client =>
            {
                var url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? "";
                if (!string.IsNullOrEmpty(url)) client.BaseAddress = new Uri(url);
            });

            // Register Auth services (UseCases)
            builder.Services.AddScoped<ApiCore.Application.Interfaces.IAuthService, ApiCore.Application.UseCases.AuthService>();

            var app = builder.Build();


            // Middleware: habilitar Swagger siempre y exponer UI en la raíz
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"ApiCore {description.GroupName}");
                }
                c.RoutePrefix = string.Empty; // servir la UI en /
            });

            app.UseCors("AllowAll");
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}