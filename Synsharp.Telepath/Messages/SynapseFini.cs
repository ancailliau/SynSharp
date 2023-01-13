namespace Synsharp.Telepath.Messages;

public class SynapseFini : SynapseMessage
{
    public ulong Tock { get; set; }
    public ulong Took { get; set; }
    public ulong Count { get; set; }
}