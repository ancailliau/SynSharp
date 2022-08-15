using System;

namespace Synsharp.Types;

public class InetUser : Str
{
    protected InetUser(string value) : base(value)
    {
    }

    public static implicit operator string(InetUser d) => d._value;
    public static implicit operator InetUser(string d) => new InetUser(d);

    public static InetUser Parse(string s)
    {
        return new InetUser(s);
    }
    
    public static InetUser Convert(object o)
    {
        if (o is string str)
            return Parse(str);

        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(InetUser).FullName}'");
    }
}