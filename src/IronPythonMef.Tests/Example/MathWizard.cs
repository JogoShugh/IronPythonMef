using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using IronPythonMef.Tests.Example.Operations;

namespace IronPythonMef.Tests.Example
{
    [Export(typeof(MathWizard))]
    public class MathWizard
    {
        [ImportMany]
        private IEnumerable<IOperation> _operations = new IOperation[]{};

        public IEnumerable<object> ExecuteScript(string script)
        {
            var lines = script.Split(new [] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                foreach (var result in from line in lines
                        select line.Split(new [] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries) into tokens
                        let operationName = tokens[0]
                        let operation = _operations.FirstOrDefault(x => x.Name.Equals(operationName, StringComparison.OrdinalIgnoreCase))
                        where operation != null
                        let args = tokens.Skip(1).Select(x => x.Trim()).ToArray()
                        select operation.Execute(args))
                {
                    yield return result;
                }
            }
        }
    }
}
