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
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Synsharp;

/// <summary>
/// Represents a JsonConverter specialized for edges.
/// </summary>
public class EdgeListConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        if (token.Type == JTokenType.Array)
        {
            var edge = new SynapseEdge
            {
                TargetIden = token[0]?.Value<string>() 
                             ?? throw new SynapseException("Could not find the target iden for an edge."),
                Properties = token[1]?.ToObject<Dictionary<string,object>>() 
                             ?? throw new SynapseException("Could not find the properties for an edge."),
            };
            return edge;
        }

        throw new SynapseException($"Could not convert '{token}' to an edge.");
    }

    public override bool CanConvert(Type objectType)
    {
        throw new NotImplementedException();
    }
}