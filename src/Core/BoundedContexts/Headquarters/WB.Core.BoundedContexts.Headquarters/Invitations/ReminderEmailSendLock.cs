using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Core.BoundedContexts.Headquarters.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public static class ReminderEmailSendLock
    {
        public static async Task AcquireAsync(IUnitOfWork unitOfWork,
            QuestionnaireIdentity questionnaireIdentity, EmailTextTemplateType reminderType)
        {
            var lockKey = SendRemindersNowRequest.GetStorageKey(questionnaireIdentity, reminderType);
            // This advisory transaction lock covers recipient selection, delivery, and the delivery-state update.
            await unitOfWork.Session.CreateSQLQuery("select pg_advisory_xact_lock(hashtext(:lockKey))")
                .SetString("lockKey", lockKey)
                .ExecuteUpdateAsync()
                .ConfigureAwait(false);
        }
    }
}
