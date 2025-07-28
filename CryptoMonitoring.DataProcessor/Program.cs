using Serilog;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CryptoMonitoring.DataProcessor.Services;
using CryptoMonitoring.DataProcessor;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddSingleton<MarketDataStorage>();
builder.Services.AddControllers();

builder.Services.AddSingleton<IConnection>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>().GetSection("RabbitMq");
    var host = cfg["Host"] ?? "rabbitmq";
    var user = cfg["User"] ?? "guest";
    var pass = cfg["Password"] ?? "guest";

    var factory = new ConnectionFactory
    {
        HostName = host,
        UserName = user,
        Password = pass,
        DispatchConsumersAsync = true
    };

    IConnection? connection = null;
    for (int attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            connection = factory.CreateConnection();
            Log.Information("✔ RabbitMQ connected on attempt #{Attempt}", attempt);
            break;
        }
        catch (Exception ex)
        {
            Log.Warning("⚠ Failed to connect to RabbitMQ (Attempt #{Attempt}): {Message}", attempt, ex.Message);
            Thread.Sleep(2000);
        }
    }

    return connection ?? throw new InvalidOperationException("RabbitMQ connection failed");
});

builder.Services.AddSingleton<IModel>(sp =>
{
    var channel = sp.GetRequiredService<IConnection>().CreateModel();
    channel.QueueDeclare(
        queue: "market.prices",
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: null);
    return channel;
});

builder.Services.AddDbContext<PostgresDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql")));

builder.Services.AddSingleton<MarketDataStorage>();
builder.Services.AddSingleton<MarketDataValidator>();
builder.Services.AddSingleton<MarketDataEnricher>();
builder.Services.AddSingleton<AnomalyDetector>();
builder.Services.AddSingleton<RedisCacheService>();
builder.Services.AddHostedService<MarketMessageConsumer>();

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("DataProcessor OK"));
app.Run();
