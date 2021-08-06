using System;
using System.Collections.Generic;

namespace IERatServer.lib
{
    public class ResponseObject
    {
        public ResponseObject()
        {
            Type = "NewTasks"; // FilePart, NewAgent
            Tasks = new List<TaskObject> { };
        }
        public string Type { get; set; }
        public Guid AgentID { get; set; }
        public List<TaskObject> Tasks { get; set; }
        public string Notes { get; set; }

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
    }
}
