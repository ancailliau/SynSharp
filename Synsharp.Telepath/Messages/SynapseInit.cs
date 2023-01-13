namespace Synsharp.Telepath.Messages;

public class SynapseInit : SynapseMessage
{
    public string Task { get; set; }
    public ulong Tick { get; set; }
    public string Text { get; set; }
}