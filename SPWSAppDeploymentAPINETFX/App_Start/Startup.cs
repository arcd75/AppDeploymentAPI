using Microsoft.AspNet.SignalR;
using Owin;
using SPWSAppDeploymentAPINETFX.Hubs;
using SPWSAppDeploymentAPINETFX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPWSAppDeploymentAPINETFX
{
    public partial class Startup
    {
        public Startup()
        {
            LoadLocals();
        }

        public async void LoadLocals()
        {
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(60);
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(6);
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(2);
            ADHub.wClients = new List<ADHub.SClient>();
            ADHub.sClients = new List<ADHub.SClient>();
            ADHub.sRequest = new List<ADHub.ServerRequest>();
            await ServerProfile.ReloadLocal();
            await SystemInstallationRecord.ReloadLocal();
            ServerInstance.serverInstances = new List<ServerInstance>();
            RequestFP.local = new List<RequestFP>();
            await ClientProfile.ReloadLocal();
            await ClientProfileDetail.ReloadLocal();
            await ClientProfileGroupMember.ReloadLocal();
            await ClientProfileGroup.ReloadLocal();
            foreach (var item in ServerProfile.local)
            {
                ServerInstance.serverInstances.Add(new ServerInstance(item.IPAddress, item.Username, item.Password));
                //var devserverContext = new ServerInstance("172.17.147.86", "sa", "devdbsvr");
                //var acsserverContext = new ServerInstance("172.17.147.71", "sa", "spwsadmin");
            }
        }
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR("/adhub",new Microsoft.AspNet.SignalR.HubConfiguration());
            ConfigureAuth(app);
        }
    }
}