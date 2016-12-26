using System;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    public interface IParaDataAccessor
    {
        void ClearParaDataFolder();
        void ClearParaDataFile();
        void ArchiveParaDataFolder();
        void StoreInterviewParadata(InterviewHistoryView view);
        string GetPathToParaDataArchiveByQuestionnaire(Guid questionnaireId, long version);
    }
}