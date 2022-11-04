using System;

namespace Synsharp.Types;

public class HashSHA256: Hex
{
    protected HashSHA256(string value) : base(value)
    {
    }
    
    public static implicit operator string(HashSHA256 d) => d.Value;
    public static implicit operator HashSHA256(string d) => new HashSHA256(d);
    
    public static HashSHA256 Convert(object o)
    {
        if (o is string str)
            return new HashSHA256(str);

        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(HashSHA256).FullName}'");
    }
}