using Newtonsoft.Json;

namespace MKBB.Class
{
    public class ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
        [JsonProperty("councilUrl")]
        public string CouncilUrl { get; private set; }
        [JsonProperty("adminUrl")]
        public string AdminUrl { get; private set; }
    }
}