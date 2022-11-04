using System;

namespace Synsharp.Types;

public class InetPort : Int
{
    protected InetPort(long value) : base(value)
    {
    }
    
    public static implicit operator long(InetPort d) => d.Value;
    public static implicit operator InetPort(long d) => new InetPort(d);

    public static InetPort Parse(string s)
    {
        return new InetPort(long.Parse(s));
    }
    
    public static InetPort Convert(object o)
    {
        if (o is string str)
            return Parse(str);

        if (o is Int32 i32)
            return new InetPort(i32);

        if (o is Int64 i64)
            return new InetPort(i64);
        
        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(InetPort).FullName}'");
    }
}