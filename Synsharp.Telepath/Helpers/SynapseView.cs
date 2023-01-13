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

using System.Text;

namespace Synsharp.Telepath.Helpers;

/// <summary>
/// Represents a view.
/// </summary>
public class SynapseView
{
    [Newtonsoft.Json.JsonProperty("iden")] public string Iden { get; set; }
    [Newtonsoft.Json.JsonProperty("creator")] public string Creator { get; set; }
    [Newtonsoft.Json.JsonProperty("name")] public string Name { get; set; } 
    [Newtonsoft.Json.JsonProperty("parent")] public string Parent { get; set; } 
    [Newtonsoft.Json.JsonProperty("nomerge")] public bool? NoMerge { get; set; }
    [Newtonsoft.Json.JsonProperty("layers")] public SynapseLayer[] Layers { get; set; }
    
    // TODO Triggers
    // TODO Parent

    public override string ToString()
    {
        var name = "unnamed";
        if (!string.IsNullOrEmpty(this.Name))
            name = this.Name;
        
        var str = new StringBuilder($"View: {this.Iden} (name: {name})\n");
        str.Append($"  Creator: {this.Creator}\n");
        str.Append("  Layers:\n");
        foreach (var layer in Layers)
        {
            var lname = "unnamed";
            if (!string.IsNullOrEmpty(layer.Name))
                lname = this.Name;
            str.Append($"    {layer.Iden}: {lname} readonly: {layer.ReadOnly}\n");   
        }
        return str.ToString();
    }
}