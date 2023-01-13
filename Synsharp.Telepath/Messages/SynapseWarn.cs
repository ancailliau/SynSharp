namespace Synsharp.Telepath.Messages;

public class SynapseWarn : SynapseMessage
{
    public string Message { get; set; }
    public Dictionary<string,dynamic> Additional { get; set; }
}