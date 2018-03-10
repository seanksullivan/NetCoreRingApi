using Newtonsoft.Json;

namespace UniversalRingApi.Entities
{
    public class DoNotDisturb
    {
        [JsonProperty(PropertyName = "seconds_left")]
        public int SecondsLeft { get; set; }
    }
}
