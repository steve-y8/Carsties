using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
	opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Inject AutoMapper
// AutoMapper helps mapping entities to DTOs
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());    // Will look for any classes that derived from Profile class

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
	DbInitializer.InitDb(app);
}
catch(Exception e)
{
	Console.WriteLine(e);
}

app.Run();
