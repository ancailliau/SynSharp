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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Synsharp;

/// <summary>
/// Represents the meta data of a synapse object.
/// </summary>
public class SynapseObjectMetaData
{
    [JsonProperty("iden")] public string Iden { get; set; }
    [JsonProperty("props")] public Dictionary<string, object> Props { get; set; }
    [JsonProperty("tags")] public Dictionary<string, object[]> Tags { get; set; }
    [JsonProperty("path")] public SynapsePath Path { get; set; }
        
    // TODO tagprops
    // TODO nodedata
}