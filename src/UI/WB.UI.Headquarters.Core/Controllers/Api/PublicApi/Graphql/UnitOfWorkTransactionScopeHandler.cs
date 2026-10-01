using HotChocolate.Execution;
using HotChocolate.Execution.Processing;
using Microsoft.Extensions.DependencyInjection;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public sealed class UnitOfWorkTransactionScopeHandler : ITransactionScopeHandler
    {
        public ITransactionScope Create(IRequestContext context)
        {
            return new MutationScope(context, context.Services.GetRequiredService<IUnitOfWork>());
        }

        private sealed class MutationScope : ITransactionScope
        {
            private readonly IRequestContext context;
            private readonly IUnitOfWork unitOfWork;
            private bool completionAttempted;

            public MutationScope(IRequestContext context, IUnitOfWork unitOfWork)
            {
                this.context = context;
                this.unitOfWork = unitOfWork;
            }

            public void Complete()
            {
                // Resolver errors can be reported without throwing through field middleware.
                if (context.Result is not IOperationResult { Data: not null, Errors: null or { Count: 0 } })
                    unitOfWork.DiscardChanges();

                // HotChocolate calls this once all mutation fields have finished, before formatting.
                // A failure must reach its exception middleware, not be masked/retried in Dispose.
                completionAttempted = true;
                unitOfWork.Complete();
            }

            public void Dispose()
            {
                if (!completionAttempted)
                    unitOfWork.DiscardChanges();

                // The workspace scope owns the unit of work and its sessions.
            }
        }
    }
}
