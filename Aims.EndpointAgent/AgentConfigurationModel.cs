using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Aims.EndpointAgent
{
    public class AgentConfigurationModel
    {
        [JsonProperty("api-endpoint")]
        public string ApiEndpoint { get; set; }

        public string Endpoints { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("ping-time")]
        public int PingTime { get; set; }

        [JsonProperty("verbose-log")]
        public bool VerboseLog { get; set; }

        [JsonProperty("environment-id")]
        public string Environment { get; set; }
    }
}