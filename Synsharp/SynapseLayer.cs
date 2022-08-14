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

namespace Synsharp;

/// <summary>
/// Represents a layer
/// </summary>
public class SynapseLayer
{
    [Newtonsoft.Json.JsonProperty("iden")] public string Iden { get; set; }
    [Newtonsoft.Json.JsonProperty("creator")] public string Creator { get; set; }
    [Newtonsoft.Json.JsonProperty("lockmemory")] public bool? LockMemory { get; set; }
    [Newtonsoft.Json.JsonProperty("logedits")] public bool? LogEdits { get; set; } 
    [Newtonsoft.Json.JsonProperty("name")] public string Name { get; set; } 
    [Newtonsoft.Json.JsonProperty("readonly")] public bool? ReadOnly { get; set; } 
    [Newtonsoft.Json.JsonProperty("totalsize")] public long? TotalSize { get; set; } 
    [Newtonsoft.Json.JsonProperty("model:version")] public int[] ModelVersion { get; set; }
}