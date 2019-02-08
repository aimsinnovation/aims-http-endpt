namespace Aims.EndpointAgent
{
    public static class AgentConstants
    {
        public static class ConfigParameters
        {
            public const string PathToConfig = "AgentConfig.json";
        }

        public static class NodeRefPart
        {
            public const string Endpoint = "endpoint";
        }

        public static class NodeType
        {
            public const string Endpoint = "aims.http-endpt.endpoint";
        }

        public static class Service
        {
            public const string ApplicationName = "AIMS HTTP Endpoints Activity Agent";
            public const string EventSource = "AIMS HTTP Endpoints Activity Agent";
            public const string Log = "Application";
            public const string ServiceName = "aims-http-endpt-agent";
        }

        public static class Status
        {
            public const string Undefined = "aims.core.undefined";
            public const string Status1xx = "aims.http-endpt.status-1xx";
            public const string Status2xx = "aims.http-endpt.status-2xx";
            public const string Status3xx = "aims.http-endpt.status-3xx";
            public const string Status4xx = "aims.http-endpt.status-4xx";
            public const string Status5xx = "aims.http-endpt.status-5xx";
        }
    }
}