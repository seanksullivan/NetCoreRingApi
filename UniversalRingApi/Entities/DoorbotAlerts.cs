using Newtonsoft.Json;

namespace UniversalRingApi.Entities
{
    public class DoorbotAlerts
    {
        [JsonProperty(PropertyName = "connection")]
        public string Connection { get; set; }
    }
}
