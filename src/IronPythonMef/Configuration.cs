using System.Collections.Generic;

namespace IronPythonMef
{
    [PluginConfiguration]
    public class Configuration
    {
        public List<string> ScriptDirectories { get; set; }

        public Configuration()
        {
            ScriptDirectories = new List<string>() {"$plugins$\\IronPython"};
        }
    }
}