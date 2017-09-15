using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using Main.Core.Entities.SubEntities;
using Npgsql;
using NpgsqlTypes;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Infrastructure.Native.Storage;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Localizable(false)]
    [Migration(21)]
    public class M021_CreateInterviewsTable : Migration
    {
        public override void Up()
        {
            Create.Table("interviews")
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("interviewid").AsGuid().Indexed()
                .WithColumn("entityid").AsGuid()
                .WithColumn("rostervector").AsCustom("int[]").Nullable()
                .WithColumn("entitytype").AsInt32()
                .WithColumn("answertype").AsInt32().Nullable()
                .WithColumn("isenabled").AsBoolean().WithDefaultValue(true)
                .WithColumn("isreadonly").AsBoolean().WithDefaultValue(false)
                .WithColumn("invalidvalidations").AsCustom("int[]").Nullable()
                .WithColumn("asstring").AsString().Nullable()
                .WithColumn("asint").AsInt32().Nullable()
                .WithColumn("aslong").AsInt64().Nullable()
                .WithColumn("asdouble").AsDouble().Nullable()
                .WithColumn("asdatetime").AsDateTime().Nullable()
                .WithColumn("aslist").AsCustom("json").Nullable()
                .WithColumn("asintarray").AsCustom("int[]").Nullable()
                .WithColumn("asintmatrix").AsCustom("json").Nullable()
                .WithColumn("asgps").AsCustom("json").Nullable()
                .WithColumn("asbool").AsBoolean().Nullable()
                .WithColumn("asyesno").AsCustom("json").Nullable()
                .WithColumn("asaudio").AsCustom("json").Nullable()
                .WithColumn("asarea").AsCustom("json").Nullable()
                .WithColumn("hasflag").AsBoolean().WithDefaultValue(false);

            Create.Index().OnTable("interviews")
                .OnColumn("interviewid").Ascending()
                .OnColumn("entityid").Ascending()
                .OnColumn("rostervector").Ascending();

            Create.UniqueConstraint("uk_interview").OnTable("interviews").Columns("interviewid", "entityid", "rostervector");

            Execute.WithConnection((connection, transaction) =>
            {
                var interviewIds = connection.Query<string>("SELECT id FROM \"readside\".\"interviewdatas\"", transaction).ToList();

                var deserializer = new EntitySerializer<InterviewData>();
                var serializer = new EntitySerializer<object>();

                foreach (string interviewId in interviewIds)
                {
                    string jsonInterviewData = null;
                    using (var dbCommand = connection.CreateCommand())
                    {
                        dbCommand.CommandText = "SELECT value FROM \"readside\".\"interviewdatas\" where \"interviewdatas\".\"id\" = @interviewid";
                        dbCommand.Transaction = transaction;
                        dbCommand.Parameters.Add(new NpgsqlParameter("interviewid", DbType.String) { Value = interviewId });


                        jsonInterviewData = (string)dbCommand.ExecuteScalar();
                    }
                    if (string.IsNullOrEmpty(jsonInterviewData)) continue;

                    var interviewData = deserializer.Deserialize(jsonInterviewData);
                    var entities = interviewData.Levels.Values.SelectMany(x => ToEntities(Guid.Parse(interviewId), x));

                    foreach (var entity in entities)
                    {
                        using (var dbCommand = connection.CreateCommand())
                        {
                            dbCommand.CommandText = "INSERT INTO \"readside\".\"interviews\" " +
                                                    "VALUES (DEFAULT, @interviewid, @entityid, @rostervector, @entitytype, @answertype, " +
                                                    "@isenabled, @isreadonly, @invalidvalidations, @asstring, @asint, @aslong, @asdouble, " +
                                                    "@asdatetime, @aslist, @asintarray, @asintmatrix, @asgps, @asbool, @asyesno, @asaudio, @asarea, @hasflag); ";

                            dbCommand.Transaction = transaction;

                            dbCommand.Parameters.Add(new NpgsqlParameter("interviewid", DbType.Guid) { Value = interviewId });
                            dbCommand.Parameters.Add(new NpgsqlParameter("entityid", DbType.Guid) { Value = entity.Identity.Id });
                            dbCommand.Parameters.Add(new NpgsqlParameter("rostervector", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = entity.Identity.RosterVector.ToArray() });
                            dbCommand.Parameters.Add(new NpgsqlParameter("entitytype", DbType.Int32) { Value = entity.EntityType  });
                            dbCommand.Parameters.Add(new NpgsqlParameter("answertype", DbType.Int32) { Value = entity.AnswerType ?? (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("isenabled", DbType.Boolean) { Value = entity.IsEnabled });
                            dbCommand.Parameters.Add(new NpgsqlParameter("isreadonly", DbType.Boolean) { Value = entity.IsReadonly });
                            dbCommand.Parameters.Add(new NpgsqlParameter("invalidvalidations", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = entity.FailedValidationIndexes ?? (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("hasflag", DbType.Boolean) { Value = entity.HasFlag });

                            dbCommand.Parameters.Add(new NpgsqlParameter("asstring", DbType.String) { Value = entity.AsString ?? (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("asint", DbType.Int32) { Value = entity.AsInt ?? (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("aslong", DbType.Int64) { Value = entity.AsLong ?? (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("asdouble", DbType.Double) { Value = entity.AsDouble ?? (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("asdatetime", DbType.DateTime) { Value = entity.AsDateTime ?? (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("asbool", DbType.Boolean) { Value = entity.AsBool ?? (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("asintarray", NpgsqlDbType.Array | NpgsqlDbType.Integer) { Value = entity.AsIntArray ?? (object)DBNull.Value });

                            dbCommand.Parameters.Add(new NpgsqlParameter("aslist", NpgsqlDbType.Json) { Value = entity.AsList != null ? serializer.Serialize(entity.AsList) : (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("asyesno", NpgsqlDbType.Json) { Value = entity.AsYesNo != null ? serializer.Serialize(entity.AsYesNo) : (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("asintmatrix", NpgsqlDbType.Json) { Value = entity.AsIntMatrix != null ? serializer.Serialize(entity.AsIntMatrix) : (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("asgps", NpgsqlDbType.Json) { Value = entity.AsGps != null ? serializer.Serialize(entity.AsGps) : (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("asaudio", NpgsqlDbType.Json) { Value = entity.AsAudio != null ? serializer.Serialize(entity.AsAudio) : (object)DBNull.Value });
                            dbCommand.Parameters.Add(new NpgsqlParameter("asarea", NpgsqlDbType.Json) { Value = entity.AsArea != null ? serializer.Serialize(entity.AsArea) : (object)DBNull.Value });

                            dbCommand.ExecuteNonQuery();
                        }
                    }
                }
            });
        }

        private IEnumerable<InterviewDbEntity> ToEntities(Guid interviewId, InterviewLevel lvl)
        {
            foreach (var question in lvl.QuestionsSearchCache)
            {
                yield return new InterviewDbEntity
                {
                    EntityType = EntityType.Question,
                    InterviewId = interviewId,
                    Identity = Identity.Create(question.Key, lvl.RosterVector),
                    IsEnabled = !question.Value.IsDisabled(),
                    IsReadonly = question.Value.IsReadonly(),
                    HasFlag = question.Value.IsFlagged(),
                    AnswerType = InterviewDbEntity.GetAnswerType(question.Value.Answer),
                    FailedValidationIndexes = question.Value.FailedValidationConditions?.Select(x => x.FailedConditionIndex)?.ToArray() ?? new int[] { },

                    AsIntArray = question.Value.Answer as int[] ??
                    (question.Value.Answer as double[])?.Select(Convert.ToInt32)?.ToArray() ??
                    (question.Value.Answer as decimal[])?.Select(Convert.ToInt32)?.ToArray(),

                    AsIntMatrix =
                    question.Value.Answer as int[][] ??
                    (question.Value.Answer as double[][])?.Select(x => x.Select(Convert.ToInt32).ToArray())?.ToArray() ??
                    (question.Value.Answer as decimal[][])?.Select(x => x.Select(Convert.ToInt32).ToArray())?.ToArray(),

                    AsInt = question.Value.Answer as int?,
                    AsString = question.Value.Answer as string,
                    AsLong = question.Value.Answer as long?,
                    AsDateTime = question.Value.Answer as DateTime?,
                    AsDouble = question.Value.Answer as double? ?? (question.Value.Answer is decimal ? (double?)Convert.ToDouble(question.Value.Answer) : null),
                    AsList = (question.Value.Answer as InterviewTextListAnswers)?.Answers ?? (question.Value.Answer as Tuple<decimal, string>[])?.Select(x=> new InterviewTextListAnswer(x.Item1, x.Item2))?.ToArray(),
                    AsYesNo = question.Value.Answer as AnsweredYesNoOption[],
                    AsGps = question.Value.Answer as GeoPosition,
                    AsAudio = question.Value.Answer as AudioAnswer,
                    AsArea = question.Value.Answer as Area
            };
            }

            foreach (var staticText in lvl.StaticTexts)
            {
                yield return new InterviewDbEntity
                {
                    EntityType = EntityType.StaticText,
                    InterviewId = interviewId,
                    Identity = Identity.Create(staticText.Key, lvl.RosterVector),
                    IsEnabled = staticText.Value.IsEnabled,
                    IsReadonly = true,
                    HasFlag = false,
                    FailedValidationIndexes = staticText.Value.FailedValidationConditions
                                                  ?.Select(x => x.FailedConditionIndex)?.ToArray() ?? new int[] { }
                };
            }

            foreach (var variable in lvl.Variables)
            {
                yield return new InterviewDbEntity
                {
                    EntityType = EntityType.Variable,
                    AnswerType = InterviewDbEntity.GetAnswerType(variable.Value),
                    InterviewId = interviewId,
                    Identity = Identity.Create(variable.Key, lvl.RosterVector),
                    IsEnabled = true,
                    IsReadonly = true,
                    HasFlag = false,
                    AsString = variable.Value as string,
                    AsLong = variable.Value as long?,
                    AsBool = variable.Value as bool?,
                    AsDateTime = variable.Value as DateTime?,
                    AsDouble = variable.Value as double? ?? (variable.Value is decimal ? (double?)Convert.ToDouble(variable.Value) : null)
                };
            }

            foreach (var variable in lvl.DisabledVariables)
            {
                yield return new InterviewDbEntity
                {
                    EntityType = EntityType.Variable,
                    InterviewId = interviewId,
                    Identity = Identity.Create(variable, lvl.RosterVector),
                    IsEnabled = false,
                    IsReadonly = true,
                    HasFlag = false
                };
            }

            foreach (var disabledGroup in lvl.DisabledGroups)
            {
                yield return new InterviewDbEntity
                {
                    EntityType = EntityType.Section,
                    InterviewId = interviewId,
                    Identity = Identity.Create(disabledGroup, lvl.RosterVector),
                    IsEnabled = false,
                    IsReadonly = false,
                    HasFlag = false
                };
            }
        }

        public override void Down()
        {
            Delete.Table("interviews");
        }
    }
}