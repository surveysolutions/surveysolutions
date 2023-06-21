using System;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Resolvers;
using Microsoft.Extensions.Logging;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class GraphqlDiagnosticEventListener : ExecutionDiagnosticEventListener
    {
        private readonly ILogger logger;
        public GraphqlDiagnosticEventListener(ILogger<GraphqlDiagnosticEventListener> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override bool EnableResolveFieldValue => true;
        
        public override void ResolverError(IMiddlewareContext context, IError error)
        {
            LogError(error);
        }
        
        public override void SyntaxError(IRequestContext context, IError error)
        {
            LogError(error);
        }
        
        public override void TaskError(IExecutionTask task, IError error)
        {
            LogError(error);
        }
        public override void RequestError(IRequestContext context, Exception exception)
        {
            logger.LogError(exception, $"Request Error: {exception.Message}");
        }

        private void LogError(IError error)
        {
            if (error.Code != null)
            {
                logger.LogInformation("{0} {1} {2}", error.Path, error.Code, error.Message);
            }

            else
            {
                if (error.Exception == null)
                {
                    logger.LogError("{0}{2}{1}", error.Path, error.Message, Environment.NewLine);
                }
                else
                {
                    logger.LogError(error.Exception,
                        "{0}{2}{1}", error.Path, error.Message, Environment.NewLine);
                }
            }
        }
    }
}
