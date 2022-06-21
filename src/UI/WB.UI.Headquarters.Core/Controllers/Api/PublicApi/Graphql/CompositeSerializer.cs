using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Serialization;
using HotChocolate.Execution;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class CompositeSerializer : DefaultHttpResultSerializer
    {
        public override async ValueTask SerializeAsync(
            IExecutionResult result,
            Stream stream,
            CancellationToken cancellationToken)
        {
            await base.SerializeAsync(result, stream, cancellationToken);
        }
        
        public override HttpStatusCode GetStatusCode(IExecutionResult result)
        {
            var baseStatusCode = base.GetStatusCode(result);

            if (result is IQueryResult && baseStatusCode == HttpStatusCode.InternalServerError && result.Errors?.Count > 0)
            {
                if (result.Errors.Any(e => e.Code == ErrorCodes.Authentication.NotAuthorized || e.Code == ErrorCodes.Authentication.NotAuthenticated))
                    return HttpStatusCode.Forbidden;    

                return HttpStatusCode.BadRequest;    

            }

            return baseStatusCode;
        }
    }
}
