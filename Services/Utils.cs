using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FactoryPlanner.Services
{
    internal class Utils
    {
        public static string ReadResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            //            foreach (var name in assembly.GetManifestResourceNames())
            //            {
            //                System.Diagnostics.Trace.WriteLine(name);
            //            }

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }
    }
}
