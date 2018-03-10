using Newtonsoft.Json;

namespace UniversalRingApi.Entities
{
    public class ChimeAlerts
    {
        [JsonProperty(PropertyName = "connection")]
        public string Connection { get; set; }
    }
}
