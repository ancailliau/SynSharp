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
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Synsharp.Attribute;

namespace Synsharp.Helpers;

public class NodeHelper
{
    private readonly SynapseClient _synapseClient;
    private readonly ILogger<NodeHelper> _logger;

    public NodeHelper(SynapseClient synapseClient, ILogger<NodeHelper> logger)
    {
        _synapseClient = synapseClient;
        _logger = logger;
    }

    public async Task<SynapseObject<T>> Add<T>(SynapseObject<T> synapseObject) where T: SynapseType
    {
        // Get the core value to assign (i.e. all but GUID)
        string value = "*";
        if (synapseObject.Value != null) 
            value = ToSafeStormValue(synapseObject.Value);

        if (string.IsNullOrEmpty(value))
        {
            throw new SynapseException($"Invalid core value for '{synapseObject.GetType().FullName}'");   
        }

        var propertyDict = new Dictionary<string, string>();
        
        // Get the form properties
        var fields = SynapseConverter.GetFields(synapseObject.GetType());
        foreach (var field in fields)
        {
            var propertyAttribute = field.GetCustomAttribute<SynapsePropertyAttribute>();
            if (propertyAttribute != null && !propertyAttribute.Name.StartsWith("."))
            {
                var val = field.GetValue(synapseObject);
                if (val != null) 
                    propertyDict.Add(propertyAttribute.Name, ToSafeStormValue(val));
            }
        }
        
        var properties = SynapseConverter.GetProperties(synapseObject.GetType());
        foreach (var propertyInfo in properties)
        {
            var propertyAttribute = propertyInfo.GetCustomAttribute<SynapsePropertyAttribute>();
            if (propertyAttribute != null && !propertyAttribute.Name.StartsWith("."))
            {
                var val = propertyInfo.GetValue(synapseObject);
                if (val != null)
                {
                    _logger.LogTrace($"Convert value for '{propertyAttribute.Name}': '{val.ToString()}'");
                    propertyDict.Add(propertyAttribute.Name, ToSafeStormValue(val));
                }
            }
        }

        // Build the storm command and execute
        var attribute = synapseObject.GetType().GetCustomAttribute<SynapseFormAttribute>();
        if (attribute != null)
        {
            var type = attribute.Name;
            var attributes = string.Join(" ", propertyDict.Select(_ => $":{_.Key}={_.Value}"));
            var command = $"[ {type}={value} {attributes} ]";
            var results = await _synapseClient.StormAsync<SynapseObject<T>>(command).ToListAsync();
            return (SynapseObject<T>)(object)results.FirstOrDefault();
        }
        else
        {
            throw new SynapseException($"Could not infer form type for '{synapseObject.GetType().FullName}'");
        }
    }

    private static string ToSafeStormValue(object val)
    {
        // TODO Refactor to avoid code duplicates with GetCoreValue in SynapseObject
        if (val == null) throw new ArgumentNullException(nameof(val));
        
        string value = string.Empty;
        if (val is string s)
        {
            value = s.Escape();
        }
        else if (val is IPAddress a)
        {
            value = a.ToString();
        }
        else if (val is Int32 i)
        {
            value = i.ToString();
        }
        else if (val is bool b)
        {
            value = b.ToString();
        }
        else if (val is SynapseType st)
        {
            value = st.GetCoreValue();
        }
        else if (val is SynapseObject so)
        {
            value = so.GetCoreValue();
        }
        else
        {
            throw new System.NotImplementedException($"Value of type '{val.GetType()}' could not be converted.");
        }

        return value;
    }

    public async Task AddLightEdge(SynapseObject o1, SynapseObject o2, string @ref)
    {
        throw new System.NotImplementedException();
    }
}