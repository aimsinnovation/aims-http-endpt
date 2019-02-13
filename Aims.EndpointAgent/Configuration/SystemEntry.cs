using System.Collections.Generic;
using Newtonsoft.Json;

namespace Aims.EndpointAgent.Configuration
{
    public class SystemEntry
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        private string _token;

        [JsonProperty("token", Required = Required.Always)]
        public string Token
        {
            get => _token;
            //set => _token = value.Unprotect();
            set => _token = value;
        }

        [JsonProperty("id", Required = Required.Always)]
        public long Id { get; set; }

        [JsonProperty("endpoints", Required = Required.Always)]
        public List<EndpointEntry> Endpoints { get; set; }
    }
}