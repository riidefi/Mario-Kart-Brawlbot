using MKBB.Class;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MKBB.Data
{
    public class PlayerData
    {
        [Key] public int ID { get; set; }
        public string PlayerID { get; set; }
        public ulong DiscordID { get; set; }
        public string PlayerLink { get; set; }
    }

    public class Profile
    {
        [JsonProperty("miiName")]
        public string MiiName { get; set; }
        [JsonProperty("controller")]
        public int ControllerID { get; set; }
        [JsonProperty("stars")]
        public Stars Stars { get; set; }
        [JsonProperty("ghosts")]
        public List<Ghost> Ghosts { get; set; }
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