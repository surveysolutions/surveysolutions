using System;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    public interface IParaDataAccessor: IReadSideRepositoryWriter<InterviewHistoryView>
    {
        void ClearParaData();
        void PersistParaDataExport();
        void ArchiveParaDataExport();
        string GetPathToParaDataByQuestionnaire(Guid questionnaireId, long version);
    }
}