using Microsoft.AspNet.SignalR;
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
        
        
        public void Join(int ClientProfileId)
        {
            if (sClients == null)
            {
                sClients = new List<SClient>();
            }
            if (sClients.Exists(sc => sc.ClientProfileId == ClientProfileId))
            {
                var client = sClients.FirstOrDefault(sc => sc.ClientProfileId == ClientProfileId);
                client.ConnectionId = Context.ConnectionId;
                client.isActive = true;

            }
            else
            {
                sClients.Add(new SClient()
                {
                    ClientProfileId = ClientProfileId,
                    ConnectionId = Context.ConnectionId,
                    isActive = true,
                });
                Clients.Client(Context.ConnectionId).RequestNetworkData();
            }
           
            Clients.Client(Context.ConnectionId).message("You have connected to server!");
            foreach (var wClient in wClients)
            {
                Clients.Client(wClient.ConnectionId).updateNetClients();
            }
            
        }

        public void ReceiveNetworkData(int ClientProfileId,string HostName,string IPAddress)
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

        public async Task UpdateRequest(int RequestId,ServerRequestStatus status,string Data)
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
                sClients.FirstOrDefault(sc => sc.ConnectionId == Context.ConnectionId).isActive = false;
            }
            foreach (var wClient in wClients)
            {
                Clients.Client(wClient.ConnectionId).updateNetClients();
            }
            return base.OnDisconnected(stopCalled);
        }

        public class SClient
        {
            public string ConnectionId { get; set; }
            public string HostName { get; set; }
            public string IPAddress { get; set; }
            public int ClientProfileId { get; set; }
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