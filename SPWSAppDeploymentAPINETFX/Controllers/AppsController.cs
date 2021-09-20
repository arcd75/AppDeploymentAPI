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
        public AppJsonResponse GetServers()
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                List<ServerInstanceResponse> sir = new List<ServerInstanceResponse>();
                foreach (var item in ServerProfile.local.ToList())
                {
                    sir.Add(new ServerInstanceResponse()
                    {
                        IPAddress = item.IPAddress,
                        ServerName = item.ServerName
                    });
                }
                result.Data = Newtonsoft.Json.JsonConvert.SerializeObject(sir);
                result.Status = true; 
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }

            return result;
        }

        [Route("Apps/GetApps/{ServerName}")]
        [HttpGet]
        public AppJsonResponse GetApps(string ServerName)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                ServerProfile sp = ServerProfile.local.FirstOrDefault(x => x.ServerName.Equals(ServerName));
                if (sp != null)
                {
                    ServerInstance si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == sp.IPAddress);
                    result.Data = Newtonsoft.Json.JsonConvert.SerializeObject(si.lApps.OrderBy(x => x.AppId).ToList());
                    result.Status = true;
                }
                else
                {
                    result.Exception = $"Server not found: {ServerName}";
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        [Route("Apps/GetVersions/{ServerName}/{AppId}")]
        [HttpGet]
        public AppJsonResponse GetVersions(string ServerName, int AppId)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                ServerProfile sp = ServerProfile.local.FirstOrDefault(x => x.ServerName.Equals(ServerName));
                if (sp != null)
                {
                    var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == sp.IPAddress);
                    result.Data = Newtonsoft.Json.JsonConvert.SerializeObject(si.lAppVersion.Where(lAV => lAV.AppId == AppId).ToList());
                    result.Status = true;
                }
                else
                {
                    result.Exception = $"Server not found: {ServerName}";
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }
        [Route("Apps/GetFiles/{ServerName}/{AppVersionId}")]
        public AppJsonResponse GetFiles(string ServerName, int AppVersionId)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                ServerProfile sp = ServerProfile.local.FirstOrDefault(x => x.ServerName.Equals(ServerName));
                if (sp != null)
                {
                    ServerInstance si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == sp.IPAddress);
                    result.Data = Newtonsoft.Json.JsonConvert.SerializeObject(si.lAppFiles.Where(lAF => lAF.AppVersionId == AppVersionId).ToList());
                    result.Status = true;
                }
                else
                {
                    result.Exception = $"Server not found: {ServerName}";
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        [Route("Apps/GetFile/{ServerName}/{AppFileId}")]
        [HttpGet]
        public AppJsonResponse GetFile(string ServerName, int AppFileId)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                ServerProfile sp = ServerProfile.local.FirstOrDefault(x => x.ServerName.Equals(ServerName));
                if (sp != null)
                {
                    ServerInstance si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == sp.IPAddress);
                    AppFileBlob data = si.Database.SqlQuery<AppFileBlob>($"Select * From dbo.AppFileBlobs where AppFileId = {AppFileId}").First();
                    result.Data = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    result.Status = true;;
                }
                else
                {
                    result.Exception = $"Server not found: {ServerName}";
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        [Route("Apps/CreateApp/{ServerName}/{AppName}")]
        [HttpGet]
        public AppJsonResponse CreateApp(string ServerName, string AppName)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                ServerProfile sp = ServerProfile.local.FirstOrDefault(x => x.ServerName.Equals(ServerName));
                if (sp != null)
                {
                    var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == sp.IPAddress);
                    si.Apps.Add(new App()
                    {
                        AppName = AppName
                    });
                    si.SaveChanges();
                    si.lApps = si.Database.SqlQuery<App>("SELECT * FROM dbo.Apps").ToList();
                    result.Status = true;
                }
                else
                {
                    result.Exception = $"Server not found: {ServerName}";
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        [Route("Apps/DeleteApp/{ServerName}/{AppName}")]
        [HttpGet]
        public AppJsonResponse DeleteApp(string ServerName, string AppName)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                ServerProfile sp = ServerProfile.local.FirstOrDefault(x => x.ServerName.Equals(ServerName));
                if (sp != null)
                {
                    var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == sp.IPAddress);
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
                    result.Status = true;
                }
                else
                {
                    result.Exception = $"Server not found: {ServerName}";
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        [Route("Apps/AddVersion/{ServerName}/{AppName}/{AppVersionName}/{isMajorRevision}")]
        [HttpGet]
        public AppJsonResponse AddVersion(string ServerName,string AppName,string AppVersionName,bool isMajorRevision)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                ServerProfile sp = ServerProfile.local.FirstOrDefault(x => x.ServerName.Equals(ServerName));
                if (sp != null)
                {
                    ServerInstance si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == sp.IPAddress);
                    App app = si.lApps.FirstOrDefault(a => a.AppName == AppName);
                    si.AppVersions.Add(new AppVersion()
                    {
                        AppId = app.AppId,
                        AppVersionName = AppVersionName,
                        Date = DateTime.Now,
                        isMajorRevision = isMajorRevision
                    }) ;
                    si.SaveChanges();
                    si.lAppVersion = si.Database.SqlQuery<AppVersion>("SELECT * FROM dbo.AppVersions").ToList();
                    result.Status = true;
                }
                else
                {
                    result.Exception = $"Server not found: {ServerName}";
                }

            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        [Route("Apps/DeleteVersion/{ServerName}/{AppName}/{AppVersionId}")]
        [HttpGet]
        public AppJsonResponse DeleteVersion(string ServerName, string AppName, int AppVersionId)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                ServerProfile sp = ServerProfile.local.FirstOrDefault(x => x.ServerName.Equals(ServerName));
                if (sp != null)
                {
                    var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == sp.IPAddress);
                    var app = si.lApps.FirstOrDefault(a => a.AppName == AppName);
                    var appVersion = si.lAppVersion.FirstOrDefault(av => av.AppVersionId == AppVersionId);
                    var appFileIds = si.lAppFiles.Where(af => af.AppVersionId == appVersion.AppVersionId).Select(af => af.AppFileId).ToArray();
                    si.AppFileBlobs.RemoveRange(si.AppFileBlobs.Where(afb => appFileIds.Contains(afb.AppFileId)));
                    si.AppFiles.RemoveRange(si.AppFiles.Where(afb => appFileIds.Contains(afb.AppFileId)));
                    si.AppVersions.Remove(si.AppVersions.FirstOrDefault(av => av.AppVersionId == AppVersionId));
                    si.SaveChanges();
                    si.lAppVersion = si.Database.SqlQuery<AppVersion>("SELECT * FROM dbo.AppVersions").ToList();
                    si.lAppFiles = si.Database.SqlQuery<AppFile>("SELECT * FROM dbo.AppFiles").ToList();
                    result.Status = true;
                }
                else
                {
                    result.Exception = $"Server not found: {ServerName}";
                }

            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        [Route("Apps/CreateDirectory/{ServerName}/{AppId}/{AppVersionId}/{AppFileId}/{DirectoryName}")]
        [HttpGet]
        public AppJsonResponse CreateDirectory(string ServerName,int AppId, int AppVersionId,int AppFileId,string DirectoryName)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                ServerProfile sp = ServerProfile.local.FirstOrDefault(x => x.ServerName.Equals(ServerName));
                if (sp != null)
                {
                    var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == sp.IPAddress);
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
                    result.Status = true;
                }
                else
                {
                    result.Exception = $"Server not found: {ServerName}";
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result; 
        }

        [Route("Apps/UploadFiles/{ServerName}/{AppId}/{AppVersionId}/{AppFileId}")]
        [HttpPost]
        public async Task<AppJsonResponse> UploadFiles(string ServerName,int AppId,int AppVersionId,int AppFileId)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                ServerProfile sp = ServerProfile.local.FirstOrDefault(x => x.ServerName.Equals(ServerName));
                if (sp != null)
                {
                    var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var uploadPathDirectory = System.IO.Path.Combine(currentDirectory, "UploadFiles");
                    if (!System.IO.Directory.Exists(uploadPathDirectory))
                    {
                        System.IO.Directory.CreateDirectory(uploadPathDirectory);
                    }
                    var provider = new MultipartFileStreamProvider(uploadPathDirectory);
                    var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == sp.IPAddress);
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
                            AppFileName = file.Headers.ContentDisposition.FileName.Replace("\"", ""),
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
                    result.Status = true;
                    result.Data = mp.FileData.Count() + " file(s) uploaded successfully";
                }
                else
                {
                    result.Exception = $"Server not found: {ServerName}";
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }
        [Route("Apps/DeleteMultiSelect/{ServerName}")]
        [HttpPost]
        public async Task<AppJsonResponse> DeleteMultiSelect(string ServerName)
        {
            AppJsonResponse result = new AppJsonResponse();
            try
            {
                var reqString = await Request.Content.ReadAsStringAsync();
                int[] ids = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(reqString);
                ServerProfile sp = ServerProfile.local.FirstOrDefault(x => x.ServerName.Equals(ServerName));
                if (sp != null)
                {
                    var si = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress == sp.IPAddress);
                    si.Database.BeginTransaction();
                    si.AppFiles.RemoveRange(si.AppFiles.Where(af => ids.Contains(af.AppFileId)));
                    si.AppFileBlobs.RemoveRange(si.AppFileBlobs.Where(afb => ids.Contains(afb.AppFileId)));
                    si.SaveChanges();
                    si.Database.CurrentTransaction.Commit();
                    si.lAppFiles = si.Database.SqlQuery<AppFile>("SELECT * FROM dbo.AppFiles").ToList();
                    result.Status = true;
                    result.Data = ids.Count() + " file" + (ids.Count() > 1 ? "s" : "") + " successfully deleted!";
                }
                else
                {
                    result.Exception = $"Server not found: {ServerName}";
                }
                
            }
            catch (Exception ex)
            {
                result.Exception = ex.ToString();
            }
            return result;
        }

        public class ServerInstanceResponse
        {
            //public string ServerName
            //{
            //    get
            //    {
            //        if (IPAddress == "172.17.147.86")
            //        {
            //            return "DevServer";
            //        }
            //        else if (IPAddress == "172.17.147.71")
            //        {
            //            return "ACSServer";
            //        }
            //        return "";
            //    }
            //}
            public string IPAddress { get; set; }
            public string ServerName { get; set; }

        }


    }


}
