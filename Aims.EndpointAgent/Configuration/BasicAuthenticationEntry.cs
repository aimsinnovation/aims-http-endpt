using Newtonsoft.Json;

namespace Aims.EndpointAgent.Configuration
{
    public class BasicAuthenticationEntry
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("login", Required = Required.Always)]
        public string Login { get; set; }

        private string _password;

        [JsonProperty("password", Required = Required.Always)]
        public string Password
        {
            get => _password;
            set => _password = value.Unprotect();
    }
    }
}