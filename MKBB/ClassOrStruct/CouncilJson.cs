using Newtonsoft.Json;
using System.Collections.Generic;

namespace MKBB.Commands
{
    public class CouncilMember
    {
        [JsonProperty("name")]
        public string SheetName { get; set; }
        [JsonProperty("id")]
        public ulong DiscordId { get; set; }
        [JsonProperty("comp")]
        public bool CompPlayer { get; set; }
        [JsonProperty("missedhw")]
        public int TimesMissedHw { get; set; }
        [JsonProperty("assignedthreads")]
        public List<ulong> AssignedThreadIds { get; set; }
        [JsonProperty("completedhw")]
        public int HwInARow { get; set; }
    }
}
