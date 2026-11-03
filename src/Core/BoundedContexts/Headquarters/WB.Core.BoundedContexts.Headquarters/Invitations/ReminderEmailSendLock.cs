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
            await AcquireAsync(unitOfWork, $"ReminderDelivery-{lockKey}").ConfigureAwait(false);
        }

        public static Task AcquireRequestAsync(IUnitOfWork unitOfWork,
            QuestionnaireIdentity questionnaireIdentity, EmailTextTemplateType reminderType)
        {
            var lockKey = SendRemindersNowRequest.GetStorageKey(questionnaireIdentity, reminderType);
            return AcquireAsync(unitOfWork, $"ReminderRequest-{lockKey}");
        }

        private static async Task AcquireAsync(IUnitOfWork unitOfWork, string lockKey)
        {
            // Advisory locks are database-wide, so include the current workspace schema in the lock key.
            await unitOfWork.Session.CreateSQLQuery(
                    "select pg_advisory_xact_lock(hashtext(current_schema() || :lockKey))")
                .SetString("lockKey", lockKey)
                .ExecuteUpdateAsync()
                .ConfigureAwait(false);
        }
    }
}
