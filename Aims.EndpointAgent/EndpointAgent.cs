using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aims.EndpointAgent.Configuration;
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
        private readonly SystemEntry _system;
        private readonly BlockingCollection<StatPoint> _statCollection = new BlockingCollection<StatPoint>();

        public EndpointAgent(EnvironmentApi api, NodeRef[] nodeRefs, int collectionTime, EventLog eventLog, SystemEntry system, bool verboseLog)
            : base(collectionTime, true)
        {
            _api = api;
            _collectionTime = collectionTime;
            _eventLog = eventLog;
            _verboseLog = verboseLog;
            _system = system;
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
            NodeRef noderefForStatPoint;

            endpoint = _nodes[ix].Name;
            noderefForStatPoint = _nodes[ix].NodeRef;

            while (_isRunning)
            {
                stopwatch.Restart();

                try
                {
                    var requestTime = new Stopwatch();
                    requestTime.Start();
                    string status = GetEndpointStatus(endpoint);
                    requestTime.Stop();
                    _statCollection.Add(makeStatPoint(noderefForStatPoint, requestTime.ElapsedMilliseconds, AgentConstants.StatType.RequestTime));

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
                        _eventLog.WriteEntry($"An error occurred while trying to ping endpoint {endpoint}: {ex}", EventLogEntryType.Error);
                    }
                }

                long timeout = _collectionTime - stopwatch.ElapsedMilliseconds;
                Thread.Sleep(timeout > 0 ? (int)timeout : 0);
            }
        }

        private StatPoint makeStatPoint(NodeRef noderef, long time, string statType)
        {
            return new StatPoint() { NodeRef = noderef, StatType = statType, Time = DateTimeOffset.Now, Value = time };
        }

        private void AddBasicAuthHeader(WebClient client, string userName, string password)
        {
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + password));
            client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
        }

        private string GetEndpointStatus(string endpoint)
        {
            using (var client = new WebClient())
            {
                try
                {
                    var endpointEntry = _system.Endpoints.FirstOrDefault(n => n.Endpoint.ToString() == endpoint);
                    if (endpointEntry.Aunthentication != null)
                    {
                        AddBasicAuthHeader(client, endpointEntry.Aunthentication.Login,
                            endpointEntry.Aunthentication.Password);
                        client.DownloadString(endpoint);
                    }
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
                while (_statCollection.Count > 0)
                {
                    _api.StatPoints.Send(new[] { _statCollection.Take() });
                }
            }
            catch (Exception ex)
            {
                if (_verboseLog)
                {
                    _eventLog.WriteEntry($"An error occurred while trying to send topology: {ex}", EventLogEntryType.Error);
                }
            }
        }
    }
}