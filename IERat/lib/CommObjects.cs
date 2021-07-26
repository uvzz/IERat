using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace IERat.lib
{
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
        public string ToJSON() {

            return new JavaScriptSerializer().Serialize(this);
        }
    }
}
