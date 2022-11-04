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
using System.Reflection;
using Newtonsoft.Json;
using Synsharp.Attribute;

namespace Synsharp;

/// <summary>
/// Provides helpers for strings
/// </summary>
public static class StringHelpers
{
    /// <summary>
    /// Returns the synapse type, like inet:ipv4.
    /// </summary>
    /// <param name="t">The C# type</param>
    /// <returns>The synapse native type</returns>
    /// <exception cref="Exception">The specified type is not a synapse object.</exception>
    public static string ToSynapseType(this Type t)
    {
        if (t.IsSubclassOf(typeof(Synsharp.SynapseObject)))
        {
            var form = t.GetCustomAttribute<SynapseFormAttribute>();
            if (form == null)
                throw new Exception("Type is not a valid Synapse type");

            return form.Name;
        }
        throw new Exception("Type is not a valid Synapse type");
    }
    
    /// <summary>
    /// Normalizes a string such that all words are separated by one space only.
    /// </summary>
    /// <param name="str">A string</param>
    /// <returns>A normalized string</returns>
    public static string OneSpace(this string str) =>
        string.Join(" ",
            str.Split(new char[0], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    /// <summary>
    /// Escapes a string
    /// </summary>
    /// <param name="str">A string</param>
    /// <returns>An escaped string</returns>
    public static string Escape(this string str) => JsonConvert.ToString(str);
}