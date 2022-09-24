using Newtonsoft.Json;
using OpenQA.Selenium.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKBB.Commands
{
    public class Tool
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("creators")]
        public string Creators { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("download")]
        public string Download { get; set; }
    }
}