using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using Microsoft.AspNet.SignalR.Client;
using StatsdClient;

namespace HubCollector
{
    public partial class ListenerService : ServiceBase
    {
        public List<HubConnection>KlondikeConnections;
        public Statsd StatsdClient;

        public ListenerService()
        {
            InitializeComponent();
            
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            EventLog.Log = "HubCollectorLog";
            StatsdClient = new Statsd(Config.Hostname, Config.Port);
            KlondikeConnections = new List<HubConnection>();

            Config.Hubs.ForEach(hub =>
            {
                var connection = new HubConnection(hub);


                IHubProxy statusHubProxy = connection.CreateHubProxy("status");
                statusHubProxy.On("updateStatus", status =>
                {
                    string name = Config.NameFromUrl(connection.Url);
                    var message = String.Format("From {2}: Status: {0}, Total: {1}", status.synchronizationState,
                        status.totalPackages, name);
                    EventLog.WriteEntry(message);
                    Console.WriteLine(message);

                    StatsdClient.LogGauge("nuget."+ name +".packageCount", (int) status.totalPackages);
                });

                KlondikeConnections.Add(connection);
            });
        }

        protected override void OnStart(string[] args)
        {
            StartListeners();
            EventLog.WriteEntry("The service was started successfully.", EventLogEntryType.Information);
        }

        public void StartListeners()
        {
            KlondikeConnections.ForEach(StartListener);
        }

        public async void StartListener(HubConnection conn)
        {
            EventLog.WriteEntry("Starting connection to " + conn.Url + "...");
            await conn.Start();
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Stopping connections to Klondike...");
            KlondikeConnections.ForEach(conn => conn.Stop());
            EventLog.WriteEntry("The service was stopped successfully.", EventLogEntryType.Information);
        }

        protected override void OnShutdown()
        {
            EventLog.WriteEntry("The service was shutdown successfully", EventLogEntryType.Information);
        }
    }
}
