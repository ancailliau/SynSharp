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

using System.Threading.Tasks;

namespace Synsharp.Helpers;

/// <summary>
/// Provides helper functions for layers.
/// </summary>
public class LayerHelper
{
    private readonly SynapseClient _client;

    /// <summary>
    /// Initializes a new LayerHelper.
    /// </summary>
    /// <param name="client">A synapse client</param>
    public LayerHelper(SynapseClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Adds a new layer
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>The layer</returns>
    public Task<SynapseLayer> AddAsync(string name)
    {
        var command = $"$layr = $lib.layer.add($lib.dict(name={name.Escape()})) return ($layr)";
        return _client.StormCallAsync<SynapseLayer>(command);
    }

    /// <summary>
    /// Returns the layer identified by the provided identifier.
    /// </summary>
    /// <param name="iden">Identifier</param>
    /// <returns>The layer</returns>
    public Task<SynapseLayer> GetAsync(string iden)
    {
        var command = $"return($lib.layer.get({iden}))";
        return _client.StormCallAsync<SynapseLayer>(command);
    }

    /// <summary>
    /// Returns the layers.
    /// </summary>
    /// <returns>The layers</returns>
    public Task<SynapseLayer[]> ListAsync()
    {
        return _client.StormCallAsync<SynapseLayer[]>($"return($lib.layer.list())");
    }
}