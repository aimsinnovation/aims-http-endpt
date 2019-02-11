using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Aims.EndpointAgent.Configuration
{
    public class Configuration
    {
        [JsonProperty("endpoint", Required = Required.Always)]
        public Uri ApiEndpoint { get; set; }

        [JsonProperty("environment", Required = Required.Always)]
        public Guid Environment { get; set; }

        [JsonProperty("authentication", Required = Required.DisallowNull)]
        public AuthenticationEntry Authentication { get; set; }

        [JsonProperty("systems", Required = Required.Always)]
        public List<SystemEntry> Systems { get; set; }

        [JsonProperty("ping-time", Required = Required.DisallowNull)]
        public int PingTime { get; set; } = 60;

        [JsonProperty("verbose-log", Required = Required.DisallowNull)]
        public bool VerboseLog { get; set; }
    }
}