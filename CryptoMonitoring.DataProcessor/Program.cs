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
using StackExchange.Redis;



//var builder = WebApplication.CreateBuilder(args);

//builder.Host.UseSerilog((ctx, lc) => lc
//    .ReadFrom.Configuration(ctx.Configuration)
//    .Enrich.FromLogContext()
//    .WriteTo.Console()
//);

//builder.Configuration.AddEnvironmentVariables();
//builder.Services.AddControllers();

//builder.Services.AddSingleton<IConnection>(sp =>
//{
//    var cfg = sp.GetRequiredService<IConfiguration>().GetSection("RabbitMq");
//    var host = cfg["Host"] ?? "rabbitmq";
//    var user = cfg["User"] ?? "guest";
//    var pass = cfg["Password"] ?? "guest";

//    var factory = new ConnectionFactory
//    {
//        HostName = host,
//        UserName = user,
//        Password = pass,
//        DispatchConsumersAsync = true
//    };

//    IConnection? connection = null;
//    for (int attempt = 1; attempt <= 10; attempt++)
//    {
//        try
//        {
//            connection = factory.CreateConnection();
//            Log.Information("✔ RabbitMQ connected on attempt #{Attempt}", attempt);
//            break;
//        }
//        catch (Exception ex)
//        {
//            Log.Warning("⚠ Failed to connect to RabbitMQ (Attempt #{Attempt}): {Message}", attempt, ex.Message);
//            Thread.Sleep(2000);
//        }
//    }

//    return connection ?? throw new InvalidOperationException("RabbitMQ connection failed");
//});

//builder.Services.AddSingleton<IModel>(sp =>
//{
//    var channel = sp.GetRequiredService<IConnection>().CreateModel();
//    channel.QueueDeclare(
//        queue: "market.prices",
//        durable: true,
//        exclusive: false,
//        autoDelete: false,
//        arguments: null);
//    return channel;
//});

//builder.Services.AddDbContext<PostgresDbContext>(opt =>
//    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


//builder.Services.AddScoped<MarketDataStorage>();
//builder.Services.AddScoped<MarketDataValidator>();
//builder.Services.AddScoped<MarketDataEnricher>();
//builder.Services.AddScoped<AnomalyDetector>();
//builder.Services.AddScoped<RedisCacheService>();
//builder.Services.AddHostedService<MarketMessageConsumer>();

//builder.Services.AddControllers();

//var app = builder.Build();
//app.MapControllers();
//app.MapGet("/health", () => Results.Ok("DataProcessor OK")); using var scope = app.Services.CreateScope();
//var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

//for (int attempt = 1; attempt <= 10; attempt++)
//{
//    try
//    {
//        db.Database.Migrate();
//        Log.Information("✅ Миграции успешно применены");
//        break;
//    }
//    catch (Exception ex) when (attempt < 10)
//    {
//        Log.Warning("❌ Попытка {Attempt}/10 не удалась: {Message}", attempt, ex.Message);
//        Thread.Sleep(2000);
//    }
//}

//app.Run();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var conn = builder.Configuration.GetConnectionString("MongoDb")
               ?? throw new InvalidOperationException("ConnectionStrings:MongoDb not set");
    return new MongoClient(conn);
});

builder.Services.AddDbContext<PostgresDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnection>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>().GetSection("RabbitMq");
    var factory = new ConnectionFactory
    {
        HostName = cfg["Host"] ?? "rabbitmq",
        UserName = cfg["User"] ?? "guest",
        Password = cfg["Password"] ?? "guest",
        DispatchConsumersAsync = true
    };

    for (int i = 1; i <= 10; i++)
    {
        try
        {
            var conn = factory.CreateConnection();
            Log.Information("✔ RabbitMQ connected on attempt #{Attempt}", i);
            return conn;
        }
        catch (Exception ex) when (i < 10)
        {
            Log.Warning("⚠ RabbitMQ connect attempt #{Attempt} failed: {Message}", i, ex.Message);
            Thread.Sleep(2000);
        }
    }

    throw new InvalidOperationException("RabbitMQ connection failed after 10 attempts");
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

builder.Services.AddScoped<MarketDataStorage>();
builder.Services.AddScoped<MarketDataValidator>();
builder.Services.AddScoped<MarketDataEnricher>();
builder.Services.AddScoped<AnomalyDetector>();




builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
  ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddScoped<RedisCacheService>();




builder.Services.AddHostedService<MarketMessageConsumer>();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("DataProcessor OK"));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
    for (int i = 1; i <= 10; i++)
    {
        try
        {
            db.Database.Migrate();
            Log.Information("✅ Миграции успешно применены");
            break;
        }
        catch (Exception ex) when (i < 10)
        {
            Log.Warning("❌ Миграция не удалась (Attempt {Attempt}): {Message}", i, ex.Message);
            Thread.Sleep(2000);
        }
    }
}

app.Run();
