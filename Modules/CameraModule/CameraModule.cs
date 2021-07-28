using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace CameraModule
{
    public static class CameraModule
    {
        private static MemoryStream Camstream = new MemoryStream();
        public static int Quality = 50;
        public static  FilterInfoCollection videoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        public static VideoCaptureDevice Device = new VideoCaptureDevice(videoCaptureDevices[0].MonikerString);
        public static int photoCounter = 1;
        public static byte[] picBytes = new byte[] { 00 };

        public static byte[] Start()
        {
            try
            {
                Read();
                photoCounter = 1;
                byte[] test = picBytes;
                picBytes = new byte[] { 00 };
                return test;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return picBytes;
            }
        }
        public static void Read()
        {
            try
            {
                Device.NewFrame += Run;
                Device.Start();
                Thread.Sleep(5000);
                Device.Stop();
            }
            catch { }
        }
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        public static void Run(object sender, NewFrameEventArgs e)
        {
            if (photoCounter > 1) { return; }
            Bitmap image = (Bitmap)e.Frame.Clone();
            using (Camstream = new MemoryStream())
            {
                Encoder myEncoder = Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, Quality);
                myEncoderParameters.Param[0] = myEncoderParameter;
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                image.Save(Camstream, jpgEncoder, myEncoderParameters);
                //image.Save($"test{photoCounter}.jpg");
                //Console.WriteLine($"[+] {DateTime.Now} Saving picture to test{photoCounter}.jpg");
                myEncoderParameters?.Dispose();
                myEncoderParameter?.Dispose();
                image?.Dispose();
                Thread.Sleep(1);
                photoCounter++;
                picBytes = Camstream.ToArray();
            }
        }
    }
}
