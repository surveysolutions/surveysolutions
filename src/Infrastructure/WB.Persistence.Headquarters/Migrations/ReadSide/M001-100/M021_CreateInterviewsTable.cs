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
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Infrastructure.Native.Storage;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
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

            Create.Index("interviews_interviewid_clst").OnTable("interviews").WithOptions().Clustered()
                .OnColumn("interviewid");
            Execute.Sql("ALTER TABLE readside.interviews CLUSTER ON interviews_interviewid_clst");

            if (!Schema.Table("interviewdatas").Exists())
                return;

            Execute.Sql(@"UPDATE readside.interviewdatas SET value=replace(value::text, '\\u0000', '')::json");

            Execute.WithConnection((connection, transaction) =>
            {
                var npgsqlConnection = connection as NpgsqlConnection;

                //var logger = ServiceLocator.Current.GetInstance<ILoggerProvider>().GetFor<M021_CreateInterviewsTable>();

                //logger.Info("Interview data -> Interviews. Reading interview ids.");

                int groupInterviewsCount = 10;

                var interviewIds = connection.Query<string>("SELECT id FROM \"readside\".\"interviewdatas\" " +
                                                            "WHERE \"interviewdatas\".\"value\" IS NOT NULL ", transaction)
                    .Batch(groupInterviewsCount);
                var allInterviewsCount = interviewIds.Sum(x=>x.Count());

                //logger.Info($"Interview data -> Interviews. {allInterviewsCount} interviews for processing.");

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
                            writer.WriteNull();
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
                        //logger.Info($"Interview data -> Interviews. " + $"Processed {processedInterviewsCount} interviews out of {allInterviewsCount} in {sw.Elapsed}");
                        totalProcessingTime = totalProcessingTime.Add(sw.Elapsed);
                        sw.Restart();
                    }
                }

                //logger.Info($"Interview data -> Interviews. " + $"Processed {processedInterviewsCount} interviews out of {allInterviewsCount} in {totalProcessingTime}");
            });
        }

        private static IEnumerable<InterviewEntity> ToEntities(Guid interviewId, InterviewLevel lvl)
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
                    AsList = question.Value.Answer as InterviewTextListAnswer[] ?? (question.Value.Answer as InterviewTextListAnswers)?.Answers ?? (question.Value.Answer as Tuple<decimal, string>[])?.Select(x => new InterviewTextListAnswer(x.Item1, x.Item2))?.ToArray(),
                    AsYesNo = question.Value.Answer as AnsweredYesNoOption[],
                    AsGps = question.Value.Answer as GeoPosition,
                    AsAudio = question.Value.Answer as AudioAnswer,
                    AsArea = question.Value.Answer as Area,
                    AsBool = question.Value.Answer as bool?
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

        private class InterviewLevel
        {
            public InterviewLevel()
            {
                this.ScopeVectors = new Dictionary<ValueVector<Guid>, int?>();
                this.DisabledGroups = new HashSet<Guid>();
                this.RosterRowTitles = new Dictionary<Guid, string>();
                this.QuestionsSearchCache = new Dictionary<Guid, InterviewQuestion>();
                this.Variables = new Dictionary<Guid, object>();
                this.StaticTexts = new Dictionary<Guid, InterviewStaticText>();
                this.DisabledVariables = new HashSet<Guid>();
            }
            public InterviewLevel(ValueVector<Guid> scopeVector, int? sortIndex, decimal[] vector)
                : this()
            {
                this.ScopeVectors = new Dictionary<ValueVector<Guid>, int?> { { scopeVector, sortIndex } };
                this.RosterVector = vector;
            }

            public decimal[] RosterVector { get; set; }
            public Dictionary<ValueVector<Guid>, int?> ScopeVectors { get; set; }
            public HashSet<Guid> DisabledGroups { get; set; }
            public Dictionary<Guid, string> RosterRowTitles { get; set; }
            public Dictionary<Guid, InterviewQuestion> QuestionsSearchCache { get; set; }
            public Dictionary<Guid, object> Variables { get; set; }
            public HashSet<Guid> DisabledVariables { get; set; }
            public Dictionary<Guid, InterviewStaticText> StaticTexts { get; set; }
        }

        private class InterviewEntity 
        {
            public virtual Guid InterviewId { get; set; }
            public virtual Identity Identity { get; set; }
            public virtual EntityType EntityType { get; set; }

            public virtual bool HasFlag { get; set; }
            public virtual bool IsEnabled { get; set; }
            public virtual int[] InvalidValidations { get; set; }
            public virtual bool IsReadonly { get; set; }
            public virtual int? AsInt { get; set; }
            public virtual double? AsDouble { get; set; }
            public virtual long? AsLong { get; set; }
            public virtual string AsString { get; set; }
            public virtual DateTime? AsDateTime { get; set; }
            public virtual bool? AsBool { get; set; }
            public virtual int[] AsIntArray { get; set; }
            public virtual InterviewTextListAnswer[] AsList { get; set; }
            public virtual AnsweredYesNoOption[] AsYesNo { get; set; }
            public virtual int[][] AsIntMatrix { get; set; }
            public virtual GeoPosition AsGps { get; set; }
            public virtual AudioAnswer AsAudio { get; set; }
            public virtual Area AsArea { get; set; }
        }

        private class InterviewData : InterviewBrief
        {
            public InterviewData()
            {
                this.Levels = new Dictionary<string, InterviewLevel>();
            }

            public UserRoles ResponsibleRole { get; set; }
            public DateTime UpdateDate { get; set; }
            public Dictionary<string, InterviewLevel> Levels { get; set; }
            public bool WasCompleted { get; set; }
            public Guid? SupervisorId { get; set; }
            public bool CreatedOnClient { get; set; }
            public bool ReceivedByInterviewer { get; set; }
            public string CurrentLanguage { get; set; }
            public bool IsMissingAssignToInterviewer { get; set; }

            public string InterviewKey { get; set; }
            public int? AssignmentId { get; set; }
        }
    }
}
