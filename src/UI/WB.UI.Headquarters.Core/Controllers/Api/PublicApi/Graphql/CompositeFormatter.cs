using System.Linq;
using System.Net;
using HotChocolate;
using HotChocolate.AspNetCore.Serialization;
using HotChocolate.Execution;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class CompositeFormatter : DefaultHttpResponseFormatter
    {
        protected override HttpStatusCode OnDetermineStatusCode(
            IQueryResult result, FormatInfo format,
            HttpStatusCode? proposedStatusCode)
        {
            if (result.Errors?.Count > 0)
            {
                if (result.Errors.Any(e => e.Code == ErrorCodes.Authentication.NotAuthorized || e.Code == ErrorCodes.Authentication.NotAuthenticated))
                    return HttpStatusCode.Forbidden;    

                return HttpStatusCode.BadRequest;
            }

            return base.OnDetermineStatusCode(result, format, proposedStatusCode);
        }
    }
}
