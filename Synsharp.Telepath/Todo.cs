using System.Collections.Generic;
using MessagePack;

namespace Synsharp.Telepath;

[MessagePackObject]
public class Todo
{
    [Key(0)] public string Name { get; set; }

    [Key(1)] public object[] Args { get; set; }

    [Key(2)] public Dictionary<string, object?> KwArgs { get; set; } = new Dictionary<string, object?>();
}