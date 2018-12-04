using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Localizable(false)]
    [Migration(201804151700)]
    public class M201804151700_AddRepotRelatedMetadataToQuestionnaireEntities : QuestionnaireEntityMigration
    {
        public override void Up()
        {
            Alter.Table("questionnaire_entities")
                .AddColumn("is_filtered_combobox").AsBoolean().WithDefaultValue(false)
                .AddColumn("linked_to_question_id").AsGuid().Nullable()
                .AddColumn("linked_to_roster_id").AsGuid().Nullable()
                .AddColumn("stata_export_caption").AsString().Nullable()
                .AddColumn("variable_label").AsString().Nullable()
                .AddColumn("question_text").AsString().Nullable()
                .AddColumn("cascade_from_question_id").AsGuid().Nullable();

            Create.Table("questionnaire_entities_answers")
                .WithColumn("entity_id").AsInt32().ForeignKey("questionnaire_entities", "id").Indexed()
                .WithColumn("text").AsString().NotNullable()
                .WithColumn("value").AsString().Nullable()
                .WithColumn("parent").AsString().Nullable()
                .WithColumn("answer_code").AsDecimal().Nullable()
                .WithColumn("parent_code").AsDecimal().Nullable();

            
            ExecuteForQuestionnaire((db, questionnaireId, questions) =>
            {
                List<Answer> answersToInsert = new List<Answer>();
                //logger.Info($"Processing {questions.Count} questions for questionnaire: {questionnaireId}");

                foreach (var question in questions)
                {
                    var entity = JsonConvert.DeserializeObject<Question>(question.item.ToString());
                    entity.QuestionnaireIdentity = questionnaireId;

                    if (entity.QuestionType == null && question.item.Value<string>("$type") == "SingleQuestion")
                    {
                        entity.QuestionType = QuestionType.SingleOption;
                    }
                    
                    db.Execute($@"UPDATE readside.questionnaire_entities
                                SET 
                                    question_type = @{nameof(Question.QuestionType)},
                                    is_filtered_combobox = @{nameof(Question.IsFilteredCombobox)},
                                    linked_to_question_id = @{nameof(Question.LinkedToQuestionId)},
                                    linked_to_roster_id = @{nameof(Question.LinkedToRosterId)},
                                    stata_export_caption = @{nameof(Question.StataExportCaption)},
                                    question_text = @{nameof(Question.QuestionText)},
                                    variable_label = @{nameof(Question.VariableLabel)},
                                    cascade_from_question_id = @{nameof(Question.CascadeFromQuestionId)}
                                WHERE questionnaireidentity = @{nameof(Question.QuestionnaireIdentity)}
                                    AND entityid = @{nameof(Question.PublicKey)};
                            ", entity);

                    var entityId = db.QuerySingle<int>($@"select id from readside.questionnaire_entities 
                                WHERE questionnaireidentity = @{nameof(Question.QuestionnaireIdentity)}
                                    AND entityid = @{nameof(Question.PublicKey)} ", entity);

                    foreach (var answer in entity.Answers ?? Array.Empty<Answer>())
                    {
                        answer.Id = entityId;
                    }

                    if (entity.Answers != null)
                    {
                        answersToInsert.AddRange(entity.Answers);

                        if (answersToInsert.Count > 5000)
                        {
                           InsertAnswers(answersToInsert, db);
                           answersToInsert.Clear();
                        }
                    }
                }

                if (answersToInsert.Any())
                {
                    InsertAnswers(answersToInsert, db);
                }
            });
        }

        private void InsertAnswers(List<Answer> answers, IDbConnection db)
        {
            //logger?.Info($"Storing {answers.Count} answers");

            db.Execute($@"insert into readside.questionnaire_entities_answers 
                                    (entity_id, text,value,parent,answer_code,parent_code)
                                values(
                                    @{nameof(Answer.Id)},
                                    @{nameof(Answer.AnswerText)},
                                    @{nameof(Answer.AnswerValue)},
                                    @{nameof(Answer.ParentValue)},
                                    @{nameof(Answer.AnswerCode)},
                                    @{nameof(Answer.ParentCode)})", answers);
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
