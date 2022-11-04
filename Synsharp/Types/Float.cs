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

public class FloatConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(((Float)value).Value);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return Str.Parse((string)reader.Value);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Float);
    }
}

[JsonConverter(typeof(FloatConverter))]
public class Float : SynapseType
{
    public float Value { get; }

    private Float(float value)
    {
        Value = value;
    }

    public static implicit operator float(Float d) => d.Value;
    public static implicit operator Float(float d) => new Float(d);

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

    public static Float Parse(string s)
    {
        return new Float(float.Parse(s));
    }
}