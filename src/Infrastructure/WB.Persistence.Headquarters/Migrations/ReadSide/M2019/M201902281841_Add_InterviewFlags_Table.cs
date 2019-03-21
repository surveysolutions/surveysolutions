using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using Npgsql;
using NpgsqlTypes;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201902281841)]
    [Localizable(false)]
    public class M201902281841_Add_InterviewFlags_Table : Migration
    {
        public override void Up()
        {
            var primaryKeyName = "pk_interviewflags";

            Create.Table("interviewflags")
                .WithColumn("interviewid").AsFixedLengthString(255).PrimaryKey(primaryKeyName)
                .WithColumn("questionidentity").AsString().PrimaryKey(primaryKeyName);

            Create.ForeignKey("fk_interviewsummary_interviewflag").FromTable("interviewflags")
                .ForeignColumn("interviewid").ToTable("interviewsummaries").PrimaryColumn("summaryid")
                .OnDelete(Rule.Cascade);

            Execute.WithConnection((connection, transaction) =>
            {
                var npgsqlConnection = connection as NpgsqlConnection;

                if(!npgsqlConnection.IsTableExistsInSchema("readside", "interviews_view")) return;

                var flags = npgsqlConnection.Query<(Guid interviewId, Guid entityId, string rosterVector)>(
                        "SELECT interviewid, entityid, rostervector FROM \"readside\".\"interviews_view\" where hasflag = true")
                    .ToArray();

                var copyFromCommand = "COPY  \"readside\".\"interviewflags\" (interviewid, questionidentity) FROM STDIN BINARY;";
                using (var writer = npgsqlConnection.BeginBinaryImport(copyFromCommand))
                {
                    foreach (var flag in flags)
                    {
                        writer.StartRow();
                        writer.Write(flag.interviewId.ToString("N"), NpgsqlDbType.Char);
                        writer.Write(Identity.Create(flag.entityId, RosterVector.Parse(flag.rosterVector)).ToString(), NpgsqlDbType.Text);
                    }

                    writer.Complete();
                }

                connection.Execute("DROP VIEW readside.interviews_view");
                connection.Execute(@"create or replace view readside.interviews_view as 
                select s.interviewid, q.entityid, rostervector, isenabled, isreadonly, invalidvalidations, warnings,
                    asstring, asint, aslong, asdouble, asdatetime, aslist, asintarray, asintmatrix, asgps, asbool,
                    asyesno, asaudio, asarea, q.entity_type, q.parentid, q.question_type
                from readside.interviews i
                join readside.interviews_id s on s.id = i.interviewid
                join readside.questionnaire_entities q on q.id = i.entityid");

                connection.Execute("ALTER TABLE \"readside\".\"interviews\" DROP COLUMN hasflag");
            });
        }

        public override void Down()
        {
        }
    }
}
