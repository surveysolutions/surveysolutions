using HotChocolate;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class GraphQLErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            if (!string.IsNullOrEmpty(error.Code))
            {
                return error;
            }
            
            return ErrorBuilder.New()
                .SetMessage(error.Exception.Message)
                .Build();
        }
    }
}
