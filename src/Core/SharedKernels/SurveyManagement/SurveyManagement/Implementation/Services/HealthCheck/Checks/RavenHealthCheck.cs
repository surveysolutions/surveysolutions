using System;
using Raven.Client;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks
{
    public class RavenHealthCheck : IAtomicHealthCheck<RavenHealthCheckResult>
    {
        private readonly IDocumentStore documentStore;

        public RavenHealthCheck(IDocumentStore ravenStore)
        {
            documentStore = ravenStore;
        }

        public RavenHealthCheckResult Check()
        {
            try
            {
                var buildNumber = documentStore.DatabaseCommands.GlobalAdmin.GetBuildNumber();
                return RavenHealthCheckResult.Happy();
            }
            catch (Exception e)
            {
                return RavenHealthCheckResult.Down("Can't connect to Raven. " + e.Message);
            }
        }
    }
}