using System;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    public interface IParaDataAccessor
    {
        void ClearParaDataFolder();
        void ArchiveParaDataFolder();
        void StoreInterviewParadata(InterviewHistoryView view);
        string GetPathToParaDataArchiveByQuestionnaire(Guid questionnaireId, long version);
    }
}