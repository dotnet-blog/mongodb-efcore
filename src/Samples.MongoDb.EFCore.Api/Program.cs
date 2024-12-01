using MassTransit;
using Microsoft.EntityFrameworkCore;
using Samples.MongoDb.EFCore.Api;
using Samples.MongoDb.EFCore.Api.Consumers;
using Samples.MongoDb.EFCore.Api.Services;
using Samples.MongoDb.EFCore.Api.Settings;
using StackExchange.Redis;
using Flurl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

#region Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);
#endregion

#region EFCore MongoDb
var mediaLibraryDatabase = builder.Configuration.GetSection("MediaLibraryDatabase").Get<MediaLibraryDatabaseSettings>();
builder.Services.AddDbContext<MediaLibraryDbContext>(options =>
{
    options.UseMongoDB(
        connectionString: mediaLibraryDatabase!.ConnectionString,
        databaseName: mediaLibraryDatabase.DatabaseName);
});
#endregion

#region RedisDb
builder.Services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisDb")!));
builder.Services.AddScoped<StackExchange.Redis.IDatabase>((provider) =>
{
    var multiplexer = provider.GetService<IConnectionMultiplexer>();
    return multiplexer!.GetDatabase();

});
#endregion

#region MassTransit RabbitMq
builder.Services.AddMassTransit(x =>
{
    x.AddConfigureEndpointsCallback((context, name, config) =>
    {
        config.UseMessageRetry(r =>
        {
            r.Interval(retryCount: 3, interval: (int)TimeSpan.FromSeconds(3).TotalMilliseconds);
            r.Ignore<ArgumentNullException>();

        });
    });

    x.AddConsumer<MovieAddedEventConsumer>(config =>
    {
        config.UseMessageRetry(r =>
        {
            //r.Handle<System.Net.WebClie>
        });
    });

    x.UsingRabbitMq((context, config) =>
        {
            var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>();
            config.Host(rabbitMqSettings!.Url, h =>
            {
                h.Username(rabbitMqSettings.Username);
                h.Password(rabbitMqSettings.Password);
            });
            config.ConfigureEndpoints(context);
        });
});
#endregion

#region OpenMoviedDb service HttpClient
var movieInfoService = builder.Configuration.GetSection("MovieInfoService").Get<MovieInfoServiceSettings>();
builder.Services.AddHttpClient<IMovieInfoService, MovieInfoService>(httpClient =>
{
    httpClient.BaseAddress = movieInfoService.Url.AppendQueryParam("apikey",movieInfoService.ApiKey).ToUri();
});
#endregion

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
