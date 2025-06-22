namespace NicDDNSUpdater;

public class DDNSConfiguration
{
    public const string SectionName = "DDNS";
    
    public string[] Hostnames { get; set; } = Array.Empty<string>();
    public int UpdateIntervalSeconds { get; set; } = 300; // Default: 5 minutes (300 seconds)
}
