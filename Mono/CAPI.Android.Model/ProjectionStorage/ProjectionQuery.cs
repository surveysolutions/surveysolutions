using System;

namespace CAPI.Android.Core.Model.ProjectionStorage
{
    public class ProjectionQuery
    {
        internal static string InsertNewProjectionQuery()
        {
            return
                @"INSERT INTO [Projections]
([PublicKey],  [Data]) 
VALUES (?, ?, ?, ?, ?, ?)";
        }

        internal static string SelectProjectionByGuidQuery(Guid publicKey)
        {
            var template =
                @"SELECT PublicKey, Data
FROM Projections
WHERE [PublicKey] = '{0}'";

            return string.Format(template, publicKey);
        }
        internal static string SelectProjectionExistanceByGuidQuery(Guid publicKey)
        {
            var template =
                @"SELECT count(PublicKey) as Count
FROM Projections
WHERE [PublicKey] = '{0}'";

            return string.Format(template, publicKey);
        }

        /*   internal static string SelectAllEventsQuery()
           {
               return
   @"SELECT PublicKey, Data
   FROM Projections";
           }*/

        internal static string CreateTables()
        {
            return
                @"CREATE TABLE Projections (
PublicKey TEXT NOT NULL,
Data TEXT NOT NULL)";
        }

        internal static string DropTables()
        {
            return @"DROP TABLE IF EXISTS Projections";
        }
    }
}