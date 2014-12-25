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
        private readonly ExtractTypesFromScript _typeExtractor;
        private readonly string _scriptSource;
        private readonly Lazy<IList<IronPythonComposablePartDefinition>> _parts ;

        // TODO: arguments make up a "ambient" of types to inject, script source and whatnot
        public IronPythonScriptCatalog(ScriptEngine engine, TextReader reader, IEnumerable<Type> typesToInject)
        {
            _engine = engine;
            _typesToInject = typesToInject;
            _scriptSource = reader.ReadToEnd();
            _typeExtractor = new ExtractTypesFromScript(engine);
            _parts = new Lazy<IList<IronPythonComposablePartDefinition>>(CreateParts);
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return _parts.Value.AsQueryable();
            }
        }

        private List<IronPythonComposablePartDefinition> CreateParts()
        {
            // TODO: don't do this, use a proper CreateScriptFrom*
            var source = _engine.CreateScriptSourceFromString(_scriptSource);
            return _typeExtractor.GetPartDefinitionsFromScript(source, _typesToInject).ToList();
        }
    }
}