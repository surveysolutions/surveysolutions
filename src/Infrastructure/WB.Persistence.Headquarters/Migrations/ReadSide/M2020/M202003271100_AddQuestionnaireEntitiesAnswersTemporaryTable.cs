using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using Dapper;
using FluentMigrator;
using Npgsql;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Localizable(false)]
    [Migration(2020_03_27_1100)]
    public class M202003271100_AddQuestionnaireEntitiesAnswersTemporaryTable : QuestionnaireEntityMigration
    {
        public override void Up()
        {
            Create.Table("_temp_questionnaire_entities_answers")
                .WithColumn("entity_id").AsInt32().ForeignKey("questionnaire_entities", "id").Indexed()
                .WithColumn("text").AsString().NotNullable()
                .WithColumn("value").AsString().Nullable()
                .WithColumn("parent").AsString().Nullable()
                .WithColumn("answer_code").AsDecimal().Nullable()
                .WithColumn("parent_code").AsDecimal().Nullable();

            ExecuteForQuestionnaire((db, questionnaireId, questions) =>
            {
                var entityMap = db.Query<(Guid, int)>($@"select entityid, id from readside.questionnaire_entities 
                                WHERE questionnaireidentity = @questionnaireId", new { questionnaireId })
                    .ToDictionary(k => k.Item1, k => k.Item2);

                IEnumerable<Answer> GetAnswers()
                {
                    foreach (var question in questions)
                    {
                        var entity = JsonSerializer.Deserialize<Question>(question.item.ToString());
                        entity.QuestionnaireIdentity = questionnaireId;

                        if ((entity.Answers?.Length ?? 0) == 0)
                        {
                            continue;
                        }

                        var entityId = entityMap[entity.PublicKey];

                        foreach (var answer in entity.Answers)
                        {
                            answer.Id = entityId;
                            yield return answer;
                        }
                    }
                }

                InsertAnswers(GetAnswers(), db);
            });
        }

        private void InsertAnswers(IEnumerable<Answer> answers, IDbConnection db)
        {
            var npgsql = db as NpgsqlConnection;
            using var writer = npgsql.BeginBinaryImport(
                @"COPY readside._temp_questionnaire_entities_answers (entity_id, text,value,parent,answer_code,parent_code) FROM STDIN BINARY;");
            long count = 0;

            foreach (var answer in answers)
            {
                count++;
                writer.StartRow();
                writer.Write(answer.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                writer.Write(answer.AnswerText, NpgsqlTypes.NpgsqlDbType.Text);
                writer.Write(answer.AnswerValue, NpgsqlTypes.NpgsqlDbType.Text);
                writer.Write(answer.ParentValue, NpgsqlTypes.NpgsqlDbType.Text);

                if (answer.AnswerCode.HasValue)
                {
                    writer.Write(answer.AnswerCode.Value, NpgsqlTypes.NpgsqlDbType.Numeric);
                }
                else
                {
                    writer.WriteNull();
                }
                if (answer.ParentCode.HasValue)
                {
                    writer.Write(answer.ParentCode.Value, NpgsqlTypes.NpgsqlDbType.Numeric);
                }
                else
                {
                    writer.WriteNull();
                }
            }

            writer.Complete();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        private class Question
        {
            public string QuestionnaireIdentity { get; set; }
            public Guid PublicKey { get; set; }
            public bool IsFilteredCombobox { get; set; }
            public Guid? CascadeFromQuestionId { get; set; }
            public Guid? LinkedToRosterId { get; set; }
            public Guid? LinkedToQuestionId { get; set; }
            public QuestionType? QuestionType { get; set; }
            public string QuestionText { get; set; }
            public string StataExportCaption { get; set; }
            public string VariableLabel { get; set; }
            public Answer[] Answers { get; set; }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        private class Answer
        {
            public string QuestionnaireIdentity { get; set; }
            public int Id { get; set; }
            public string AnswerText { get; set; }
            public string AnswerValue { get; set; }
            public string ParentValue { get; set; }
            public decimal? AnswerCode { get; set; }
            public decimal? ParentCode { get; set; }
        }

        private enum QuestionType
        {
            SingleOption = 0,
            [Obsolete("db contains at least one questionnaire")]
            YesNo = 1,
            MultyOption = 3,
            Numeric = 4,
            DateTime = 5,
            GpsCoordinates = 6,
            Text = 7,
            [Obsolete("db contains a bunch of them")]
            AutoPropagate = 8,
            TextList = 9,
            QRBarcode = 10,
            Multimedia = 11,
            Area = 12,
            Audio = 13
        }


        public override void Down()
        {
            Delete.Column("is_filtered_combobox").FromTable("questionnaire_entities");
            Delete.Column("linked_to_question_id").FromTable("questionnaire_entities");
            Delete.Column("linked_to_roster_id").FromTable("questionnaire_entities");
            Delete.Column("cascade_from_question_id").FromTable("questionnaire_entities");
        }
    }
}
