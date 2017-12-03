using System.IO;
using System.Reflection;

namespace Fapl.AdoptionUpdates.Shared.Helpers
{
    public static class AssemblyHelper
    {
        public static string GetManifestResourceString(this Assembly assembly, string name)
        {
            using (Stream data = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(name))
            using (TextReader reader = new StreamReader(data))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
