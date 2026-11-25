using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Core.BoundedContexts.Headquarters.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public static class ReminderEmailSendLock
    {
        private static readonly TimeSpan AcquisitionTimeout = TimeSpan.FromSeconds(10);

        public static async Task<bool> TryAcquireDeliveryAsync(IUnitOfWork unitOfWork,
            QuestionnaireIdentity questionnaireIdentity)
        {
            var lockKey = questionnaireIdentity.ToString();
            return await TryAcquireAsync(unitOfWork, $"ReminderDelivery-{lockKey}").ConfigureAwait(false);
        }

        public static async Task AcquireRequestAsync(IUnitOfWork unitOfWork,
            QuestionnaireIdentity questionnaireIdentity, Guid requestId,
            CancellationToken cancellationToken = default)
        {
            await AcquireAsync(unitOfWork, $"ReminderRequest-{questionnaireIdentity}", cancellationToken)
                .ConfigureAwait(false);
            await AcquireAsync(unitOfWork, $"ReminderRequestId-{requestId:N}", cancellationToken)
                .ConfigureAwait(false);
        }

        public static Task AcquireSchedulingAsync(IUnitOfWork unitOfWork, Guid requestId,
            CancellationToken cancellationToken = default)
        {
            return AcquireAsync(unitOfWork, $"ReminderScheduling-{requestId:N}", cancellationToken);
        }

        public static Task<bool> TryAcquireExecutionAsync(IUnitOfWork unitOfWork, Guid requestId) =>
            TryAcquireAsync(unitOfWork, $"ReminderExecution-{requestId:N}");

        private static async Task AcquireAsync(IUnitOfWork unitOfWork, string lockKey,
            CancellationToken cancellationToken)
        {
            using var timeout = new CancellationTokenSource(AcquisitionTimeout);
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, timeout.Token);

            try
            {
                while (!await TryAcquireAsync(unitOfWork, lockKey, linkedCancellation.Token).ConfigureAwait(false))
                    await Task.Delay(50, linkedCancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested &&
                                                     !cancellationToken.IsCancellationRequested)
            {
                throw new ReminderSendLockTimeoutException();
            }
        }

        private static async Task<bool> TryAcquireAsync(IUnitOfWork unitOfWork, string lockKey,
            CancellationToken cancellationToken = default)
        {
            // Advisory locks are database-wide, so include the current workspace schema in the lock key.
            return await unitOfWork.Session.CreateSQLQuery(
                    "select pg_try_advisory_xact_lock(hashtext(current_schema() || :lockKey))")
                .SetString("lockKey", lockKey)
                .UniqueResultAsync<bool>(cancellationToken)
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

    public class ReminderSendLockTimeoutException : Exception
    {
        public ReminderSendLockTimeoutException()
            : base("Another reminder email request is being processed. Retry shortly.")
        {
        }
    }
}
