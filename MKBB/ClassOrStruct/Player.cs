using Newtonsoft.Json;
using System.Collections.Generic;

namespace MKBB.Commands
{
    public class Player
    {
        [JsonProperty("playerId")]
        public string PlayerId { get; set; }
        [JsonProperty("miiName")]
        public string MiiName { get; set; }
        public ulong DiscordId { get; set; }
        public string PlayerLink { get; set; }
        [JsonProperty("ghosts")]
        public List<Ghost> Ghosts { get; set; }
    }

    public class GetStars
    {
        [JsonProperty("stars")]
        public Stars Stars { get; set; }
    }

    public class Stars
    {
        [JsonProperty("bronze")]
        public int Bronze { get; set; }
        [JsonProperty("silver")]
        public int Silver { get; set; }
        [JsonProperty("gold")]
        public int Gold { get; set; }
    }
}