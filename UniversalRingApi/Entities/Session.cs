using Newtonsoft.Json;

namespace UniversalRingApi.Entities
{
    public class Session
    {
        [JsonProperty(PropertyName = "profile")]
        public Profile Profile { get; set; }
    }
}
