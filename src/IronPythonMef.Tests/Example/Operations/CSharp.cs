using System;
using System.ComponentModel.Composition;

namespace IronPythonMef.Tests.Example.Operations
{
    [Export(typeof(IOperation))]
    public class Power : IOperation
    {
        public object Execute(params object[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException(Usage, "args");
            }

            var x = Convert.ToDouble(args[0]);
            var y = Convert.ToDouble(args[1]);

            return Math.Pow(x, y);
        }

        public string Name
        {
            get { return "pow"; }
        }

        public string Usage
        {
            get { return "pow n, y -- calculates n to the y power"; }
        }
    }

    [Export(typeof(IOperation))]
    public class Factorial : IOperation
    {
        public object Execute(params object[] args)
        {
            if (args.Length < 1)
            {
                throw new ArgumentException(Usage, "args");
            }

            var n = Convert.ToInt32(args[0]);

            int i;
            long x = 1;
            for (i = n; i > 1; i--) x = x * i;

            return x;
        }

        public string Name
        {
            get { return "fac"; }
        }

        public string Usage
        {
            get { return "fac n -- calculates n!"; }
        }
    }
}