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
public class InetIPv6Converter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((InetIPv6)value).Value.ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return Str.Parse((string)reader.Value);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(InetIPv6);
    }
}

[JsonConverter(typeof(InetIPv6Converter))]

public class InetIPv6 : SynapseType
{
    protected bool Equals(InetIPv6 other)
    {
        return Equals(Value, other.Value);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((InetIPv6)obj);
    }

    public override int GetHashCode()
    {
        return (Value != null ? Value.GetHashCode() : 0);
    }

    public IPAddress Value { get; }

    private InetIPv6(IPAddress value)
    {
        Value = value;
    }

    public static implicit operator IPAddress(InetIPv6 d) => d.Value;
    public static implicit operator InetIPv6(IPAddress d) => new InetIPv6(d);

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

    public static InetIPv6 Parse(string s)
    {
        return new InetIPv6(IPAddress.Parse(s));
    }
    
    public static InetIPv6 Convert(object o)
    {
        if (o is string str)
            return Parse(str);

        if (o is Int32 int32)
            return Parse(int32.ToString());
        
        if (o is Int64 int64)
            return Parse(int64.ToString());

        if (o is IPAddress ipAddress)
            return new InetIPv6(ipAddress);
        
        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(InetIPv6).FullName}'");
    }
}