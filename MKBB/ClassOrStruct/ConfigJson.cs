﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKBB
{
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
        [JsonProperty("councilUrl")]
        public string CouncilUrl { get; private set; }
        [JsonProperty("adminUrl")]
        public string AdminUrl { get; private set; }
    }
}
