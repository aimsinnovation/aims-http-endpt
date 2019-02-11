using System;
using Newtonsoft.Json;

namespace Aims.EndpointAgent.Configuration
{
    public class EndpointEntry
    {
        [JsonProperty("endpoint", Required = Required.Always)]
        public Uri Endpoint { get; set; }

        [JsonProperty("authentication", Required = Required.DisallowNull)]
        public BasicAuthenticationEntry Aunthentication { get; set; }
    }
}