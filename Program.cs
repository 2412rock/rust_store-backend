using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Hosting;
using Rust_store_backend.Services;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Rust_store_backend.Models.DB;
using Rust_store_backend;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseKestrel(options =>
//{
//    options.Listen(IPAddress.Any, 4300); // Explicitly set the port here
//});
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(4300, listenOptions =>
    {
        listenOptions.UseHttps("/app/backendcertificate.pfx");
    });
});

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalHost",
        builder =>
        {
            builder.WithOrigins("http://localhost")
                   .AllowAnyHeader()
                   .AllowAnyMethod().AllowCredentials();
        });
    options.AddPolicy("AllowAnyOrigin",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });

    options.AddPolicy("AllowOVerflowOrigin",
        builder =>
        {
            builder.WithOrigins("https://overflowapp.xyz")
                   .AllowAnyHeader()
                   .AllowAnyMethod().AllowCredentials();
        });

});

builder.Logging.ClearProviders(); // Remove default providers
builder.Logging.AddConsole();
builder.Services.AddHttpClient();
builder.Services.AddScoped<RCONService>();
builder.Services.AddScoped<StartupService>();
builder.Services.AddSingleton<DepositToAllService>();


var saPassword = Environment.GetEnvironmentVariable("SA_PASSWORD");

//string hostIp = Environment.GetEnvironmentVariable("DB_IP");
string hostIp =  Environment.GetEnvironmentVariable("DB_IP");

builder.Services.AddDbContext<RustDBContext>(options =>
   options.UseSqlServer($"Server={hostIp},1436;Database=RustDB;User Id=sa;Password={saPassword};TrustServerCertificate=True"));

var app = builder.Build();


app.Lifetime.ApplicationStarted.Register(async () =>
{
    using (var scope = app.Services.CreateScope())
    {
        var startupService = scope.ServiceProvider.GetRequiredService<StartupService>();
        startupService.Initialize();
    }
});

app.UseCors("AllowAnyOrigin");
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
// Configure the HTTP request pipeline.
//app.UseMiddleware<AccessLogMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
