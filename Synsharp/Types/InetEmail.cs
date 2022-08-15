using System;
using System.Net.Mail;

namespace Synsharp.Types;

public class InetEmail : SynapseType
{
    protected MailAddress _value;

    protected bool Equals(InetEmail other)
    {
        return Equals(_value, other._value);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((InetEmail)obj);
    }

    public override int GetHashCode()
    {
        return (_value != null ? _value.GetHashCode() : 0);
    }

    private InetEmail(MailAddress value)
    {
        _value = value;
    }

    public static implicit operator MailAddress(InetEmail d) => d._value;
    public static implicit operator InetEmail(MailAddress d) => new InetEmail(d);

    public override string ToString()
    {
        return _value.ToString();
    }

    public override string GetCoreValue()
    {
        return _value.ToString();
    }

    public static InetEmail Parse(string s)
    {
        return new InetEmail(new MailAddress(s));
    }
    
    public static InetEmail Convert(object o)
    {
        if (o is string str)
            return Parse(str);

        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(InetEmail).FullName}'");
    }
}