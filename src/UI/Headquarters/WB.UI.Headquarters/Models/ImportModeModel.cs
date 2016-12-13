using System;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.UI.Headquarters.Models
{
    public class ImportModeModel
    {
        public QuestionnaireInfo QuestionnaireInfo
        {
            get;
            set;
        }

        public long NewVersionNumber { get; set; }
        public DateTime PreviousVersionUploadedDate { get; set; }
    }
}