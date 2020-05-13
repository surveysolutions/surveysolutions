using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using Dapper;
using FluentMigrator;
using Npgsql;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202003301500)]
    public class M202003301500_ExtendAnswersToFeaturedQuestions : QuestionnaireEntityMigration
    {
        public override void Up()
        {
            AddQuestionnaireEntitiesAnswersTemporaryTable();
            Execute.Sql(@"create table readside._answsers as
                select a.id as aid, qe.id as id, qea.value::int as answer_code
                from readside.answerstofeaturedquestions a
                join readside.interviewsummaries s on s.id = a.interview_id 
                join readside.questionnaire_entities qe 
                    on qe.questionnaireidentity = s.questionnaireidentity and qe.entityid = a.questionid 
                left join readside._temp_questionnaire_entities_answers qea 
                    on qea.entity_id = qe.id and qea.text = a.answervalue");
            
            DeleteQuestionnaireEntitiesAnswersTemporaryTable();

            Execute.Sql("create index _answers_idx on readside._answsers (aid);");

         
            // Rewriting big tables are much much faster then updating them
            Execute.Sql(@"create table readside._answerstofeaturedquestions as 
                select afq.id, afq.answervalue, afq.interview_id, afq.""position"", 
                      anscodes.id as question_id, 
                      anscodes.answer_code as answer_code, lower(afq.answervalue) as answer_lower_case  
                from readside.answerstofeaturedquestions afq 
                join readside._answsers anscodes on anscodes.aid = afq.id;");

            // dropping original table and rename new one
            Execute.Sql("DROP table readside.answerstofeaturedquestions");
            Execute.Sql("ALTER TABLE readside._answerstofeaturedquestions RENAME TO answerstofeaturedquestions;");

            Execute.Sql("DROP table readside._answsers");

            Execute.Sql(
                @"ALTER TABLE readside.answerstofeaturedquestions ADD CONSTRAINT ""PK_answerstofeaturedquestions"" 
                    PRIMARY KEY (id);
                CREATE INDEX answerstofeaturedquestions_answervalue ON readside.answerstofeaturedquestions 
                    USING btree (answervalue text_pattern_ops);");

            // restoring indexes
            Create.Index("answerstofeaturedquestions_answer_lower_case_idx")
                .OnTable("answerstofeaturedquestions")
                .OnColumn("answer_lower_case");

            Create.Index("answerstofeaturedquestions_answer_code_idx")
                .OnTable("answerstofeaturedquestions")
                .OnColumn("answer_code");

            Create.Index("answerstofeaturedquestions_interview_id_position_idx")
                .OnTable("answerstofeaturedquestions")
                .OnColumn("interview_id").Ascending()
                .OnColumn("position").Ascending();

            Create.ForeignKey()
                .FromTable("answerstofeaturedquestions").InSchema("readside").ForeignColumn("question_id")
                .ToTable("questionnaire_entities").InSchema("readside").PrimaryColumn("id");

            Create.ForeignKey()
                .FromTable("answerstofeaturedquestions").InSchema("readside").ForeignColumn("interview_id")
                .ToTable("interviewsummaries").InSchema("readside").PrimaryColumn("id");
        }

        public override void Down()
        {
        }

        private void DeleteQuestionnaireEntitiesAnswersTemporaryTable()
        {
            Delete.Table("_temp_questionnaire_entities_answers").InSchema("readside");
        }

        private void AddQuestionnaireEntitiesAnswersTemporaryTable()
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
                                WHERE questionnaireidentity = @questionnaireId", new {questionnaireId})
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
    }
}
