using System.Collections.Generic;
using MessagePack;

namespace Synsharp.Telepath;

[MessagePackObject]
public class Auth
{
    [Key(0)]
    public string User { get; set; }
    [Key(1)]
    public IDictionary<string,string> Params { get; set; }
}