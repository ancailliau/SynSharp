using System;

namespace Synsharp.Types;

public class HashMD5: Hex
{
    protected HashMD5(string value) : base(value)
    {
    }
    
    public static implicit operator string(HashMD5 d) => d.Value;
    public static implicit operator HashMD5(string d) => new HashMD5(d);
    
    public static HashMD5 Convert(object o)
    {
        if (o is string str)
            return new HashMD5(str);

        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(HashMD5).FullName}'");
    }
}