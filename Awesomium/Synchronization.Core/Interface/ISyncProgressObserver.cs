using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synchronization.Core.Interface
{
    public interface ISyncProgressObserver
    {
        void SetBeginning();
        void SetProgress(ISyncProgressStatus status);
        void SetCompleted(ISyncProgressStatus status);
    }
}
