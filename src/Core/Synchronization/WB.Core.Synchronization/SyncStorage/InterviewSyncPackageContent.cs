using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.SyncStorage
{
    public class InterviewSyncPackageContent : IView
    {
        public InterviewSyncPackageContent()
        {
        }

        public InterviewSyncPackageContent(string packageId, string content, string metaInfo)
        {
            PackageId = packageId;
            Content = content;
            MetaInfo = metaInfo;
        }

        public string PackageId { get; set; }
        public string Content { get; set; }
        public string MetaInfo { get; set; }
    }
}
