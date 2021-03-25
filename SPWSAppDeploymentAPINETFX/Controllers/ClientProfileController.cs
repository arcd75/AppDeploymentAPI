using SPWSAppDeploymentAPINETFX.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SPWSAppDeploymentAPINETFX.Controllers
{
    public class ClientProfileController : ApiController
    {
        [Route("ClientProfile/RequestId")]
        [HttpGet]
        public string RequestId()
        {
            string result = "";
            string Status = "";
            //AppsController.AppJsonResponse response = new AppsController.AppJsonResponse();
            try
            {
                var reqpf = new RequestFP();
                using (var adc = new ADContext())
                {
                    var clientProfile = new ClientProfile() {
                        AssetTag = "",
                    };
                    adc.Database.BeginTransaction();
                    while (true)
                    {
                        clientProfile = adc.ClientProfiles.Add(clientProfile);

                        adc.SaveChanges();
                        if (!RequestFP.local.Exists(rfp => rfp.ClientProfileId == clientProfile.ClientProfileId))
                        {
                            reqpf.ClientProfileId = clientProfile.ClientProfileId;
                            break;
                        }
                    }
                    adc.Database.CurrentTransaction.Commit();

                    //adc.Database.CurrentTransaction.Dispose();






                    RequestFP.local.Add(reqpf);
                }
                result = Newtonsoft.Json.JsonConvert.SerializeObject(reqpf);
                Status = "Ok!";
            }
            catch (Exception ex)
            {
                Status = "Exception!";
                result = ex.ToString();
                //throw;
            }
            Debug.WriteLine(Request.Headers.Host);
        
            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppsController.AppJsonResponse() { Status = Status, Data = result}); ;
        }

        [Route("ClientProfile/CompleteId/{FPID}")]
        [HttpGet]
        public string CompleteId(int FPID)
        {
            string result = "";
            string Status = "";
            try
            {
               
                var reqfp = RequestFP.local.FirstOrDefault(rfp => rfp.FPID == FPID);
               
                Status = "Ok!";
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Status = "Exception!";
                //throw;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppsController.AppJsonResponse() { Status = Status, Data = result});
        }

        [Route("ClientProfile/AddProperty/{ClientProfileId}/{Property}/{Value}")]
        [HttpGet]
        public string AddProperty(int ClientProfileId,string Property,string Value)
        {
            string result = "";
            string Status = "";
            try
            {
                switch (Property)
                {
                    case "HostName":
                        {
                            
                        }
                        break;
                    case "IPAddress":
                        {

                        }
                        break;
                        

                    default:
                        break;
                        break;
                }
            }
            catch (Exception ex)
            {


                //throw;
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppsController.AppJsonResponse() { Status = Status, Data = result });
        }
    }
}
