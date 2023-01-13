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

namespace Synsharp.Telepath.Helpers;

/// <summary>
/// Provides helper functions for layers.
/// </summary>
public class LayerHelper
{
    private readonly TelepathClient _telepathClient;

    /// <summary>
    /// Initializes a new LayerHelper.
    /// </summary>
    /// <param name="telepathClient">A synapse client</param>
    public LayerHelper(TelepathClient telepathClient)
    {
        _telepathClient = telepathClient;
    }

    /// <summary>
    /// Adds a new layer
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>The layer</returns>
    public async Task<SynapseLayer?> AddAsync(string name)
    {
        var command = "$layr = $lib.layer.add($lib.dict(name=$name)) return ($layr)";
        var proxy = await _telepathClient.GetProxyAsync();
        var opts = new StormOps()
        {
            Vars = new Dictionary<string, dynamic>()
            {
                { "name", name }
            }
        };
        return await proxy.CallStormAsync<SynapseLayer>(command, opts);
    }

    /// <summary>
    /// Returns the layer identified by the provided identifier.
    /// </summary>
    /// <param name="iden">Identifier</param>
    /// <returns>The layer</returns>
    public async Task<SynapseLayer?> GetAsync(string iden)
    {
        var command = "return($lib.layer.get($iden))";
        
        var proxy = await _telepathClient.GetProxyAsync();
        var opts = new StormOps()
        {
            Vars = new Dictionary<string, dynamic>()
            {
                { "iden", iden }
            }
        };
        return await proxy.CallStormAsync<SynapseLayer>(command);
    }

    /// <summary>
    /// Returns the layers.
    /// </summary>
    /// <returns>The layers</returns>
    public async Task<SynapseLayer[]> ListAsync()
    {
        
        var proxy = await _telepathClient.GetProxyAsync();
        return await proxy.CallStormAsync<SynapseLayer[]>($"return($lib.layer.list())");
    }
}