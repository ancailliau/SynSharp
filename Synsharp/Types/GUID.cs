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

public class GUIDConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((GUID) value).Value);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return Str.Parse((string)reader.Value);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Guid);
    }
}

[JsonConverter(typeof(GUIDConverter))]
public class GUID : SynapseType
{
    protected bool Equals(GUID other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((GUID)obj);
    }

    public override int GetHashCode()
    {
        return (Value != null ? Value.GetHashCode() : 0);
    }

    public string Value { get; }

    protected GUID(string value)
    {
        Value = value;
    }

    public static implicit operator string(GUID d) => d.Value;
    public static implicit operator GUID(string d) => new GUID(d);

    public override string ToString()
    {
        return Value;
    }

    public override string GetEscapedCoreValue()
    {
        return string.IsNullOrEmpty(Value) ? "*" : Value;
    }

    public override string GetCoreValue()
    {
        return Value.ToString();
    }

    public static GUID Parse(string s)
    {
        return new GUID(s);
    }
}