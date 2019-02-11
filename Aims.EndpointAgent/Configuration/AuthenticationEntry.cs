using System.Collections.Generic;
using Newtonsoft.Json;

namespace Aims.EndpointAgent.Configuration
{
    public class AuthenticationEntry
    {
        [JsonProperty("basic", Required = Required.DisallowNull)]
        public List<BasicAuthenticationEntry> Basic { get; set; }
    }
}