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

        public Agent(Config config, SystemEntry system, EventLog eventLog)
        {
            var api = new Api(config.Root.ApiEndpoint, system.Token).ForEnvironment(config.Root.Environment);
            var nodeRefs = system.Endpoints
                .Select(p => new NodeRef
                {
                    NodeType = AgentConstants.NodeType.Endpoint,
                    Parts = new Dictionary<string, string> { { "endpoint", p.Endpoint.ToString() } },
                })
                .ToArray();

            _endpointAgent = new EndpointAgent(api, nodeRefs, config.Root.PingTime * 1000, eventLog, config.Root.VerboseLog);
        }

        public void Dispose()
        {
            _endpointAgent.Dispose();
        }
    }
}