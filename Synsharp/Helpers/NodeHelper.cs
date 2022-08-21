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

    public async Task<T> Add<T>(T synapseObject) where T : SynapseObject
    {
        var synapseCoreType = synapseObject.GetType();
        if (synapseCoreType.IsSubclassOf(typeof(SynapseObject<>)))
        {
            throw new SynapseException("Cannot add an object that is not 'SynapseObject<>' with node helper.");
        }

        // Get the core value to assign (i.e. all but GUID)
        string value = "*";
        var coreValue = (
                synapseCoreType.GetProperty("Value")
                ?? throw new SynapseException($"Property 'Value' not found on '{synapseCoreType.FullName}'")
            ).GetValue(synapseObject);
        if (coreValue != null)
        {
            var mi = coreValue.GetType().GetMethod("GetCoreValue")
                     ?? throw new SynapseException($"Method 'GetCoreValue' not found on '{coreValue.GetType().FullName}'");
            value = (string)mi.Invoke(coreValue, null); // synapseObject.Value.GetCoreValue()
        }
        
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
                if (val != null && val is SynapseType sval)
                {
                    _logger.LogTrace($"Convert value for '{propertyAttribute.Name}': '{val.ToString()}'");
                    propertyDict.Add(propertyAttribute.Name, sval.GetCoreValue());
                }
                else
                {
                    throw new SynapseException($"The property '{field.Name}' has no value or value is not a SynapseType.");
                }
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
                    if (val is SynapseType sval)
                    {
                        _logger.LogTrace($"Convert value for '{propertyAttribute.Name}': '{val.ToString()}'");
                        propertyDict.Add(propertyAttribute.Name, sval.GetCoreValue());
                    }
                    else
                    {
                        throw new SynapseException($"The property '{propertyInfo.Name}' has no value or value is not a SynapseType.");
                    }
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
            var results = await _synapseClient.StormAsync<T>(command).ToListAsync();
            return results.FirstOrDefault();
        }
        else
        {
            throw new SynapseException($"Could not infer form type for '{synapseObject.GetType().FullName}'");
        }
    }

    private (string type, string value) GetSelector<T>(SynapseObject<T> @object) where T: SynapseType
    {
        var type = default(string);
        var attribute = @object.GetType().GetCustomAttribute<SynapseFormAttribute>();
        if (attribute != null)
        {
            type = attribute.Name;
        } else {
            throw new SynapseException($"Missing SynapseFormAttribute on class '{@object.GetType().FullName}'");   
        }
        
        var val = @object.Value.GetCoreValue();
        if (string.IsNullOrEmpty(val))
        {
            throw new SynapseException($"Invalid core value for '{@object.GetType().FullName}'");   
        }
        
        return (type, val);
    }

    public async Task AddLightEdge<T1,T2>(SynapseObject<T1> o1, SynapseObject<T2> o2, string @ref) where T1: SynapseType where T2: SynapseType
    {
        var (t1, v1) = GetSelector(o1);
        var (t2, v2) = GetSelector(o2);

        var command = $"{t1}={v1} [ <({@ref})+ {{ {t2}={v2} }} ]";
        _ = await _synapseClient.StormAsync<object>(command).ToListAsync();
    }
}