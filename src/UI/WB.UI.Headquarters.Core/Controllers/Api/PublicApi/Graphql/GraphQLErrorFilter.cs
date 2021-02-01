using System;
using HotChocolate;
using Microsoft.Extensions.Logging;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class GraphQLErrorFilter : IErrorFilter
    {
        private readonly ILogger logger;

        public GraphQLErrorFilter(ILogger<GraphQLErrorFilter> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IError OnError(IError error)
        {
            if (!string.IsNullOrEmpty(error.Code))
            {
                return error;
            }
            
            if (error.Exception == null)
            {
                logger.LogError("{0}{2}{1}", error.Path, error.Message, Environment.NewLine);
            }
            else
            {
                logger.LogError(error.Exception,
                    "{0}{2}{1}", error.Path, error.Message, Environment.NewLine);
            }
            
            return ErrorBuilder.New()
                .SetMessage(error.Exception.Message)
                .Build();
        }
    }
}
