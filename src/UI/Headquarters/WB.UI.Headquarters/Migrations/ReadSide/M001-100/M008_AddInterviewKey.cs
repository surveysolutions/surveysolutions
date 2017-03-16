using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;

namespace WB.UI.Headquarters.Migrations.ReadSide
{

    [Migration(8)]
    public class M008_AddInterviewKey : Migration
    {
        class JsonString : Dapper.SqlMapper.ICustomQueryParameter
        {
            private string _json;

            public JsonString(string json)
            {
                _json = json;
            }

            public void AddParameter(System.Data.IDbCommand command, string name)
            {
                var param = (NpgsqlParameter)command.CreateParameter();
                param.ParameterName = name;
                param.Value = _json;
                param.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Json;
                command.Parameters.Add(param);
            }
        }


        public override void Up()
        {
            Random random = new Random();

            Alter.Table("interviewsummaries").AddColumn("key")
                .AsString(12).Nullable().Unique("interviewsummaries_unique_key");


            Execute.WithConnection((con, transaction) =>
            {
                List<dynamic> existingInterviewIds = con.Query("select interviewid from readside.interviewsummaries").ToList();
                long globalSequence = con.ExecuteScalar<long>("select MAX(globalsequence) from events.events");

                HashSet<InterviewKey> uniqueKeys = new HashSet<InterviewKey>();
                while (uniqueKeys.Count != existingInterviewIds.Count)
                {
                    var next = random.Next(99999999);
                    uniqueKeys.Add(new InterviewKey(next));
                }

                var keysList = uniqueKeys.ToList();

                for (int i = 0; i < existingInterviewIds.Count; i++)
                {
                    var interviewKeyTouse = keysList[i];
                    var eventString = JsonConvert.SerializeObject(new InterviewKeyAssigned(interviewKeyTouse),
                        Formatting.Indented,
                        EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);

                    Guid existingInterviewId = existingInterviewIds[i].interviewid;
                    var existingSequence =
                        con.ExecuteScalar<int>("select MAX(eventsequence) from events.events WHERE eventsourceid = @id",
                            new { id = existingInterviewId });

                    con.Execute(
                        @"INSERT INTO events.events(id, origin, ""timestamp"", eventsourceid, globalsequence, value, eventsequence, eventtype)
                          VALUES(@id, @origin, @timestamp, @eventSourceId, @globalSequence, @value, @eventSequence, @eventType)",
                        new
                        {
                            id = Guid.NewGuid(),
                            origin = (string)null,
                            timestamp = DateTime.UtcNow,
                            eventsourceid = existingInterviewId,
                            globalSequence = ++globalSequence,
                            value = new JsonString(eventString),
                            eventSequence = ++existingSequence,
                            eventType = nameof(InterviewKeyAssigned)
                        });
                    con.Execute("UPDATE readside.interviewsummaries SET key = @key WHERE interviewid = @interviewId",
                        new
                        {
                            key = interviewKeyTouse.ToString(),
                            interviewId = existingInterviewId
                        });
                }
            });
        }

        public override void Down()
        {
            Delete.Column("key").FromTable("interviewsummaries");
        }
    }
}