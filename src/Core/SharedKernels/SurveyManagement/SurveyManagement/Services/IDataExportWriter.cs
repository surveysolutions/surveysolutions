using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    internal interface IDataExportWriter : IReadSideRepositoryCleaner, IChacheableRepositoryWriter
    {
        void AddActionRecord(InterviewActionExportView actions, Guid questionnaireId, long questionnaireVersion);
        void AddOrUpdateInterviewRecords(InterviewDataExportView items, Guid questionnaireId, long questionnaireVersion);
        void DeleteInterviewRecords(Guid interviewId);
    }
}