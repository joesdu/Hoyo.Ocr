using EasilyNET.AutoDependencyInjection.Contexts;
using EasilyNET.AutoDependencyInjection.Modules;
using Microsoft.OpenApi.Models;

namespace Hoyo.Ocr;

public class SwaggerModule : AppModule
{
    /**
     * https://github.com/domaindrivendev/Swashbuckle.AspNetCore
     */
    private const string name = $"{title}-{version}";

    private const string title = "Ocr.Api";
    private const string version = "v1";

    public override void ConfigureServices(ConfigureServicesContext context)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        _ = context.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(name, new()
            {
                Title = title,
                Version = version,
                Description = "中国身份证OCR识别(无法保证能正确识别),Console.WriteLine(\"🐂🍺\")"
            });
            c.EasilySwaggerGenOptions(name);
            c.AddSecurityDefinition("Bearer", new()
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
        });
    }

    public override void ApplicationInitialization(ApplicationContext context)
    {
        var app = context.GetApplicationBuilder();
        _ = app.UseSwagger().UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"/swagger/{name}/swagger.json", $"{title} {version}");
            c.EasilySwaggerUIOptions();
        });
    }
}