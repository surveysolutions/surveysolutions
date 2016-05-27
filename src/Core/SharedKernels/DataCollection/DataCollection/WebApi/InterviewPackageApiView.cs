using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    [Obsolete ("Sinse v5.3")]
    public class InterviewPackagesApiView
    {
        public List<SynchronizationChunkMeta> Packages { get; set; }
        public List<InterviewApiView> Interviews { get; set; }
    }
}