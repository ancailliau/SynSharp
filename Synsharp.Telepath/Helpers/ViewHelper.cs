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

using Microsoft.Extensions.Logging;

namespace Synsharp.Telepath.Helpers;

/// <summary>
/// Provides helper functions for views.
/// </summary>
public class ViewHelper
{
    private readonly TelepathClient _telepathClient;

    /// <summary>
    /// Initializes a new ViewHelper.
    /// </summary>
    /// <param name="telepathClient">A synapse client</param>
    /// <param name="createLogger"></param>
    public ViewHelper(TelepathClient telepathClient, ILogger<ViewHelper> createLogger)
    {
        _telepathClient = telepathClient;
    }

    /// <summary>
    /// Forks the current view (or the view identified by the specified identifier).
    /// </summary>
    /// <param name="iden">The view to fork (empty string for current view)</param>
    /// <param name="name">The name of the new view</param>
    /// <returns>The forked view</returns>
    public async Task<SynapseView?> Fork(string iden = null, string name = null)
    {
        var opts = new StormOps()
        {
            ReadOnly = false,
            Vars = new Dictionary<string, dynamic>()
            {
                { "iden", iden },
                { "name", name }
            }
        };
        
        var proxy = await _telepathClient.GetProxyAsync();
        if (string.IsNullOrEmpty(iden))
            return await proxy.CallStormAsync<SynapseView>("$view=$lib.view.get().fork($name) return($view)", opts);
        else
            return await proxy.CallStormAsync<SynapseView>("$view=$lib.view.get($iden).fork($name) return($view)", opts);
    }
    
    /// <summary>
    /// Returns the view identified by the specified identifier.
    /// </summary>
    /// <param name="iden">The identifier</param>
    /// <returns>The view</returns>
    public async Task<SynapseView?> GetAsync(string iden = "")
    {
        var opts = new StormOps()
        {
            Vars = new Dictionary<string, dynamic>()
            {
                { "iden", iden }
            }
        };
        var proxy = await _telepathClient.GetProxyAsync();
        return await proxy.CallStormAsync<SynapseView>("try { return($lib.view.get($iden)) } catch * as err { return($lib.null) }", opts);
    }
    
    /// <summary>
    /// Returns the list of views.
    /// </summary>
    /// <returns>The views</returns>
    public async Task<SynapseView[]?> List()
    {
        var proxy = await _telepathClient.GetProxyAsync();
        return await proxy.CallStormAsync<SynapseView[]>("return($lib.view.list())");
    }
    
    /// <summary>
    /// Deletes the view identified by the specified identifier.
    /// The method removes the top layer if specified.
    /// </summary>
    /// <param name="iden">The identifier</param>
    public async Task Delete(string iden, bool removeLayer = false)
    {
        if (string.IsNullOrEmpty(iden))
            throw new ArgumentException("You must provide a valid view identifier.");
        
        var opts = new StormOps()
        {
            Vars = new Dictionary<string, dynamic>()
            {
                { "iden", iden },
                { "rm", removeLayer }
            }
        };
        var proxy = await _telepathClient.GetProxyAsync();
        _ = await proxy.CallStormAsync("try { " +
                                       " $view = $lib.view.get($iden)" +
                                       " $lib.view.del($iden)" +
                                       " if ($rm) { " +
                                       "    if ($view) { " +
                                       "        $layr = $view.layers.index(0)" +
                                       "        if ($layr) { $lib.layer.del($layr.iden) } " +
                                       "    } " +
                                       " }" +
                                       "} " +
                                       "catch LayerInUse as err { }" +
                                       "catch NoSuchView as err { }", opts);
    }

    public async Task Merge(string iden)
    {
        var command = "try { $view = $lib.view.get($iden) $view.merge() } catch NoSuchView as err { }";
        var opts = new StormOps()
        {
            Vars = new Dictionary<string, dynamic>()
            {
                { "iden", iden }
            }
        };
        var proxy = await _telepathClient.GetProxyAsync();
        _ = await proxy.CallStormAsync(command, opts);
    }
}
