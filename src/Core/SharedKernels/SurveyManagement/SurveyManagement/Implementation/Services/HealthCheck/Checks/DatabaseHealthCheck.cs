using System;
using Raven.Client;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck
{
    public class DatabaseHealthCheck : IDatabaseHealthCheck
    {
        private readonly IDocumentStore documentStore;

        public DatabaseHealthCheck(IDocumentStore ravenStore)
        {
            documentStore = ravenStore;
        }

        public ConnectionHealthCheckResult Check()
        {
            try
            {
                var buildNumber = documentStore.DatabaseCommands.GlobalAdmin.GetBuildNumber();
                return ConnectionHealthCheckResult.Happy();
            }
            catch (Exception e)
            {
                return ConnectionHealthCheckResult.Down("Can't connect to Raven. " + e.Message);
            }
        }
    }
}