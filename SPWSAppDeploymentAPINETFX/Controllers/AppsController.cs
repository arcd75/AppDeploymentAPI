using SPWSAppDeploymentAPINETFX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
            string Status = "";
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
                Status = "Ok!";
            }
            catch (Exception ex)
            {
                Status = "Exception!";
                result = ex.ToString();
                //throw;
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppJsonResponse() { Status = Status, Data = result });
        }

        [Route("Apps/GetApps/{ServerName}")]
        [HttpGet]
        public string GetApps(string ServerName)
        {
            string result = "";
            string Status = "";
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
                result = Newtonsoft.Json.JsonConvert.SerializeObject(si.lApps);
                Status = "Ok!";
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Status = "Exception!";
                //throw;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppJsonResponse() { Status = Status, Data = result });
        }

        [Route("Apps/GetVersions/{ServerName}/{AppId}")]
        [HttpGet]
        public string GetVersions(string ServerName, int AppId)
        {

            string result = "";
            string Status = "";
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
                Status = "Ok!";
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Status = "Exception!";
                //throw;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppJsonResponse() { Status = Status, Data = result });
        }
        [Route("Apps/GetFiles/{ServerName}/{AppId}/{AppVersionId}")]
        public string GetFiles(string ServerName, int AppId, int AppVersionId)
        {
            string result = "";
            string Status = "";
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
                Status = "Ok!";
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Status = "Exception!";
                //throw;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppJsonResponse() { Status = Status, Data = result });
        }

        [Route("Apps/GetFile/{ServerName}/{AppFileId}")]
        public string GetFile(string ServerName, int AppFileId)
        {
            string result = "";
            string Status = "";
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
                var data = si.Database.SqlQuery<AppFileBlob>($"Select * From dbo.AppFileBlobs where AppFileId = {AppFileId}").First();
                result = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                Status = "Ok!";
                //result = Newtonsoft.Json.JsonConvert.SerializeObject(si.lAppFile.Where(lAF => lAF.AppFileId == AppFileId).ToList());
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Status = "Exception!";
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppJsonResponse() { Status = Status, Data = result });
        }

        [Route("Apps/CreateApp/{ServerName}/{AppName}")]
        [HttpGet]
        public string CreateApp(string ServerName, string AppName)
        {
            string result = "";
            string Status = ""; 
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
                si.Apps.Add(new App()
                {
                    AppName = AppName
                });
                si.SaveChanges();
                si.lApps = si.Database.SqlQuery<App>("SELECT * FROM dbo.Apps").ToList();
                Status = "Ok!";
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Status = "Exception!";
                //throw;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppJsonResponse() { Status = Status, Data = result });
        }

        [Route("Apps/DeleteApp/{ServerName}/{AppName}")]
        [HttpGet]
        public string DeleteApp(string ServerName, string AppName)
        {
            string result = "";
            string Status = "";
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
                var app = si.lApps.FirstOrDefault(a => a.AppName == AppName);
                var appVersions = si.lAppVersion.Where(av => av.AppId == app.AppId).ToList();
                foreach (var appVersion in appVersions)
                {
                    var appFileIds = si.lAppFiles.Where(af => af.AppVersionId == appVersion.AppVersionId).Select(af => af.AppFileId).ToArray();
                    si.AppFileBlobs.RemoveRange(si.AppFileBlobs.Where(afb => appFileIds.Contains(afb.AppFileId)));
                    si.AppFiles.RemoveRange(si.AppFiles.Where(afb => appFileIds.Contains(afb.AppFileId)));

                }
                si.AppVersions.RemoveRange(si.AppVersions.Where(av => av.AppId == app.AppId));
                si.Apps.Remove(si.Apps.FirstOrDefault(a => a.AppId == app.AppId));
                si.SaveChanges();
                si.lApps = si.Database.SqlQuery<App>("SELECT * FROM dbo.Apps").ToList();
                si.lAppVersion = si.Database.SqlQuery<AppVersion>("SELECT * FROM dbo.AppVersions").ToList();
                si.lAppFiles = si.Database.SqlQuery<AppFile>("SELECT * FROM dbo.AppFiles").ToList();
                Status = "Ok!";
                //si.Apps.Remove();
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                Status = "Exception!";
                //throw;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(new AppJsonResponse() { Status = Status, Data = result });
        }

        [Route("Apps/AddVersion/{ServerName}/{AppName}/{AppVersionName}/{isMajorRevision}")]
        [HttpGet]
        public string AddVersion(string ServerName,string AppName,string AppVersionName,bool isMajorRevision)
        {
            string result = "";
            AppJsonResponse response = new AppJsonResponse();
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
                var app = si.lApps.FirstOrDefault(a => a.AppName == AppName);
                si.AppVersions.Add(new AppVersion()
                {
                    AppId = app.AppId,
                    AppVersionName = AppVersionName,
                    Date = DateTime.Now,
                    isMajorRevision = isMajorRevision
                }) ;
                si.SaveChanges();
                si.lAppVersion = si.Database.SqlQuery<AppVersion>("SELECT * FROM dbo.AppVersions").ToList();
                response.Status = "Ok!";
            }
            catch (Exception ex)
            {
                response.Status = "Exception!";
                response.Data = ex.ToString();
                //throw;
            }
            result = Newtonsoft.Json.JsonConvert.SerializeObject(response);
            return result;
        }

        [Route("Apps/DeleteVersion/{ServerName}/{AppName}/{AppVersionId}")]
        [HttpGet]
        public string DeleteVersion(string ServerName, string AppName, int AppVersionId)
        {
            string result = "";
            AppJsonResponse response = new AppJsonResponse();
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
                var app = si.lApps.FirstOrDefault(a => a.AppName == AppName);
                var appVersion = si.lAppVersion.FirstOrDefault(av => av.AppVersionId == AppVersionId);
                var appFileIds = si.lAppFiles.Where(af => af.AppVersionId == appVersion.AppVersionId).Select(af => af.AppFileId).ToArray();
                si.AppFileBlobs.RemoveRange(si.AppFileBlobs.Where(afb => appFileIds.Contains(afb.AppFileId)));
                si.AppFiles.RemoveRange(si.AppFiles.Where(afb => appFileIds.Contains(afb.AppFileId)));
                si.AppVersions.Remove(si.AppVersions.FirstOrDefault(av => av.AppVersionId == AppVersionId));
                si.SaveChanges();
                si.lAppVersion = si.Database.SqlQuery<AppVersion>("SELECT * FROM dbo.AppVersions").ToList();
                si.lAppFiles = si.Database.SqlQuery<AppFile>("SELECT * FROM dbo.AppFiles").ToList();
                response.Status = "Ok!";
            }
            catch (Exception ex)
            {
                response.Status = "Exception!";
                response.Data = ex.ToString();
                //throw;
            }
            result = Newtonsoft.Json.JsonConvert.SerializeObject(response);
            return result;
        }

        [Route("Apps/CreateDirectory/{ServerName}/{AppId}/{AppVersionId}/{AppFileId}/{DirectoryName}")]
        [HttpGet]
        public string CreateDirectory(string ServerName,int AppId, int AppVersionId,int AppFileId,string DirectoryName)
        {
            string result = "";
            AppJsonResponse response = new AppJsonResponse();
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
                var app = si.lApps.FirstOrDefault(a => a.AppId == AppId);
                var appVersion = si.lAppVersion.FirstOrDefault(av => av.AppId == AppId && av.AppVersionId == AppVersionId);
                var parentFolder = si.lAppFiles.FirstOrDefault(af => af.AppFileId == AppFileId);
                si.AppFiles.Add(new AppFile()
                {
                    AppFileSize = "0",
                    AppFileExt = "",
                    AppFileName = DirectoryName,
                    AppVersionId = appVersion.AppVersionId,
                    isFolder = true,
                    LastWriteTime = DateTime.Now,
                    parentFolder = AppFileId 
                }) ;
                si.SaveChanges();
                si.lAppFiles = si.Database.SqlQuery<AppFile>("SELECT * FROM dbo.AppFiles").ToList();
                response.Status = "Ok!";
                
            }
            catch (Exception ex)
            {
                response.Status = "Exception!";
                response.Data = ex.ToString();
                //throw;
            }
            result = Newtonsoft.Json.JsonConvert.SerializeObject(response);
            return result; 


        }

        [Route("Apps/UploadFiles/{ServerName}/{AppId}/{AppVersionId}/{AppFileId}")]
        [HttpPost]
        public async Task<string> UploadFiles(string ServerName,int AppId,int AppVersionId,int AppFileId)
        {
            string result = "";
            AppJsonResponse response = new AppJsonResponse();
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
                var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var uploadPathDirectory = System.IO.Path.Combine(currentDirectory, "UploadFiles");
                if (!System.IO.Directory.Exists(uploadPathDirectory))
                {
                    System.IO.Directory.CreateDirectory(uploadPathDirectory);
                }
                //string rootpath = System.Web.HttpContext.Current.Server.MapPath("~/UploadFiles");
                var provider = new MultipartFileStreamProvider(uploadPathDirectory);
                var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == IPAddress);
                var app = si.lApps.FirstOrDefault(a => a.AppId == AppId);
                var appVersion = si.lAppVersion.FirstOrDefault(av => av.AppId == AppId && av.AppVersionId == AppVersionId);
                var parentFolder = si.lAppFiles.FirstOrDefault(af => af.AppFileId == AppFileId);
                var mp = await Request.Content.ReadAsMultipartAsync(provider);
                si.Database.BeginTransaction();
                foreach (var file in mp.FileData)
                {
      
                    byte[] data = System.IO.File.ReadAllBytes(file.LocalFileName);
                    AppFile newFile = new AppFile()
                    {
                        AppFileName = file.Headers.ContentDisposition.FileName.Replace("\"",""),
                        AppFileSize = data.Length.ToString(),
                        AppFileExt = file.Headers.ContentDisposition.FileName.Split('.').Last(),
                        isFolder = false,
                        LastWriteTime = DateTime.Now,
                        parentFolder = AppFileId,
                        AppVersionId = AppVersionId
                    };
                    newFile = si.AppFiles.Add(newFile);
                    si.SaveChanges();
                    si.AppFileBlobs.Add(new AppFileBlob()
                    {
                        AppFileId = newFile.AppFileId,
                        FileBlob = data,
                    });
                    si.SaveChanges();
                   
                }
                si.Database.CurrentTransaction.Commit();
                si.lAppFiles = si.Database.SqlQuery<AppFile>("SELECT * FROM dbo.AppFiles").ToList();
                response.Status = "Ok!";
                response.Data = mp.FileData.Count() + " file(s) uploaded successfully";
            }
            catch (Exception ex)
            {
                response.Status = "Exception!";
                response.Data = ex.ToString();
                //throw;
            }
            result = Newtonsoft.Json.JsonConvert.SerializeObject(response);
            return result;
        }
        [Route("Apps/DeleteMultiSelect/{ServerName}")]
        [HttpPost]
        public async Task<string> DeleteMultiSelect(string ServerName)
        {
            string result = "";
            AppJsonResponse response = new AppJsonResponse();
            try
            {
                var reqString = await Request.Content.ReadAsStringAsync();
                int[] ids = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(reqString);
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
                si.Database.BeginTransaction();
                si.AppFiles.RemoveRange(si.AppFiles.Where(af => ids.Contains(af.AppFileId)));
                si.AppFileBlobs.RemoveRange(si.AppFileBlobs.Where(afb => ids.Contains(afb.AppFileId)));
                si.SaveChanges();
                si.Database.CurrentTransaction.Commit();
                si.lAppFiles = si.Database.SqlQuery<AppFile>("SELECT * FROM dbo.AppFiles").ToList();
                response.Status = "Ok!";
                response.Data = ids.Count() + " file" + (ids.Count() > 1 ? "s" : "") + " successfully deleted!";
                
            }
            catch (Exception ex)
            {
                response.Status = "Exception!";
                response.Data = ex.ToString();
                //throw;
            }
            result = Newtonsoft.Json.JsonConvert.SerializeObject(response);
            return result;
        }

        public class ServerInstanceResponse
        {
            public string ServerName
            {
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

        public class AppJsonResponse
        {
            public string Status { get; set; }
            public string Data { get; set; }
        }


    }


}
