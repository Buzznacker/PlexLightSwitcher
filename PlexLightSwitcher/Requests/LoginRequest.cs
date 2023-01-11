using Newtonsoft.Json;

namespace PlexLightSwitcher.Requests
{
    public class LoginRequest
    {
        public LoginRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }

        [JsonProperty("username")]
        public string Username { get; set; } 
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("interface")]
        public string InterWeb { get; set; } = "neviweb";
        [JsonProperty("stayConnected")]
        public int StayConnected { get; set; } = 0;
        [JsonProperty("name")]
        public string Name { get; set; } = "iphone 12";
        [JsonProperty("model")]
        public string Model { get; set; } = "s";
        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; } = "Apple";
        [JsonProperty("version")]
        public string Version { get; set; } = "14.4";
    }
}
