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
                    var clientProfile = new ClientProfile()
                    {
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

            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppsController.AppJsonResponse() { Status = Status, Data = result }); ;
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
            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppsController.AppJsonResponse() { Status = Status, Data = result });
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

                    string[] column1Rep = { "HostName", "Serial Number", "MAC", "License", "Operating System", "System Manufacturer", "System Model", "Processor", "RAM","Last Windows Update","StartUp Date" };

                    if (reqData.Exists(cpd => column1Rep.Contains(cpd.ColumnName)))
                    {
                        foreach (var col in column1Rep)
                        {
                            var colDetail = reqData.FirstOrDefault(cpd => cpd.ColumnName == col);
                            if (colDetail != null)
                            {
                                var clientProfileDetail = new ClientProfileDetail();
                                var instance = ClientProfileDetail.local.FirstOrDefault(cpd => cpd.ClientProfileId == ClientProfileId && cpd.ColumnName == col);
                                if (instance == null)
                                {
                                    colDetail.ClientProfileId = ClientProfileId;
                                    adc.ClientProfileDetails.Add(colDetail);
                                }
                                else
                                {
                                    instance = adc.ClientProfileDetails.Find(instance.ClientProfileDetailId);
                                    instance.Value = colDetail.Value;
                                }
                            }
                           
                        }

                    }
                    if (reqData.Exists(cpd => cpd.ColumnName == "IPAddress"))
                    {
                        var IPAddressData = reqData.Where(cpd => cpd.ColumnName == "IPAddress").ToList();
                        var ExistingIPAddressData = ClientProfileDetail.local.Where(cpd => cpd.ClientProfileId == ClientProfileId && cpd.ColumnName == "IPAddress").ToList();
                        // GET Records that matches in the DB and retain
                        var RetainedIPAddresses = ExistingIPAddressData.Where(cpd => IPAddressData.Select(ipd => ipd.Value).ToArray().Contains(cpd.Value)).ToList();
                        // Remove IP Addresses that Didnt match retained IP
                        var ripaa = RetainedIPAddresses.Select(cpd => cpd.Value);
                        adc.ClientProfileDetails.RemoveRange(adc.ClientProfileDetails.Where(cpd => cpd.ColumnName == "IPAddress" && cpd.ClientProfileId == ClientProfileId && !ripaa.Contains(cpd.Value)));
                        // Add New records that didnt exist in the existing ip address but existing in the request
                        
                        var NewIPAdressData = IPAddressData.Where(ipd => !ripaa.Contains(ipd.Value));
                        foreach (var item in NewIPAdressData)
                        {
                            item.ClientProfileId = ClientProfileId;
                            adc.ClientProfileDetails.Add(item);
                        }
                    }




                    adc.SaveChanges();
                    adc.Database.CurrentTransaction.Commit();
                    ClientProfileDetail.ReloadLocal();

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

        [Route("ClientProfile/AddGroup/{GroupName}")]
        [HttpGet]
        public async Task<string> AddGroup(string GroupName)
        {
            string result = "";
            string Status = "";
            try
            {
                using (var adc = new ADContext())
                {
                    adc.Database.BeginTransaction();
                    adc.ClientProfileGroups.Add(new ClientProfileGroup()
                    {
                        Name = GroupName
                    });
                    adc.SaveChanges();

                    adc.Database.CurrentTransaction.Commit();
                    await ClientProfileGroup.ReloadLocal();
                }
                result = $"{GroupName} has been added to the database!";
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

        [Route("ClientProfile/DeleteGroup/{GroupId}")]
        [HttpGet]
        public async Task<string> DeleteGroup(int GroupId)
        {
            string result = "";
            string Status = "";
            try
            {
                using (var adc = new ADContext())
                {
                    adc.Database.BeginTransaction();
                    var Group = adc.ClientProfileGroups.Find(GroupId);
                    var GroupMembers = adc.ClientProfileMembers.Where(cpm => cpm.ClientProfileGroupId == GroupId).ToList();
                    adc.ClientProfileMembers.RemoveRange(GroupMembers);
                    adc.ClientProfileGroups.Remove(Group);

                    adc.Database.CurrentTransaction.Commit();
                    await ClientProfileGroupMember.ReloadLocal();
                    await ClientProfileGroup.ReloadLocal();
                    result = $"{Group.Name} has been deleted from the database!";
                }

                Status = "Ok!";
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Status = "Exception!";
                //throw;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppsController.AppJsonResponse() { Status = Status, Data = result.ToString() });
        }

        [Route("ClientProfile/GetGroups")]
        [HttpGet]
        public string GetGroups()
        {
            string result = "";
            string Status = "";
            try
            {
                result = Newtonsoft.Json.JsonConvert.SerializeObject(ClientProfileGroup.local);
                Status = "Ok!";
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Status = "Exception!";
                //throw;
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppsController.AppJsonResponse() { Status = Status, Data = result.ToString() });
        }

        [Route("ClientProfile/GetMembers/{GroupId}")]
        [HttpGet]
        public string GetMembers(int GroupId)
        {
            string result = "";
            string Status = "";
            try
            {
                result = Newtonsoft.Json.JsonConvert.SerializeObject(ClientProfileGroupMember.local.Where(cpgm => cpgm.ClientProfileGroupId == GroupId).ToList());
                Status = "Ok!";
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Status = "Exception!";
                //throw;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppsController.AppJsonResponse() { Status = Status, Data = result.ToString() });
        }

        [Route("ClientProfile/AddGroupMembers/{GroupId}")]
        [HttpPost]
        public async Task<string> AddGroupMembers(int GroupId)
        {
            string result = "";
            string Status = "";
            try
            {
                var requestString = await Request.Content.ReadAsStringAsync();
                int[] Ids = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(requestString);
                using (var adc = new ADContext())
                {
                    adc.Database.BeginTransaction();
                    foreach (var id in Ids)
                    {
                        var match = ClientProfileGroupMember.local.Where(cpgm => cpgm.ClientProfileId == id && cpgm.ClientProfileGroupId == GroupId).ToList();
                        if (match.Count == 0)
                        {
                            adc.ClientProfileMembers.Add(new ClientProfileGroupMember() { ClientProfileGroupId = GroupId, ClientProfileId = id });
                        }
                    }

                    adc.SaveChanges();
                    adc.Database.CurrentTransaction.Commit();
                }
                await ClientProfileGroupMember.ReloadLocal();
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

        [Route("ClientProfile/DeleteGroupMembers/{GroupId}")]
        [HttpPost]
        public async Task<string> DeleteGroupMembers(int GroupId)
        {
            string result = "";
            string Status = "";
            try
            {
                var requestString = await Request.Content.ReadAsStringAsync();
                long[] Ids = Newtonsoft.Json.JsonConvert.DeserializeObject<long[]>(requestString);
                using (var adc = new ADContext())
                {
                    adc.Database.BeginTransaction();
                    var members = adc.ClientProfileMembers.Where(cpm => Ids.Contains(cpm.ClientProfileGroupMemberId));
                    adc.ClientProfileMembers.RemoveRange(members);
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

        [Route("ClientProfile/GetMemberDetails/{ClientProfileId}")]
        [HttpGet]
        public async Task<string> GetMemberDetails(int ClientProfileId)
        {
            string result = "";
            string Status = "";
            try
            {
                if (ClientProfileDetail.local.Exists(cpd => cpd.ClientProfileId == ClientProfileId))
                {
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(ClientProfileDetail.local.Where(cpd => cpd.ClientProfileId == ClientProfileId).ToList());
                }
                Status = "Ok!";
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Status = "Exception!";
                //throw;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppsController.AppJsonResponse() { Status = Status, Data = result.ToString() });
        }




    }


}
