using System;
using System.ComponentModel;
using System.Linq;
using Dapper;
using FluentMigrator;
using Npgsql;
using NpgsqlTypes;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201902281841)]
    [Localizable(false)]
    public class M201902281841_Add_InterviewFlags_Table : Migration
    {
        public override void Up()
        {
            var primaryKeyName = "pk_interviewflags";

            Create.Table("interviewflags")
                .WithColumn("interviewid").AsGuid().PrimaryKey(primaryKeyName)
                .WithColumn("entityid").AsGuid().PrimaryKey(primaryKeyName)
                .WithColumn("rostervector").AsString().Nullable().PrimaryKey(primaryKeyName);

            Execute.WithConnection((connection, transaction) =>
            {
                var npgsqlConnection = connection as NpgsqlConnection;

                if(!npgsqlConnection.IsTableExistsInSchema("readside", "interviews_view")) return;

                var flags = npgsqlConnection.Query<(Guid interviewId, Guid entityId, string rosterVector)>(
                        "SELECT interviewid, entityid, rostervector FROM \"readside\".\"interviews_view\" where hasflag = true")
                    .ToArray();

                var copyFromCommand = "COPY  \"plainstore\".\"interviewflags\" (interviewid, entityid, rostervector) FROM STDIN BINARY;";
                using (var writer = npgsqlConnection.BeginBinaryImport(copyFromCommand))
                {
                    foreach (var flag in flags)
                    {
                        writer.StartRow();
                        writer.Write(flag.interviewId, NpgsqlDbType.Uuid);
                        writer.Write(flag.entityId, NpgsqlDbType.Uuid);
                        writer.Write(flag.rosterVector, NpgsqlDbType.Text);
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
