using System;
using Raven.Client;
using WB.Core.Infrastructure.HealthCheck;


namespace WB.Core.Infrastructure.Storage.Raven.Implementation
{
    public class DatabaseHealthCheck : IDatabaseHealthCheck, IEventStoreHealthCheck
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
                return ConnectionHealthCheckResult.Down(e.Message);
            }
        }
    }
}