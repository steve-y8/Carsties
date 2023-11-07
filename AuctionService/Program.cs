using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
	// If the message queue is down when publishing a message,
	// persist the message in an outbox.
	x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
	{
		o.QueryDelay = TimeSpan.FromSeconds(10);

		o.UsePostgres();
		o.UseBusOutbox();
	});

    // Define message consumers
    //x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
	x.AddConsumer<AuctionCreatedFaultConsumer>();
	x.AddConsumer<BidPlacedConsumer>();
	x.AddConsumer<AuctionFinishedConsumer>();

    // This is to distinguish the AConsumer in this service from AConsumer of another service
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

	// Using RabbitMQ as the service bus
	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.ConfigureEndpoints(context);
	});
});

// Inject Authentication service
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		// How are we going to validate the token for this resource server

		// Tell this resource server who the token was issued by
		options.Authority = builder.Configuration["IdentityServiceUrl"];
		options.RequireHttpsMetadata = false;
		options.TokenValidationParameters.ValidateAudience = false;
        
		// username claim is in the token. Refer to IdentityService.Services.CustomProfileService
        options.TokenValidationParameters.NameClaimType = "username";	
	});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthentication();	// Middleware for Authentication service. Has to come before UseAuthorization
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
