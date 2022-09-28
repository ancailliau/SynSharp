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
using Newtonsoft.Json;
using Synsharp.Attribute;
using Synsharp.Forms;
using Synsharp.Types;

namespace Synsharp;

/// <summary>
/// Represents a synapse object
/// </summary>
public abstract class SynapseObject
{
    [SynapseProperty(".created")] public Time Created { set; get; }
    [SynapseProperty("iden")] public Str Iden { set; get; }

    public TagTree Tags { get; set; } = new();

    public abstract string GetCoreValue();
}

/// <summary>
/// Represents a synapse generic object
/// </summary>
/// <typeparam name="T">The core value type</typeparam>
public abstract class SynapseObject<T> : SynapseObject where T: SynapseType
{
    protected T _value;
    
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
    }

    public override string GetCoreValue()
    {
        if (_value == null) throw new ArgumentNullException(nameof(_value));
        
        string value = string.Empty;
        if (_value is string s)
        {
            value = s.Escape();
        }
        else if (_value is IPAddress a)
        {
            value = a.ToString();
        }
        else if (_value is Int32 i)
        {
            value = i.ToString();
        }
        else if (_value is SynapseType st)
        {
            value = st.GetCoreValue();
        }
        else if (_value is SynapseObject so)
        {
            value = so.GetCoreValue();
        }
        else
        {
            throw new System.NotImplementedException($"Value of type '{_value.GetType()}' could not be converted.");
        }

        return value;
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
        
    private T Norm<S>(S s)
    {
        // TODO Avoid duplicates...
        MethodInfo method = typeof(T).GetMethod("Convert", BindingFlags.Public | BindingFlags.Static);
        if (method is not null)
        {
            return (T)method.Invoke(null, new object[]{ s });
        }

        throw new SynapseException($"Type '{typeof(T).FullName}' does not contain a public static method 'Convert'.");
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