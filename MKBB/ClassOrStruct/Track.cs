using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace MKBB.Commands
{
    public class Track
    {
        [JsonProperty("_links")]
        public Link Link { get; set; }
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
        public string LeaderboardLink { get; set; }
        public string BKTLink { get; set; }
        public string BKTHolder { get; set; }
        [JsonProperty("fastestTimeLastChange")]
        public string BKTUploadTime { get; set; }
        public string CategoryName { get; set; }
        public string SlotID { get; set; }
    }

    public class Link
    {
        [JsonProperty("item")]
        public Href Href { get; set; }
    }

    public class Href
    {
        [JsonProperty("href")]
        public string LeaderboardLink { get; set; }
    }

    public class LeaderboardInfo
    {
        [JsonProperty("leaderboards")]
        public List<Track> Leaderboard { get; set; }
    }

    public class GhostList
    {
        [JsonProperty("ghosts")]
        public List<Ghost> List { get; set; }
    }

    public class Ghost
    {
        [JsonProperty("_links")]
        public Link Link { get; set; }
        [JsonProperty("player")]
        public string BKTHolder { get; set; }
        [JsonProperty("dateSet")]
        public string DateSet { get; set; }
    }
}