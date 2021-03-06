﻿using System.Collections;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace Aims.EndpointAgent
{
    internal static class Program
    {
        private static void InstallService()
        {
            using (var installer = new ProjectInstaller())
            {
                installer.Context = new InstallContext("", new[] {$"/assemblypath={Assembly.GetExecutingAssembly().Location}"});
                installer.Install(new Hashtable());
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            if (args.Length >= 1 && args[0] == "/install")
            {
                InstallService();
                return;
            }

            var service = new AgentService();
            if (args.Contains("/console"))
            {
                service.Start();
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                ServiceBase.Run(service);
            }
        }
    }
}