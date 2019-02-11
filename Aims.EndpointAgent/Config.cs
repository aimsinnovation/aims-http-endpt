namespace Aims.EndpointAgent
{
    public class Config
    {
        public Config()
        {
            Root = new AgentConfigurationReader<Configuration.Configuration>(AgentConstants.ConfigParameters.PathToConfig).Json;
        }

        public Configuration.Configuration Root { get; }
    }
}