using System;

namespace IERatServer.lib
{
    public class TaskObject
    {
        public TaskObject() {
            TaskID = Guid.NewGuid();
            Status = "New";
            args = "";
            Time = DateTime.Now.ToString();
        }
        public Guid TaskID { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
        public string args { get; set; }
        public string Type { get; set; }
        public string Time { get; set; }
    }
}
