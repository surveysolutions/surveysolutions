using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Dapper;
using FluentMigrator;
using Main.Core.Entities.SubEntities;
using Npgsql;
using NpgsqlTypes;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
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
            var primaryKeyName = "pk_interviews";

            Create.Table("interviews")
                .WithColumn("interviewid").AsGuid().PrimaryKey(primaryKeyName)
                .WithColumn("entityid").AsGuid().PrimaryKey(primaryKeyName)
                .WithColumn("rostervector").AsCustom("int[]").Nullable().PrimaryKey(primaryKeyName)
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
                .WithColumn("aslist").AsCustom("jsonb").Nullable()
                .WithColumn("asintarray").AsCustom("int[]").Nullable()
                .WithColumn("asintmatrix").AsCustom("jsonb").Nullable()
                .WithColumn("asgps").AsCustom("jsonb").Nullable()
                .WithColumn("asbool").AsBoolean().Nullable()
                .WithColumn("asyesno").AsCustom("jsonb").Nullable()
                .WithColumn("asaudio").AsCustom("jsonb").Nullable()
                .WithColumn("asarea").AsCustom("jsonb").Nullable()
                .WithColumn("hasflag").AsBoolean().WithDefaultValue(false);

            if (!Schema.Table("interviewdatas").Exists())
                return;

            Execute.WithConnection((connection, transaction) =>
            {
                var npgsqlConnection = connection as NpgsqlConnection;

                var logger = ServiceLocator.Current.GetInstance<ILogger>();

                logger.Info("Interview data -> Interviews. Reading interview ids.");

                int groupInterviewsCount = 10;

                var interviewIds = connection.Query<string>("SELECT id FROM \"readside\".\"interviewdatas\" " +
                                                            "WHERE \"interviewdatas\".\"value\" IS NOT NULL ", transaction)
                    .Batch(groupInterviewsCount);
                var allInterviewsCount = interviewIds.Sum(x=>x.Count());

                logger.Info($"Interview data -> Interviews. {allInterviewsCount} interviews for processing.");

                var deserializer = new EntitySerializer<InterviewData>();
                var serializer = new EntitySerializer<object>();

                int processedInterviewsCount = 0;

                var sw = new Stopwatch();
                sw.Start();
                TimeSpan totalProcessingTime = TimeSpan.Zero;
                foreach (var groupOfInterviewIds in interviewIds)
                {
                    var interviewsEntities = connection.Query<string>(
                            $"SELECT value FROM \"readside\".\"interviewdatas\" " +
                            $"WHERE \"interviewdatas\".\"value\" IS NOT NULL " +
                            $"AND \"interviewdatas\".\"id\" IN ({string.Join(",", groupOfInterviewIds.Select(x => $"'{x}'"))})",
                            transaction)
                        .Select(deserializer.Deserialize)
                        .SelectMany(
                            interview => interview.Levels.Values.SelectMany(x => ToEntities(interview.InterviewId, x)));

                    var copyFromCommand =
                        "COPY  \"readside\".\"interviews\" (interviewid, entityid, rostervector, entitytype, answertype, " +
                        "isenabled, isreadonly, invalidvalidations, asstring, asint, aslong, asdouble, " +
                        "asdatetime, asbool, asintarray, aslist, asyesno, asintmatrix, asgps, asaudio, asarea, hasflag) FROM STDIN BINARY;";

                    using (var writer = npgsqlConnection.BeginBinaryImport(copyFromCommand))
                    {
                        foreach (var entity in interviewsEntities)
                        {
                            writer.StartRow();
                            writer.Write(entity.InterviewId, NpgsqlDbType.Uuid);
                            writer.Write(entity.Identity.Id, NpgsqlDbType.Uuid);
                            writer.Write(entity.Identity.RosterVector.ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Integer);
                            writer.Write(entity.EntityType, NpgsqlDbType.Integer);
                            if (entity.AnswerType == null) writer.WriteNull(); else writer.Write(entity.AnswerType, NpgsqlDbType.Integer);
                            writer.Write(entity.IsEnabled, NpgsqlDbType.Boolean);
                            writer.Write(entity.IsReadonly, NpgsqlDbType.Boolean);
                            if(entity.InvalidValidations == null) writer.WriteNull(); else writer.Write(entity.InvalidValidations, NpgsqlDbType.Array | NpgsqlDbType.Integer);
                            if(entity.AsString == null) writer.WriteNull(); else writer.Write(entity.AsString, NpgsqlDbType.Text);
                            if (entity.AsInt == null) writer.WriteNull(); else writer.Write(entity.AsInt, NpgsqlDbType.Integer);
                            if (entity.AsLong == null) writer.WriteNull(); else writer.Write(entity.AsLong, NpgsqlDbType.Bigint);
                            if (entity.AsDouble == null) writer.WriteNull(); else writer.Write(entity.AsDouble, NpgsqlDbType.Double);
                            if (entity.AsDateTime == null) writer.WriteNull(); else writer.Write(entity.AsDateTime, NpgsqlDbType.Timestamp);
                            if (entity.AsBool == null) writer.WriteNull(); else writer.Write(entity.AsBool, NpgsqlDbType.Boolean);
                            if (entity.AsIntArray == null) writer.WriteNull(); else writer.Write(entity.AsIntArray, NpgsqlDbType.Array | NpgsqlDbType.Integer);
                            if (entity.AsList == null) writer.WriteNull(); else writer.Write(serializer.Serialize(entity.AsList), NpgsqlDbType.Jsonb);
                            if (entity.AsYesNo == null) writer.WriteNull(); else writer.Write(serializer.Serialize(entity.AsYesNo), NpgsqlDbType.Jsonb);
                            if (entity.AsIntMatrix == null) writer.WriteNull(); else writer.Write(serializer.Serialize(entity.AsIntMatrix), NpgsqlDbType.Jsonb);
                            if (entity.AsGps == null) writer.WriteNull(); else writer.Write(serializer.Serialize(entity.AsGps), NpgsqlDbType.Jsonb);
                            if (entity.AsAudio == null) writer.WriteNull(); else writer.Write(serializer.Serialize(entity.AsAudio), NpgsqlDbType.Jsonb);
                            if (entity.AsArea == null) writer.WriteNull(); else writer.Write(serializer.Serialize(entity.AsArea), NpgsqlDbType.Jsonb);
                            writer.Write(entity.HasFlag, NpgsqlDbType.Boolean);
                        }
                    }

                    processedInterviewsCount += groupOfInterviewIds.Count();

                    if (processedInterviewsCount % 10000 == 0)
                    {
                        logger.Info($"Interview data -> Interviews. " +
                                    $"Processed {processedInterviewsCount} interviews out of {allInterviewsCount} in {sw.Elapsed}");
                        totalProcessingTime = totalProcessingTime.Add(sw.Elapsed);
                        sw.Restart();
                    }
                }

                logger.Info($"Interview data -> Interviews. " +
                            $"Processed {processedInterviewsCount} interviews out of {allInterviewsCount} in {totalProcessingTime}");

                //logger.Info("Interview data -> Interviews. Removing interview data table.");
                //connection.Execute("DROP TABLE \"readside\".\"interviewdatas\"", transaction);
                //logger.Info("Interview data -> Interviews. Removed interview data table.");
            });
        }

        private IEnumerable<InterviewEntity> ToEntities(Guid interviewId, InterviewLevel lvl)
        {
            foreach (var question in lvl.QuestionsSearchCache)
            {
                yield return new InterviewEntity
                {
                    EntityType = EntityType.Question,
                    InterviewId = interviewId,
                    Identity = Identity.Create(question.Key, lvl.RosterVector),
                    IsEnabled = !question.Value.IsDisabled(),
                    IsReadonly = question.Value.IsReadonly(),
                    HasFlag = question.Value.IsFlagged(),
                    AnswerType = InterviewEntity.GetAnswerType(question.Value.Answer),
                    InvalidValidations = question.Value.FailedValidationConditions?.Select(x => x.FailedConditionIndex)?.ToArray() ?? new int[] { },

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
                    AsList = (question.Value.Answer as InterviewTextListAnswers)?.Answers ?? (question.Value.Answer as Tuple<decimal, string>[])?.Select(x => new InterviewTextListAnswer(x.Item1, x.Item2))?.ToArray(),
                    AsYesNo = question.Value.Answer as AnsweredYesNoOption[],
                    AsGps = question.Value.Answer as GeoPosition,
                    AsAudio = question.Value.Answer as AudioAnswer,
                    AsArea = question.Value.Answer as Area
                };
            }

            foreach (var staticText in lvl.StaticTexts)
            {
                yield return new InterviewEntity
                {
                    EntityType = EntityType.StaticText,
                    InterviewId = interviewId,
                    Identity = Identity.Create(staticText.Key, lvl.RosterVector),
                    IsEnabled = staticText.Value.IsEnabled,
                    IsReadonly = true,
                    HasFlag = false,
                    InvalidValidations = staticText.Value.FailedValidationConditions
                                                  ?.Select(x => x.FailedConditionIndex)?.ToArray() ?? new int[] { }
                };
            }

            foreach (var variable in lvl.Variables)
            {
                yield return new InterviewEntity
                {
                    EntityType = EntityType.Variable,
                    AnswerType = InterviewEntity.GetAnswerType(variable.Value),
                    InterviewId = interviewId,
                    Identity = Identity.Create(variable.Key, lvl.RosterVector),
                    IsEnabled = !lvl.DisabledVariables.Contains(variable.Key),
                    IsReadonly = true,
                    HasFlag = false,
                    AsString = variable.Value as string,
                    AsLong = variable.Value as long?,
                    AsBool = variable.Value as bool?,
                    AsDateTime = variable.Value as DateTime?,
                    AsDouble = variable.Value as double? ?? (variable.Value is decimal ? (double?)Convert.ToDouble(variable.Value) : null)
                };
            }

            foreach (var variable in lvl.DisabledVariables.Except(lvl.Variables?.Keys?.ToHashSet() ?? new HashSet<Guid>()))
            {
                yield return new InterviewEntity
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
                yield return new InterviewEntity
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