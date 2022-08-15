using System;

namespace Synsharp.Types;

public class InetPasswd : Str
{
    protected InetPasswd(string value) : base(value)
    {
    }
    
    public static implicit operator string(InetPasswd d) => d._value;
    public static implicit operator InetPasswd(string d) => new InetPasswd(d);

    public static InetPasswd Parse(string s)
    {
        return new InetPasswd(s);
    }
    
    public static InetPasswd Convert(object o)
    {
        if (o is string str)
            return Parse(str);

        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(InetPasswd).FullName}'");
    }
}