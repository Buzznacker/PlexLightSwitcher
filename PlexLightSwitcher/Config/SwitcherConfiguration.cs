using Newtonsoft.Json;

namespace PlexLightSwitcher.Config
{
    public class SwitcherConfiguration
    {
        public string PlayerName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string LocationName { get; set; }
        public Events EventsToScenes { get; set; }
    }

    public class Events
    {
        [JsonProperty("media.stop")]
        public string MediaStop { get; set; }
        [JsonProperty("media.play.episode")]
        public string MediaPlayEpisode { get; set; }
        [JsonProperty("media.play.movie")]
        public string MediaPlayMovie { get; set; }
    }
}
