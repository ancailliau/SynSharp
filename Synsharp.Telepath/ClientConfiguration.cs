namespace Synsharp.Telepath;

public class ClientConfiguration
{
    public int Timeout { get; set; } = 10 * 1000;

    /// <summary>
    /// Milliseconds to sleep before retrying to connnect
    /// </summary>
    public int RetrySleep { get; set; } = 200;

    public int LinkPoolSize { get; set; } = 4;
}