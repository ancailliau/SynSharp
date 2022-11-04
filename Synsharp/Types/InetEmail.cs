using System;
using System.Net.Mail;
using Newtonsoft.Json;

namespace Synsharp.Types;


public class InetEmailConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((InetEmail)value).Value.ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return Str.Parse((string)reader.Value);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(InetEmail);
    }
}

[JsonConverter(typeof(InetEmailConverter))]
public class InetEmail : SynapseType
{
    public MailAddress Value { get; }

    protected bool Equals(InetEmail other)
    {
        return Equals(Value, other.Value);
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
        return (Value != null ? Value.GetHashCode() : 0);
    }

    private InetEmail(MailAddress value)
    {
        Value = value;
    }

    public static implicit operator MailAddress(InetEmail d) => d.Value;
    public static implicit operator InetEmail(MailAddress d) => new InetEmail(d);

    public override string ToString()
    {
        return Value.ToString();
    }

    public override string GetEscapedCoreValue()
    {
        return Value.ToString();
    }

    public override string GetCoreValue()
    {
        return Value.ToString();
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