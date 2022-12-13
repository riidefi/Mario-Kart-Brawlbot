using Newtonsoft.Json;

namespace MKBB.Commands
{
    public class Tool
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("creators")]
        public string Creators { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("download")]
        public string Download { get; set; }
    }
}