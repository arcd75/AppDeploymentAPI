using Microsoft.AspNet.SignalR;
using SPWSAppDeploymentAPINETFX.Hubs;
using SPWSAppDeploymentAPINETFX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SPWSAppDeploymentAPINETFX.Controllers
{
    public class ADController : ApiController
    {
        //[Route("AD/GetServers")]
        //[HttpGet]
        //public string GetServers()
        //{
        //    string result = "";
        //    try
        //    {
        //        var devserverContext = new ServerInstance("172.17.147.86", "sa", "devdbsvr");
        //        var acsserverContext = new ServerInstance("172.17.147.71", "sa", "spwsadmin");
        //        var Dapps = devserverContext.Database.SqlQuery<App>("Select * FROM dbo.Apps").ToList();
        //        var Aapps = acsserverContext.Database.SqlQuery<App>("Select * FROM dbo.Apps").ToList();
        //        List<App> applist = new List<App>();
        //        applist.AddRange(Dapps);
        //        applist.AddRange(Aapps);
        //        result += Newtonsoft.Json.JsonConvert.SerializeObject(applist);
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.ToString();
        //    }
        //    return result;
        //}

        [Route("AD/GetClients")]
        [HttpGet]
        public AppJsonResponse GetClients()
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                result.Data = Newtonsoft.Json.JsonConvert.SerializeObject(ADHub.sClients);
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        [Route("AD/GetClientTask/{HostName}")]
        [HttpGet]
        public string GetClientTask(string HostName)
        {
            string result = "";

            try
            {
                var instance = GlobalHost.ConnectionManager.GetHubContext<ADHub>();
                var client = ADHub.sClients.FirstOrDefault(sc => sc.HostName == HostName);
                if (client == null)
                {
                    result = "Client Not Found!";
                }
                else
                {
                    var newServerRequest = new ADHub.ServerRequest()
                    {
                        RequestId = ADHub.sRequest.Count() + 1,
                        DateTime = DateTime.Now,
                        Task = "GetTask",
                        Status = ADHub.ServerRequestStatus.Pending,
                    };
                    ADHub.sRequest.Add(newServerRequest);
                    instance.Clients.Client(client.ConnectionId).getTask(newServerRequest.RequestId);
                    do
                    {
                        newServerRequest = ADHub.sRequest.ToList().FirstOrDefault(sr => sr.RequestId == newServerRequest.RequestId);
                        var interval = DateTime.Now.Subtract(newServerRequest.DateTime).TotalSeconds;
                        if (interval > 60)
                        {
                            break;
                        }
                    } while (newServerRequest.Status != ADHub.ServerRequestStatus.Success);
                    if (newServerRequest.Status == ADHub.ServerRequestStatus.Success)
                    {
                        result = ADHub.sRequest.FirstOrDefault(sr => sr.RequestId == newServerRequest.RequestId).Data;
                    }
                    else
                    {
                        result = "Request Timed Out!";
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }
            return result;
        }


        [Route("AD/KillTask/{HostName}/{PID}")]
        [HttpGet]
        public string KillTask(string HostName,int PID)
        {
            string result = "";
            try
            {
                var instance = GlobalHost.ConnectionManager.GetHubContext<ADHub>();
                var client = ADHub.sClients.FirstOrDefault(sc => sc.HostName == HostName);
                if (client == null)
                {
                    result = "Client Not Found!";
                }
                else
                {
                    var newServerRequest = new ADHub.ServerRequest()
                    {
                        RequestId = ADHub.sRequest.Count() + 1,
                        DateTime = DateTime.Now,
                        Task = "KillTask",
                        Status = ADHub.ServerRequestStatus.Pending,

                    };
                    ADHub.sRequest.Add(newServerRequest);
                    instance.Clients.Client(client.ConnectionId).killTask(newServerRequest.RequestId,PID);
                    do
                    {
                        newServerRequest = ADHub.sRequest.ToList().FirstOrDefault(sr => sr.RequestId == newServerRequest.RequestId);
                        var interval = DateTime.Now.Subtract(newServerRequest.DateTime).TotalSeconds;
                        if (interval > 60)
                        {
                            break;
                        }
                    } while (newServerRequest.Status != ADHub.ServerRequestStatus.Success);
                    if (newServerRequest.Status == ADHub.ServerRequestStatus.Success)
                    {
                        result = ADHub.sRequest.FirstOrDefault(sr => sr.RequestId == newServerRequest.RequestId).Data;
                    }
                    else
                    {
                        result = "Request Timed Out!";
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                //throw;
            }
            return result;
        }

        [Route("AD/CustomTask/{HostName}/{TaskPath}/{Arguments}")]
        [HttpGet]
        public string CustomTask(string HostName,string TaskPath,string Arguments)
        {
            string result = "";
            string a = "";
            switch (a)
            {
                case "a":
                    {

                    }
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}