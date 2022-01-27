using ScyberLog;
using ScyberLog.WebApi;
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "CA2017:Number of parameters supplied in the logging message template do not match the number of named placeholders", Justification = "ScyberLog Captures unused parameters")]

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging((HostBuilderContext hostingContext, ILoggingBuilder loggingBuilder) => 
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddScyberLog(hostingContext.Configuration.GetSection("ScyberLog"));
    });

// Add services to the container.
builder.Services.AddHostedService<Worker>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
