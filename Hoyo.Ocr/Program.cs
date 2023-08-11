using Hoyo.Ocr;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

//添加Serilog配置
builder.Host.UseSerilog((hbc, lc) =>
{
    const LogEventLevel logLevel = LogEventLevel.Information;
    _ = lc.ReadFrom.Configuration(hbc.Configuration)
          .MinimumLevel.Override("Microsoft", logLevel)
          .MinimumLevel.Override("System", logLevel)
          .Enrich.FromLogContext();
    _ = lc.WriteTo.Async(wt => wt.Console());
    _ = lc.WriteTo.Debug();
    _ = lc.WriteTo.Map(MapData, (key, log) => log.Async(o => o.File(Path.Combine("logs", @$"{key.time:yyyyMMdd}{Path.DirectorySeparatorChar}{key.level.ToString().ToLower()}.log"), logLevel)));
    return;
    static (DateTime time, LogEventLevel level) MapData(LogEvent @event) => (@event.Timestamp.LocalDateTime, @event.Level);
});

// 自动注入服务模块
builder.Services.AddApplication<AppWebModule>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) _ = app.UseDeveloperExceptionPage();

// 添加自动化注入的一些中间件.
app.InitializeApplication();
app.MapControllers();
app.Run();