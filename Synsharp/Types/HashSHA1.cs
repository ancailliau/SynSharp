using System;

namespace Synsharp.Types;

public class HashSHA1: Hex
{
    protected HashSHA1(string value) : base(value)
    {
    }
    
    public static implicit operator string(HashSHA1 d) => d.Value;
    public static implicit operator HashSHA1(string d) => new HashSHA1(d);
    
    public static HashSHA1 Convert(object o)
    {
        if (o is string str)
            return new HashSHA1(str);

        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(HashSHA1).FullName}'");
    }
}