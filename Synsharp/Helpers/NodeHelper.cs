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
using System.Text.RegularExpressions;
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

    public async IAsyncEnumerable<T> Add<T>(IEnumerable<T> synapseObjects, string view = null) where T : SynapseObject
    {
        foreach (var o in synapseObjects.Batch(50))
        {
            string command = string.Join(" ", $"[ {o.Select(BuildCommand)} ]");

            if (!string.IsNullOrEmpty(command))
            {
                var results = _synapseClient.StormAsync<T>(command,
                    new ApiStormQueryOpts()
                    {
                        View = view
                    });
                await foreach (var r in results)
                {
                    yield return r;
                }
            }   
        }
    }
    
    public async Task<T> Add<T>(T synapseObject, string view = null) where T : SynapseObject
    {
        string command = BuildCommand(synapseObject);

        if (!string.IsNullOrEmpty(command))
        {
            var results = await _synapseClient.StormAsync<T>($"[ {command} ]",
                    new ApiStormQueryOpts()
                    {
                        View = view
                    })
                .ToListAsync();
            return results.FirstOrDefault();
        }

        return null;
    }

    private string BuildCommand<T>(T synapseObject) where T : SynapseObject
    {
        var value = GetCoreValueDynamic(synapseObject);
        var propertyDict = GetPropertyDict(synapseObject);

        var tagRegex = new Regex(@"[a-zA-Z0-9\.]+");
        if (synapseObject.Tags.Any(_ => !tagRegex.Match(_).Success))
            throw new SynapseException("Tags should match [a-zA-Z0-9.]+");

        // Build the storm command and execute
        var attribute = synapseObject.GetType().GetCustomAttribute<SynapseFormAttribute>();
        if (attribute != null)
        {
            var type = attribute.Name;
            var attributes = string.Join(" ", propertyDict.Select(_ => $":{_.Key}={_.Value}"));
            var tags = string.Join(" ", synapseObject.Tags.Select(_ => $"+#{_}"));
            
            return $"{type}={StringHelpers.Escape(value)} {attributes} {tags}";
        }
        else
        {
            throw new SynapseException($"Could not infer form type for '{synapseObject.GetType().FullName}'");
        }
    }

    private Dictionary<string, string> GetPropertyDict<T>(T synapseObject) where T : SynapseObject
    {
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
                        throw new SynapseException(
                            $"The property '{propertyInfo.Name}' has no value or value is not a SynapseType.");
                    }
                }
            }
        }

        return propertyDict;
    }

    private static string GetCoreValueDynamic<T>(T synapseObject) where T : SynapseObject
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

        return value;
    }

    private (string type, string value) GetSelector(SynapseObject @object)
    {
        var type = default(string);
        var attribute = @object.GetType().GetCustomAttribute<SynapseFormAttribute>();
        if (attribute != null)
        {
            type = attribute.Name;
        } else {
            throw new SynapseException($"Missing SynapseFormAttribute on class '{@object.GetType().FullName}'");   
        }
        
        var val = GetCoreValueDynamic(@object);
        if (string.IsNullOrEmpty(val))
        {
            throw new SynapseException($"Invalid core value for '{@object.GetType().FullName}'");   
        }
        
        return (type, val);
    }
    
    public async Task AddLightEdge(IEnumerable<SynapseLightEdge> edges, string view = null)
    {
        var commands = new List<string>();

        foreach (var edge in edges)
        {
            var (t1, v1) = GetSelector(edge.Source);
            var (t2, v2) = GetSelector(edge.Target);
            commands.Add($"{t1}={StringHelpers.Escape(v1)} [ <({edge.Verb})+ {{ {t2}={StringHelpers.Escape(v2)} }} ]");   
        }
        
        _ = await _synapseClient.StormAsync<object>(string.Join(" ", commands), new ApiStormQueryOpts(){View= view}).ToListAsync();
    }
    
    public async Task AddLightEdge(SynapseObject o1, SynapseObject o2, string @ref, string view = null)
    {
        var (t1, v1) = GetSelector(o1);
        var (t2, v2) = GetSelector(o2);

        var command = $"{t1}={StringHelpers.Escape(v1)} [ <({@ref})+ {{ {t2}={StringHelpers.Escape(v2)} }} ]";
        _ = await _synapseClient.StormAsync<object>(command, new ApiStormQueryOpts(){View= view}).ToListAsync();
    }

    public async Task RemoveLightEdge(SynapseObject o1, SynapseObject o2, string @ref, string view = null)
    {
        var (t1, v1) = GetSelector(o1);
        var (t2, v2) = GetSelector(o2);

        var command = $"{t1}={StringHelpers.Escape(v1)} [ <({@ref})- {{ {t2}={StringHelpers.Escape(v2)} }} ]";
        _ = await _synapseClient.StormAsync<object>(command, new ApiStormQueryOpts(){View= view}).ToListAsync();
    }

    /// <summary>
    /// Returns the node identified by the specified iden
    /// </summary>
    /// <param name="iden">The identifier</param>
    /// <param name="view">The view</param>
    /// <typeparam name="T">The type of the node</typeparam>
    /// <returns>The node</returns>
    public async Task<T> GetAsync<T>(string iden, string view = null)
    {
        return await _synapseClient.StormAsync<T>("",
                new ApiStormQueryOpts()
                {
                    View = view,
                    Idens = new[]
                    {
                        iden
                    }
                })
            .SingleOrDefaultAsync();
    }
    
    public IAsyncEnumerable<T> GetAllAsync<T>(string view = null)
    {
        var attribute = typeof(T).GetCustomAttribute<SynapseFormAttribute>();
        if (attribute != null)
        {
            var type = attribute.Name;
            return _synapseClient.StormAsync<T>($"{type}",
                    new ApiStormQueryOpts()
                    {
                        View = view
                    });
        }

        return AsyncEnumerable.Empty<T>();
    }

    public async Task Remove(string iden, string viewIden = null)
    {
        if (iden == null) throw new ArgumentNullException(nameof(iden));
        _ = (await _synapseClient.StormAsync<SynapseObject>(
                $"| delnode",
                new ApiStormQueryOpts() { View = viewIden, Idens = new[] { iden } })
            .ToListAsync());
    }

    public async Task AddTag(string iden, string tagName, string viewIden = null)
    {
        if (iden == null) throw new ArgumentNullException(nameof(iden));
        var tagRegex = new Regex(@"[a-zA-Z0-9\.]+");
        if (!tagRegex.Match(tagName).Success)
            throw new SynapseException("Tags should match [a-zA-Z0-9.]+");
        _ = (await _synapseClient.StormAsync<SynapseObject>(
                $"[ +#{tagName} ]",
                new ApiStormQueryOpts() { View = viewIden, Idens = new[] { iden } })
            .ToListAsync());
    }

    public async Task RemoveTag(string iden, string tagName, string viewIden = null)
    {
        if (iden == null) throw new ArgumentNullException(nameof(iden));
        var tagRegex = new Regex(@"[a-zA-Z0-9\.]+");
        if (!tagRegex.Match(tagName).Success)
            throw new SynapseException("Tags should match [a-zA-Z0-9.]+");
        _ = (await _synapseClient.StormAsync<SynapseObject>(
                $"[ -#{tagName} ]",
                new ApiStormQueryOpts() { View = viewIden, Idens = new[] { iden } })
            .ToListAsync());
    }

    public IAsyncEnumerable<T> GetAsyncByProperty<T>(Dictionary<string,string> propertyValues, string view = null)
    {
        var attribute = typeof(T).GetCustomAttribute<SynapseFormAttribute>();
        if (attribute != null)
        {
            var type = attribute.Name;

            string selector = "{type}";
            if (propertyValues.Count == 1)
            {
                var p = propertyValues.Single();
                selector = $"{type}:{p.Key}={p.Value}";
            }
            else if (propertyValues.Count > 1)
            {
                var plist = propertyValues.ToList();
                var p = plist.First();
                var attributes = string.Join(" ", plist.Skip(1).Select(_ => $"+:{_.Key}={_.Value}"));
                selector = $"{type}:{p.Key}={p.Value} {attributes}";
            }
            
            return _synapseClient.StormAsync<T>(selector,new ApiStormQueryOpts(){View= view});
        }
        else
        {
            throw new SynapseException($"Could not infer form type for '{typeof(T).FullName}'");
        }
    }
}