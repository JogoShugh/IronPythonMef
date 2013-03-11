using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using IronPython.Hosting;
using NUnit.Framework;

namespace IronPythonMef.Tests
{
    [TestFixture]
    public class MefImportAndExportTests
    {
        private readonly IEnumerable<Type> _injectTypes = new List<Type>
        {
            typeof (IActOnItem),
            typeof (IItemSource),
            typeof (BasePythonItemSource)
        };

        [Test]
        public void ExtractTokensFromString()
        {
            const string pythonCode = 
@"
class StringItemSource(IItemSource):
    def GetAllItems(self):
        return [""Item 1"", ""Item 2"", ""Item 3""]
";

            var engine = Python.CreateEngine();
            var script = engine.CreateScriptSourceFromString(pythonCode);
            var typeExtractor = new ExtractTypesFromScript(engine);
            var types = typeExtractor.GetTypesFromScript(script, _injectTypes).ToList();
            Assert.AreEqual(1, types.Count());
            Assert.AreEqual("StringItemSource", types.First().Name);

            var instance = types.First().Activator();
            Assert.IsInstanceOf<IItemSource>(instance);
        }

        [Test]
        public void CanComposeExportsFromPythonCode()
        {
            const string pythonCode = 
@"
class StringItemSource(BasePythonItemSource):
    __exports__ = [IItemSource]

    def GetAllItems(self):
        return [""Item 1"", ""Item 2"", ""Item 3""]
";

            var engine = Python.CreateEngine();
            var script = engine.CreateScriptSourceFromString(pythonCode);
            var typeExtractor = new ExtractTypesFromScript(engine);
            var exports = typeExtractor.GetPartsFromScript(script, _injectTypes).ToList();

            var container = new CompositionContainer();
            var batch = new CompositionBatch(exports, new ComposablePart[] { });
            container.Compose(batch);

            var instance = new MockImporter();
            container.SatisfyImportsOnce(instance);

            Assert.AreEqual(1, instance.ItemSources.Count());
        }

        [Test]
        public void CanComposeExportsFromPythonCodeWithDecorator()
        {
            const string pythonCode = 
@"
@export(IItemSource)
class StringItemSource(BasePythonItemSource):
    def GetAllItems(self):
        return [""Item 1"", ""Item 2"", ""Item 3""]
";

            var engine = Python.CreateEngine();
            var script = engine.CreateScriptSourceFromString(pythonCode);
            var typeExtractor = new ExtractTypesFromScript(engine);
            var exports = typeExtractor.GetPartsFromScript(script, _injectTypes).ToList();

            var container = new CompositionContainer();
            var batch = new CompositionBatch(exports, new ComposablePart[] { });
            container.Compose(batch);

            var instance = new MockImporter();
            container.SatisfyImportsOnce(instance);

            Assert.AreEqual(1, instance.ItemSources.Count());
        }

//        [Test]
//        public void can_export_types_from_python_with_export_decorator()
//        {
//            const string pythonCode = 
//@"@export(ITranslateApiInputToAssetXml)
//class TranslateYamlToAssetXml(ITranslateApiInputToAssetXml):
//    def CanTranslate(self, contentType):
//        return contentType.lower() in map(str.lower, ['text/yaml', 'application/yaml', 'yaml'])
//    
//    def Execute(self, input):
//        output = '<Asset><Attribute name=""Name"" act=""set"">' + input + '</Attribute></Asset>'
//        reader = StringReader(output)
//        doc = XPathDocument(reader)
//        return doc
//";

//            var engine = Python.CreateEngine();
//            var script = engine.CreateScriptSourceFromString(pythonCode);

//            var types = new[]
//                            {
//                                typeof (ITranslateApiInputToAssetXml),
//                                typeof (System.Xml.XPath.XPathDocument),
//                                typeof (System.IO.StringReader)
//                            };
//            var typeExtractor = new ExtractTypesFromScript(engine);
//            var exports = typeExtractor.GetPartsFromScript(script, types.ToList()).ToList();

//            var container = new CompositionContainer();
//            var batch = new CompositionBatch(exports, new ComposablePart[] { });
//            container.Compose(batch);

//            var instance = new MockTranslatorImporter();
//            container.SatisfyImportsOnce(instance);
            
//            Assert.AreEqual(1, instance.Translators.Count());

//            var translators = instance.Translators.ToList();

//            Assert.IsTrue(translators[0].CanTranslate("text/yaml"));
//            Assert.IsFalse(translators[0].CanTranslate("text/buggabugga"));

//            const string expected = 
//@"<Asset>
//  <Attribute name=""Name"" act=""set"">My Test</Attribute>
//</Asset>";
//            var doc = translators[0].Execute("My Test");
//            var actual = doc.CreateNavigator().OuterXml;

//            Assert.AreEqual(expected, actual);
//        }

        [Test]
        public void CanComposeMultipleExportsFromPythonCodeWithDecorator()
        {
            const string pythonCode = 
@"
@export(IItemSource)
@export(IActOnItem)
class StringItemSource(BasePythonItemSource, IActOnItem):
    def GetAllItems(self):
        return [""Item 1"", ""Item 2"", ""Item 3""]

    @property
    def Text(self):
        return ""TextItem""

    @property
    def TypedItemType(self):
        return clr.GetClrType(type(""""))
";

            var engine = Python.CreateEngine();
            var script = engine.CreateScriptSourceFromString(pythonCode);
            var typeExtractor = new ExtractTypesFromScript(engine);
            var exports = typeExtractor.GetPartsFromScript(script, _injectTypes).ToList();

            var container = new CompositionContainer();
            var batch = new CompositionBatch(exports, new ComposablePart[] { });
            container.Compose(batch);

            var instance = new MockImporter();
            container.SatisfyImportsOnce(instance);

            Assert.AreEqual(1, instance.ItemSources.Count());
            Assert.AreEqual(1, instance.Actions.Count());
        }

        [Test]
        public void CanImportListIntoPythonClass()
        {
            const string pythonCode = 
@"
class StringItemSource:
    def import_actions(self, actions):
        self.actions = actions
    def normal_method(self):
        pass

StringItemSource.import_actions.func_dict['imports'] = IronPythonImportDefinition('import_action', IActOnItem, 'ZeroOrOne', True, True)
";

            var engine = Python.CreateEngine();
            var script = engine.CreateScriptSourceFromString(pythonCode);

            var typeExtractor = new ExtractTypesFromScript(engine);
            var exports = typeExtractor.GetPartsFromScript(script, _injectTypes).ToList();

            var container = new CompositionContainer(new TypeCatalog(typeof(MockExporter), typeof(MockImportActions)));

            var batch = new CompositionBatch(exports, new ComposablePart[] { });

            container.Compose(batch);

            var value = container.GetExportedValue<MockImportActions>();
            Assert.AreEqual(1, value.ActOnItems.Count());
            IEnumerable actions = exports.First().Instance.actions;
            Assert.AreEqual(1, actions.OfType<IActOnItem>().Count());
            Assert.AreEqual(1, actions.OfType<MockExporter>().Count());
        }

        [Test]
        public void CanImportIntoPythonClassUsingDecorator()
        {
            const string pythonCode = 
@"
class StringItemSource:
    @import_many(IActOnItem)
    def import_actions(self, actions):
        self.actions = actions
";

            var engine = Python.CreateEngine();
            var paths = engine.GetSearchPaths();
            paths.Add(@"D:\documents\dev\ILoveLucene\lib\ironpython\Lib");
            engine.SetSearchPaths(paths);
            var script = engine.CreateScriptSourceFromString(pythonCode);

            var typeExtractor = new ExtractTypesFromScript(engine);
            var exports = typeExtractor.GetPartsFromScript(script, _injectTypes).ToList();

            var container = new CompositionContainer(new TypeCatalog(typeof(MockExporter), typeof(MockImportActions)));

            var batch = new CompositionBatch(exports, new ComposablePart[] { });

            container.Compose(batch);

            var value = container.GetExportedValue<MockImportActions>();
            Assert.AreEqual(1, value.ActOnItems.Count());
            IEnumerable actions = exports.First().Instance.actions;
            Assert.AreEqual(1, actions.OfType<IActOnItem>().Count());
            Assert.AreEqual(1, actions.OfType<MockExporter>().Count());
        }

        [Test]
        public void GenericTypesAreIncludedWithCorrectName()
        {
            const string pythonCode = 
@"
class Something(GenericClass[str]):
    def GetString(self, something):
        return str(something)
";

            var engine = Python.CreateEngine();
            var script = engine.CreateScriptSourceFromString(pythonCode);
            var scope = engine.CreateScope();

            scope.InjectType(typeof(GenericClass<>));
            Assert.True(scope.ContainsVariable("GenericClass"));

            script.Execute(scope);
            var cls = scope.GetVariable("Something");
            var instance = cls();
            Assert.IsInstanceOf<GenericClass<String>>(instance);
            var e = (GenericClass<string>)instance;
            Assert.AreEqual("Babalu", e.GetString("Babalu"));
        }

        [Test]
        public void CanImportJustOneItemIntoPythonClassUsingDecorator()
        {
            const string pythonCode = 
@"
class StringItemSource:
    @import_one(IActOnItem)
    def import_action(self, action):
        self.action = action
";

            var engine = Python.CreateEngine();
            var script = engine.CreateScriptSourceFromString(pythonCode);

            var typeExtractor = new ExtractTypesFromScript(engine);
            var exports = typeExtractor.GetPartsFromScript(script, _injectTypes).ToList();

            var container = new CompositionContainer(new TypeCatalog(typeof(MockExporter), typeof(MockImportActions)));

            var batch = new CompositionBatch(exports, new ComposablePart[] { });

            container.Compose(batch);

            object action = exports.First().Instance.action;
            Assert.NotNull(action);
            Assert.IsInstanceOf<IActOnItem>(action);
        }

        //        [Test]
        //        public void TouchedFileCausesRecomposition()
        //        {
        //            var ironpythonDir = "IronPythonCommands".AsNewDirectoryInfo();
        //            @"
        //@export(IItemSource)
        //class StringItemSource(BasePythonItemSource):
        //    def GetAllItems(self):
        //        return [""Item 1"", ""Item 2"", ""Item 3""]
        //".WriteToFileInPath(ironpythonDir, "python.py");

        //            var container = new CompositionContainer(new TypeCatalog(typeof(MockImporter)));

        //            var commands = new IronPythonCommandsMefExport(container, new DebugLogger());
        //            commands.CoreConfiguration = new CoreConfiguration("data".AsNewDirectoryInfo().FullName, ".");
        //            commands.Configuration = new Configuration {ScriptDirectories = new List<string> {ironpythonDir.FullName}};
        //            commands.Execute();

        //            var importer = container.GetExportedValue<MockImporter>();
        //            Assert.AreEqual(1, importer.ItemSources.Count());

        //            var newCode = @"
        //@export(IItemSource)
        //class StringItemSource(BasePythonItemSource):
        //    def GetAllItems(self):
        //        return [""Item 1"", ""Item 2"", ""Item 3""]
        //
        //@export(IItemSource)
        //class SecondStringItemSource(BasePythonItemSource):
        //    def GetAllItems(self):
        //        return [""Item 1"", ""Item 2"", ""Item 3""]
        //
        //";

        //            EventHelper.WaitForEvent(e => commands.RefreshedFiles += e,
        //                e => commands.RefreshedFiles -= e,
        //                () => newCode.WriteToFileInPath(ironpythonDir, "python.py"));

        //            importer = container.GetExportedValue<MockImporter>();
        //            Assert.AreEqual(2, importer.ItemSources.Count());
        //        }

        [Test]
        public void CanInjectTypesIntoIronPythonFileToAndExportThem()
        {
            var ironpythonDir = "IronPythonCommands".AsNewDirectoryInfo();
            @"
@export(IItemSource)
class StringItemSource(BasePythonItemSource):
    def GetAllItems(self):
        return [""Item 1"", ""Item 2"", ""Item 3""]
        ".WriteToFileInPath(ironpythonDir, "python.py");

            var container = new CompositionContainer(new TypeCatalog(typeof(MockImporter)));


            var engine = Python.CreateEngine();


            var file = new IronPythonFile(new FileInfo(Path.Combine(ironpythonDir.FullName, "python.py")), engine, container,
                               new ExtractTypesFromScript(engine), new[] { typeof(IItemSource), typeof(BasePythonItemSource) });

            file.Compose();

        }

        [Test]
        public void ClassWithoutExportsResultsInZeroParts()
        {
            const string pythonCode = 
@"
class StringItemSource(BasePythonItemSource):
    def GetAllItems(self):
        return [""Item 1"", ""Item 2"", ""Item 3""]
";

            var engine = Python.CreateEngine();
            var script = engine.CreateScriptSourceFromString(pythonCode);
            var typeExtractor = new ExtractTypesFromScript(engine);
            var exports = typeExtractor.GetPartsFromScript(script, _injectTypes).ToList();

            Assert.AreEqual(0, exports.Count());
        }
    }

    public abstract class GenericClass<T>
    {
        public abstract string GetString(T t);
    }

    [Export(typeof(MockImporter))]
    public class MockImporter
    {
        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<IItemSource> ItemSources { get; set; }

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<IActOnItem> Actions { get; set; }
    }

    [Export(typeof(MockImportActions))]
    public class MockImportActions
    {
        [ImportMany]
        public IEnumerable<IActOnItem> ActOnItems { get; set; }
    }

    [Export(typeof(IActOnItem))]
    public class MockExporter : IActOnItem
    {
        public string Text
        {
            get { return "Act"; }
        }

        public Type TypedItemType
        {
            get { return typeof(string); }
        }
    }

    public static class PathHelper
    {
        public static DirectoryInfo AsNewDirectoryInfo(this string path)
        {
            var configurationDirectory = new DirectoryInfo(path);
            if (configurationDirectory.Exists)
            {
                configurationDirectory.Delete(true);
            }
            configurationDirectory.Create();
            return configurationDirectory;
        }

        public static void WriteToFileInPath(this string content, DirectoryInfo path, string filename)
        {
            File.WriteAllText(Path.Combine(path.FullName, filename), content);
        }
    }
}