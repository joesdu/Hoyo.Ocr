using EasilyNET.AutoDependencyInjection.Contexts;
using EasilyNET.AutoDependencyInjection.Modules;

namespace Hoyo.Ocr;

/// <inheritdoc />
public class CorsModule : AppModule
{
    //private CorsModule()
    //{
    //    // 使模块在自动注入的时候忽略.
    //    Enable = false;
    //}
    /// <inheritdoc />
    public override void ConfigureServices(ConfigureServicesContext context)
    {
        var config = context.Services.GetConfiguration();
        var allow = config["AllowedHosts"] ?? "*";
        _ = context.Services.AddCors(c => c.AddPolicy("AllowedHosts", s => s.WithOrigins(allow.Split(",")).AllowAnyMethod().AllowAnyHeader()));
    }

    /// <inheritdoc />
    public override void ApplicationInitialization(ApplicationContext context)
    {
        var app = context.GetApplicationBuilder();
        _ = app.UseCors("AllowedHosts");
    }
}