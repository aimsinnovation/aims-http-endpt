using System;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;

namespace Aims.EndpointAgent
{
    public static class Config
    {
        private static readonly AgentConfigurationReaderWriter<AgentConfigurationModel> modelReaderWriter;

        static Config()
        {
            _isUseJsonSource = false;
            modelReaderWriter = new AgentConfigurationReaderWriter<AgentConfigurationModel>(AgentConstants.ConfigParameters.PathToConfig);
        }

        private static bool _isUseJsonSource;
        private static AgentConfigurationModel _agentConfigurationModel;

        private static AgentConfigurationModel ConfigurationModel
        {
            get
            {
                if (_agentConfigurationModel == null) return modelReaderWriter.Read();
                return _agentConfigurationModel;
            }

            set => _agentConfigurationModel = value;
        }

        public static void UseJsonSource()
        {
            _isUseJsonSource = true;
        }

        public static string ApiEndPoint
        {
            get
            {
                if (_isUseJsonSource)
                {
                    return ConfigurationModel.ApiEndpoint;
                }
                return ConfigurationManager.AppSettings["api-endpoint"];
            }
        }

        public static Guid EnvironmentId
        {
            get
            {
                Guid value;
                if (_isUseJsonSource)
                {
                    if (!Guid.TryParse(ConfigurationModel.Environment, out value))
                        throw new FormatException("'environment-id' setting has invalid format.");
                    return value;
                }

                if (!Guid.TryParse(ConfigurationManager.AppSettings["environment-id"], out value))
                    throw new FormatException("'environment-id' setting has invalid format.");
                return value;
            }
        }

        public static int PingTime
        {
            get
            {
                int value;

                if (_isUseJsonSource)
                {
                    return ConfigurationModel.PingTime;
                }

                if (!Int32.TryParse(ConfigurationManager.AppSettings["ping-time"], out value))
                    throw new FormatException("'ping-time' setting has invalid format.");

                return value;
            }
        }

        public static string[] Endpoints
        {
            get
            {
                if (_isUseJsonSource)
                {
                    return ParsePaths(ConfigurationModel.Endpoints);
                }
                return ParsePaths(ConfigurationManager.AppSettings["endpoints"]);
            }
        }

        public static string Token
        {
            get
            {
                if (_isUseJsonSource)
                {
                    return ConfigurationModel.Token;
                }
                return ConfigurationManager.AppSettings["token"];
            }
        }

        public static bool VerboseLog
        {
            get
            {
                if (_isUseJsonSource)
                {
                    return ConfigurationModel.VerboseLog;
                }
                bool value;
                return Boolean.TryParse(ConfigurationManager.AppSettings["verbose-log"], out value) && value;
            }
        }

        private static string[] ParsePaths(string csv)
        {
            return Regex.Matches(csv, @"(""[^""]*""|[^;])+")
                .Cast<Match>()
                .Select(m => m.Value.Trim('"'))
                .Distinct()
                .ToArray();
        }
    }
}