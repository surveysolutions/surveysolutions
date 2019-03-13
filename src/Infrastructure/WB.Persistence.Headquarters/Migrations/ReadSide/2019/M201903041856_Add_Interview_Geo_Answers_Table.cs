using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201903041856)]
    [Localizable(false)]
    public class M201903041856_Add_Interview_Geo_Answers_Table : Migration
    {
        private class GeoPosition
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double Accuracy { get; set; }
            public double Altitude { get; set; }
            public DateTimeOffset Timestamp { get; set; }
        }

        public class InterviewGpsAnswer
        {
            public Guid InterviewId { get; set; }
            public Guid EntityId { get; set; }
            public string RosterVector { get; set; }
            public string Answer { get; set; }
            public bool IsEnabled { get; set; }
        }

        public override void Up()
        {
            var primaryKeyName = "pk_interview_geo_answers";

            Create.Table("interview_geo_answers")
                .WithColumn("interviewid").AsGuid().PrimaryKey(primaryKeyName)
                .WithColumn("questionid").AsGuid().PrimaryKey(primaryKeyName)
                .WithColumn("rostervector").AsString().Nullable().PrimaryKey(primaryKeyName)
                .WithColumn("latitude").AsDouble()
                .WithColumn("longitude").AsDouble()
                .WithColumn("timestamp").AsDateTimeOffset().Nullable()
                .WithColumn("isenabled").AsBoolean().WithDefaultValue(true);

            Execute.WithConnection((connection, transaction) =>
            {
                var npgsqlConnection = connection as NpgsqlConnection;

                if(!npgsqlConnection.IsTableExistsInSchema("readside", "interviews_view")) return;

                var geoAnswers = npgsqlConnection.Query<InterviewGpsAnswer>(
                        "SELECT interviewid, entityid, rostervector, asgps as answer, isenabled  FROM \"readside\".\"interviews_view\" where asgps is not null")
                    .ToArray();

                var copyFromCommand = "COPY  \"readside\".\"interview_geo_answers\" (interviewid, questionid, rostervector, latitude, longitude, timestamp, isenabled) FROM STDIN BINARY;";
                using (var writer = npgsqlConnection.BeginBinaryImport(copyFromCommand))
                {
                    foreach (var geoAnswer in geoAnswers)
                    {
                        writer.StartRow();
                        writer.Write(geoAnswer.InterviewId, NpgsqlDbType.Uuid);
                        writer.Write(geoAnswer.EntityId, NpgsqlDbType.Uuid);
                        writer.Write(geoAnswer.RosterVector, NpgsqlDbType.Text);

                        var gps = JsonConvert.DeserializeObject<GeoPosition>(geoAnswer.Answer);
                        
                        writer.Write(gps.Latitude, NpgsqlDbType.Double);
                        writer.Write(gps.Longitude, NpgsqlDbType.Double);
                        writer.Write(gps.Timestamp, NpgsqlDbType.TimestampTz);
                        writer.Write(geoAnswer.IsEnabled, NpgsqlDbType.Boolean);
                    }

                    writer.Complete();
                }

                connection.Execute("DROP VIEW readside.interviews_view");
                connection.Execute(@"create or replace view readside.interviews_view as 
                select s.interviewid, q.entityid, rostervector, isenabled, isreadonly, invalidvalidations, warnings,
                    asstring, asint, aslong, asdouble, asdatetime, aslist, asintarray, asintmatrix, asbool,
                    asyesno, asaudio, asarea, q.entity_type, q.parentid, q.question_type
                from readside.interviews i
                join readside.interviews_id s on s.id = i.interviewid
                join readside.questionnaire_entities q on q.id = i.entityid");

                connection.Execute("ALTER TABLE \"readside\".\"interviews\" DROP COLUMN asgps");
            });
        }

        public override void Down()
        {
        }
    }
}
