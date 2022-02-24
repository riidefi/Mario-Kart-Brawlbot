using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTTB.Commands
{
    public class CouncilMember
    {
        [JsonProperty("name")]
        public string SheetName { get; set; }
        [JsonProperty("id")]
        public ulong DiscordId { get; set; }
    }
}
