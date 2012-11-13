using System.Linq;
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
            var mathWiz = new MathWizard();

            new CompositionHelper().ComposeWithTypesExportedFromPythonAndCSharp(
                mathWiz,
                "Operations.Python.py",
                typeof(IMathCheatSheet),
                typeof(IOperation));

            const string mathScript =
@"fib 6
fac 6
abs -99
pow 2 4
crc 3
";
            var results = mathWiz.ExecuteScript(mathScript).ToList();

            Assert.AreEqual(8, results[0]);
            Assert.AreEqual(720, results[1]);
            Assert.AreEqual(99f, results[2]);
            Assert.AreEqual(16m, results[3]);
            Assert.AreEqual(9.4247782230377197d, results[4]);
        }
    }
}
