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
using Newtonsoft.Json;

namespace Synsharp.Types;

public class HexConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((Hex)value).Value);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return Str.Parse((string)reader.Value);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Hex);
    }
}

[JsonConverter(typeof(HexConverter))]
public class Hex : SynapseType
{
    protected bool Equals(Hex other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Hex)obj);
    }

    public override int GetHashCode()
    {
        return (Value != null ? Value.GetHashCode() : 0);
    }

    public string Value { get; }

    protected Hex(string value)
    {
        Value = value;
    }

    public static implicit operator string(Hex d) => d.Value;
    public static implicit operator Hex(string d) => new Hex(d);

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

    public static Hex Parse(string s)
    {
        return new Hex(s);
    }
    
    public static Hex Convert(object o)
    {
        if (o is string str)
            return Parse(str);
        if (o is Hex x)
            return x;

        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(Hex).FullName}'");
    }
}