using Newtonsoft.Json;

namespace UniversalRingApi.Entities
{
    public class DoorbotHistoryEventRecording
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }

}
