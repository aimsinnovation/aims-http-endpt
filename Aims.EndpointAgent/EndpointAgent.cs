using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Aims.Sdk;

namespace Aims.EndpointAgent
{
    public class EndpointAgent : MonitorBase<Node>
    {
        private readonly EnvironmentApi _api;
        private readonly EventLog _eventLog;
        private readonly int _collectionTime;
        private readonly Node[] _nodes;
        private readonly bool _verboseLog;

        public EndpointAgent(EnvironmentApi api, NodeRef[] nodeRefs, int collectionTime, EventLog eventLog, bool verboseLog)
            : base(collectionTime, true)
        {
            _api = api;
            _collectionTime = collectionTime;
            _eventLog = eventLog;
            _verboseLog = verboseLog;
            _nodes = nodeRefs
                .Select(r => new Node
                {
                    NodeRef = r,
                    Name = r.Parts[AgentConstants.NodeRefPart.Endpoint],
                    ModificationTime = DateTimeOffset.Now,
                    Status = null,
                    Properties = new Dictionary<string, string>(),
                })
                .ToArray();

            Start();
        }

        protected sealed override void Start()
        {
            base.Start();
            var statusCheckers = new List<Task>();
            for (var i = 0; i < _nodes.Length; i++)
            {
                int nodeIndex = i;
                statusCheckers.Add(Task.Run(() => CheckStatus(nodeIndex)));
            }
        }

        protected override Node[] Collect()
        {
            lock (_nodes)
            {
                return _nodes
                    .Where(n => n.Status != null)
                    .ToArray();
            }
        }

        private void CheckStatus(object index)
        {
            var stopwatch = new Stopwatch();
            string endpoint;
            int ix = (int)index;

            lock (_nodes)
            {
                endpoint = _nodes[ix].Name;
            }

            while (_isRunning)
            {
                stopwatch.Restart();

                try
                {
                    string status = GetEndpointStatus(endpoint);
                    lock (_nodes)
                    {
                        _nodes[ix].ModificationTime = DateTimeOffset.UtcNow;
                        _nodes[ix].Status = status;
                    }
                }
                catch (Exception ex)
                {
                    if (_verboseLog)
                    {
                        _eventLog.WriteEntry(String.Format("An error occurred while trying to ping endpoint {1}: {0}",
                                                           ex, endpoint), EventLogEntryType.Error);
                    }
                }

                long timeout = _collectionTime - stopwatch.ElapsedMilliseconds;
                Thread.Sleep(timeout > 0 ? (int)timeout : 0);
            }
        }

        private static string GetEndpointStatus(string endpoint)
        {
            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadString(endpoint);
                }
                catch (WebException ex)
                {
                    var response = (HttpWebResponse)ex.Response;
                    if (response == null) return AgentConstants.Status.Status5xx;

                    int statusGroup = (int)response.StatusCode / 100;

                    switch (statusGroup)
                    {
                        case 1:
                            return AgentConstants.Status.Status1xx;

                        case 2:
                            return AgentConstants.Status.Status2xx;

                        case 3:
                            return AgentConstants.Status.Status3xx;

                        case 4:
                            return AgentConstants.Status.Status4xx;

                        case 5:
                            return AgentConstants.Status.Status5xx;
                    }
                }
            }

            return AgentConstants.Status.Status2xx;
        }

        protected override void Send(Node[] items)
        {
            try
            {
                _api.Nodes.Send(items);
            }
            catch (Exception ex)
            {
                if (_verboseLog)
                {
                    _eventLog.WriteEntry(String.Format("An error occurred while trying to send topology: {0}", ex),
                        EventLogEntryType.Error);
                }
            }
        }
    }
}