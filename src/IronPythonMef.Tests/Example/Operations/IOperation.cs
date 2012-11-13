namespace IronPythonMef.Tests.Example.Operations
{
    public interface IOperation
    {
        object Execute(params object[] args);
        string Name { get; }
        string Usage { get; }
    }
}
