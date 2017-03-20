using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json;
using NLog;
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
            Random random = new Random((int)DateTime.Now.Ticks);

            Alter.Table("interviewsummaries").AddColumn("key")
                .AsString(12).Nullable().Unique("interviewsummaries_unique_key");

            if (Schema.Schema("events").Exists())
            {
                Execute.WithConnection((con, transaction) =>
                {
                    List<dynamic> existingInterviewIds =
                        con.Query("select interviewid from readside.interviewsummaries").ToList();
                    long globalSequence = con.ExecuteScalar<long>("select MAX(globalsequence) from events.events");
                    var currentClassLogger = LogManager.GetCurrentClassLogger();
                    currentClassLogger.Info(
                        "Starting add of interview keys. Total interviews count: {0} current global sequence: {1}",
                        existingInterviewIds.Count, globalSequence);

                    HashSet<int> uniqueKeys = new HashSet<int>();
                    Stopwatch watch = Stopwatch.StartNew();
                    while (uniqueKeys.Count != existingInterviewIds.Count)
                    {
                        var maxUniqueKeyValue = 99999999;
                        var next = random.Next(maxUniqueKeyValue);
                        while (uniqueKeys.Contains(next))
                        {
                            next++;
                            if (next > maxUniqueKeyValue) next = 0;
                        }
                        uniqueKeys.Add(next);
                    }
                    currentClassLogger.Info("Generated unique ids for interviews took {0:g}", watch.Elapsed);
                    var keysList = uniqueKeys.ToList();

                    for (int i = 0; i < existingInterviewIds.Count; i++)
                    {
                        var interviewKeyTouse = new InterviewKey(keysList[i]);
                        var eventString = JsonConvert.SerializeObject(new InterviewKeyAssigned(interviewKeyTouse),
                            Formatting.Indented,
                            EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);

                        Guid existingInterviewId = existingInterviewIds[i].interviewid;
                        var existingSequence =
                            con.ExecuteScalar<int>(
                                "select MAX(eventsequence) from events.events WHERE eventsourceid = @id",
                                new { id = existingInterviewId });

                        con.Execute(
                            @"INSERT INTO events.events(id, origin, ""timestamp"", eventsourceid, globalsequence, value, eventsequence, eventtype)
                          VALUES(@id, @origin, @timestamp, @eventSourceId, @globalSequence, @value, @eventSequence, @eventType)",
                            new
                            {
                                id = Guid.NewGuid(),
                                origin = (string) null,
                                timestamp = DateTime.UtcNow,
                                eventsourceid = existingInterviewId,
                                globalSequence = ++globalSequence,
                                value = new JsonString(eventString),
                                eventSequence = ++existingSequence,
                                eventType = nameof(InterviewKeyAssigned)
                            });
                        con.Execute(
                            "UPDATE readside.interviewsummaries SET key = @key WHERE interviewid = @interviewId",
                            new
                            {
                                key = interviewKeyTouse.ToString(),
                                interviewId = existingInterviewId
                            });
                    }
                });
            }
        }

        public override void Down()
        {
            Delete.Column("key").FromTable("interviewsummaries");
        }
    }
}