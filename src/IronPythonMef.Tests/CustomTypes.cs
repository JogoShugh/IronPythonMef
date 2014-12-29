using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace IronPythonMef.Tests
{
    public interface IActOnItem
    {
        string Text { get; }
        Type TypedItemType { get; }
    }

    public interface IDependency
    {
        
    }

    [Export(typeof(IDependency))]
    public class Dependency : IDependency
    {
        
    }


    public interface IItemSource
    {
        Task<IEnumerable<object>> GetItems();
        string Id { get; }
        /// <summary>
        /// If an itemsource is persistent, then we use a filesystem directory to store the index results.
        /// Otherwise we use an in memory directory
        /// </summary>
        bool Persistent { get; }
    }

    public abstract class BaseItemSource : IItemSource
    {
        public abstract Task<IEnumerable<object>> GetItems();

        public virtual string Id
        {
            get { return GetType().FullName; }
        }

        public virtual bool Persistent
        {
            get { return true; }
        }
    }

    public abstract class BasePythonItemSource : BaseItemSource
    {
        public override Task<IEnumerable<object>> GetItems()
        {
            return Task.Factory.StartNew(() => GetAllItems());
        }

        protected abstract IEnumerable<object> GetAllItems();
    }
}
