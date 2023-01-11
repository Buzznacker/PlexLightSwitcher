using Microsoft.Extensions.Logging;
using PlexLightSwitcher.Worker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add logging to file
builder.Services.AddLogging(
    logBuilder => logBuilder.AddFile("logs/logs-{0:yyyy}-{0:MM}-{0:dd}.log", fileLoggerOpts => fileLoggerOpts.FormatLogFileName = fName => string.Format(fName, DateTime.Now))
);

// Register our background process that handles the plex messages
builder.Services.AddSingleton(service =>
{
    var loggingFactory = service.GetService<ILoggerFactory>();
    return new APIWorker(loggingFactory.CreateLogger<APIWorker>());
});
builder.Services.AddHostedService(sp => sp.GetService<APIWorker>());


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
