using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace IronPythonMef.Tests.Example
{
    public class CompositionHelper
    {
        public void ComposeWithTypesExportedFromPythonAndCSharp(
            object compositionTarget,
            string scriptsToImport,
            params Type[] typesToImport)
        {
            ScriptSource script;
            var engine = Python.CreateEngine();
            using (var scriptStream = GetType().Assembly.
                GetManifestResourceStream(GetType(), scriptsToImport))
            using (var scriptText = new StreamReader(scriptStream))
            {
                script = engine.CreateScriptSourceFromString(scriptText.ReadToEnd());
            }

            var typeExtractor = new ExtractTypesFromScript(engine);
            var exports = typeExtractor.GetPartsFromScript(script, typesToImport).ToList();

            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());

            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch(exports, new ComposablePart[] { });
            container.Compose(batch);
            
            container.SatisfyImportsOnce(compositionTarget);
        }
    }
}
