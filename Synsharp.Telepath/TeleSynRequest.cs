using System.Collections.Generic;
using MessagePack;

namespace Synsharp.Telepath;

[MessagePackObject]
public class TeleSynRequest
{
    [Key("auth")]
    public Auth Auth { get; set; }
    [Key("vers")]
    public int[] Vers { get; set; }
    [Key("name")]
    public string Name { get; set; }
}

[MessagePackObject]
public class TeleSynReply
{
    [Key("sess")]
    public string Sess { get; set; }
    [Key("vers")]
    public int[] Vers { get; set; }
    [Key("retn")]
    public object[] Retn { get; set; }
    [Key("name")]
    public string Name { get; set; }
    [Key("sharinfo")]
    public TeleSynSharinfoReply Sharinfo { get; set; }
}

[MessagePackObject]
public class TeleSynSharinfoReply
{
    [Key("meths")]
    public Dictionary<string,MethProps> Meths { get; set; }
    [Key("syn:commit")]
    public string SynCommit { get; set; }
    [Key("syn:version")]
    public int[] SynVersion { get; set; }
    [Key("classes")]
    public string[] Classes { get; set; }
}

[MessagePackObject]
public class MethProps
{
    [Key("genr")]
    public bool Genr { get; set; }
}


[MessagePackObject]
public class TeleTaskFini
{
    [Key("task")]
    public string Task { get; set; }
    
    [Key("retn")]
    public object[] Retn { get; set; }
    
    [Key("type")]
    public string? Type { get; set; }
}