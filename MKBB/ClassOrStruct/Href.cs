using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKBB.Commands
{
    public class Link
    {
        [JsonProperty("item")]
        public Href Href { get; set; }
    }

    public class Href
    {
        [JsonProperty("href")]
        public string URL { get; set; }
    }
}
