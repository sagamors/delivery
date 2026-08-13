using System.Reflection;
using AutoMapper;
using Clients.Geo;
using Confluent.Kafka;
using Ddd;
using DeliveryApp.Api;
using DeliveryApp.Api.Adapters.BackgroundJobs;
using DeliveryApp.Api.Adapters.Kafka.BasketEvents;
using DeliveryApp.Core;
using DeliveryApp.Core.Application.EventHandlers;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using DeliveryApp.Infrastructure.Adapters.Grpc.GeoService;
using DeliveryApp.Infrastructure.Adapters.Kafka;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using MediatR;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Quartz;

// Для локальной разработки Geo-сервис обычно поднят по обычному http:// (без TLS)
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// AutoMapper
builder.Services.AddSingleton<IMapper>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    var assemblies = AppDomain.CurrentDomain.GetAssemblies();

    var config = new MapperConfiguration(
        cfg => { cfg.AddMaps(assemblies); },
        loggerFactory
    );

    config.AssertConfigurationIsValid();
    return config.CreateMapper();
});

// Health Checks
builder.Services.AddHealthChecks();

// MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(GetAllCouriersHandler).Assembly));

// Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin() // Не делайте так в проде!
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Configuration
builder.Services.ConfigureOptions<SettingsSetup>();
var connectionString = builder.Configuration["CONNECTION_STRING"];

// Domain Services
builder.Services.AddTransient<IOrderDispatcher, OrderDispatcher>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        connectionString,
        sql => sql.MigrationsAssembly("DeliveryApp.Infrastructure")
    );
    options.EnableSensitiveDataLogging();
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICourierRepository, CourierRepository>();

// 9 модуль: Geo Service (gRPC Client)
builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<Settings>>().Value;
    var channel = GrpcChannel.ForAddress(settings.GeoServiceGrpcHost);
    return new Geo.GeoClient(channel);
});
builder.Services.AddScoped<IGeoService, GeoService>();

// 10 модуль
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior =
        BackgroundServiceExceptionBehavior.StopHost;
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHostedService<ConsumerService>();

builder.Services.AddSingleton(di=> new ProducerBuilder<string, byte[]>(new ProducerConfig
        {
            BootstrapServers = di.GetRequiredService<IOptions<Settings>>().Value.MessageBrokerHost
        }).Build());

builder.Services.AddScoped<IOrderEventsProducer, OrderEventsProducer>();

// CRON Jobs
builder.Services.AddQuartz(configure =>
{
    var assignOrdersJobKey = new JobKey(nameof(AssignOrderJob));
    configure
        .AddJob<AssignOrderJob>(assignOrdersJobKey, configurator => { })
        .AddTrigger(trigger => trigger.ForJob(assignOrdersJobKey)
            .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(1)
                .RepeatForever()));
});
builder.Services.AddQuartzHostedService();

// 8 модуль
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver =
            new CamelCasePropertyNamesContractResolver();

        options.SerializerSettings.Converters.Add(
            new StringEnumConverter(new CamelCaseNamingStrategy()));
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("1.0.0", new OpenApiInfo
    {
        Title = "Basket Service",
        Description = "Сервис корзины",
        Contact = new OpenApiContact
        {
            Name = "Kirill Vetchinkin",
            Url = new Uri("https://microarch.ru"),
            Email = "info@microarch.ru"
        }
    });

    // XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    // Устранение конфликтов DTO
    options.CustomSchemaIds(type => type.FullName);
});

var app = builder.Build();

// -----------------------------------
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHealthChecks("/health");
app.UseRouting();
app.UseCors();

// 8 модуль
app.UseSwagger(c => { c.RouteTemplate = "openapi/{documentName}/openapi.json"; });

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "openapi";
    options.SwaggerEndpoint(
        "/openapi/1.0.0/openapi.json",
        "Basket Service v1");
});

app.MapControllers();

// 6 модуль
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
