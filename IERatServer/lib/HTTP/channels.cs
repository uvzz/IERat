using System;

namespace IERatServer.lib
{
    public class AgentChannel
    {
        public AgentChannel()
        {
            agent = new Agent();
        }
        public string LastHeartBeatTime { get; set; }
        public int InteractNum { get; set; }
        public string IPAddress { get; set; }
        public Agent agent { get; set; }

        public void UpdateHeartBeatTime()
        {
            this.LastHeartBeatTime = DateTime.Now.ToString();
        }
    }

}
