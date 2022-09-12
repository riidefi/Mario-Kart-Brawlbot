using Newtonsoft.Json;
using OpenQA.Selenium.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
