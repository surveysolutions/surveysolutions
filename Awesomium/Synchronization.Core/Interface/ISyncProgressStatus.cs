using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synchronization.Core.Interface
{
    public interface ISyncProgressStatus
    {
        int ProgressPercents { get; }
        bool IsError { get; }
        bool IsCanceled { get; }
    }
}
