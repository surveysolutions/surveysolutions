using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Synchronization.ImportManager
{
    public interface IImportManager
    {
        void Import(List<string> zipData);
    }
}
