using MessagePack;
using Nito.Collections;

namespace Synsharp.Telepath;

[MessagePackObject]
public class TelepathMessage<T> : TelepathMessage
{
    [Key(1)] public T? Data { get; set; } = default;
}

[MessagePackObject]
public class TelepathMessage
{
    [Key(0)]
    public string Type { get; set; }
}