using Newtonsoft.Json;

namespace PlexLightSwitcher.Models
{
    public class NeviWebLocation
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
