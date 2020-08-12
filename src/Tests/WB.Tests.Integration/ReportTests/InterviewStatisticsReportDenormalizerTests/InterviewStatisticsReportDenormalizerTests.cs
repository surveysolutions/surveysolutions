using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Integration.InterviewFactoryTests;

namespace WB.Tests.Integration.ReportTests.InterviewStatisticsReportDenormalizerTests
{
    internal class InterviewStatisticsReportDenormalizerTests : InterviewFactorySpecification
    {
        private QuestionnaireDocument questionnaire;
        private IQuestionnaire plainQuestionnaire;

        private InterviewStatisticsReportDenormalizer denormalizer;

        internal enum Relation { Head = 1, Spouse, Child }
        internal enum Sex { Male = 1, Female }
        internal enum Dwelling { House = 1, Barrack, Hole }

        private readonly Guid dwellingQuestion = Id.g1;
        private readonly Guid relationQuestion = Id.g2;
        private readonly Guid sexQuestion = Id.g3;
        private readonly Guid numericIntQuestion = Id.g4;
        private readonly Guid numericRealQuestion = Id.g5;
        private readonly Guid pastDwellingsMultyQuestion = Id.g6;

        private Guid interviewId;
        private SurveyStatisticsReport reporter;

        [SetUp]
        public void SettingUp()
        {
            this.questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(numericIntQuestion),
                Create.Entity.NumericRealQuestion(numericRealQuestion),
                Create.Entity.SingleOptionQuestion(dwellingQuestion, "dwelling", answers: GetAnswersFromEnum<Dwelling>()),
                Create.Entity.MultyOptionsQuestion(pastDwellingsMultyQuestion, variable: "pastDwelling", options: GetAnswersFromEnum<Dwelling>()),
                Create.Entity.Roster(Id.gA, variable: "hh_member", children: new[]
                {
                    Create.Entity.SingleOptionQuestion(relationQuestion, variable: "relation", answers: GetAnswersFromEnum<Relation>()),
                    Create.Entity.SingleOptionQuestion(sexQuestion, variable: "sex", answers:  GetAnswersFromEnum<Sex>())
                })
            );
            this.plainQuestionnaire = new PlainQuestionnaire(this.questionnaire, 1, null, new SubstitutionService());
            interviewId = Guid.NewGuid();

            var questionnaireStorageLocal = PrepareQuestionnaire(questionnaire, 1);

            this.denormalizer = new InterviewStatisticsReportDenormalizer(questionnaireStorageLocal);
            this.reporter = new SurveyStatisticsReport(new InterviewReportDataRepository(UnitOfWork));
        }

        [Test]
        public void when_multy_answer_given_should_insert_into_database()
        {
            var summary =  Create.Entity.InterviewSummary(
                interviewId: interviewId,
                status: InterviewStatus.Completed,
                responsibleName: "responsible",
                responsibleId: Id.gC,
                questionnaireId: questionnaire.PublicKey,
                questionnaireVersion: 1,
                teamLeadId: Id.gE,
                teamLeadName: "test",
                questionnaireVariable: plainQuestionnaire.VariableName);

            StoreInterviewSummary(summary, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

            denormalizer.Update(summary, Create.PublishedEvent.MultyOptionQuestionAnswered(interviewId,
                pastDwellingsMultyQuestion,
                new[] {(decimal) Dwelling.Barrack, (decimal) Dwelling.Hole}));
            
            UnitOfWork.Session.Flush();

            var report = reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionId = pastDwellingsMultyQuestion
            });

            AssertReportHasTotal(report, Dwelling.Barrack, 1);
            AssertReportHasTotal(report, Dwelling.Hole, 1);
        }

        [Test]
        public void when_new_answer_given_should_insert_into_database()
        {
            var summary =  Create.Entity.InterviewSummary(
                interviewId: interviewId,
                status: InterviewStatus.Completed,
                responsibleName: "responsible",
                responsibleId: Id.gC,
                questionnaireId: questionnaire.PublicKey,
                questionnaireVersion: 1,
                teamLeadId: Id.gE,
                teamLeadName: "test",
                questionnaireVariable: plainQuestionnaire.VariableName);

            StoreInterviewSummary(summary, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

            denormalizer.Update(summary, Create.PublishedEvent.SingleOptionQuestionAnswered(interviewId, dwellingQuestion,
                (decimal) Dwelling.Barrack));
            
            UnitOfWork.Session.Flush();

            var report = reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionId = dwellingQuestion
            });

            AssertReportHasTotal(report, Dwelling.Barrack, 1);
        }

        [Test]
        public void when_answer_disabled_should_not_generate_report_for_disabled_answers()
        {
            var summary =
                Create.Entity.InterviewSummary(
                    interviewId: interviewId,
                    status: InterviewStatus.Completed,
                    responsibleName: "responsible",
                    responsibleId: Id.gC,
                    questionnaireId: questionnaire.PublicKey,
                    questionnaireVersion: 1,
                    teamLeadId: Id.gE,
                    teamLeadName: "test",
                    questionnaireVariable: plainQuestionnaire.VariableName);

            denormalizer.Update(summary, Create.PublishedEvent.SingleOptionQuestionAnswered(interviewId, dwellingQuestion, (decimal)Dwelling.Hole));
            denormalizer.Update(summary, Create.PublishedEvent.SingleOptionQuestionAnswered(interviewId, relationQuestion, (decimal)Relation.Child));

            denormalizer.Update(summary, new QuestionsDisabled(new[] { Create.Identity(dwellingQuestion) }, DateTimeOffset.UtcNow)
                .ToPublishedEvent(summary.InterviewId));

            StoreInterviewSummary(summary, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

            UnitOfWork.Session.Flush();

            var report = reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionId = dwellingQuestion
            });

            AssertReportHasTotal(report, Dwelling.Hole, 0);
            UnitOfWork.Session.Flush();

            report = reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionId = relationQuestion
            });

            AssertReportHasTotal(report, Relation.Child, 1);
        }

        [Test]
        public void when_answer_removed_should_not_generate_report_for_removed_answers()
        {
            var summary = Create.Entity.InterviewSummary(
                interviewId: interviewId,
                status: InterviewStatus.Completed,
                responsibleName: "responsible",
                responsibleId: Id.gC,
                questionnaireId: questionnaire.PublicKey,
                questionnaireVersion: 1,
                teamLeadId: Id.gE,
                teamLeadName: "test");

            StoreInterviewSummary(summary, new QuestionnaireIdentity(questionnaire.PublicKey, 1));
            
            denormalizer.Update(summary, Create.PublishedEvent.SingleOptionQuestionAnswered(interviewId, dwellingQuestion, (decimal)Dwelling.Hole));
            denormalizer.Update(summary, Create.PublishedEvent.SingleOptionQuestionAnswered(interviewId, relationQuestion, (decimal)Relation.Child));

            denormalizer.Update(summary, new AnswersRemoved(null, new[] { Create.Identity(dwellingQuestion) }, DateTimeOffset.UtcNow)
                .ToPublishedEvent(summary.InterviewId));

            UnitOfWork.Session.Flush();

            var report = reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionId = dwellingQuestion
            });

            AssertReportHasTotal(report, Dwelling.Hole, 0);

            report = reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionId = relationQuestion
            });

            AssertReportHasTotal(report, Relation.Child, 1);
        }

        [Test]
        public void when_numeric_answers_applied()
        {
            var summary = Create.Entity.InterviewSummary(
                interviewId: interviewId,
                status: InterviewStatus.Completed,
                responsibleName: "responsible",
                responsibleId: Id.gC,
                questionnaireId: questionnaire.PublicKey,
                questionnaireVersion: 1,
                teamLeadId: Id.gE,
                teamLeadName: "test",
                questionnaireVariable: plainQuestionnaire.VariableName);

            StoreInterviewSummary(summary, new QuestionnaireIdentity(questionnaire.PublicKey, 1));
            
            denormalizer.Update(summary,
                new NumericIntegerQuestionAnswered(Guid.NewGuid(), numericIntQuestion, Array.Empty<decimal>(),
                        DateTimeOffset.UtcNow, 10)
                    .ToPublishedEvent(summary.InterviewId));

            denormalizer.Update(summary,
                new NumericRealQuestionAnswered(Guid.NewGuid(), numericRealQuestion, Array.Empty<decimal>(),
                        DateTimeOffset.UtcNow, 44)
                    .ToPublishedEvent(summary.InterviewId));

            UnitOfWork.Session.Flush();

            var report = reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionId = numericIntQuestion
            });

            AssertReportHasTotal(report, "Count", 1);
            AssertReportHasTotal(report, "Max", 10);
            AssertReportHasTotal(report, "average", 10);
            AssertReportHasTotal(report, "percentile_05", 10);
            AssertReportHasTotal(report, "percentile_95", 10);
            AssertReportHasTotal(report, "percentile_50", 10);
            AssertReportHasTotal(report, "median", 10);
            AssertReportHasTotal(report, "min", 10);
            AssertReportHasTotal(report, "sum", 10);


            report = reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionId = numericRealQuestion
            });
            
            AssertReportHasTotal(report, "Count", 1);
            AssertReportHasTotal(report, "Max", 44);
            AssertReportHasTotal(report, "average", 44);
            AssertReportHasTotal(report, "percentile_05", 44);
            AssertReportHasTotal(report, "percentile_95", 44);
            AssertReportHasTotal(report, "percentile_50", 44);
            AssertReportHasTotal(report, "median", 44);
            AssertReportHasTotal(report, "min", 44);
            AssertReportHasTotal(report, "sum", 44);
        }

        private void AssertReportHasTotal<T>(ReportView report, T @enum, int amount) where T : Enum
        {
            AssertReportHasTotal(report, @enum.ToString(), amount);
        }

        private void AssertReportHasTotal(ReportView report, string header, int amount) 
        {
            var indexInTotal = Array.IndexOf(report.Headers, header);

            try
            {
                Assert.That(report.Totals[indexInTotal], Is.EqualTo(amount));
            }
            catch
            {
                UnitOfWork.Session.Flush();
                var rows = UnitOfWork.Session.Query<InterviewStatisticsReportRow>().ToList();
                Console.WriteLine(string.Join("\r\n", rows.Select(r => $" | {r.RosterVector} | {r.EntityId} | {r.Type} | [{string.Join(", ", r.Answer)}] | {r.IsEnabled}")));
            }
        }
    }
}
