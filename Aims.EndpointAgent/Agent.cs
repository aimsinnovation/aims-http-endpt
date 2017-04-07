using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aims.Sdk;

namespace Aims.EndpointAgent
{
    public class Agent : IDisposable
    {
        private readonly EndpointAgent _endpointAgent;

        public Agent(Uri apiAddress, Guid environmentId, string token, int pingTime, EventLog eventLog)
        {
            var api = new Api(apiAddress, token)
                .ForEnvironment(environmentId);
            var nodeRefs = Config.Endpoints
                .Select(p => new NodeRef
                {
                    NodeType = AgentConstants.NodeType.Endpoint,
                    Parts = new Dictionary<string, string> { { "endpoint", p } },
                })
                .ToArray();

            _endpointAgent = new EndpointAgent(api, nodeRefs, pingTime * 1000, eventLog);
        }

        public void Dispose()
        {
            _endpointAgent.Dispose();
        }
    }
}