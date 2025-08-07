using CryptoMonitoring.ReportGenerator.Data;
using CryptoMonitoring.ReportGenerator.Services;
using CryptoMonitoring.ReportGenerator.Builders;
using Microsoft.EntityFrameworkCore;
using RazorLight;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration);
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

Log.Information("ReportGenerator is starting up");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IRazorLightEngine>(new RazorLightEngineBuilder()
    .UseEmbeddedResourcesProject(typeof(ReportService))
    .UseMemoryCachingProvider()
    .Build());

builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportBuilder, DailyReportBuilder>();
builder.Services.AddScoped<IReportBuilder, TechnicalReportBuilder>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/plain";

        var feature = context.Features.Get<IExceptionHandlerFeature>();
        await context.Response.WriteAsync(feature?.Error.ToString() ?? "Unknown error");
    });
});

app.UseSerilogRequestLogging(); 


app.MapGet("/ping", () => Results.Ok("pong"));



app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
