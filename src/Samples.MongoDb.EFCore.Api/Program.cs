using MassTransit;
using Microsoft.EntityFrameworkCore;
using Samples.MongoDb.EFCore.Api;
using Samples.MongoDb.EFCore.Api.Consumers;
using Samples.MongoDb.EFCore.Api.Services;
using Samples.MongoDb.EFCore.Api.Settings;
using StackExchange.Redis;
using Flurl;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Mime;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

#region API version

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

builder.Services.AddEndpointsApiExplorer();
#endregion

#region Swagger

builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);
    options.EnableAnnotations();
    // resolve the IApiVersionDescriptionProvider service
    // note: that we have to build a temporary service provider here because one has not been created yet


    using (var serviceProvider = builder.Services.BuildServiceProvider())
    {
        var provider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();
        var assembly = typeof(Program).Assembly;
        // add a swagger document for each discovered API version
        // NOTE: you might choose to skip or document deprecated API versions differently
        String assemblyDescription = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo()
            {
                Title = $"{assembly.GetCustomAttribute<AssemblyProductAttribute>().Product} {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated ? $"{assemblyDescription} - DEPRECATED" : $"{assemblyDescription}" +
                $"<p><a href='/api/health' target='_blank'>Healthchecks</a></p>" +
                $"<p>Build #{assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}</p>"
            });
        }
    }

    // integrate xml comments
    var currentAssembly = Assembly.GetExecutingAssembly();
    var xmlDocs = currentAssembly.GetReferencedAssemblies()
    .Union(new AssemblyName[] { currentAssembly.GetName() })
    .Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml"))
    .Where(f => File.Exists(f)).ToArray();

    Array.ForEach(xmlDocs, (d) =>
    {
        options.IncludeXmlComments(d);
    });

});
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
var redisConnectionString = builder.Configuration.GetConnectionString("RedisDb")!;
builder.Services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddScoped<StackExchange.Redis.IDatabase>((provider) =>
{
    var multiplexer = provider.GetService<IConnectionMultiplexer>();
    return multiplexer!.GetDatabase();

});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});
#endregion

#region MassTransit RabbitMq
var rabbitMqConnectionString = builder.Configuration.GetConnectionString("RabbitMq")!;
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
        //config.UseMessageRetry(r =>
        //{
        //    //r.Handle<System.Net.WebClie>
        //});
    });

    x.UsingRabbitMq((context, config) =>
        {
            config.Host(rabbitMqConnectionString);
            config.ConfigureEndpoints(context);
        });
});
#endregion

#region OpenMoviedDb service HttpClient
var movieInfoService = builder.Configuration.GetSection("MovieInfoService").Get<MovieInfoServiceSettings>();
builder.Services.AddHttpClient<IMovieInfoService, MovieInfoService>(httpClient =>
{
    httpClient.BaseAddress = movieInfoService.Url.AppendQueryParam("apikey", movieInfoService.ApiKey).ToUri();
});
#endregion

#region Healthchecks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MediaLibraryDbContext>()
    .AddRedis(redisConnectionString: redisConnectionString)
    .AddRabbitMQ(rabbitConnectionString: "amqp://rabbitmq:rabbitmq@localhost/sample.api");

#endregion

var app = builder.Build();

#region Configure healthcheck pipeline
var options = new HealthCheckOptions();
options.ResultStatusCodes[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable;
options.ResponseWriter = async (ctx, rpt) =>
{
    var result = JsonSerializer.Serialize(new
    {
        status = rpt.Status.ToString(),
        services = rpt.Entries.Select(e => new
        {
            name = e.Key,
            healthy = e.Value.Status == HealthStatus.Healthy,
            status = Enum.GetName(typeof(HealthStatus), e.Value.Status)
        })
    }, new JsonSerializerOptions { 
        WriteIndented = true ,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    });
    ctx.Response.ContentType = MediaTypeNames.Application.Json;
    await ctx.Response.WriteAsync(result);
};
app.MapHealthChecks("/api/health", options);
#endregion
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
