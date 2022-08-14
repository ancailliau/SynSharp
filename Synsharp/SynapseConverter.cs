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
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Synsharp.Attribute;

namespace Synsharp;

public class SynapseConverter
{
    private static Dictionary<Type, Delegate> _instanceCreationMethods = new();

    private static Dictionary<Type, Dictionary<string, FieldInfo>> _cachedField = new();
    private static Dictionary<Type, Dictionary<string, PropertyInfo>> _cachedProperty = new();

    public static object? Deserialize(Type type, JToken? value, JToken? meta)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (meta == null) throw new ArgumentNullException(nameof(meta));
        var coreValue = ConvertFromJTokenTypeToBaseType(value);
        return GetInstance(type, coreValue, meta.ToObject<SynapseObjectMetaData>());
    }

    private static object ConvertFromJTokenTypeToBaseType(JToken? value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
            
        object coreValue = null;
        if (value.Type == JTokenType.Array)
        {
            coreValue = value.Select(t => ConvertFromJTokenTypeToBaseType(t)).ToArray();
        }
        else if (value.Type == JTokenType.String)
        {
            coreValue = value.Value<string>();
        }
        else if (value.Type == JTokenType.Integer)
        {
            coreValue = value.Value<Int64>();
        }
        else if (value.Type == JTokenType.Boolean)
        {
            coreValue = value.Value<bool>();
        }

        return coreValue;
    }

    private static object? GetInstance<T>(Type type, T coreValue, SynapseObjectMetaData meta = null)
    {
        Delegate constructorCallingLambda = null;

        if (!_instanceCreationMethods.ContainsKey(type))
        {
            var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public,
                null,
                CallingConventions.HasThis,
                Array.Empty<Type>(),
                Array.Empty<ParameterModifier>());

            var expressionCall = Expression.New(constructor);
            constructorCallingLambda = Expression.Lambda(expressionCall, null).Compile();
            _instanceCreationMethods[type] = constructorCallingLambda;
        }
        else
        {
            constructorCallingLambda = _instanceCreationMethods[type];
        }

        if (constructorCallingLambda != null)
        {
            var instance = constructorCallingLambda.DynamicInvoke();
            if (instance != null)
            {
                SetValue(type, coreValue, instance);
                if (meta != null) SetMetadata(type, meta, instance);

                return instance;
            }
            else
            {
                throw new Exception("Could not create instance.");
            }
        }
        else
        {
            throw new Exception("Expression for calling constructor could not be built.");
        }

        return default(object);
    }

    private static void SetValue<T>(Type type, T coreValue, object? instance)
    {
        MethodInfo method = type.GetMethod(nameof(SynapseObject<T>.SetValue));
        if (method is not null)
        {
            MethodInfo generic = method.MakeGenericMethod(typeof(T));
            generic.Invoke(instance, new object[]{ coreValue });
        }
        else
            throw new Exception($"Type '{type.FullName}' does not contain a 'SetValue' method.");
    }
 
    private static void SetMetadata(Type type, SynapseObjectMetaData? metaObj, object? i)
    {
        if (!_cachedField.ContainsKey(type) & !_cachedProperty.ContainsKey(type))
        {
            _cachedField[type] = new();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(pi => pi.GetCustomAttributes(typeof(SynapsePropertyAttribute), false).Any());
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<SynapsePropertyAttribute>();
                if (attribute != null)
                {
                    _cachedField[type].Add(attribute.Name, field);
                }
            }

            _cachedProperty[type] = new();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(pi => pi.GetCustomAttributes(typeof(SynapsePropertyAttribute), false).Any());
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<SynapsePropertyAttribute>();
                if (attribute != null)
                {
                    _cachedProperty[type].Add(attribute.Name, property);
                }
            }
        }
            
        foreach (var kv in metaObj.Tags)
        {
            ((SynapseObject)i).Tags.Add(kv.Key);
        }
            
        foreach (var kv in metaObj.Props)
        {
            if (_cachedField[type].ContainsKey(kv.Key))
            {
                var fieldInfo = _cachedField[type][kv.Key];
                fieldInfo.SetValue(i, Convert(kv.Value.GetType(), fieldInfo.FieldType, kv.Value));
            } else if (_cachedProperty[type].ContainsKey(kv.Key))
            {
                var fieldInfo = _cachedProperty[type][kv.Key];
                fieldInfo.SetValue(i, Convert(kv.Value.GetType(), fieldInfo.PropertyType, kv.Value));
            }
        }

        // Set the iden value
        if (_cachedField[type].ContainsKey("iden"))
        {
            var fieldInfo = _cachedField[type]["iden"];
            fieldInfo.SetValue(i, metaObj.Iden);
        }
        else if (_cachedProperty[type].ContainsKey("iden"))
        {
            var propertyInfo = _cachedProperty[type]["iden"];
            propertyInfo.SetValue(i, metaObj.Iden);
        }

        var pathContainer = metaObj.Path;
        // TODO Path
    }

    private static object? Convert(Type from, Type to, object value)
    {
        if (to == from)
            return value;
            
        if (to == typeof(DateTime))
        {
            if (from == typeof(Int64))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds((Int64) value).UtcDateTime;
            }
            else if (from == typeof(Int32))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds((Int32) value).UtcDateTime;
            }
            else if (from == typeof(long))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds((long) value).UtcDateTime;
            }
            else
            {
                throw new NotImplementedException($"Cannot convert from '{from.FullName}' to '{to.FullName}'");
            }
        }
        else if (to == typeof(Int32))
        {
            return System.Convert.ToInt32(value);
        }
        else if (to.IsSubclassOf(typeof(SynapseObject)))
        {
            return GetInstance(to, value.ToString());
        }
            
        throw new NotImplementedException($"Cannot convert from '{from.FullName}' to '{to.FullName}'");
    }
}