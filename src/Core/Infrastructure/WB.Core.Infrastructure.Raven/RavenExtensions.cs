using System;
using Raven.Client;
using Raven.Client.Connection;
using Raven.Client.Extensions;

namespace WB.Core.Infrastructure.Raven
{
    public static class RavenExtensions
    {
        public static void DeleteDatabase(this IDocumentStore ravenStore, string databaseName, bool hardDelete = false)
        {
            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException("databaseName");

            var databaseCommands = ravenStore.DatabaseCommands;
            var relativeUrl = "/admin/databases/" + databaseName;

            if (hardDelete)
                relativeUrl += "?hard-delete=true";

            try
            {
                var serverClient = databaseCommands.ForSystemDatabase() as ServerClient;

                var httpJsonRequest = serverClient.CreateRequest("DELETE", relativeUrl);
                httpJsonRequest.ExecuteRequest();
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("Failed to delete '{0}' database", databaseName), ex);
            }
        }

        public static void CreateDatabase(this IDocumentStore ravenStore, string databaseName)
        {
            try
            {
                ravenStore.DatabaseCommands.EnsureDatabaseExists(databaseName);
            }
            catch (Exception ex)
            {

                throw new Exception(string.Format("Failed to create '{0}' database", databaseName), ex);
            }
            
        }
    }
}
