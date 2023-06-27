namespace Synsharp.Telepath.Messages;

public class SynapseNode : SynapseMessage
{
    public string Form { get; set; }
    public dynamic Valu { get; set; }

    public string Iden { get; set; }
    public Dictionary<string, object> Props { get; set; }
    public Dictionary<string, long?[]> Tags { get; set; } = new();
    
    // TODO path property
    // TODO tagprops
    
    public object Repr { get; set; }
    public Dictionary<string, object>? Reprs { get; set; }
    
    // TODO tagpropreprs
}