using System;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
	public class Query
	{
		internal static string InsertNewEventQuery()
		{
			return 
@"INSERT INTO [Events]
([EventSourceId], [EventId], [Name], [Data], [Sequence], [TimeStamp]) 
VALUES (?, ?, ?, ?, ?, ?)";
		}

		internal static string SelectAllEventsFromQuery(Guid id, long minVersion, long maxVersion)
		{
			var template = 
@"SELECT TimeStamp, EventId, Data, Sequence
FROM Events
WHERE [EventSourceId] = '{0}'
AND Sequence >= {1}
AND Sequence <= {2}
ORDER BY Sequence";

			return string.Format(template, id, minVersion, maxVersion);
		}



		internal static string CreateTables()
		{
			return 
@"CREATE TABLE Events (
EventSourceId TEXT NOT NULL,
EventId TEXT  NOT NULL,
Sequence INTEGER  NOT NULL,
TimeStamp INTEGER NOT NULL,
Data BLOB NOT NULL,
Name TEXT  NOT NULL)";
		}

		internal static string DropTables()
		{
			return @"DROP TABLE IF EXISTS Events";
		}


	}
}