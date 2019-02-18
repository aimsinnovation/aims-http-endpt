using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aims.EndpointAgent.Configuration;
using Aims.Sdk;

namespace Aims.EndpointAgent
{
    public class Agent : IDisposable
    {
        private readonly EndpointAgent _endpointAgent;

        public Agent(Configuration.Configuration config, SystemEntry system, EventLog eventLog)
        {
            var api = new Api(config.ApiEndpoint, system.Token).ForEnvironment(config.Environment);
            var nodeRefs = system.Endpoints
                .Select(p => new NodeRef
                {
                    NodeType = AgentConstants.NodeType.Endpoint,
                    Parts = new Dictionary<string, string> { { "endpoint", p.Endpoint.ToString() } },
                })
                .ToArray();

            _endpointAgent = new EndpointAgent(api, nodeRefs, config.PingTime * 1000, eventLog, system, config.VerboseLog);
        }

        public void Dispose()
        {
            _endpointAgent.Dispose();
        }
    }
}