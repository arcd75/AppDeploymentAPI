using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SPWSAppDeploymentAPINETFX.Models
{


    public class ClientProfile
    {
        [Key]
        public long ClientProfileId { get; set; }
        public string SID { get; set; }
        public string Asset { get; set; }

        public static List<ClientProfile> local;

        public static Task ReloadLocal()
        {
            return Task.Factory.StartNew(() =>
            {
                using (var adc = new ADContext())
                {
                    local = adc.Database.SqlQuery<ClientProfile>($"SELECT * FROM dbo.ClientProfile").ToList();
                }
            });
        }
    }

    public class ClientProfileDetail
    {
        [Key]
        public long ClientProfileDetailId { get; set; }
        
        public long ClientProfileId { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
        public static List<ClientProfileDetail> local;
        public static Task ReloadLocal()
        {
            return Task.Factory.StartNew(() =>
            {
                using (var adc = new ADContext())
                {
                    local = adc.Database.SqlQuery<ClientProfileDetail>($"SELECT * FROM dbo.ClientProfileDetails").ToList();
                }
            });
        }

    }

    public class ClientProfileGroupMember
    {
        [Key]
        public long ClientProfileGroupMemberId { get; set; }
        public long ClientProfileGroupId { get; set; }
        public long ClientProfileId { get; set; }
        public static List<ClientProfileGroupMember> local;
        public static Task ReloadLocal()
        {
            return Task.Factory.StartNew(() =>
            {
                using (var adc = new ADContext())
                {
                    local = adc.Database.SqlQuery<ClientProfileGroupMember>($"SELECT * FROM dbo.ClientProfileGroupMemberships").ToList();
                }
            });
        }
    }

    public class ClientProfileGroup
    {
        [Key]
        public long ClientProfileGroupId { get; set; }
        public string Name { get; set; }
        public static List<ClientProfileGroup> local;
        public static Task ReloadLocal()
        {
            return Task.Factory.StartNew(() =>
            {
                using (var adc = new ADContext())
                {
                    local = adc.Database.SqlQuery<ClientProfileGroup>($"SELECT * FROM dbo.ClientProfileGroups").ToList();
                }
            });
        }
    }

    public class RequestFP
    {
        public RequestFP()
        {
            this.FPID = local.Count() + 1;
        }
        public int FPID { get; set; }
        public long ClientProfileId { get; set; }
        public static List<RequestFP> local;
    }

    public class SystemInstallationRecord
    {
        [Key]
        public int SystemInstallationId { get; set; }
        public int ServerId { get; set; }
        public int AppId { get; set; }
        public string MachineName { get; set; }
        public string Version { get; set; }
        public string IPAddress { get; set; }
        public DateTime LastUpdated { get; set; }
        public int ClientProfileId { get; set; }
        [NotMapped]
        public string AppName
        {
            get
            {
                using (var context = new ADContext())
                {
                    var server = context.ServerProfiles.FirstOrDefault(x => x.ServerId == ServerId);
                    var instance = ServerInstance.serverInstances.FirstOrDefault(s => s.IPAddress.Equals(server.IPAddress));
                    return instance.lApps.FirstOrDefault(x => x.AppId == AppId).AppName;
                }
            }
            set => AppName = value;
        }
        public static List<SystemInstallationRecord> local;
        public static Task ReloadLocal()
        {
            return Task.Factory.StartNew(() =>
            {
                using (ADContext context = new ADContext())
                {
                    local = context.Database.SqlQuery<SystemInstallationRecord>("SELECT * FROM dbo.SystemInstallationRecords").ToList();

                }

            });
        }
    }

    public class ServerProfile
    {
        [Key]
        public int ServerId { get; set; }
        public string ServerName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string IPAddress { get; set; }
        public static List<ServerProfile> local;
        public static Task ReloadLocal()
        {
            return Task.Factory.StartNew(() =>
            {
                using (ADContext context = new ADContext())
                {
                    local = context.Database.SqlQuery<ServerProfile>("SELECT * FROM dbo.ServerProfiles").ToList();
                }
            });
        }
    }

    public class ADUser
    {
        [Key]
        public long UserId { get; set; }
        public string DomainName { get; set; }
        public string UserName { get; set; }
        public static List<ADUser> local;
        public static Task ReloadLocal() {
            return Task.Factory.StartNew(() =>
            {
                using (ADContext context = new ADContext())
                {
                    local = context.Database.SqlQuery<ADUser>("SELECT * FROM dbo.Users").ToList();
                }
            });
        }
    }

    public class App
    {
        [Key]
        public int AppId { get; set; }
        public string AppName { get; set; }
    }

    public class AppFile
    {
        [Key]
        public int AppFileId { get; set; }
        public int AppVersionId { get; set; }
        public string AppFileName { get; set; }
        public string AppFileSize { get; set; }
        public string AppFileExt { get; set; }
        public bool isFolder { get; set; }
        public int parentFolder { get; set; }
        public DateTime LastWriteTime { get; set; }
    }

    public class AppVersion
    {
        [Key]
        public int AppVersionId { get; set; }
        public int AppId { get; set; }
        public string AppVersionName { get; set; }
        public bool isMajorRevision { get; set; }
        public DateTime Date { get; set; }
    }

    public class AppFileBlob
    {
        [Key]
        public int AppFileBlobId { get; set; }

        public byte[] FileBlob { get; set; }
        public int AppFileId { get; set; }
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
    }






    public class ADContext : DbContext
    {
        public ADContext()
        {
            this.Database.Connection.ConnectionString = "Data Source=172.17.147.67;Initial Catalog=SPWSAppDeploymentAPIDb;User Id=sa;Password=devp@$$word";
            this.Database.Connection.Open();
        }
        public DbSet<SystemInstallationRecord> SystemInstallationRecords { get; set; }
        public DbSet<ServerProfile> ServerProfiles { get; set; }
        public DbSet<ADUser> ADUsers { get; set; }
        public DbSet<ClientProfile> ClientProfiles { get; set; }
        public DbSet<ClientProfileDetail> ClientProfileDetails { get; set; }
        public DbSet<ClientProfileGroup> ClientProfileGroups { get; set; }
        public DbSet<ClientProfileGroupMember> ClientProfileMembers { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SystemInstallationRecord>().ToTable("SystemInstallationRecords");
            modelBuilder.Entity<ServerProfile>().ToTable("ServerProfiles");
            modelBuilder.Entity<ADUser>().ToTable("Users");
            modelBuilder.Entity<ClientProfile>().ToTable("ClientProfile");
            modelBuilder.Entity<ClientProfileDetail>().ToTable("ClientProfileDetails");
            modelBuilder.Entity<ClientProfileGroup>().ToTable("ClientProfileGroups");
            modelBuilder.Entity<ClientProfileGroupMember>().ToTable("ClientProfileGroupMemberships");
        }
        public void InitializeDatabase(ADContext context)
        {

            if (!context.Database.Exists())
            {
                context.Database.Create();
                Seed(context);
                context.SaveChanges();
            }
        }

        private void Seed(ADContext context)
        {
            throw new NotImplementedException();
        }


    }

    public class ServerInstance : DbContext
    {
        public string IPAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConnectionString
        {
            get
            {
                return string.Format("Data Source={0};Initial Catalog=SPWSAppDeployment;User Id={1};Password={2};", this.IPAddress, this.Username, this.Password);
            }
        }
        public ServerInstance(string IPAddress, string Username, string Password)
        {
            this.IPAddress = IPAddress;
            this.Username = Username;
            this.Password = Password;
            this.Database.Connection.ConnectionString = this.ConnectionString;
            this.Database.Connection.Open();
            lApps = this.Database.SqlQuery<App>("SELECT * FROM dbo.Apps").ToList();
            lAppVersion = this.Database.SqlQuery<AppVersion>("SELECT * FROM dbo.AppVersions").ToList();
            lAppFiles = this.Database.SqlQuery<AppFile>("SELECT * FROM dbo.AppFiles").ToList();
            //lAppFile = this.Database.SqlQuery<AppFileBlob>("SELECT * FROM dbo.AppFileBlobs").ToList();

        }
        public DbSet<App> Apps { get; set; }
        public DbSet<AppFile> AppFiles { get; set; }
        public DbSet<AppFileBlob> AppFileBlobs { get; set; }
        public DbSet<AppVersion> AppVersions { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<App>().ToTable("Apps");
            modelBuilder.Entity<AppFile>().ToTable("AppFiles");
            modelBuilder.Entity<AppFileBlob>().ToTable("AppFileBlobs");
            modelBuilder.Entity<AppVersion>().ToTable("AppVersions");
        }

        public void InitializeDatabase(ServerInstance context)
        {

            if (!context.Database.Exists())
            {
                context.Database.Create();
                Seed(context);
                context.SaveChanges();
            }
        }

        private void Seed(ServerInstance context)
        {
            throw new NotImplementedException();
        }
        public List<App> lApps { get; set; }
        public List<AppFile> lAppFiles { get; set; }
        public List<AppVersion> lAppVersion { get; set; }
        public List<AppFileBlob> lAppFile { get; set; }
        public static List<ServerInstance> serverInstances;

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        //    optionsBuilder.UseSqlServer(this.ConnectionString);
    }


    public class AppJsonResponse
    {
        public bool Status { get; set; } = false;
        public string Data { get; set; }
        public string Exception { get; set; }
    }
}