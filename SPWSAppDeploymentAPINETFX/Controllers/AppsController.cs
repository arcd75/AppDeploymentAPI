using SPWSAppDeploymentAPINETFX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SPWSAppDeploymentAPINETFX.Controllers
{
    public class AppsController : ApiController
    {
        [Route("Apps/GetServers")]
        [HttpGet]
        public string GetServers()
        {
            string result = "";
            try
            {
                //var newSIList = new List<ServerInstance>();
                List<ServerInstanceResponse> sir = new List<ServerInstanceResponse>();
                foreach (var item in ServerInstance.serverInstances.ToList())
                {
                    //item.Password = "";
                    //newSIList.Add(item);
                    sir.Add(new ServerInstanceResponse()
                    {
                        IPAddress = item.IPAddress
                    });
                }
                result = Newtonsoft.Json.JsonConvert.SerializeObject(sir);
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                //throw;
            }

            return result;
        }

        [Route("Apps/GetApps/{ServerName}")]
        [HttpGet]
        public string GetApps(string ServerName)
        {
            string result = "";
            try
            {
                string IPAddress = "";
                if (ServerName == "DevServer")
                {
                    IPAddress  = "172.17.147.86";
                }
                else if (ServerName == "ACSServer")
                {
                    IPAddress = "172.17.147.71";
                }
                var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == IPAddress);
                result = Newtonsoft.Json.JsonConvert.SerializeObject(si.lApps);
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                //throw;
            }
            return result;
        }

        [Route("Apps/GetVersions/{ServerName}/{AppId}")]
        [HttpGet]
        public string GetVersions(string ServerName,int AppId)
        {

            string result = "";
            try
            {
                string IPAddress = "";
                if (ServerName == "DevServer")
                {
                    IPAddress = "172.17.147.86";
                }
                else if (ServerName == "ACSServer")
                {
                    IPAddress = "172.17.147.71";
                }
                var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == IPAddress);
               result = Newtonsoft.Json.JsonConvert.SerializeObject(si.lAppVersion.Where(lAV => lAV.AppId == AppId).ToList());

            }
            catch (Exception ex) 
            {
                result = ex.ToString();
                //throw;
            }
            return result;
        }
        [Route("Apps/GetFiles/{ServerName}/{AppId}/{AppVersionId}")]
        public string GetFiles(string ServerName,int AppId, int AppVersionId)
        {
            string result = "";
            try
            {
                string IPAddress = "";
                if (ServerName == "DevServer")
                {
                    IPAddress = "172.17.147.86";
                }
                else if (ServerName == "ACSServer")
                {
                    IPAddress = "172.17.147.71";
                }
                var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == IPAddress);
                result = Newtonsoft.Json.JsonConvert.SerializeObject(si.lAppFiles.Where(lAF => lAF.AppVersionId == AppVersionId).ToList());
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                //throw;
            }
            return result;
        }

        public class ServerInstanceResponse
        {
            public string ServerName { 
                get
                {
                    if (IPAddress == "172.17.147.86")
                    {
                        return "DevServer";
                    }
                    else if (IPAddress == "172.17.147.71")
                    {
                        return "ACSServer";
                    }
                    return "";
                }
            }
            public string IPAddress { get; set; }
            
           
        }
    }


}
