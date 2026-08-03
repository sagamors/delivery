using System.Reflection;
using AutoMapper;
using Ddd;
using DeliveryApp.Api;
using DeliveryApp.Api.Adapters.BackgroundJobs;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Quartz;

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
            policy.AllowAnyOrigin(); // Не делайте так в проде!
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
