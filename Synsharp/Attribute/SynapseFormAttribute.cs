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

namespace Synsharp.Attribute;

/// <summary>
/// Represents a Form attribute, used to decorate classes representing forms.
/// </summary>
public class SynapseFormAttribute : System.Attribute
{
    /// <summary>
    /// The name of the form
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A short description of the type
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new attribute decorator for a class representing the specified form.
    /// </summary>
    /// <param name="name">The represented form</param>
    public SynapseFormAttribute(string name)
    {
        Name = name;
    }
}