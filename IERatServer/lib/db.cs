using ConsoleTables;
using IERat;
using IERat.lib;
using System;
using System.Collections.Generic;

namespace IERatServer.lib
{
    public class TaskHistoryObject
    {
        public TaskHistoryObject()
        {
            task = new TaskObject();
        }
        public Guid AgentID{ get; set; }
        public TaskObject task { get; set; }
    }
    public class Db
    {
        public static List<AgentChannel> channels = new();
        public static List<TaskHistoryObject> TaskHistory = new();
        public static int IncomingAgentCounter = 1;
        public static void NewAgentChannel(Agent newAgent, string IP)
        {
            channels.Add(new AgentChannel
            {
                agent = newAgent,
                LastHeartBeatTime = DateTime.Now.ToString(),
                IPAddress = IP,
                InteractNum = IncomingAgentCounter
            }) ;
            IncomingAgentCounter++;
        }

        public static void ListChannels()
        {
            var table = new ConsoleTable("#", "IP", "Machine", "User", "OS", "Last Seen" , "RAT Version");
            foreach (AgentChannel agentChannel in channels)
            {
                table.AddRow(agentChannel.InteractNum, agentChannel.IPAddress, agentChannel.agent.Hostname,
                    $"{agentChannel.agent.Username}@{agentChannel.agent.Domain}",
                    agentChannel.agent.OSVersion, agentChannel.LastHeartBeatTime, agentChannel.agent.Version);
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
            var table = new ConsoleTable("Agent ID", "Command", "args", "Result", "Time");
            foreach (TaskHistoryObject taskHistoryObject in TaskHistory)
            {
                if (taskHistoryObject.task.Result.Length > 50)
                {
                    table.AddRow(taskHistoryObject.AgentID, taskHistoryObject.task.Type, taskHistoryObject.task.args,
                        "<Output too long>", taskHistoryObject.task.Time);
                }
                else
                {
                    table.AddRow(taskHistoryObject.AgentID, taskHistoryObject.task.Type, taskHistoryObject.task.args,
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
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"\n---> Lost connection with agent #{agentChannel.InteractNum}\n");
                                Console.ForegroundColor = ConsoleColor.White;
                                TimedOutChannels.Add(agentChannel);
                            }
                        }
                        foreach (AgentChannel TimedOutChannel in TimedOutChannels)
                        {
                            if (TimedOutChannel.InteractNum == CLI.InteractContext) { CLI.loop._client.Prompt = "IERat$ "; }
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
