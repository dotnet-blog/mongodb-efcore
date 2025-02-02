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
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Quartz;
using Samples.MongoDb.EFCore.Api.Jobs;
using Samples.MongoDb.EFCore.Api.Extensions;
using Serilog;
using Masking.Serilog;
using Samples.MongoDb.EFCore.Api.Dtos;
using FluentValidation;
using Polly;
using Microsoft.Extensions.Http.Resilience;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Validation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
#endregion

#region ASP.NET Core
builder.Services.AddControllers()
                    .ConfigureApiBehaviorOptions(options =>
                    {
                        options.InvalidModelStateResponseFactory = context =>
                        {
                            var errorMessages = new List<ErrorMessageModel>();
                            var errorModel = new ErrorModel()
                            {
                                CorrelationId = context.HttpContext.GetLogCorrelationId(),
                                Error = "Validation failed",

                            };
                            foreach (var error in context.ModelState)
                            {
                                errorMessages.AddRange(error.Value.Errors.Select(e => new ErrorMessageModel(error.Key, e.ErrorMessage)));
                            }
                            errorModel.Messages = errorMessages;
                            return new BadRequestObjectResult(errorModel);
                        };
                    });

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddHttpContextAccessor();
#endregion

#region Logging

var s = builder.Services;
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId("x-correlation-id", true)
    .ReadFrom.Configuration(builder.Configuration)
    .Destructure.ByMaskingProperties("ApiKey")
    .Filter.ByExcluding(logEvent => logEvent.Properties.ContainsKey("ApiKey"))
    .CreateLogger();

s.AddLogging(c => c.AddSerilog());

#endregion

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
})
    .AddResilienceHandler("movie-info-service-pipeline", pipelineBuilder =>
    {
        pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Constant,
            ShouldHandle = args =>
            {
                // Retry for specific HTTP responses or exceptions
                if (args.Outcome.Result is HttpResponseMessage response)
                {
                    return ValueTask.FromResult(response.StatusCode == HttpStatusCode.InternalServerError);
                }

                if (args.Outcome.Exception is HttpRequestException error)
                {
                    return ValueTask.FromResult(true);
                }

                // Do not handle other cases
                return ValueTask.FromResult(false);
            }
        });
        pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(15));
    });
#endregion

#region Healthchecks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MediaLibraryDbContext>()
    .AddRedis(redisConnectionString: redisConnectionString);
#endregion

#region Quartz jobs
builder.Services.AddQuartz(q =>
  {
      q.AddJob<MoviesInfoCheckJob>(options => options.WithIdentity(nameof(MoviesInfoCheckJob)));
      q.AddTrigger(opts => opts
          .ForJob(nameof(MoviesInfoCheckJob))
          .WithIdentity($"{nameof(MoviesInfoCheckJob)}-Trigger")
          .WithCronSchedule(builder.Configuration.GetJobSchedule<string>(nameof(MoviesInfoCheckJob))));
  });
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
#endregion

var app = builder.Build();

app.UseExceptionHandling();

#region Configure healthcheck pipeline
app.MapHealthChecks("/api/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
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
