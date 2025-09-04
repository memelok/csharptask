using CryptoMonitoring.ReportGenerator.Builders;
using CryptoMonitoring.ReportGenerator.Data;
using CryptoMonitoring.ReportGenerator.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;   
using RazorLight;
using Serilog;
using System.IO;
using Microsoft.Extensions.Hosting;

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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine, LogLevel.Information));



builder.Services.AddSingleton<IRazorLightEngine>(sp =>
{
    var env = sp.GetRequiredService<IHostEnvironment>();
    var templatesPath = Path.Combine(env.ContentRootPath, "Templates");

    if (!Directory.Exists(templatesPath))
    {
        Log.Error("Razor templates directory not found: {Path}", templatesPath);
        throw new DirectoryNotFoundException($"Templates directory not found: {templatesPath}");
    }

    Log.Information("Using Razor templates path: {Path}", templatesPath);

    return new RazorLightEngineBuilder()
        .UseFileSystemProject(templatesPath)
        .EnableDebugMode()
        .UseMemoryCachingProvider()
        .Build();
});



builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<IReportBuilder, DailyReportBuilder>();
builder.Services.AddScoped<IReportBuilder, TechnicalReportBuilder>();
builder.Services.AddScoped<IReportBuilder, PortfolioReportBuilder>();
builder.Services.AddScoped<IReportBuilder, VolatilityReportBuilder>();

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
