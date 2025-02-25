using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Hosting;
using Rust_store_backend.Services;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Rust_store_backend.Models.DB;
using Rust_store_backend.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(4500); // Use ListenAnyIP instead of Listen
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

builder.Services.AddHttpClient();
builder.Services.AddTransient<RCONService>();
builder.Services.AddScoped<StartupService>();


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
