using System;

namespace DataEntryClient.CompleteQuestionnaire
{
    public interface ICompleteQuestionnaireSync
    {
        void Export();
        Guid? GetLastSyncEventGuid(Guid clientKey);
        void UploadEvents(Guid clientKey, Guid? lastSyncEvent);

        void Import();
    }
}