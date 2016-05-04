using System.Collections.Generic;

namespace WB.Core.Infrastructure.Versions
{
    public interface IProductVersionHistory
    {
        void RegisterCurrentVersion();

        IEnumerable<ProductVersionChange> GetHistory();
    }
}