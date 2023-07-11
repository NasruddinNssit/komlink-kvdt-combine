using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.Debugging
{
    public class CommonDebugging
    {
        public static bool IsSystemRunWithDebugDev(out Type aDebugDevType)
        {
            aDebugDevType = null;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly[] assb2 = assemblies.Where(ass => (ass.GetName().Name.Contains("NssIT."))).ToArray();

            foreach (Assembly assb3 in assb2)
            {
                Type[] allTypeList = assb3.GetTypes();

                foreach (Type mytype in allTypeList.Where(aType => (aType.GetInterfaces().Contains(typeof(IDebuggingDevelopment)) && (aType.IsInterface == false))))
                {
                    aDebugDevType = mytype;
                    return true;
                }
            }

            return false;
        }
    }
}
