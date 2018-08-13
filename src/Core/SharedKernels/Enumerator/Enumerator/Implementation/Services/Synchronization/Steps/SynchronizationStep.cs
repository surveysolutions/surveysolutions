using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public abstract class SynchronizationStep : ISynchronizationStep
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly ILogger logger;

        protected SynchronizationStep(int sortOrder, ISynchronizationService synchronizationService, ILogger logger)
        {
            this.synchronizationService = synchronizationService;
            this.logger = logger;
            SortOrder = sortOrder;
        }

        public int SortOrder { get; protected set; }

        public abstract Task ExecuteAsync();

        public string Name => this.GetType().Name;

        public EnumeratorSynchonizationContext Context { get; set; }

        protected async Task TrySendUnexpectedExceptionToServerAsync(Exception exception)
        {
            if (exception.GetSelfOrInnerAs<OperationCanceledException>() != null)
                return;

            try
            {
                await this.synchronizationService.SendUnexpectedExceptionAsync(
                    this.ToUnexpectedExceptionApiView(exception), CancellationToken.None);
            }
            catch (Exception ex)
            {
                this.logger.Error("Synchronization. Exception when send exception to server", ex);
            }
        }

        private UnexpectedExceptionApiView ToUnexpectedExceptionApiView(Exception exception)
        {
            return new UnexpectedExceptionApiView
            {
                Message = exception.Message,
                StackTrace = string.Join(Environment.NewLine,
                    exception.UnwrapAllInnerExceptions().Select(ex => $"{ex.Message} {ex.StackTrace}"))
            };
        }
    }
}
