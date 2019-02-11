using Newtonsoft.Json;

namespace Aims.EndpointAgent.Configuration
{
    public class BasicAuthenticationEntry
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}