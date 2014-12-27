using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using Microsoft.Scripting.Hosting;

namespace IronPythonMef
{
    public class IronPythonScriptCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged
    {
        private readonly ScriptEngine _engine;
        private readonly IEnumerable<Type> _typesToInject;
        private readonly ExtractTypesFromScript _typeExtractor;
        private string _scriptSource;
        private Lazy<IList<IronPythonComposablePartDefinition>> _parts ;

        // TODO: arguments make up a "ambient" of types to inject, script source and whatnot
        public IronPythonScriptCatalog(ScriptEngine engine, TextReader reader, IEnumerable<Type> typesToInject)
        {
            _engine = engine;
            _typesToInject = typesToInject;
            _scriptSource = reader.ReadToEnd();
            _typeExtractor = new ExtractTypesFromScript(engine);
            _parts = new Lazy<IList<IronPythonComposablePartDefinition>>(() => CreateParts(_scriptSource));
            Changed = (sender, args) => { };
            Changing = (sender, args) => { };
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return _parts.Value.AsQueryable();
            }
        }

        private List<IronPythonComposablePartDefinition> CreateParts(string scriptSource)
        {
            // TODO: don't do this, use a proper CreateScriptFrom*
            var source = _engine.CreateScriptSourceFromString(scriptSource);
            return _typeExtractor.GetPartDefinitionsFromScript(source, _typesToInject).ToList();
        }

        public void Reload(TextReader changedPythonCode)
        {
            var previousParts = _parts.Value;
            var newScriptSource = changedPythonCode.ReadToEnd();
            var newParts = CreateParts(newScriptSource);

            using (var atomicComposition = new AtomicComposition())
            {
                OnChanging(new ComposablePartCatalogChangeEventArgs(newParts, previousParts, atomicComposition));
                _parts = new Lazy<IList<IronPythonComposablePartDefinition>>(() => newParts);
                _scriptSource = newScriptSource;
                atomicComposition.Complete();
            }
            OnChanged(new ComposablePartCatalogChangeEventArgs(newParts, previousParts, null));
        }

        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed;

        public void OnChanged(ComposablePartCatalogChangeEventArgs e)
        {
            var eventHandler = Changed;
            if (eventHandler == null) return;
            eventHandler(this, e);
        }

        public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing;

        public void OnChanging(ComposablePartCatalogChangeEventArgs e)
        {
            var eventHandler = Changing;
            if (eventHandler == null) return;
            eventHandler(this, e);
        }
    }
}