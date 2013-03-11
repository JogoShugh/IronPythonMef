﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Linq;

namespace IronPythonMef
{
    public class IronPythonFile
    {
        private readonly FileInfo _pythonFile;
        private readonly ScriptEngine _engine;
        private readonly CompositionContainer _mefContainer;
        private readonly ExtractTypesFromScript _extractTypesFromScript;
        private readonly IEnumerable<Type> _typesToInject;
        private IEnumerable<IronPythonComposablePart> _currentParts;

        public IronPythonFile(FileInfo pythonFile, ScriptEngine engine, CompositionContainer mefContainer, ExtractTypesFromScript extractTypesFromScript)
            : this(pythonFile,engine,mefContainer,extractTypesFromScript, new Type[]{})
        {
        }
        
        public IronPythonFile(FileInfo pythonFile, ScriptEngine engine, CompositionContainer mefContainer, ExtractTypesFromScript extractTypesFromScript, IEnumerable<Type> typesToInject)
        {
            _pythonFile = pythonFile;
            _engine = engine;
            _mefContainer = mefContainer;
            _extractTypesFromScript = extractTypesFromScript;
            _typesToInject = typesToInject;
            _currentParts = new List<IronPythonComposablePart>();
        }

        public void Compose()
        {
            var script = _engine.CreateScriptSourceFromFile(_pythonFile.FullName);
            IEnumerable<IronPythonComposablePart> previousParts = _currentParts;
            IEnumerable<IronPythonComposablePart> newParts = new List<IronPythonComposablePart>();
            try
            {
                newParts = _extractTypesFromScript.GetPartsFromScript(script, _typesToInject).ToList();
            }
            catch (SyntaxErrorException e)
            {
                throw new SyntaxErrorExceptionPrettyWrapper(String.Format("Error compiling '{0}", _pythonFile.FullName), e);
            }
            catch (UnboundNameException e)
            {
                throw new PythonException(String.Format("Error executing '{0}'", _pythonFile.FullName), e);
            }
            _currentParts = newParts;
            var batch = new CompositionBatch(_currentParts, previousParts);
            _mefContainer.Compose(batch);
        }

        public void Decompose()
        {
            var batch = new CompositionBatch(new ComposablePart[] {}, _currentParts);
            _mefContainer.Compose(batch);
            _currentParts = new List<IronPythonComposablePart>();
        }

        public class PythonException : Exception
        {
            public PythonException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }

        [Serializable]
        public class SyntaxErrorExceptionPrettyWrapper : PythonException
        {
            private readonly SyntaxErrorException _innerException;
            public SyntaxErrorExceptionPrettyWrapper(string message, SyntaxErrorException innerException)
                : base(message, innerException)
            {
                _innerException = innerException;
            }

            public override string Message
            {
                get
                {
                    return String.Format("{4}\nLine {0}\n{1}\n{2}^---{3}", _innerException.Line,
                                         _innerException.GetCodeLine(),
                                         String.Join("", Enumerable.Repeat(" ", _innerException.Column).ToArray()),
                                         _innerException.Message,
                                         base.Message);
                }
            }
        }
    }
}