using System.Collections.Generic;

namespace WB.Core.Synchronization
{
    public interface IImportManager
    {
        void Import(List<string> zipData);
    }
}
