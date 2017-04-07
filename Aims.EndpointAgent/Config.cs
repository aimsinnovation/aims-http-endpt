using System;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace Aims.EndpointAgent
{
    public static class Config
    {
        public static string ApiEndPoint
        {
            get { return ConfigurationManager.AppSettings["api-endpoint"]; }
        }

        public static Guid EnvironmentId
        {
            get
            {
                Guid value;
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
                if (!Int32.TryParse(ConfigurationManager.AppSettings["ping-time"], out value))
                    throw new FormatException("'ping-time' setting has invalid format.");

                return value;
            }
        }

        public static string[] Endpoints
        {
            get { return ParsePaths(ConfigurationManager.AppSettings["endpoints"]); }
        }

        public static string Token
        {
            get { return ConfigurationManager.AppSettings["token"]; }
        }

        public static bool VerboseLog
        {
            get
            {
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