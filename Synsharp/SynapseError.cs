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

namespace Synsharp;

/// <summary>
/// Represents an error, as returned by a synapse server.
/// </summary>
public class SynapseError : Exception
{
    public string Code { get; }

    /// <summary>
    /// Initializes a SynapseError instance.
    /// </summary>
    /// <param name="code">The error code, as returned by the server.</param>
    /// <param name="message">The error message, as returned by the server.</param>
    public SynapseError(string code, string message) : base(code + ": " + message)
    {
        Code = code;
    }
}