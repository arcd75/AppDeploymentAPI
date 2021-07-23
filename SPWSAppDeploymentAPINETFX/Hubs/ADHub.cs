using Microsoft.AspNet.SignalR;
using SPWSAppDeploymentAPINETFX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SPWSAppDeploymentAPINETFX.Hubs
{
    public class ADHub : Hub
    {
        public static List<SClient> wClients;
        public static List<SClient> sClients;
        public static List<ServerRequest> sRequest;


        public void Join(int ClientProfileId, string connectionid)
        {
            if (sClients == null)
            {
                sClients = new List<SClient>();
            }
            if (sClients.Exists(sc => sc.ClientProfileId == ClientProfileId))
            {
                var client = sClients.FirstOrDefault(sc => sc.ClientProfileId == ClientProfileId);
                client.ConnectionId = connectionid;
                client.isActive = true;

            }
            else
            {
                sClients.Add(new SClient()
                {
                    ClientProfileId = ClientProfileId,
                    ConnectionId = connectionid,
                    isActive = true,
                });
                Clients.Client(Context.ConnectionId).RequestNetworkData();
            }

            Clients.Client(Context.ConnectionId).message();
            foreach (var wClient in wClients)
            {
                Clients.Client(wClient.ConnectionId).updateNetClients();
            }

        }

        public void ReceiveNetworkData(int ClientProfileId, string HostName, string IPAddress)
        {
            var client = sClients.FirstOrDefault(sc => sc.ClientProfileId == ClientProfileId);
            client.HostName = HostName;
            client.IPAddress = IPAddress;
        }

        public void WebJoin(string HostName)
        {
            if (wClients == null)
            {
                wClients = new List<SClient>();
            }
            if (wClients.Exists(wc => wc.HostName == HostName))
            {
                wClients.FirstOrDefault(wc => wc.HostName == HostName).ConnectionId = Context.ConnectionId;
            }
            else
            {
                wClients.Add(new SClient()
                {
                    ConnectionId = Context.ConnectionId,
                    HostName = HostName,
                });

            }
        }


        public async Task UpdateRequest(int RequestId, ServerRequestStatus status, string Data)
        {
            await Task.Factory.StartNew(() =>
            {
                var request = sRequest.FirstOrDefault(sr => sr.RequestId == RequestId);
                request.Status = status;
                request.Data = Data;
            });
        }

        public async Task Disconnect(string HostName)
        {
            await Task.Factory.StartNew(() =>
            {

                if (sClients.Exists(sc => sc.HostName == HostName))
                {
                    //sClients.Remove();
                    sClients.FirstOrDefault(sc => sc.HostName == HostName).isActive = false;
                }
                foreach (var wClient in wClients)
                {
                    Clients.Client(wClient.ConnectionId).updateNetClients();
                }
            });


        }

        public override Task OnDisconnected(bool stopCalled)
        {
            if (sClients.Exists(sc => sc.ConnectionId == Context.ConnectionId))
            {
                var cl = sClients.FirstOrDefault(sc => sc.ConnectionId == Context.ConnectionId);
                cl.isActive = false;
                cl.ConnectionId = "";
            }
            foreach (var wClient in wClients)
            {
                Clients.Client(wClient.ConnectionId).updateNetClients();
            }
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            Console.WriteLine(Context.ConnectionId);
            Clients.Client(Context.ConnectionId).connected(Context.ConnectionId);
            return base.OnReconnected();
        }
        public override Task OnConnected()
        {
            Console.WriteLine(Context.ConnectionId);
            Clients.Client(Context.ConnectionId).connected(Context.ConnectionId);
            return base.OnConnected();
        }

        public void CloseApp(int[] ClientIds, string serverName, int appId)
        {
            string IPAddress = "";
            if (serverName == "DevServer")
            {
                IPAddress = "172.17.147.86";
            }
            else if (serverName == "ACSServer")
            {
                IPAddress = "172.17.147.71";
            }
            var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == IPAddress);
            var app = si.lApps.FirstOrDefault(a => a.AppId == appId);
            foreach (var Id in ClientIds)
            {
                var client = sClients.FirstOrDefault(sc => sc.ClientProfileId == Id);
                Clients.Client(client.ConnectionId).closeApp(serverName, app.AppName);
            }
        }

        public void PushUpdates(int[] ClientIds)
        {
            foreach (var Id in ClientIds)
            {
                var client = sClients.FirstOrDefault(sc => sc.ClientProfileId == Id);
                Clients.Client(client.ConnectionId).pushSelfUpdate();
            }
        }

        public void InstallApp(int[] ClientIds, string serverName, int appId)
        {
            string IPAddress = "";
            if (serverName == "DevServer")
            {
                IPAddress = "172.17.147.86";
            }
            else if (serverName == "ACSServer")
            {
                IPAddress = "172.17.147.71";
            }
            var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == IPAddress);
            var app = si.lApps.FirstOrDefault(a => a.AppId == appId);
            foreach (var Id in ClientIds)
            {
                var client = sClients.FirstOrDefault(sc => sc.ClientProfileId == Id);
                Clients.Client(client.ConnectionId).install(serverName, Newtonsoft.Json.JsonConvert.SerializeObject(app));
            }
        }

        public void UninstallApp(int[] ClientIds, string serverName, int appId)
        {
            string IPAddress = "";
            if (serverName == "DevServer")
            {
                IPAddress = "172.17.147.86";
            }
            else if (serverName == "ACSServer")
            {
                IPAddress = "172.17.147.71";
            }
            var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == IPAddress);
            var app = si.lApps.FirstOrDefault(a => a.AppId == appId);
            foreach (var Id in ClientIds)
            {
                var client = sClients.FirstOrDefault(sc => sc.ClientProfileId == Id);
                Clients.Client(client.ConnectionId).uninstall(serverName, Newtonsoft.Json.JsonConvert.SerializeObject(app));
            }
        }

        public void UpdateApp() {
            foreach (SClient item in sClients)
            {
                if (!string.IsNullOrEmpty(item.ConnectionId)) {
                    Clients.Client(item.ConnectionId).update();
                }
            }
        }

        public void ApplicationRecord(string server,string data, bool Uninstall = false) {
            data = Newtonsoft.Json.JsonConvert.DeserializeObject(data).ToString();
            if (!string.IsNullOrEmpty(data))
            {
                SystemInstallationRecord record = Newtonsoft.Json.JsonConvert.DeserializeObject<SystemInstallationRecord>(data);
                using (var context = new ADContext())
                {
                    int serverId = context.ServerProfiles.FirstOrDefault(x => x.ServerName.Equals(server)).ServerId; 
                    var result = context.SystemInstallationRecords.FirstOrDefault(x => x.MachineName.Equals(record.MachineName) && x.AppId == record.AppId && x.ClientProfileId == record.ClientProfileId && x.ServerId == serverId);
                    if (result != null)
                    {
                        if (Uninstall)
                        {
                            context.SystemInstallationRecords.Remove(result);
                            var localData = SystemInstallationRecord.local.FirstOrDefault(x => x.MachineName.Equals(record.MachineName) && x.AppId == record.AppId && x.ClientProfileId == record.ClientProfileId && x.ServerId == serverId);
                            SystemInstallationRecord.local.Remove(localData);
                        }
                        else {
                            result.Version = record.Version;
                            result.LastUpdated = record.LastUpdated;
                            var localData = SystemInstallationRecord.local.FirstOrDefault(x => x.MachineName.Equals(record.MachineName) && x.AppId == record.AppId && x.ClientProfileId == record.ClientProfileId && x.ServerId == serverId);
                            if (localData != null)
                            {
                                localData.Version = record.Version;
                                localData.LastUpdated = record.LastUpdated;
                            }
                        }
                    }
                    else if(!Uninstall) {
                        record.ServerId = serverId;
                        context.SystemInstallationRecords.Add(record);
                        SystemInstallationRecord.local.Add(record);
                    }
                    context.SaveChanges();
                }
            }
        }

        public class SClient
        {
            public string ConnectionId { get; set; }
            public string HostName { get; set; }
            public string IPAddress { get; set; }
            public long ClientProfileId { get; set; }
            public DateTime LastActiveTime { get; set; }
            public bool isActive { get; set; }
        }

        public enum ServerRequestStatus
        {
            Pending = 1,
            Sent = 2,
            Error = 3,
            Success = 4
        }

        public class ServerRequest
        {
            public int RequestId { get; set; }
            public DateTime DateTime { get; set; }
            public string Task { get; set; }
            public ServerRequestStatus Status { get; set; }
            public string Data { get; set; }
        }
    }
}