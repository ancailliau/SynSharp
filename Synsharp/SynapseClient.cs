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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Synsharp.Attribute;
using Synsharp.Helpers;
using Synsharp.Response;

namespace Synsharp;

public class SynapseClient : IDisposable
{
    private readonly ILogger<SynapseClient> _logger;
    public LayerHelper Layer { get; }
    public ViewHelper View { get; }
    public NodeHelper Nodes { get; set; }
    
    public StormInitResponse? Init { get; set; }
    public StormFiniResponse? Fini { get; set; }
    public TextWriter Output { private get; set; }

    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;
    private readonly Uri _baseAddress;
    private static Dictionary<string, Type> _cachedType = new();
    private readonly SynapseSettings _settings;
    private bool _loggedIn;
    private readonly Stream _memoryStream;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new SynapseClient.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <param name="logger">The logger</param>
    public SynapseClient(SynapseSettings settings, ILoggerFactory loggerFactory = null, Stream stream = null) 
        : this(settings.URL, loggerFactory, stream)
    {
        _settings = settings;
    }

    /// <summary>
    /// Initializes a new SynapseClient.
    /// </summary>
    /// <param name="serverUrl">The url to connect to.</param>
    /// <param name="logger">The logger</param>
    /// <param name="stream"></param>
    public SynapseClient(string serverUrl, ILoggerFactory loggerFactory = null, Stream stream = null)
    {
        if (loggerFactory == null)
            _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        else
            _loggerFactory = loggerFactory;
        _logger = (_loggerFactory ?? throw new InvalidOperationException()).CreateLogger<SynapseClient>();
        
        _baseAddress = new Uri(serverUrl);
        _cookieContainer = new CookieContainer();

        if (stream == null)
            _memoryStream = new MemoryStream();
        else
            _memoryStream = stream;
        Output = new StreamWriter(_memoryStream);
        
        var handler = new HttpClientHandler()
        { 
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            CookieContainer = _cookieContainer
        };

        _client = new HttpClient(handler) { BaseAddress = _baseAddress };
            
        Layer = new LayerHelper(this);
        View = new ViewHelper(this);
        Nodes = new NodeHelper(this, (_loggerFactory ?? throw new InvalidOperationException()).CreateLogger<NodeHelper>());
    }

    /// <summary>
    /// Logs in the client with the credentials in the settings.
    /// </summary>
    /// <returns>Whether the client successfully authenticated</returns>
    public Task<bool> LoginAsync() => LoginAsync(_settings.UserName, _settings.Password);
    
    /// <summary>
    /// Logs in the client with the specified credentials.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="password">The password</param>
    /// <returns>Whether the client successfully authenticated</returns>
    public async Task<bool> LoginAsync(string user, string password)
    {
        _logger.LogTrace($"Attempt to login with '{user}'");
        try
        {
            var info = new {user, passwd = password};
            var response = await _client.PostAsJsonAsync("/api/v1/login", info);

            _logger.LogTrace($"Received response '{response.StatusCode}' from server");
            var content = await response.Content.ReadAsStringAsync();
            var contentObject = JsonConvert.DeserializeObject<SynapseResponse<LoginResponse>>(content);
            if (contentObject != null)
            {
                _loggedIn = contentObject.Status == "ok";
                return contentObject.Status == "ok";
            }
        }
        catch (Exception e)
        {
            throw new SynapseException($"Could not login with the user '{user}'.", e);
        }

        return false;
    }

    /// <summary>
    /// Executes a storm query.
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>A task</returns>
    /// <exception cref="SynapseError">If the server returns an error</exception>
    /// <exception cref="SynapseException">If the clients fails to execute the query</exception>
    public Task StormCallAsync(string query) => StormCallAsync<object>(query); 
        
    /// <summary>
    /// Executes a storm query and returns a single result
    /// </summary>
    /// <param name="query">The query</param>
    /// <typeparam name="T">The type of data returned</typeparam>
    /// <returns>The query result</returns>
    /// <exception cref="SynapseError">If the server returns an error</exception>
    /// <exception cref="SynapseException">If the clients fails to execute the query</exception>
    public async Task<T> StormCallAsync<T>(string query)
    {
        if (!_loggedIn) await LoginAsync(_settings.UserName, _settings.Password);
        _logger.LogDebug($"Will send the query '{query}'");
        
        var info = new {query};
        var content = new StringContent(JsonConvert.SerializeObject(info), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/storm/call")
        {
            Content = content,
        };
        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        _logger.LogDebug($"Receveid a server response with response code {response.StatusCode}");
            
        var stream = await response.Content.ReadAsStreamAsync();
        using (StreamReader streamReader = new StreamReader(stream))
        using (JsonReader reader = new JsonTextReader(streamReader) {SupportMultipleContent = true})
        {
            while (true) // reads all returned data
            {
                if (!await reader.ReadAsync())
                {
                    break;
                }

                var token = await JToken.LoadAsync(reader);
                _logger.LogTrace($"Receveid result:\n" + token.ToString());
                if (token is JObject oToken)
                {
                    if (oToken.ContainsKey("status"))
                    {
                        var status = oToken["status"].Value<string>();
                        _logger.LogTrace($"Receveid response with status {status}");
                        if (status == "ok")
                        {
                            if (oToken.ContainsKey("result"))
                            {
                                try
                                {
                                    return oToken["result"].ToObject<T>();
                                }
                                catch (Exception e)
                                {
                                    throw new SynapseException($"Unable to convert response to object", e);
                                }
                            }
                        } else if (status == "err")
                        {
                            var code = oToken["code"]?.Value<string>() ?? "";
                            var mesg = oToken["mesg"]?.Value<string>() ?? "";
                            throw new SynapseError(code, mesg);
                        }
                        else
                        {
                            throw new SynapseException($"Unsupported status: '{status}'");
                        }
                    }
                    else
                    {
                        throw new SynapseException($"Invalid response from server: '{token.ToString()}'");
                    }
                }
                else
                {
                    throw new SynapseException($"Invalid response from server, response is not a JSON token.");
                }
            }
        }
        return default(T);
    }

    /// <summary>
    /// Executes a storm query and returns a list of result
    /// </summary>
    /// <param name="query">The query</param>
    /// <typeparam name="T">The type of data returned</typeparam>
    /// <returns>The query results</returns>
    /// <exception cref="SynapseError">If the server returns an error</exception>
    /// <exception cref="SynapseException">If the clients fails to execute the query</exception>
    public async IAsyncEnumerable<T> StormAsync<T>(string query)
    {
        if (!_loggedIn) await LoginAsync(_settings.UserName, _settings.Password);
        _logger.LogDebug($"Will send the query '{query}'");

        var info = new {query};
        var content = new StringContent(JsonConvert.SerializeObject(info), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/storm")
        {
            Content = content,
        };
        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        _logger.LogDebug($"Receveid a server response with response code {response.StatusCode}");
            
        var stream = await response.Content.ReadAsStreamAsync();
        using (StreamReader streamReader = new StreamReader(stream))
        using (JsonReader reader = new JsonTextReader(streamReader) { SupportMultipleContent = true })
        {
            while (true) // reads all the returned data
            {
                if (!await reader.ReadAsync())
                {
                    break;
                }
                    
                var token = await JToken.LoadAsync(reader);
                if (token.Type == JTokenType.Array)
                {
                    var messageType = token[0].Value<string>();
                    
                    if (messageType == "err")
                    {
                        var element = token[1] as JArray;
                        _logger.LogTrace(token.ToString());
                        var code = element[0]?.Value<string>() ?? "";
                        _logger.LogTrace(element[1].GetType().FullName);
                        var mesg = ((JObject)element[1])["mesg"]?.Value<string>() ?? "";
                        throw new SynapseError(code, mesg);
                    }
                    else if (messageType == "init")
                    {
                        Init = token[1].ToObject<StormInitResponse>();
                    }
                    else if (messageType == "fini")
                    {
                        Fini = token[1].ToObject<StormFiniResponse>();
                        // var plural = Fini.Count > 1 ? "s" : "";
                        // Console.WriteLine($"complete. {Fini.Count} node{plural} in {Fini.Took} ms.");
                    }
                    else if (messageType == "print")
                    {
                        var message = token[1].ToObject<StormPrintResponse>();
                        Output.WriteLine(message.Mesg);
                    }
                    else if (messageType == "node")
                    {
                        _logger.LogTrace($"Receveid node:\n" + token[1].ToString());
                        var responseObject = token[1];
                        if (responseObject.Type is JTokenType.Array)
                        {
                            var objectDefinitionArray = responseObject[0];
                            var objectMetadataObject = responseObject[1];

                            if (objectMetadataObject != null && objectDefinitionArray != null 
                                                             && objectDefinitionArray.Type == JTokenType.Array 
                                                             && objectMetadataObject.Type == JTokenType.Object)
                            {
                                var type = objectDefinitionArray[0]?.Value<string>() ?? "";
                                var inferedType = GetFormCandidates(type);
                                if (inferedType != null)
                                {
                                    var data = SynapseConverter.Deserialize(inferedType,
                                        objectDefinitionArray[1],
                                        objectMetadataObject);
                                    yield return (T) data;
                                }
                                else
                                {
                                    throw new NotSupportedException($"Type '{type}' is not supported.");
                                }
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                        else
                        {
                            throw new SynapseException($"Received an unexpected answer from server: {responseObject.GetType().FullName}");
                        }
                    }
                    else
                    {
                        _logger.LogTrace($"Ignoring response message type: '{messageType}'");
                    }
                }
            }
        }
    }
    
    private static Type GetFormCandidates(string name)
    {
        if (_cachedType.ContainsKey(name))
        {
            return _cachedType[name];
        }
        else
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var customAttributes = type.GetCustomAttributes(typeof(SynapseFormAttribute), true)
                        .Where(_ => (_ as SynapseFormAttribute)?.Name == name);
                    var count = customAttributes.Count();
                    if (count == 1)
                    {
                        _cachedType[name] = type;
                        return type;
                    }

                    if (count > 1)
                    {
                        throw new SynapseException("Multiple classes are decorated with [SynapseFormAttribute(\"{name}\")]");
                    }

                }
            }   
        }

        _cachedType[name] = null;
        return null;
    }

    public void Dispose()
    {
        _client?.Dispose();
        Output?.Dispose();
        _memoryStream?.Dispose();
        _loggerFactory?.Dispose();
    }
}