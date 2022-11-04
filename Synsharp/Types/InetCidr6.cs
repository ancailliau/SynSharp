/*
 * Copyright 2022 Antoine Cailliau
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Net;
using Newtonsoft.Json;

namespace Synsharp.Types;

public class InetCidr6Converter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((InetCidr6)value).Value.ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return Str.Parse((string)reader.Value);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(InetCidr6);
    }
}

[JsonConverter(typeof(InetCidr6Converter))]
public class InetCidr6 : SynapseType
{
    protected bool Equals(InetCidr6 other)
    {
        return Equals(Value, other.Value);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((InetCidr6)obj);
    }

    public override int GetHashCode()
    {
        return (Value != null ? Value.GetHashCode() : 0);
    }

    public IPNetwork Value { get; }

    private InetCidr6(IPNetwork value)
    {
        Value = value;
    }

    public static implicit operator IPNetwork(InetCidr6 d) => d.Value;
    public static implicit operator InetCidr6(IPNetwork d) => new InetCidr6(d);

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

    public static InetCidr6 Parse(string s)
    {
        return new InetCidr6(IPNetwork.Parse(s));
    }
    
    public static InetCidr6 Convert(object o)
    {
        if (o is string str)
            return Parse(str);
        
        if (o is IPNetwork address)
            return new InetCidr6(address);

        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(InetCidr6).FullName}'");
    }
}