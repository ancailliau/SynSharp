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

public class BoolConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((Bool)value).Value);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return Str.Parse((string)reader.Value);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Bool);
    }
}

[JsonConverter(typeof(BoolConverter))]
public class Bool : SynapseType
{
    protected bool Equals(Bool other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Bool)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public bool Value { get; }

    private Bool(bool value)
    {
        Value = value;
    }

    public static implicit operator bool(Bool d) => d.Value;
    public static implicit operator Bool(bool d) => new Bool(d);

    public override string ToString()
    {
        return Value ? "True" : "False";
    }

    public override string GetEscapedCoreValue()
    {
        return Value ? "1" : "0";
    }

    public override string GetCoreValue()
    {
        return Value ? "1" : "0";
    }

    public static Bool Parse(string s)
    {
        return new Bool(Boolean.Parse(s));
    }
}