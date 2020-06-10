using System;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using Microsoft.Extensions.Logging;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class TransactionMiddleware
    {
        private FieldDelegate _next;

        public TransactionMiddleware(FieldDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(IMiddlewareContext context, IUnitOfWork unitOfWork)
        {

            try
            {
                await _next(context);
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
