using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using Env = System.Environment;

namespace Aims.EndpointAgent
{
    public class AgentService : ServiceBase
    {
        private readonly EventLog _eventLog;
        private readonly object _lock = new object();
        private IEnumerable<Agent> _agents = Enumerable.Empty<Agent>();

        public AgentService()
        {
            InitializeComponent();
            _eventLog = new EventLog(AgentConstants.Service.Log) { Source = AgentConstants.Service.EventSource };
        }

        internal void Start()
        {
            OnStart(new string[0]);
        }

        private void DisposeAgents()
        {
            foreach (var agent in _agents)
            {
                agent.Dispose();
            }
            _agents  = Enumerable.Empty<Agent>();
        }
        protected override void OnStart(string[] args)
        {
            lock (_lock)
            {
                try
                {
                    var config = new Config();
                    var agents = config.Root.Systems.Select(
                        system => new Agent(config, system,_eventLog));
                    DisposeAgents();
                    _agents = agents;
                }
                catch (Exception ex)
                {
                    _eventLog.WriteEntry($"Failed to start the agent:{Env.NewLine}{ex}", EventLogEntryType.Error);
                }
            }
        }

        protected override void OnStop()
        {
            lock (_lock)
            {
                try
                {
                    DisposeAgents();
                }
                catch (Exception ex)
                {
                    _eventLog.WriteEntry($"Failed to stop the agent:{Env.NewLine}{ex}", EventLogEntryType.Error);
                }
            }
        }

        private void InitializeComponent()
        {
            ServiceName = AgentConstants.Service.ApplicationName;
        }
    }
}