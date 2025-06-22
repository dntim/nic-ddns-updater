using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace NicDDNSUpdater;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
          var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting NIC DDNS Updater v2.0.0");
        
        var ddnsService = host.Services.GetRequiredService<DDNSUpdateService>();
        await ddnsService.StartAsync();
        
        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<DDNSConfiguration>(context.Configuration.GetSection(DDNSConfiguration.SectionName));
                services.AddSingleton<DDNSUpdateService>();
                services.AddHttpClient();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });
}

public class DDNSUpdateService
{
    private readonly ILogger<DDNSUpdateService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<DDNSConfiguration> _configuration;
    private readonly Timer _timer;

    public DDNSUpdateService(
        ILogger<DDNSUpdateService> logger, 
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<DDNSConfiguration> configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        
        // Timer will be configured when service starts
        _timer = new Timer(async _ => await UpdateDDNS(), null, Timeout.Infinite, Timeout.Infinite);
    }    public async Task StartAsync()
    {
        var config = _configuration.CurrentValue;
        var intervalMs = config.UpdateIntervalSeconds * 1000;
        
        _logger.LogInformation("DDNS Update Service started. Hostnames: {Hostnames}, Update Interval: {Seconds} seconds", 
            string.Join(", ", config.Hostnames), config.UpdateIntervalSeconds);
        
        // Perform initial update
        await UpdateDDNS();
        
        // Start the timer for periodic updates
        _timer.Change(intervalMs, intervalMs);
    }

    private async Task UpdateDDNS()
    {
        try
        {
            var config = _configuration.CurrentValue;
            var username = GetCredential("username");
            var password = GetCredential("password");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogError("DDNS credentials not found. Please check Docker secrets or environment variables.");
                return;
            }

            if (config.Hostnames.Length == 0)
            {
                _logger.LogWarning("No hostnames configured for DDNS updates.");
                return;
            }

            using var httpClient = _httpClientFactory.CreateClient();
            
            // Create basic auth header
            var authInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authInfo);

            // Update each hostname
            foreach (var hostname in config.Hostnames)
            {
                await UpdateSingleHostname(httpClient, hostname);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during DDNS update");
        }
    }

    private async Task UpdateSingleHostname(HttpClient httpClient, string hostname)
    {
        try
        {
            var url = $"https://api.nic.ru/dyndns/update?hostname={hostname}";
            
            _logger.LogInformation("Updating DDNS for hostname: {Hostname}", hostname);
            
            var response = await httpClient.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("DDNS update successful for {Hostname}: {Response}", hostname, result);
            }
            else
            {
                _logger.LogWarning("DDNS update failed for {Hostname}. Status: {StatusCode}, Response: {Response}", 
                    hostname, response.StatusCode, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating DDNS for hostname: {Hostname}", hostname);
        }
    }    private string? GetCredential(string credentialName)
    {
        // Try Docker secrets first (mounted at /run/secrets/)
        var secretPath = $"/run/secrets/ddns_{credentialName}";
        if (File.Exists(secretPath))
        {
            _logger.LogDebug("Reading {CredentialName} from Docker secret", credentialName);
            return File.ReadAllText(secretPath).Trim();
        }

        // Fallback to environment variables (for development/testing)
        var envVarName = $"DDNS__{credentialName.ToUpperInvariant()}";
        var envValue = Environment.GetEnvironmentVariable(envVarName);
        if (!string.IsNullOrEmpty(envValue))
        {
            _logger.LogDebug("Reading {CredentialName} from environment variable {EnvVar}", credentialName, envVarName);
            return envValue;
        }

        _logger.LogWarning("Credential {CredentialName} not found. Please check Docker secrets or environment variables.", credentialName);
        return null;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
