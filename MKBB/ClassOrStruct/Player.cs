using Newtonsoft.Json;
using OpenQA.Selenium.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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