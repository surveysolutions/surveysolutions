using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization
{
    public interface ISyncPackagesProcessor
    {
        void ProcessNextSyncPackage();
    }
}  