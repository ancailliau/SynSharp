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

using System.Text;
using Microsoft.Extensions.Logging;
using Synsharp.Telepath.Messages;

namespace Synsharp.Telepath.Helpers;

public class NodeHelper
{
    private readonly TelepathClient _synapseTelepathClient;
    private readonly ILogger<NodeHelper> _logger;

    public NodeHelper(TelepathClient synapseTelepathClient, ILogger<NodeHelper> logger)
    {
        _synapseTelepathClient = synapseTelepathClient;
        _logger = logger;
    }

    public async Task<SynapseNode?> AddAsync(SynapseNode node, StormOps? opts = null)
    {
        var variables = new Dictionary<string, dynamic>();
        var stormQuery = new StringBuilder();

        stormQuery.Append("[");

        stormQuery.Append($" *$form=$valu ");

        if (node.Props != null)
            foreach (var prop in node.Props)
            {
                stormQuery.Append($" :{prop.Key}=${prop.Key} ");
                variables.Add(prop.Key, prop.Value);
            }

        if (node.Tags != null)
            foreach (var prop in node.Tags)
            {
                stormQuery.Append($" +#{prop.Key} ");
            }

        stormQuery.Append("]");
                    
        variables.Add("form", node.Form);
        variables.Add("valu", node.Valu);
        
        opts.Vars = variables ;
        var proxy = await _synapseTelepathClient.GetProxyAsync();
        await foreach (var o in proxy.Storm(stormQuery.ToString(), opts))
        {
            if (o is SynapseNode synapseNode)
                return synapseNode;
        }

        return null;
    }

    public async IAsyncEnumerable<SynapseNode?> AddAsync(List<SynapseNode> nodes, StormOps? stormOps = null)
    {
        foreach (var node in nodes)
        {
            yield return await AddAsync(node, stormOps);
        }
    }

    public async IAsyncEnumerable<SynapseNode> AddLightEdgeAsync(SynapseNode[] nodes, string verb, SynapseNode docNode, StormOps? stormOps = null)
    {
        var command = "[ <($verb)+ { *$form=$valu } ]";
        var vars = new Dictionary<string, dynamic>()
        {
            { "verb", verb },
            { "form", docNode.Form },
            { "valu", docNode.Valu }
        };
        var ops = stormOps ?? new StormOps();
        ops.Vars = vars;
        ops.Idens = nodes.Select(_ => _.Iden).ToArray();
        if (ops.Idens.Length > 0)
        {
            var proxy = await _synapseTelepathClient.GetProxyAsync();
            await foreach (var message in proxy.Storm(command, ops))
                if (message is SynapseNode node)
                    yield return node;
        }
        else
        {
            _logger?.LogDebug("No nodes for light edge");
        }
    }

    public async Task RemoveLightEdgeAsync(string[] iden, string verb, SynapseNode docNode, StormOps? stormOps = null)
    {
        var command = "[ <($verb)- { *$form=$valu } ]";
        var vars = new Dictionary<string, dynamic>()
        {
            { "verb", verb },
            { "form", docNode.Form },
            { "valu", docNode.Valu }
        };
        var ops = stormOps ?? new StormOps();
        ops.Vars = vars;
        ops.Idens = iden;

        var proxy = await _synapseTelepathClient.GetProxyAsync();
        await proxy.Storm(command, ops).ToListAsync();
    }

    public async Task DeleteAsync(SynapseNode docNode, StormOps? stormOps = null)
    {
        var command = "*$form=$valu | delnode ";
        var ops = stormOps ?? new StormOps();
        ops.Vars = new Dictionary<string, dynamic>() { { "form", docNode.Form }, { "valu", docNode.Valu } };

        var proxy = await _synapseTelepathClient.GetProxyAsync();
        await proxy.Storm(command, ops).ToListAsync();
    }

    public async Task DeleteAsync(string nodeIden, StormOps? stormOps = null)
    {
        var command = "| delnode ";
        var ops = stormOps ?? new StormOps();
        ops.Idens = new [] { nodeIden };

        var proxy = await _synapseTelepathClient.GetProxyAsync();
        await proxy.Storm(command, ops).ToListAsync();
    }

    public async Task AddTag(string nodeIden, string tagName, StormOps? stormOps = null)
    {
        var command = $" [ +#$tagName ] ";
        var ops = stormOps ?? new StormOps();
        ops.Idens = new [] { nodeIden };
        ops.Vars = new Dictionary<string, dynamic>() { { "tagName", tagName } };

        var proxy = await _synapseTelepathClient.GetProxyAsync();
        await proxy.Storm(command, ops).ToListAsync();
    }

    public async Task RemoveTag(string nodeIden, string tagName, StormOps? stormOps = null)
    {
        var command = $" [ -#$tagName ] ";
        var ops = stormOps ?? new StormOps();
        ops.Idens = new [] { nodeIden };
        ops.Vars = new Dictionary<string, dynamic>() { { "tagName", tagName } };

        var proxy = await _synapseTelepathClient.GetProxyAsync();
        await proxy.Storm(command, ops).ToListAsync();
    }

    public async Task<SynapseNode?> GetAsync(string nodeIden, StormOps? stormOps = null)
    {
        var command = "";
        var ops = stormOps ?? new StormOps();
        ops.Idens = new [] { nodeIden };

        var proxy = await _synapseTelepathClient.GetProxyAsync();
        return await proxy.Storm(command, ops).OfType<SynapseNode>().FirstOrDefaultAsync();
    }
}