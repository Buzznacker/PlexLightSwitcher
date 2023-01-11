using Newtonsoft.Json;

namespace PlexLightSwitcher.Models
{
    public class PlexMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; }
        public Player Player { get; set; }
        public Metadata Metadata { get; set; }

    }

    public class Player
    {
        [JsonProperty("local")]
        public bool Local { get; set; }
        [JsonProperty("publicAddress")]
        public string PublicAddress { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
 }
