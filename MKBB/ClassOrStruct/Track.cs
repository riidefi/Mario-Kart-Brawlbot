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
        public Link LinkContainer { get; set; }
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
        public int M1 { get; set; }
        public int M2 { get; set; }
        public int M3 { get; set; }
        public int M6 { get; set; }
        public int M9 { get; set; }
        public int M12 { get; set; }
        public string LeaderboardLink { get; set; }
        public string BKTLink { get; set; }
        public string BKTHolder { get; set; }
        [JsonProperty("fastestTimeLastChange")]
        public string BKTUploadTime { get; set; }
        public string CategoryName { get; set; }
        public string SlotID { get; set; }

        public int ReturnOnlinePopularity(string month)
        {
            switch (month)
            {
                case "m1":
                    return M1;
                case "m2":
                    return M2;
                case "m3":
                    return M3;
                case "m6":
                    return M6;
                case "m9":
                    return M9;
                case "m12":
                    return M12;
            }
            return -1;
        }
    }

    public class NewTracks {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("trackid")]
        public string SHA1 { get; set; }
        [JsonProperty("ghosts")]
        public List<Ghost> Ghosts { get; set; }
        [JsonProperty("average")]
        public string AverageTime { get; set; }
    }

    public class LeaderboardInfo
    {
        [JsonProperty("leaderboards")]
        public List<Track> Leaderboard { get; set; }
    }

    public class GhostList
    {
        [JsonProperty("ghosts")]
        public List<BKT> List { get; set; }
    }

    public class BKT
    {
        [JsonProperty("_links")]
        public Link LinkContainer { get; set; }
        [JsonProperty("player")]
        public string BKTHolder { get; set; }
        [JsonProperty("dateSet")]
        public string DateSet { get; set; }
    }
}