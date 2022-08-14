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
/// Represents a property attribute, used to decorate properties of classes representing properties of forms.
/// </summary>
public class SynapsePropertyAttribute : System.Attribute
{
    /// <summary>
    /// The name of the property
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new SynapsePropertyAttribute.
    /// </summary>
    /// <param name="name">The name of the represented property</param>
    public SynapsePropertyAttribute(string name)
    {
        Name = name;
    }
}