using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Hosting;
using Rust_store_backend.Services;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Rust_store_backend.Models.DB;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options =>
{
    options.Listen(IPAddress.Any, 4600, listenOptions =>
    {
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

builder.Services.AddHttpClient();
builder.Services.AddTransient<RCONService>();


var saPassword = Environment.GetEnvironmentVariable("SA_PASSWORD");

//string hostIp = Environment.GetEnvironmentVariable("DB_IP");
var env = builder.Environment.EnvironmentName;
string hostIp = env == "Development" ? "192.168.1.237" : Environment.GetEnvironmentVariable("DB_IP");

builder.Services.AddDbContext<RustDBContext>(options =>
   options.UseSqlServer($"Server={hostIp},1436;Database=RustDB;User Id=sa;Password={saPassword};TrustServerCertificate=True"));

var app = builder.Build();

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
