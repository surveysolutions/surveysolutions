using System;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels
{
    public class CountDaysOfInterviewInStatusInputModel
    {
        public Guid? QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }

        public InterviewExportedAction[] Statuses { get; set; }
    }
}