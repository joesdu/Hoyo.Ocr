using EasilyNET.AutoDependencyInjection.Contexts;
using EasilyNET.AutoDependencyInjection.Extensions;
using EasilyNET.AutoDependencyInjection.Modules;
using EasilyNET.Core.Misc;
using EasilyNET.WebCore.Attributes;
using EasilyNET.WebCore.SwaggerFilters;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Hoyo.Ocr;

public class SwaggerModule : AppModule
{
    /**
     * https://github.com/domaindrivendev/Swashbuckle.AspNetCore
     */
    private const string Name = $"{Title}-{Version}";

    private const string Title = "Ocr.Api";
    private const string Version = "v1";
    private static readonly Dictionary<string, string> docsDic = new();
    private static readonly Dictionary<string, string> endPointDic = new();

    public override void ConfigureServices(ConfigureServicesContext context)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        _ = context.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(Name, new()
            {
                Title = Title,
                Version = Version,
                Description = "中国身份证OCR识别(无法保证能正确识别),Console.WriteLine(\"🐂🍺\")"
            });
            var controllers = AssemblyHelper.FindTypesByAttribute<ApiGroupAttribute>();
            foreach (var ctrl in controllers)
            {
                var attr = ctrl.GetCustomAttribute<ApiGroupAttribute>();
                if (attr is null) continue;
                if (docsDic.ContainsKey(attr.Name)) continue;
                _ = docsDic.TryAdd(attr.Name, attr.Description);
                c.SwaggerDoc(attr.Name, new()
                {
                    Title = attr.Title,
                    Version = attr.Version,
                    Description = attr.Description
                });
            }
            var files = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            foreach (var file in files)
            {
                c.IncludeXmlComments(file, true);
            }
            c.DocInclusionPredicate((docName, apiDescription) =>
            {
                //反射拿到值
                var actionList = apiDescription.ActionDescriptor.EndpointMetadata.Where(x => x is ApiGroupAttribute).ToList();
                if (actionList.Count != 0)
                {
                    return actionList.FirstOrDefault() is ApiGroupAttribute attr && attr.Name == docName;
                }
                var not = apiDescription.ActionDescriptor.EndpointMetadata.Where(x => x is not ApiGroupAttribute).ToList();
                return not.Count != 0 && docName == Name;
                //判断是否包含这个分组
            });
            c.AddSecurityDefinition("Bearer", new()
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
            c.OperationFilter<SwaggerAuthorizeFilter>();
            c.DocumentFilter<SwaggerHiddenApiFilter>();
            c.SchemaFilter<SwaggerSchemaFilter>();
        });
    }

    public override void ApplicationInitialization(ApplicationContext context)
    {
        var app = context.GetApplicationBuilder();
        _ = app.UseSwagger().UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"/swagger/{Name}/swagger.json", $"{Title} {Version}");
            var controllers = AssemblyHelper.FindTypesByAttribute<ApiGroupAttribute>();
            foreach (var ctrl in controllers)
            {
                var attr = ctrl.GetCustomAttribute<ApiGroupAttribute>();
                if (attr is null) continue;
                if (endPointDic.ContainsKey(attr.Name)) continue;
                _ = endPointDic.TryAdd(attr.Name, attr.Description);
                c.SwaggerEndpoint($"/swagger/{attr.Name}/swagger.json", $"{attr.Title} {attr.Version}");
            }
        });
    }
}