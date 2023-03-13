using MKBB.Class;
using System.ComponentModel.DataAnnotations;

namespace MKBB.Data
{
    public class TrackData
    {
        [Key] public int ID { get; set; }
        public string Name { get; set; }//
        public string Authors { get; set; }
        public string Version { get; set; }
        public string TrackSlot { get; set; }
        public string MusicSlot { get; set; }
        public decimal SpeedMultiplier { get; set; }
        public int LapCount { get; set; }
        public string SHA1 { get; set; }//
        public DateTime LastChanged { get; set; }//
        public int TimeTrialPopularity { get; set; }//
        public int M1 { get; set; }
        public int M2 { get; set; }
        public int M3 { get; set; }
        public int M6 { get; set; }
        public int M9 { get; set; }
        public int M12 { get; set; }
        public string LeaderboardLink { get; set; }//
        public string CategoryName { get; set; }
        public string SlotID { get; set; }
        public string? EasyStaffSHA1 { get; set; }
        public string? ExpertStaffSHA1 { get; set; }
        public bool CustomTrack { get; set; }
        public bool Is200cc { get; set; }

        public OldTrackData ConvertToOld()
        {
            return new OldTrackData()
            {
                Name = Name,
                Authors = Authors,
                Version = Version,
                TrackSlot = TrackSlot,
                MusicSlot = MusicSlot,
                SpeedMultiplier = SpeedMultiplier,
                LapCount = LapCount,
                SHA1 = SHA1,
                LastChanged = LastChanged,
                TimeTrialPopularity = TimeTrialPopularity,
                M1 = M1,
                M2 = M2,
                M3 = M3,
                M6 = M6,
                M9 = M9,
                M12 = M12,
                LeaderboardLink = LeaderboardLink,
                CategoryName = CategoryName,
                SlotID = SlotID,
                EasyStaffSHA1 = EasyStaffSHA1,
                ExpertStaffSHA1 = ExpertStaffSHA1,
                CustomTrack = CustomTrack,
                Is200cc = Is200cc
            };
        }

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

    public class OldTrackData
    {
        [Key] public int ID { get; set; }
        public string Name { get; set; }
        public string Authors { get; set; }
        public string Version { get; set; }
        public string TrackSlot { get; set; }
        public string MusicSlot { get; set; }
        public decimal SpeedMultiplier { get; set; }
        public int LapCount { get; set; }
        public string SHA1 { get; set; }
        public DateTime LastChanged { get; set; }
        public int TimeTrialPopularity { get; set; }
        public int M1 { get; set; }
        public int M2 { get; set; }
        public int M3 { get; set; }
        public int M6 { get; set; }
        public int M9 { get; set; }
        public int M12 { get; set; }
        public string LeaderboardLink { get; set; }
        public string CategoryName { get; set; }
        public string SlotID { get; set; }
        public string? EasyStaffSHA1 { get; set; }
        public string? ExpertStaffSHA1 { get; set; }
        public bool CustomTrack { get; set; }
        public bool Is200cc { get; set; }
    }
}