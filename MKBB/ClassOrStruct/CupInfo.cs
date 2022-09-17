using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKBB
{
    public struct CupInfo
    {
        public string Track1Name;
        public string Track2Name;
        public string Track3Name;
        public string Track4Name;
        public string Track1BMG;
        public string Track2BMG;
        public string Track3BMG;
        public string Track4BMG;
        public int Track1Slot;
        public int Track2Slot;
        public int Track3Slot;
        public int Track4Slot;
        public int Track1Music;
        public int Track2Music;
        public int Track3Music;
        public int Track4Music;

        public CupInfo(string t1n, string t2n, string t3n, string t4n, string t1b, string t2b, string t3b, string t4b, int t1s, int t2s, int t3s, int t4s, int t1m, int t2m, int t3m, int t4m)
        {
            Track1Name = t1n;
            Track2Name = t2n;
            Track3Name = t3n;
            Track4Name = t4n;
            Track1BMG = t1b;
            Track2BMG = t2b;
            Track3BMG = t3b;
            Track4BMG = t4b;
            Track1Slot = t1s;
            Track2Slot = t2s;
            Track3Slot = t3s;
            Track4Slot = t4s;
            Track1Music = t1m;
            Track2Music = t2m;
            Track3Music = t3m;
            Track4Music = t4m;
        }
    }
}
