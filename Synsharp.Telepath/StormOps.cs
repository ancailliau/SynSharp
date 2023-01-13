using MessagePack;

namespace Synsharp.Telepath;

[MessagePackObject]
public class StormOps
{
    [Key("debug")] public bool? Debug { get; set; }
    //[Key("editformat")] public EditFormat? EditFormat { get; set; } = Telepath.EditFormat.NodeEdit;
    [Key("idens")] public string[] Idens { get; set; } = Array.Empty<string>();
    [Key("limit")] public long? Limit { get; set; }
    //[Key("mode")] public Mode? Mode { get; set; }
    //[Key("ndefs")] public Tuple<string,dynamic>? Ndefs { get; set; }
    [Key("path")] public bool? Path { get; set; }
    [Key("readonly")] public bool? ReadOnly { get; set; }
    [Key("repr")] public bool? Repr { get; set; }
    //[Key("show")] public string[]? Show { get; set; }
    [Key("task")] public string? Task { get; set; }
    [Key("user")] public string? User { get; set; }
    [Key("vars")] public Dictionary<string, dynamic> Vars { get; set; }
    [Key("view")] public string? View { get; set; }
}

public enum Mode
{
    Lookup, AutoAdd, Search
}

public enum EditFormat
{
    NodeEdit, None, Count
}