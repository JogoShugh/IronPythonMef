using System.IO;

namespace IronPythonMef.Tests.Example
{
    public class CompositionHelper
    {
        public TextReader GetResourceScript(string resourceScriptPath)
        {
            using (var scriptStream = GetType().Assembly.
                GetManifestResourceStream(GetType(), resourceScriptPath))
            using (var scriptText = new StreamReader(scriptStream))
            {
                return new StringReader(scriptText.ReadToEnd());
            }
        }
    }
}
