using System;
using System.IO;

namespace IERatServer.lib
{
    class Logger
    {
        public static bool Log(string type, string strMessage)
        {
            try
            {
                using (FileStream objFilestream = new("IERatServer.log", FileMode.Append, FileAccess.Write))
                {
                    using StreamWriter objStreamWriter = new((Stream)objFilestream);
                    objStreamWriter.WriteLine($"{DateTime.Now}:{type}:{strMessage}");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
