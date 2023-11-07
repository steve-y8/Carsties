using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Inject Reverse Proxy
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// This part must be the same as AuctionService.Program
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

// Reverse Proxy middleware??
app.MapReverseProxy();

// Identity server middleware. Same as IdentityService.Program
app.UseAuthentication();
app.UseAuthorization();

app.Run();
