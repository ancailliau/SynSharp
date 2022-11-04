using System;
using Newtonsoft.Json;

namespace Synsharp.Types;

public class InetFqdnConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((InetFqdn)value).Value.ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return Str.Parse((string)reader.Value);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(InetFqdn);
    }
}

[JsonConverter(typeof(InetFqdnConverter))]
public class InetFqdn : SynapseType
{
    public string Value { get; }

    public InetFqdn(string value)
    {
        Value = value;
    }

    protected bool Equals(InetFqdn other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is string s) return Value == s;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((InetFqdn)obj);
    }

    public override int GetHashCode()
    {
        return (Value != null ? Value.GetHashCode() : 0);
    }

    public static implicit operator string(InetFqdn d) => d.Value;
    public static implicit operator InetFqdn(string d) => new InetFqdn(d);

    public override string ToString()
    {
        return Value;
    }

    public override string GetEscapedCoreValue()
    {
        return Value;
    }

    public override string GetCoreValue()
    {
        return Value.ToString();
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