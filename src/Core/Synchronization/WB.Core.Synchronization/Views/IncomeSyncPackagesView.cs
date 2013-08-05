using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Synchronization.Views
{
    public class IncomeSyncPackagesView
    {
        public IncomeSyncPackagesView(int avalibleIncomPackagesCount)
        {
            AvalibleIncomPackagesCount = avalibleIncomPackagesCount;
        }

        public int AvalibleIncomPackagesCount { get; private set; }
    }
}
