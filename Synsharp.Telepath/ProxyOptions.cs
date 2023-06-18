using Microsoft.Extensions.Logging;

namespace Synsharp.Telepath;

public class ProxyOptions
{
    public string CertDir { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".syn/certs/");
    public string? CertName { get; set; }
    public string HostName { get; set; }
}