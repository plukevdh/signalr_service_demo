using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using Microsoft.AspNet.SignalR.Client;
using StatsdClient;

namespace HubCollector
{
    public partial class ListenerService : ServiceBase
    {
        public HubConnection KlondikeConnection;
        public MetricsConfig StatsdConfig;

        public ListenerService()
        {
            InitializeComponent();
            
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            EventLog.Log = "HubCollectorLog";

            KlondikeConnection = new HubConnection("https://nuget-rai.bfgdev.com/api/signalr")
            {
                TraceLevel = TraceLevels.All
            };

            IHubProxy statusHubProxy = KlondikeConnection.CreateHubProxy("status");
            statusHubProxy.On("updateStatus",
                status =>
                {
                    Console.WriteLine("Status: {0}, Total: {1}", status.synchronizationState, status.totalPackages);
                    EventLog.WriteEntry("Status: {0}, Total: {1}", status.synchronizationState, status.totalPackages);
                    Metrics.Gauge("count", (double)status.totalPackages);
                }
            );

            StatsdConfig = new MetricsConfig
            {
                StatsdServerName = "v1113.bfgdev.inside",
                StatsdServerPort = 8125,
                Prefix = "RAIKlondike"
            };
        }

        protected override void OnStart(string[] args)
        {
            StartListener();
            StatsdClient.Metrics.Configure(StatsdConfig);
            EventLog.WriteEntry("The service was started successfully.", EventLogEntryType.Information);
        }

        public async void StartListener()
        {
            System.Diagnostics.Debugger.Launch();
            await KlondikeConnection.Start();
        }

        protected override void OnStop()
        {
            KlondikeConnection.Stop();
            EventLog.WriteEntry("The service was stopped successfully.", EventLogEntryType.Information);
        }

        protected override void OnShutdown()
        {
            EventLog.WriteEntry("The service was shutdown successfully", EventLogEntryType.Information);
        }
    }
}
