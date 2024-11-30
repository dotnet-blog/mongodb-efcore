using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Samples.MongoDb.EFCore.Api;
using Samples.MongoDb.EFCore.Api.Consumers;
using Samples.MongoDb.EFCore.Api.Settings;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

//EFCore MongoDb
var mediaLibraryDatabase = builder.Configuration.GetSection("MediaLibraryDatabase").Get<MediaLibraryDatabaseSettings>();
builder.Services.AddDbContext<MediaLibraryDbContext>(options =>
{
    options.UseMongoDB(
        connectionString: mediaLibraryDatabase!.ConnectionString,
        databaseName: mediaLibraryDatabase.DatabaseName);
});

//Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("SequencesRedis")));
builder.Services.AddScoped<StackExchange.Redis.IDatabase>((provider) =>
{
    var multiplexer = provider.GetService<IConnectionMultiplexer>();
    return multiplexer.GetDatabase();

});

//MassTransit RabbitMq
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MovieAddedEventConsumer>();

    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        cfg.UseMessageRetry(r => r.Interval(
            retryCount: 3, 
            interval: (int)TimeSpan.FromSeconds(3).TotalMilliseconds));
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>();
        cfg.Host(rabbitMqSettings.Url, h =>
        {
            h.Username(rabbitMqSettings.Username);
            h.Password(rabbitMqSettings.Password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
