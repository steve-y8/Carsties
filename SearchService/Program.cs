using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());    // Will look for any classes that derived from Profile class

// Inject MassTransit.
// MassTransit provides a consistent abstraction on top of the supported message transports.
builder.Services.AddMassTransit(x =>
{
	// Define consumers
	x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
	//x.AddConsumer<AuctionCreatedConsumer>();
	//x.AddConsumer<AuctionUpdatedConsumer>();
	//x.AddConsumer<AuctionDeletedConsumer>();
	//x.AddConsumer<BidPlacedConsumer>();
	//x.AddConsumer<AuctionFinishedConsumer>();

	// This is to distinguish the AConsumer in this service from AConsumer of another service
	x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

	// Using RabbitMQ as the service bus
	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
		{
            // If Username and Password are not provided in the config file,
            // use guest as the Username and Password
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });

		// Configure message retry for search-auction-created queue if AuctionCreatedConsumer failed.
		cfg.ReceiveEndpoint("search-auction-created", e =>
		{
			e.UseMessageRetry(r => r.Interval(5, 5));	// Retry 5 times with 5 sec interval

			e.ConfigureConsumer<AuctionCreatedConsumer>(context);
		});

		// Configure message retry for search-auction-updated queue if AuctionUpdatedConsumer failed.
		/*cfg.ReceiveEndpoint("search-auction-updated", e =>
		{
			e.UseMessageRetry(r => r.Interval(5, 5));

			e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
		});*/

		// Configure message retry for search-auction-deleted queue if AuctionDeletedConsumer failed.
		/*cfg.ReceiveEndpoint("search-auction-deleted", e =>
		{
			e.UseMessageRetry(r => r.Interval(5, 5));

			e.ConfigureConsumer<AuctionDeletedConsumer>(context);
		});*/

		// Configure the endpoints for all defined consumers.
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
