using EasilyNET.AutoDependencyInjection.Attributes;
using EasilyNET.AutoDependencyInjection.Contexts;
using EasilyNET.AutoDependencyInjection.Modules;
using EasilyNET.WebCore.Filters;
using EasilyNET.WebCore.JsonConverters;
using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global

namespace Hoyo.Ocr;

/**
 * 要实现自动注入,一定要在这个地方添加
 */
[DependsOn(typeof(DependencyAppModule),
    typeof(CorsModule),
    typeof(SwaggerModule),
    typeof(OcrServerModule))]
public class AppWebModule : AppModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ConfigureServicesContext context)
    {
        context.Services.AddControllers(c =>
        {
            _ = c.Filters.Add<ActionExecuteFilter>();
            _ = c.Filters.Add<ExceptionFilter>();
        }).AddJsonOptions(c =>
        {
            c.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
            c.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
            c.JsonSerializerOptions.Converters.Add(new TimeOnlyNullJsonConverter());
            c.JsonSerializerOptions.Converters.Add(new DateOnlyNullJsonConverter());
            c.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
            c.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        context.Services.AddEndpointsApiExplorer();
        context.Services.AddHttpContextAccessor();
        base.ConfigureServices(context);
    }

    /// <inheritdoc />
    public override void ApplicationInitialization(ApplicationContext context)
    {
        var app = context.GetApplicationBuilder();
        app.UseResponseTime();
        app.UseAuthorization();
        base.ApplicationInitialization(context);
    }
}