using System;
using System.ComponentModel;
using System.Data;
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
                .WithColumn("interviewid").AsString(255).PrimaryKey(primaryKeyName)
                .WithColumn("questionid").AsGuid().PrimaryKey(primaryKeyName)
                .WithColumn("rostervector").AsString().Nullable().PrimaryKey(primaryKeyName)
                .WithColumn("latitude").AsDouble()
                .WithColumn("longitude").AsDouble()
                .WithColumn("timestamp").AsString().Nullable()
                .WithColumn("isenabled").AsBoolean().WithDefaultValue(true);

            //Create.ForeignKey("fk_interviewsummary_interview_geo_answer").FromTable("interview_geo_answers")
            //    .ForeignColumn("interviewid").ToTable("interviewsummaries").PrimaryColumn("summaryid")
            //    .OnDelete(Rule.Cascade);

            Execute.WithConnection((connection, transaction) =>
            {
                var npgsqlConnection = connection as NpgsqlConnection;

                if(!npgsqlConnection.IsTableExistsInSchema("readside", "interviews_view")) return;

                var geoAnswers = npgsqlConnection.Query<InterviewGpsAnswer>(
                        @"SELECT interviewid, entityid, rostervector, asgps as answer, isenabled " 
                        + @"FROM ""readside"".""interviews_view"" where asgps is not null")
                    .ToArray();

                var copyFromCommand = @"COPY  ""readside"".""interview_geo_answers"" " 
                                      + "(interviewid, questionid, rostervector, latitude, longitude, timestamp, isenabled) " 
                                      + "FROM STDIN BINARY;";

                using (var writer = npgsqlConnection.BeginBinaryImport(copyFromCommand))
                {
                    foreach (var geoAnswer in geoAnswers)
                    {
                        writer.StartRow();
                        writer.Write(geoAnswer.InterviewId.ToString("N"), NpgsqlDbType.Char);
                        writer.Write(geoAnswer.EntityId, NpgsqlDbType.Uuid);
                        writer.Write(geoAnswer.RosterVector, NpgsqlDbType.Text);

                        var gps = JsonConvert.DeserializeObject<GeoPosition>(geoAnswer.Answer);
                        
                        writer.Write(gps.Latitude, NpgsqlDbType.Double);
                        writer.Write(gps.Longitude, NpgsqlDbType.Double);
                        writer.Write(gps.Timestamp.ToString(), NpgsqlDbType.Text);
                        writer.Write(geoAnswer.IsEnabled, NpgsqlDbType.Boolean);
                    }

                    writer.Complete();
                }
            });
        }

        public override void Down()
        {
        }
    }
}
