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

namespace Synsharp.Response;

/// <summary>
/// Represents a storm response object embedded in a storm response.
/// </summary>
public class StormResponseObject
{
    [Newtonsoft.Json.JsonProperty("time")] public long? Time { get; set; }
    [Newtonsoft.Json.JsonProperty("user")] public string User { get; set; }
    [Newtonsoft.Json.JsonProperty("ndef")] public string[] Ndef { get; set; }
    [Newtonsoft.Json.JsonProperty("prop")] public string Prop { get; set; }
    [Newtonsoft.Json.JsonProperty("valu")] public object Valu { get; set; }
    [Newtonsoft.Json.JsonProperty("oldv")] public object Oldv { get; set; }
}