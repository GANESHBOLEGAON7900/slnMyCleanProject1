using Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using MyCleanProject1.Api.Middleware;
using MyCleanProject1.Application;
using MyCleanProject1.Application.Contracts;
using MyCleanProject1.Infrastructure;
using MyCleanProject1.Persistence;
using MyCleanProject1.Identity;
using MyCleanProject1.Api.Services;
using MyCleanProject1.Api.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using MyCleanProject1.Api.SwaggerHelper;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);



//SERILOG IMPLEMENTATION

IConfiguration configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
        optional: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configurationBuilder)
    .CreateBootstrapLogger().Freeze();

new LoggerConfiguration()
    .ReadFrom.Configuration(configurationBuilder)
    .CreateLogger();

builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration));


// Add services to the container.

//readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";


IConfiguration Configuration = builder.Configuration;
string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var services = builder.Services;

string Urls = Configuration.GetSection("URLWhiteListings").GetSection("URLs").Value;
services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        builder =>
        {
            builder.WithOrigins(Urls).AllowAnyHeader().AllowAnyMethod();
        });
});
services.AddApplicationServices();

services.AddInfrastructureServices(Configuration);
services.AddPersistenceServices(Configuration);
services.AddIdentityServices(Configuration);
services.AddScoped<ILoggedInUserService, LoggedInUserService>();
services.AddSwaggerExtension();
services.AddSwaggerVersionedApiExplorer();
services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
services.AddSwaggerGen(options => options.OperationFilter<SwaggerDefaultValues>());
services.AddControllers();
services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(@"bin\debug\configuration"));
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    try
    {
        Log.Information("Application Starting");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "An error occured while starting the application");
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),  
// specifying the Swagger JSON endpoint.  
IApiVersionDescriptionProvider provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwaggerUI(
options =>
{
                // build a swagger endpoint for each discovered API version  
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    }
});

app.UseCustomExceptionHandler();

app.UseCors("Open");

app.UseAuthorization();
app.MapControllers();

app.Run();

//For Integration test
public partial class Program { }
