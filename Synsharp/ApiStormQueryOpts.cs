using System.Collections.Generic;
using Newtonsoft.Json;

namespace Synsharp;

public class ApiStormQueryOpts
{
    [JsonProperty("view", NullValueHandling = NullValueHandling.Ignore)] public string View { get; set; }
    [JsonProperty("idens", NullValueHandling = NullValueHandling.Ignore)] public string[] Idens { get; set; }
}