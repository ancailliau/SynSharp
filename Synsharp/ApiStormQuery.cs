using Newtonsoft.Json;

namespace Synsharp
{
    public class ApiStormQuery
    {
        [JsonProperty("opts")] public ApiStormQueryOpts Opts { get; set; }
        [JsonProperty("query")] public string Query { get; set; }
    }
}