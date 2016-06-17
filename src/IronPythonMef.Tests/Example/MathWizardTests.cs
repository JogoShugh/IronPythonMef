using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using IronPythonMef.Tests.Example.Operations;
using NUnit.Framework;

namespace IronPythonMef.Tests.Example
{
    [TestFixture]
    public class MathWizardTests
    {
        [Test]
        public void runs_script_with_operations_from_both_csharp_and_python()
        {
            var currentAssemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var ironPythonScriptCatalog = new IronPythonScriptCatalog(
                new CompositionHelper().GetResourceScript("Operations.Python.py"),
                typeof (IMathCheatSheet), typeof (IOperation));

            var masterCatalog = new AggregateCatalog(currentAssemblyCatalog, ironPythonScriptCatalog);
         
            var container = new CompositionContainer(masterCatalog);
            var mathWiz = container.GetExportedValue<MathWizard>();

            const string mathScript =
@"fib 6
fac 6
abs -99
pow 2 4
crc 3
";
            var results = mathWiz.ExecuteScript(mathScript).ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(8, results[0]);
            Assert.AreEqual(720, results[1]);
            Assert.AreEqual(99f, results[2]);
            Assert.AreEqual(16m, results[3]);
            Assert.AreEqual(9.4247782230377197d, results[4]);
        }
    }
}
