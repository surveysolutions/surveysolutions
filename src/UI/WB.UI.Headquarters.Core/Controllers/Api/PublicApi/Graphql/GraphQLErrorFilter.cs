using HotChocolate;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class GraphQLErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            return ErrorBuilder.New()
                .SetMessage(error.Exception.Message)
                .Build();
        }
    }
}
