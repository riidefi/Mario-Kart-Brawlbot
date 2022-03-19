using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace CTTB.Commands
{
    public class Track
    {
        [JsonProperty("categoryId")]
        public int Category { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("trackId")]
        public string SHA1 { get; set; }
        [JsonProperty("fastestTimeSimple")]
        public string BestTime { get; set; }
        [JsonProperty("lastChanged")]
        public DateTime TrackAdded { get; set; }
        public string WiimmfiName { get; set; }
        [JsonProperty("popularity")]
        public int TimeTrialScore { get; set; }
        public int WiimmfiScore { get; set; }
    }

    public class LeaderboardInfo
    {
        [JsonProperty("leaderboards")]
        public List<Track> Leaderboard { get; set; }
    }
}