using Newtonsoft.Json;

namespace UniversalRingApi.Entities
{
    public class ChimeFeatures
    {
        [JsonProperty(PropertyName = "ringtones_enabled")]
        public bool RingtonesEnabled { get; set; }
    }

}
