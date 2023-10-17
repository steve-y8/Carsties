using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Data;
using SearchService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Inject MassTransit.
// MassTransit provides a consistent abstraction on top of the supported message transports.
builder.Services.AddMassTransit(x =>
{
	// Using RabbitMQ as the service bus
	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.ConfigureEndpoints(context);
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

try
{
	await DbInitializer.InitDb(app);
}
catch(Exception e)
{
	Console.WriteLine(e);
}

app.Run();
