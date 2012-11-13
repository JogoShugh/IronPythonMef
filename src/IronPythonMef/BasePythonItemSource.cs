using System.Collections.Generic;
using System.Threading.Tasks;

namespace IronPythonMef
{
    public abstract class BasePythonItemSource : BaseItemSource
    {
        public override Task<IEnumerable<object>> GetItems()
        {
            return Task.Factory.StartNew(() => GetAllItems());
        }

        protected abstract IEnumerable<object> GetAllItems();
    }
}