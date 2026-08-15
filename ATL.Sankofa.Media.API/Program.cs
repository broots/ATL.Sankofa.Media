using System.Text;
using ATL.Sankofa.Media.Business;
using ATL.Sankofa.Media.Data;
using ATL.Sankofa.Media.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add media services (DbContext, repositories, Cloudflare, business services)
builder.Services.AddMediaServices(builder.Configuration);

// Background sync of Processing videos (moved off the request path from GetPublicFeedAsync)
builder.Services.AddHostedService<ATL.Sankofa.Media.API.BackgroundServices.VideoStatusSyncService>();

// ASP.NET Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<MediaDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured."));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
})
.AddGoogle(options =>
{
    var google = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = google["ClientId"] ?? string.Empty;
    options.ClientSecret = google["ClientSecret"] ?? string.Empty;
})
.AddFacebook(options =>
{
    var facebook = builder.Configuration.GetSection("Authentication:Facebook");
    options.AppId = facebook["AppId"] ?? string.Empty;
    options.AppSecret = facebook["AppSecret"] ?? string.Empty;
})
.AddTwitter(options =>
{
    var twitter = builder.Configuration.GetSection("Authentication:Twitter");
    options.ConsumerKey = twitter["ConsumerKey"] ?? string.Empty;
    options.ConsumerSecret = twitter["ConsumerSecret"] ?? string.Empty;
    options.RetrieveUserDetails = true;
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// CORS for Blazor WASM and MAUI clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClients", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:5001" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await ATL.Sankofa.Media.API.DataSeeder.SeedRolesAsync(services);
    await ATL.Sankofa.Media.API.DataSeeder.SeedAdminUserAsync(services, app.Configuration);
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseCors("AllowClients");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
