using System;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class TransactionMiddleware
    {
        private readonly FieldDelegate next;

        public TransactionMiddleware(FieldDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(IMiddlewareContext context, IUnitOfWork unitOfWork)
        {
            try
            {
                await next(context);
                unitOfWork.AcceptChanges();
            }
            catch (Exception)
            {
                unitOfWork.DiscardChanges();
                throw;
            }
        }
    }
}
