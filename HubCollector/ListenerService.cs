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
        public Statsd StatsdClient;

        public ListenerService()
        {
            InitializeComponent();
            
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            EventLog.Log = "HubCollectorLog";
            StatsdClient = new Statsd("v1113.bfgdev.inside", 8125);

            KlondikeConnection = new HubConnection("https://nuget-rai.bfgdev.com/api/signalr")
            {
                TraceLevel = TraceLevels.All
            };

            IHubProxy statusHubProxy = KlondikeConnection.CreateHubProxy("status");
            statusHubProxy.On("updateStatus",
                status =>
                {
                    //Console.WriteLine("Status: {0}, Total: {1}", status.synchronizationState, status.totalPackages);
                    EventLog.WriteEntry("Message from SignalR received.");
                    EventLog.WriteEntry("Status: {0}, Total: {1}", status.synchronizationState, status.totalPackages);
                    StatsdClient.LogGauge("nuget.rai.packageCount", (int)status.totalPackages);
                }
            );

        }

        protected override void OnStart(string[] args)
        {
            StartListener();
            EventLog.WriteEntry("The service was started successfully.", EventLogEntryType.Information);
        }

        public async void StartListener()
        {
            EventLog.WriteEntry("Starting connection to Klondike...");
            await KlondikeConnection.Start();
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Stopping connection to Klondike...");
            KlondikeConnection.Stop();
            EventLog.WriteEntry("The service was stopped successfully.", EventLogEntryType.Information);
        }

        protected override void OnShutdown()
        {
            EventLog.WriteEntry("The service was shutdown successfully", EventLogEntryType.Information);
        }
    }
}
