using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using Microsoft.Scripting.Hosting;

namespace IronPythonMef
{
    public class IronPythonScriptCatalog : ComposablePartCatalog
    {
        private readonly ScriptEngine _engine;
        private readonly IEnumerable<Type> _typesToInject;
        private ExtractTypesFromScript _typeExtractor;
        private string _scriptSource;
        private IList<IronPythonComposablePartDefinition> _parts;

        // TODO: arguments make up a "ambient" of types to inject, script source and whatnot
        public IronPythonScriptCatalog(ScriptEngine engine, TextReader reader, IEnumerable<Type> typesToInject )
        {
            _engine = engine;
            _typesToInject = typesToInject;
            // TODO: don't do this, use a proper CreateScriptFrom*
            _scriptSource = reader.ReadToEnd();
            _typeExtractor = new ExtractTypesFromScript(engine);
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                // TODO: locking
                var source = _engine.CreateScriptSourceFromString(_scriptSource); 

                _parts =  _typeExtractor.GetPartDefinitionsFromScript(source, _typesToInject).ToList();
                return _parts.AsQueryable();
            }
        }

    }
}