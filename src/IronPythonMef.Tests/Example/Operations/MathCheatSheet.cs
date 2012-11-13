using System.ComponentModel.Composition;

namespace IronPythonMef.Tests.Example.Operations
{
    [Export(typeof(IMathCheatSheet))]
    public class MathCheatSheet : IMathCheatSheet
    {
        public MathCheatSheet()
        {
            Pi = 3.141592653589793f;
        }

        public float Pi { get; set; }
    }
}