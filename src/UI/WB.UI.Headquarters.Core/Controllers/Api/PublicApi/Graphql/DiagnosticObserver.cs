 /*
 //TODO: resolve new approach
using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Resolvers;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
     public class DiagnosticObserver : IDiagnosticObserver
    {
        private readonly ILogger _logger;
        public DiagnosticObserver(ILogger<DiagnosticObserver> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [DiagnosticName("HotChocolate.Execution.Resolver.Error")]
        public void OnResolverError(
            IResolverContext context,
            IEnumerable<IError> errors)
        {
            foreach (IError error in errors)
            {
                string path = string.Join("/",
                    error.Path.Select(t => t.ToString()));
                
                if (error.Code != null)
                {
                    _logger.LogInformation("{0} {1} {2}", path, error.Code, error.Message);
                }

                else
                {
                    if (error.Exception == null)
                    {
                        _logger.LogError("{0}{2}{1}", path, error.Message, Environment.NewLine);
                    }
                    else
                    {
                        _logger.LogError(error.Exception,
                            "{0}{2}{1}", path, error.Message, Environment.NewLine);
                    }
                }
            }
        }
    }
}
*/

