using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace IERatServer.lib
{
    public class ServerUtils
    {
        public static string GenerateResponse(string payload)
        {
            string favicon = $"data:image/x-icon;base64,{Base64Encode(payload)}";
            return $"<html><head><link rel=\"icon\" href=\"{favicon}\" type=\"image/x-icon\"><title=\"IERat\"></title></head><body><h1>It's working!</h1></body></html>";
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static byte[] Compress(byte[] data)
        {
            using var compressedStream = new MemoryStream();
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
            zipStream.Write(data, 0, data.Length);
            zipStream.Close();
            return compressedStream.ToArray();
        }
        public static byte[] Decompress(byte[] data)
        {
            using var compressedStream = new MemoryStream(data);
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            zipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
    }
}
