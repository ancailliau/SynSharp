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


public class TimeConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((Time)value).Value.ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return Str.Parse((string)reader.Value);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Time);
    }
}

[JsonConverter(typeof(TimeConverter))]
public class Time : SynapseType
{
    public DateTime Value { get; }

    private Time(DateTime value)
    {
        Value = value;
    }

    public static implicit operator DateTime(Time d) => d.Value;
    public static implicit operator Time(DateTime d) => new Time(d);

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

    public static Time Parse(DateTime s)
    {
        return new Time(s);
    }

    public static Time Convert(object o)
    {
        if (o is Int64 int64)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(int64).UtcDateTime;
        }

        if (o is Int32 int32)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(int32).UtcDateTime;
        }

        throw new NotImplementedException($"Cannot convert from '{o.GetType().FullName}' to '{typeof(CryptoX509Cert).FullName}'");
    }
}