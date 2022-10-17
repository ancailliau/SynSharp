using System;

namespace Synsharp.Types;

public class InetFqdn : SynapseType
{
    private string _value;
    
    public InetFqdn(string value)
    {
        _value = value;
    }

    protected bool Equals(InetFqdn other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is string s) return _value == s;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((InetFqdn)obj);
    }

    public override int GetHashCode()
    {
        return (_value != null ? _value.GetHashCode() : 0);
    }

    public static implicit operator string(InetFqdn d) => d._value;
    public static implicit operator InetFqdn(string d) => new InetFqdn(d);

    public override string ToString()
    {
        return _value;
    }

    public override string GetEscapedCoreValue()
    {
        return _value;
    }

    public override string GetCoreValue()
    {
        return _value.ToString();
    }

    public static InetFqdn Parse(string s)
    {
        return new InetFqdn(s);
    }
    
    public static InetFqdn Convert(object o)
    {
        if (o is string str)
            return Parse(str);

        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(InetFqdn).FullName}'");
    }
}