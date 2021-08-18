using System.Threading;
using NClap.Metadata;
using NClap;
using System;

namespace IERatServer
{
    class ProgramArguments
    {
        [NamedArgument(ArgumentFlags.Required)]
        public string IP { get; set; }
        [NamedArgument(ArgumentFlags.Required)]
        public int Port { get; set; }
    }
    public class IERatServer
    {
        public static void Main(string[] args)
        {
            ProgramArguments programArgs;
            if (!CommandLineParser.TryParse(args, out programArgs)) { return; }

            try
            {
                Thread APIServerThread = new(() => API.Start(programArgs.IP, programArgs.Port));
                APIServerThread.Start();
                Thread DetectTimeOutThread = new(() => lib.Db.DetectTimeOut());
                DetectTimeOutThread.Start();
                CLI.RunInteractiveShell();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] Error - " + ex.Message);
            }
        }
    }
}
