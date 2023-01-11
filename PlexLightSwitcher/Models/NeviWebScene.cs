using Newtonsoft.Json;

namespace PlexLightSwitcher.Models
{
    public class NeviWebScene
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int LocationId { get; set; }
        public string Name { get; set; }
        public int Icon { get; set; }
        public int OrderIdx { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime Created { get; set; }
        public List<DeviceAction> Actions { get; set; }
    }

    public class DeviceAction
    {
        public int Id { get; set; }
        [JsonProperty("device$id")]
        public int DeviceId { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public int OrderIdx { get; set; }
    }

}
