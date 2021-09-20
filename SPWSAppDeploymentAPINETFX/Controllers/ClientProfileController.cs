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
        [Route("ClientProfile/RequestId/{SerialNo}")]
        [HttpGet]
        public AppJsonResponse RequestId(string SerialNo)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                var reqpf = new RequestFP();
                ClientProfile sid = ClientProfile.local.FirstOrDefault(x => x.SID.Equals(SerialNo));
                if (sid != null)
                {
                    reqpf.ClientProfileId = sid.ClientProfileId;
                }
                else
                {
                    sid = new ClientProfile { Asset = "", SID = SerialNo };
                    using (var adc = new ADContext())
                    {
                        adc.Database.BeginTransaction();
                        sid = adc.ClientProfiles.Add(sid);
                        adc.SaveChanges();
                        adc.Database.CurrentTransaction.Commit();
                        ClientProfile.local.Add(sid);
                        var have = ClientProfileDetail.local.FirstOrDefault(x => x.ColumnName.Equals("Serial Number") && x.Value.Equals(SerialNo));
                        if (have != null)
                        {
                            adc.Database.BeginTransaction();
                            IQueryable<ClientProfileDetail> rec = adc.ClientProfileDetails.Where(x => x.ClientProfileId == have.ClientProfileId);
                            foreach (var item in rec)
                            {
                                item.ClientProfileId = sid.ClientProfileId;
                            }
                            adc.Database.CurrentTransaction.Commit();
                            var recLcl = ClientProfileDetail.local.Where(x => x.ClientProfileId == have.ClientProfileId);
                            foreach (var item in recLcl)
                            {
                                item.ClientProfileId = sid.ClientProfileId;
                            }
                        }
                    }
                }
                result.Data = Newtonsoft.Json.JsonConvert.SerializeObject(reqpf);
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
                //throw;
            }
            Debug.WriteLine(Request.Headers.Host);

            return result;
        }

        [Route("ClientProfile/CompleteId/{FPID}")]
        [HttpGet]
        public AppJsonResponse CompleteId(int FPID)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {

                var reqfp = RequestFP.local.FirstOrDefault(rfp => rfp.FPID == FPID);

                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
                //throw;
            }
            return result;
        }

        [Route("ClientProfile/SendUpdate/{ClientProfileId}")]
        [HttpPost]
        public async Task<AppJsonResponse> SendUpdate(int ClientProfileId)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                using (var adc = new ADContext())
                {
                    adc.Database.BeginTransaction();
                    var requestString = await Request.Content.ReadAsStringAsync();
                    var reqData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClientProfileDetail>>(requestString);

                    string[] column1Rep = { "HostName", "Serial Number", "MAC", "License", "Operating System", "System Manufacturer", "System Model", "Processor", "RAM", "Last Windows Update", "StartUp Date" };

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
                    await ClientProfileDetail.ReloadLocal();

                }
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
                //throw;
            }

            return result;
        }

        [Route("ClientProfile/AddGroup/{GroupName}")]
        [HttpGet]
        public Task<AppJsonResponse> AddGroup(string GroupName)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                using (var adc = new ADContext())
                {
                    adc.Database.BeginTransaction();
                    var group = adc.ClientProfileGroups.Add(new ClientProfileGroup()
                    {
                        Name = GroupName
                    });
                    adc.SaveChanges();

                    adc.Database.CurrentTransaction.Commit();
                    ClientProfileGroup.local.Add(group);
                }
                result.Data = $"{GroupName} has been added to the database!";
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
                //throw;
            }
            return Task.FromResult(result);
        }

        [Route("ClientProfile/DeleteGroup/{GroupId}")]
        [HttpGet]
        public async Task<AppJsonResponse> DeleteGroup(int GroupId)
        {
            AppJsonResponse result = new AppJsonResponse();
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
                    result.Data = $"{Group.Name} has been deleted from the database!";
                }

                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        [Route("ClientProfile/GetGroups")]
        [HttpGet]
        public AppJsonResponse GetGroups()
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                result.Data = Newtonsoft.Json.JsonConvert.SerializeObject(ClientProfileGroup.local);
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }

            return result;
        }

        [Route("ClientProfile/GetMembers/{GroupId}")]
        [HttpGet]
        public AppJsonResponse GetMembers(int GroupId)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                result.Data = Newtonsoft.Json.JsonConvert.SerializeObject(ClientProfileGroupMember.local.Where(cpgm => cpgm.ClientProfileGroupId == GroupId).ToList());
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        [Route("ClientProfile/AddGroupMembers/{GroupId}")]
        [HttpPost]
        public async Task<AppJsonResponse> AddGroupMembers(int GroupId)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                var requestString = await Request.Content.ReadAsStringAsync();
                string[] Ids = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(requestString);
                using (var adc = new ADContext())
                {
                    foreach (var id in Ids)
                    {
                        var data = ClientProfile.local.FirstOrDefault(x => x.SID.Equals(id));
                        if (data != null)
                        {
                            var match = ClientProfileGroupMember.local.Where(cpgm => cpgm.ClientProfileId == data.ClientProfileId && cpgm.ClientProfileGroupId == GroupId).ToList();
                            if (match.Count == 0)
                            {
                                adc.Database.BeginTransaction();
                                var m = adc.ClientProfileMembers.Add(new ClientProfileGroupMember() { ClientProfileGroupId = GroupId, ClientProfileId = data.ClientProfileId });
                                adc.SaveChanges();
                                adc.Database.CurrentTransaction.Commit();
                                ClientProfileGroupMember.local.Add(m);
                            }
                        }
                    }

                }
                result.Status = true;

            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        [Route("ClientProfile/DeleteGroupMembers/{GroupId}")]
        [HttpPost]
        public async Task<AppJsonResponse> DeleteGroupMembers(int GroupId)
        {
            AppJsonResponse result = new AppJsonResponse();
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
                result.Status = true;

            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        [Route("ClientProfile/GetMemberDetails/{ClientProfileId}")]
        [HttpGet]
        public Task<AppJsonResponse> GetMemberDetails(int ClientProfileId)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                if (ClientProfileDetail.local.Exists(cpd => cpd.ClientProfileId == ClientProfileId))
                {
                    result.Data = Newtonsoft.Json.JsonConvert.SerializeObject(ClientProfileDetail.local.Where(cpd => cpd.ClientProfileId == ClientProfileId).ToList());
                    result.Status = true;
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return Task.FromResult(result);
        }
        [Route("ClientProfile/GetAppDetails/{ClientProfileId}/{MachineName}")]
        public Task<AppJsonResponse> GetAppDetails(int ClientProfileId, string MachineName)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                if (SystemInstallationRecord.local.Exists(x=>x.ClientProfileId == ClientProfileId)) {
                    result.Data = Newtonsoft.Json.JsonConvert.SerializeObject(SystemInstallationRecord.local.Where(x => x.ClientProfileId == ClientProfileId && x.MachineName.Equals(MachineName)).ToList());
                    result.Status = true;
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return Task.FromResult(result);
        }
    }
}
