using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Core.BoundedContexts.Headquarters.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public static class ReminderEmailSendLock
    {
        public static async Task<bool> TryAcquireDeliveryAsync(IUnitOfWork unitOfWork,
            QuestionnaireIdentity questionnaireIdentity, EmailTextTemplateType reminderType)
        {
            var lockKey = SendRemindersNowRequest.GetStorageKey(questionnaireIdentity, reminderType);
            return await TryAcquireAsync(unitOfWork, $"ReminderDelivery-{lockKey}").ConfigureAwait(false);
        }

        public static async Task AcquireRequestAsync(IUnitOfWork unitOfWork,
            QuestionnaireIdentity questionnaireIdentity, EmailTextTemplateType reminderType)
        {
            var lockKey = SendRemindersNowRequest.GetStorageKey(questionnaireIdentity, reminderType);
            while (!await TryAcquireAsync(unitOfWork, $"ReminderRequest-{lockKey}").ConfigureAwait(false))
                await Task.Delay(50).ConfigureAwait(false);
        }

        public static async Task AcquireSchedulingAsync(IUnitOfWork unitOfWork, Guid requestId)
        {
            while (!await TryAcquireAsync(unitOfWork, $"ReminderScheduling-{requestId:N}").ConfigureAwait(false))
                await Task.Delay(50).ConfigureAwait(false);
        }

        public static Task<bool> TryAcquireExecutionAsync(IUnitOfWork unitOfWork, Guid requestId) =>
            TryAcquireAsync(unitOfWork, $"ReminderExecution-{requestId:N}");

        private static async Task<bool> TryAcquireAsync(IUnitOfWork unitOfWork, string lockKey)
        {
            // Advisory locks are database-wide, so include the current workspace schema in the lock key.
            return await unitOfWork.Session.CreateSQLQuery(
                    "select pg_try_advisory_xact_lock(hashtext(current_schema() || :lockKey))")
                .SetString("lockKey", lockKey)
                .UniqueResultAsync<bool>()
                .ConfigureAwait(false);
        }
    }

    public class ReminderDeliveryInProgressException : Exception
    {
        public ReminderDeliveryInProgressException()
            : base("Another reminder email batch is currently being sent.")
        {
        }
    }
}
