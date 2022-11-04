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

public class IntConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((Int)value).Value.ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return Str.Parse((string)reader.Value);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Int);
    }
}

[JsonConverter(typeof(IntConverter))]
public class Int : SynapseType
{
    public long Value { get; }

    protected bool Equals(Int other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj is int i) return Value == i;
        if (obj is long l) return Value == l;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Int)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    protected Int(long value)
    {
        Value = value;
    }
    public static implicit operator long(Int d) => d.Value;
    public static implicit operator Int(long d) => new Int(d);
    public static implicit operator Int(int d) => new Int(d);
    
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

    public static Int Parse(string s)
    {
        return new Int(int.Parse(s));
    }
    
    public static Int Convert(object o)
    {
        if (o is string str)
            return Parse(str);

        if (o is Int64 int64)
            return new Int(int64);
        
        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(Int).FullName}'");
    }
}