using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;

namespace IronPythonMef.Tests
{
    [TestFixture]
    public class IronPythonScriptCatalogTests
    {
        private readonly Type[] _injectTypes = {
            typeof (IActOnItem),
            typeof (IItemSource),
            typeof (IDependency),
            typeof (BasePythonItemSource)
        };

        private ScriptEngine _engine;

        [SetUp]
        public void Setup()
        {
             _engine = Python.CreateEngine();
        }

        [Test]
        public void CatalogCanExportValueBasedOnDecorator()
        {
            const string pythonCode =
@"
@export(IItemSource)
class StringItemSource(BasePythonItemSource):
    def GetAllItems(self):
        return [""Item 1"", ""Item 2"", ""Item 3""]
";

            var catalog = new IronPythonScriptCatalog(_engine, new StringReader(pythonCode), _injectTypes);
            var container = new CompositionContainer(catalog);
            var itemSources = container.GetExportedValues<IItemSource>().ToList();
            Assert.AreEqual(1, itemSources.Count);
            var strings = itemSources[0].GetItems().Result.Cast<string>().ToList();
            Assert.AreEqual(3, strings.Count);
            Assert.AreEqual("Item 1", strings[0]);
            Assert.AreEqual("Item 2", strings[1]);
            Assert.AreEqual("Item 3", strings[2]);
        }

        [Test]
        public void PythonCodeCanUseImports()
        {
            const string pythonCode =
@"
@export(IItemSource)
class StringItemSource(BasePythonItemSource):
    def GetAllItems(self):
        return [""Item 1"", ""Item 2"", ""Item 3""]

    @import_one(IDependency)
    def import_Dependency(self, dep):
        self.dep = dep
";

            var catalog = new IronPythonScriptCatalog(_engine, new StringReader(pythonCode), _injectTypes);
            var typeCatalog = new TypeCatalog(typeof(Dependency));
            var container = new CompositionContainer(new AggregateCatalog(catalog, typeCatalog));
            var itemSource = container.GetExportedValues<IItemSource>().Cast<dynamic>().FirstOrDefault();
            Assert.NotNull(itemSource);
            Assert.NotNull(itemSource.dep);
            Assert.IsInstanceOfType(typeof(Dependency), itemSource.dep);
        }

        [Test]
        public void CanRecomposeFromCatalog()
        {
            const string pythonCode =
@"
@export(IItemSource)
class StringItemSource(BasePythonItemSource):
    def GetAllItems(self):
        return [""Item 1"", ""Item 2"", ""Item 3""]
";
            const string changedPythonCode =
@"
@export(IItemSource)
class StringItemSource(BasePythonItemSource):
    def GetAllItems(self):
        return [""Item 2""]
";

            var pythonCatalog = new IronPythonScriptCatalog(_engine, new StringReader(pythonCode), _injectTypes);
            var typeCatalog = new TypeCatalog(typeof (ItemSources));
            var aggregateCatalog = new AggregateCatalog(pythonCatalog,typeCatalog);
            var container = new CompositionContainer(aggregateCatalog);

            var itemSources = container.GetExportedValues<ItemSources>().First();
            var strings = itemSources.Sources.SelectMany(s => s.GetItems().Result).Cast<string>().ToList();
            Assert.AreEqual(3, strings.Count);
            Assert.AreEqual("Item 1", strings[0]);
            Assert.AreEqual("Item 2", strings[1]);
            Assert.AreEqual("Item 3", strings[2]);

            pythonCatalog.Reload(new StringReader(changedPythonCode));

            strings = itemSources.Sources.SelectMany(s => s.GetItems().Result).Cast<string>().ToList();
            Assert.AreEqual(1, strings.Count);
            Assert.AreEqual("Item 2", strings[0]);
        }

        [Export(typeof(ItemSources))]
        class ItemSources
        {
            [ImportMany(typeof(IItemSource), AllowRecomposition = true)]
            public IEnumerable<IItemSource> Sources { get; set; }
        }
    }
}