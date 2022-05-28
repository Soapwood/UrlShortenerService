/*
 * DICE Interview - URLShortenerService 
 * Tom O'Neill 2022
 */

namespace UrlShortenerService.Program;

using ConfigurationSettings;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using URLShortenerService.Models;

/// <summary>
/// URLShortenerService Program Class
/// </summary>
public class UrlShortenerService
{
    private static IConfiguration? _config { get; set; }

    public static void Main(string[] args)
    {
        // Service builder
        var builder = WebApplication.CreateBuilder(args);

        WebHost.CreateDefaultBuilder().ConfigureLogging((hostingContext, logging) =>
        {
            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            logging.AddConsole();
            logging.AddDebug();
            logging.AddEventSourceLogger();
        });

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddMvc();
        builder.Services.AddRazorPages();

        // Read appsettings configuration
        _config = builder.Configuration;
        builder.Services.Configure<MySqlSettings>(_config.GetSection("MySqlConfig"));
        string connString = $"Server={_config["MySqlConfig:internalContainerHostname"]}; " +
                            $"Port={_config["MySqlConfig:port"]}; " +
                            $"Database={_config["MySqlConfig:database"]}; " +
                            $"Uid={_config["MySqlConfig:userId"]}; " +
                            $"Pwd={_config["MySqlConfig:password"]};";

        try
        {
            // Connect to MySQL
            builder.Services.AddDbContext<UrlContext>(ops =>
                ops.UseMySql(connString, ServerVersion.AutoDetect(connString)));
        } catch (Exception ex)
        {
            throw new ApplicationException("MySQL service could not be reached", ex);
        }

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configure application
        var app = builder.Build();
        app.UseSwagger();
        app.UseSwaggerUI();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");

            // The default HSTS value is 30 days.
            app.UseHsts();
        }

        // WebServer config
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapSwagger();
        app.MapRazorPages();

        // Run
        app.Run();
    }
}