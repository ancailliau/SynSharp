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
using System.Text;
using Synsharp.Attribute;
using Synsharp.Forms;

namespace Synsharp;

/// <summary>
/// Represents a synapse object
/// </summary>
public class SynapseObject
{
    [SynapseProperty(".created")] protected DateTime Created { set; get; }
    [SynapseProperty("iden")] protected string Iden { set; get; }

    public HashSet<string> Tags { get; set; } = new();
}

/// <summary>
/// Represents a synapse generic object
/// </summary>
/// <typeparam name="T">The core value type</typeparam>
public abstract class SynapseObject<T> : SynapseObject
{
    private T _value;
    
    /// <summary>
    /// Returns the core value of the object.
    /// </summary>
    public T Value => _value;

    private Dictionary<Type, Func<object, T>> _normalizers = new();

    /// <summary>
    /// Initializes a new SynapseObject. 
    /// </summary>
    public SynapseObject()
    {
        AddNorm<T>(s => s);
            
        // Adds conversion from int to IPAddress
        if (typeof(T) == typeof(IPAddress))
        {
            // (T)(object) is needed to outsmart the compiler :nerd_face:
            AddNorm<Int16>(i => (T)(object)IPAddress.Parse(i.ToString()));
            AddNorm<Int32>(i => (T)(object)IPAddress.Parse(i.ToString()));
            AddNorm<Int64>(i => (T)(object)IPAddress.Parse(i.ToString()));
        }
            
        // Adds conversion from string to Hex
        if (typeof(T) == typeof(Hex))
        {
            AddNorm<string>(i => (T)(object)Hex.Parse(i));
        }
            
        // Adds conversion from string to GUID
        if (typeof(T) == typeof(GUID))
        {
            AddNorm<string>(i => (T)(object)GUID.Parse(i));
        }

        // Adds all default conversion from string
        var mi = typeof(T).GetMethod("Parse", 
            BindingFlags.Static | BindingFlags.Public,
            null,
            new Type[] { typeof(string) },
            null);
        if (mi != null) AddNorm<string>(s => (T) (object) mi.Invoke(null, new object?[] {s}));
    }
        
    /// <summary>
    /// Sets the core value of the object
    /// </summary>
    /// <param name="value">The value</param>
    /// <typeparam name="TS">The type of the core value</typeparam>
    public void SetValue<TS>(TS value)
    {
        _value = Norm(value);
    }
        
    protected void AddNorm<S>(Func<S, T> norm)
    {
        if (_normalizers.ContainsKey(typeof(S)))
            _normalizers[typeof(S)] = (s) => norm((S)s);
        else
            _normalizers.Add(typeof(S), (s) => norm((S)s));
    }

    private T Norm<S>(S s)
    {
        try
        {
            if (_normalizers.ContainsKey(s.GetType()))
            {
                return _normalizers[s.GetType()](s);
            }
        }
        catch
        {
            throw new NotImplementedException("Normaliation failed");
        }

        throw new NotImplementedException($"Failed to normalize '{s}' ('{s.GetType().FullName}') to '{typeof(T).FullName}'.");
    }

    private bool Equals(SynapseObject<T> other)
    {
        return EqualityComparer<T>.Default.Equals(_value, other._value);
    }

    private bool Equals(T other)
    {
        return EqualityComparer<T>.Default.Equals(_value, other);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() == this.GetType()) 
            return Equals((SynapseObject<T>) obj);
        if (obj.GetType() == this._value.GetType())
            return Equals((T) obj);
        return false;
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(_value);
    }

    public override string ToString()
    {
        var formAttribute = this.GetType().GetCustomAttribute<SynapseFormAttribute>();

        var sb = new StringBuilder($"{formAttribute.Name}={_value}");
        foreach (var fieldInfo in GetSynapseFields())
        {
            var value = fieldInfo.GetValue(this);
            if (value != null)
            {
                var attribute = fieldInfo.GetCustomAttribute<SynapsePropertyAttribute>() ??
                                throw new ArgumentNullException("field.GetCustomAttribute<SynapsePropAttribute>()");
                PushPropertyToStringBuilder(attribute, value, sb);
            }
        }

        foreach (var propertyInfo in GetSynapseProperties())
        {
            var value = propertyInfo.GetValue(this);
            if (value != null)
            {
                var attribute = propertyInfo.GetCustomAttribute<SynapsePropertyAttribute>() ??
                                throw new ArgumentNullException("field.GetCustomAttribute<SynapsePropAttribute>()");
                PushPropertyToStringBuilder(attribute, value, sb);
            }
        }
            
        if (this.Tags.Any())
            sb.Append($"{Environment.NewLine}\ttags = {string.Join(", ", this.Tags.Select(t => "#" + t))}");

        return sb.ToString();
    }

    private static void PushPropertyToStringBuilder(SynapsePropertyAttribute? attribute, object? value,
        StringBuilder sb)
    {
        if (attribute == null) throw new ArgumentNullException(nameof(attribute));
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (sb == null) throw new ArgumentNullException(nameof(sb));

        var name = attribute.Name;
        if (!name.StartsWith("."))
        {
            name = ":" + name;
        }

        if (value is SynapseObject<string> strValue)
            sb.Append($"{Environment.NewLine}\t{name} = {strValue._value}");
        else if (value is SynapseObject<int> intValue)
            sb.Append($"{Environment.NewLine}\t{name} = {intValue._value}");
        else if (value is SynapseObject<IPAddress> ipValue)
            sb.Append($"{Environment.NewLine}\t{name} = {ipValue._value}");
        else
            sb.Append($"{Environment.NewLine}\t{name} = {value.ToString()}");
    }

    private IEnumerable<FieldInfo> GetSynapseFields()
    {
        return this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(pi => pi.GetCustomAttributes(typeof(SynapsePropertyAttribute), false).Any());
    }

    private IEnumerable<PropertyInfo> GetSynapseProperties()
    {
        return this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(pi => pi.GetCustomAttributes(typeof(SynapsePropertyAttribute), false).Any());
    }
}