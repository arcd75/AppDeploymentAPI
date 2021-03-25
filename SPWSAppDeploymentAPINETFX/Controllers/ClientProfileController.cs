using SPWSAppDeploymentAPINETFX.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

        [Route("ClientProfile/SendUpdate/{ClientProfileId}")]
        [HttpPost]
        public async Task<string> SendUpdate(int ClientProfileId)
        {
            string result = "";
            string Status = "";
            try
            {
                using (var adc = new ADContext())
                {
                    adc.Database.BeginTransaction();
                    var requestString = await Request.Content.ReadAsStringAsync();
                    var reqData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClientProfileDetail>>(requestString);
                    if (reqData.Exists(cpd => cpd.ColumnName == "HostName"))
                    {
                        var hostNameDetail = reqData.FirstOrDefault(cpd => cpd.ColumnName == "HostName");
                        var clientProfileDetail = new ClientProfileDetail();
                        var instance = ClientProfileDetail.local.FirstOrDefault(cpd => cpd.ClientProfileId == ClientProfileId && cpd.ColumnName == "HostName");
                        if (instance == null)
                        {
                            
                            adc.ClientProfileDetails.Add(hostNameDetail);
                        }
                        else
                        {
                            instance = adc.ClientProfileDetails.Find(instance.ClientProfileDetailId);
                            instance.Value = hostNameDetail.Value;
                        }
                    }
                    if (reqData.Exists(cpd => cpd.ColumnName == "IPAddress"))
                    {
                        var IPAddressData = reqData.Where(cpd => cpd.ColumnName == "IPAddress").ToList();
                        var ExistingIPAddressData = ClientProfileDetail.local.Where(cpd => cpd.ClientProfileId == ClientProfileId && cpd.ColumnName == "IPAddress").ToList();
                        // GET Records that matches in the DB and retain
                        var RetainedIPAddresses = ExistingIPAddressData.Where(cpd => IPAddressData.Select(ipd => ipd.Value).ToArray().Contains(cpd.Value)).ToList();
                        // Remove IP Addresses that Didnt match retained IP
                        adc.ClientProfileDetails.RemoveRange(adc.ClientProfileDetails.Where(cpd => cpd.ColumnName == "IPAddress" && cpd.ClientProfileId == ClientProfileId && !RetainedIPAddresses.Select(ripa => ripa.Value).ToArray().Contains(cpd.Value)));
                        // Add New records that didnt exist in the existing ip address but existing in the request
                        var NewIPAdressData = IPAddressData.Where(ipd => !RetainedIPAddresses.Select(cpd => cpd.Value).ToArray().Contains(ipd.Value));
                        foreach (var item in NewIPAdressData)
                        {
                            adc.ClientProfileDetails.Add(item);
                        }
                    }
                  
                   
                    adc.SaveChanges();
                    adc.Database.CurrentTransaction.Commit();

                }
                Status = "Ok!";
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Status = "Exception!";
                //throw;
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppsController.AppJsonResponse() { Status = Status, Data = result });
        }
    }
}
