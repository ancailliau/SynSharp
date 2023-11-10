
namespace Synsharp.Telepath.Messages;

public static class SynConvert
{
    public static SynapseMessage ToMessage(dynamic o)
    {
        if (o[0] == "init")
            return ToInit(o[1]);
        if (o[0] == "fini")
            return ToFini(o[1]);
        if (o[0] == "err")
            return ToErr(o[1]);
        if (o[0] == "warn")
            return ToWarn(o[1]);
        if (o[0] == "print")
            return ToPrint(o[1]);
        if (o[0] == "node")
            return ToNode(o[1]);
        if (o[0] == "node:edits")
            return ToNodeEdits(o[1]);
        throw new NotImplementedException(o[0] + " is not (yet) supported.");
    }

    private static SynapseNodeEdit ToNodeEdits(dynamic o)
    {
        return new SynapseNodeEdit();
    }

    public static SynapseInit ToInit(dynamic o)
    {
        return new SynapseInit()
        {
            Task = o["task"],
            Tick = Convert.ToUInt64(o["tick"]),
            Text = o["text"]
        };
    }
    
    public static SynapseFini ToFini(dynamic o)
    {
        return new SynapseFini()
        {
            Tock = Convert.ToUInt64(o["tock"]),
            Took = Convert.ToUInt64(o["took"]),
            Count = Convert.ToUInt64(o["count"])
        };
    }
    
    public static SynapseErr ToErr(dynamic o)
    {
        var err = new SynapseErr()
        {
            ErrorType = o[0]
        };
        if (o[1].ContainsKey("mesg"))
            err.Message = o[1]["mesg"];
        if (o[1].ContainsKey("ename"))
            err.ErrorName = o[1]["ename"];
        if (o[1].ContainsKey("eline"))
            err.ErrorLineNumber = o[1]["eline"];
        if (o[1].ContainsKey("efile"))
            err.ErrorFile = o[1]["efile"];
        if (o[1].ContainsKey("esrc"))
            err.ErrorSourceLine = o[1]["esrc"];
        return err;
    }
    
    public static SynapseWarn ToWarn(dynamic o)
    {
        var props = new Dictionary<string, dynamic>();
        foreach (var kv in o)
        {
            if (kv.Key != "mesg")
                props[kv.Key.ToString()] = kv.Value;
        }
        
        return new SynapseWarn()
        {
            Message = o["mesg"],
            Additional = props
        };
    }
    
    public static SynapsePrint ToPrint(dynamic o)
    {
        if (o.ContainsKey("mesg"))
        {
            return new SynapsePrint()
            {
                Message = o["mesg"]
            };   
        }
        else
        {
            return new SynapsePrint() { Message = "SynSharp Error: Could not get message" };
        }
    }
    
    public static SynapseNode ToNode(dynamic o)
    {
        
        var data = o[0];
        Dictionary<object,object> meta = (Dictionary<object,object>) o[1];

        var tags = new Dictionary<string, long?[]>();
        foreach (var t in (Dictionary<object,object>)meta["tags"])
        {
            var val = (object[])t.Value;
            tags[t.Key.ToString()] = new long?[] {(long?)val[0], (long?)val[1]};
        }

        var props = new Dictionary<string, object>();
        foreach (var kv in (Dictionary<object,object>)meta["props"])
        {
            props[kv.Key.ToString()] = kv.Value;
        }

        Dictionary<string, object> reprs = default;
        if (meta.ContainsKey("reprs"))
        {
            reprs = new();
            foreach (var kv in (Dictionary<object,object>)meta["reprs"])
            {
                reprs[kv.Key.ToString()] = kv.Value;
            }
        }

        var node = new SynapseNode()
        {
            Form = (string)data[0], 
            Valu = data[1],
            Reprs = reprs,
            Props = props,
            Tags = tags
        };

        if (meta.ContainsKey("repr"))
        {
            node.Repr = meta["repr"];
        }

        if (meta.ContainsKey("iden"))
            node.Iden = (string)meta["iden"];
        
        return node;
    }
}

internal class SynapseNodeEdit : SynapseMessage
{
}