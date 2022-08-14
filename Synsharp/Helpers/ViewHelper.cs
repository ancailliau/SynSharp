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
using System.Threading.Tasks;

namespace Synsharp.Helpers;

/// <summary>
/// Provides helper functions for views.
/// </summary>
public class ViewHelper
{
    private readonly SynapseClient _client;

    /// <summary>
    /// Initializes a new ViewHelper.
    /// </summary>
    /// <param name="client">A synapse client</param>
    public ViewHelper(SynapseClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Forks the current view (or the view identified by the specified identifier).
    /// </summary>
    /// <param name="iden">The view to fork (empty string for current view)</param>
    /// <param name="name">The name of the new view</param>
    /// <returns>The forked view</returns>
    public async Task<SynapseView> Fork(string iden = "", string name = null)
    {
        var setName = "";
        if (!string.IsNullOrEmpty(name))
        {
            setName = $"$view.set(name, {name.Escape()})";
        }
        
        return await _client.StormCallAsync<SynapseView>($"$view=$lib.view.get({iden}).fork() {setName} return($view)");
    }
    
    /// <summary>
    /// Returns the view identified by the specified identifier.
    /// </summary>
    /// <param name="iden">The identifier</param>
    /// <returns>The view</returns>
    public async Task<SynapseView> GetAsync(string iden = "")
    {
        return await _client.StormCallAsync<SynapseView>($"return($lib.view.get({iden}))");
    }
    
    /// <summary>
    /// Returns the list of views.
    /// </summary>
    /// <returns>The views</returns>
    public async Task<SynapseView[]> List()
    {
        return await _client.StormCallAsync<SynapseView[]>("return($lib.view.list())");
    }
    
    /// <summary>
    /// Deletes the view identified by the specified identifier.
    /// </summary>
    /// <param name="iden">The identifier</param>
    public async Task Delete(string iden)
    {
        if (string.IsNullOrEmpty(iden))
            throw new ArgumentException("You must provide a valid view identifier.");
        
        await _client.StormCallAsync($"$lib.view.del({iden})");
    }
}