using MessagePack;

namespace Synsharp.Telepath;

[MessagePackObject]
public class TaskInitRequest
{
    [Key("name")] public string? Name { get; set; }
    [Key("todo")] public Todo Todo { get; set; }
    [Key("task")] public string Task { get; set; }
}

[MessagePackObject]
public class T2InitRequest
{
    [Key("todo")] public Todo Todo { get; set; }
    [Key("name")] public string? Name { get; set; }
    [Key("sess")] public string Sess { get; set; }
}