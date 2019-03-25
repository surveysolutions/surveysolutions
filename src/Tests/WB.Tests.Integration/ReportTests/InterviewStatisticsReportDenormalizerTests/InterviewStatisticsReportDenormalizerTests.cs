using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
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
        private InterviewStatisticsReportDenormalizer denormalizer;

        internal enum Relation { Head = 1, Spouse, Child }
        internal enum Sex { Male = 1, Female }
        internal enum Dwelling { House = 1, Barrack, Hole }

        private readonly Guid dwellingQuestion = Id.g1;
        private readonly Guid relationQuestion = Id.g2;
        private readonly Guid sexQuestion = Id.g3;
        private readonly Guid numericIntQuestion = Id.g4;
        private readonly Guid numericRealQuestion = Id.g5;
        private readonly Guid interviewId = Guid.NewGuid();
        private SurveyStatisticsReport reporter;

        [SetUp]
        public void SettingUp()
        {
            this.questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(numericIntQuestion),
                Create.Entity.NumericRealQuestion(numericRealQuestion),
                Create.Entity.SingleOptionQuestion(dwellingQuestion, "dwelling", answers: GetAnswersFromEnum<Dwelling>()),
                Create.Entity.Roster(Id.gA, variable: "hh_member", children: new[]
                {
                    Create.Entity.SingleOptionQuestion(relationQuestion, variable: "relation", answers: GetAnswersFromEnum<Relation>()),
                    Create.Entity.SingleOptionQuestion(sexQuestion, variable: "sex", answers:  GetAnswersFromEnum<Sex>())
                })
            );

            PrepareQuestionnaire(questionnaire, 1);

            this.denormalizer = new InterviewStatisticsReportDenormalizer(UnitOfWork, questionnaireStorage);
            this.reporter = new SurveyStatisticsReport(new InterviewReportDataRepository(UnitOfWork));
        }

        [Test]
        public void when_new_answer_given_should_insert_into_database()
        {
            var summary = new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed,
                ResponsibleName = "responsible",
                ResponsibleId = Id.gC,
                QuestionnaireId = questionnaire.PublicKey,
                QuestionnaireVersion = 1,
                TeamLeadId = Id.gE,
                TeamLeadName = "test"
            };

            StoreInterviewSummary(summary, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

            denormalizer.Update(summary, Create.PublishedEvent.SingleOptionQuestionAnswered(interviewId, dwellingQuestion,
                (decimal) Dwelling.Barrack));
            
            var report = reporter.GetReport(new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),

                Question = questionnaire.Find<SingleQuestion>(dwellingQuestion)
            });

            AssertReportHasTotal(report, Dwelling.Barrack, 1);
        }

        [Test]
        public void when_answer_disabled_should_not_generate_report_for_disabled_answers()
        {
            var summary = new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed,
                ResponsibleName = "responsible",
                ResponsibleId = Id.gC,
                QuestionnaireId = questionnaire.PublicKey,
                QuestionnaireVersion = 1,
                TeamLeadId = Id.gE,
                TeamLeadName = "test"
            };

            StoreInterviewSummary(summary, new QuestionnaireIdentity(questionnaire.PublicKey, 1));
            UnitOfWork.Session.Flush();
            denormalizer.Update(summary, Create.PublishedEvent.SingleOptionQuestionAnswered(interviewId, dwellingQuestion, (decimal)Dwelling.Hole));
            denormalizer.Update(summary, Create.PublishedEvent.SingleOptionQuestionAnswered(interviewId, relationQuestion, (decimal)Relation.Child));

            denormalizer.Update(summary, new QuestionsDisabled(new[] { Create.Identity(dwellingQuestion) }, DateTimeOffset.UtcNow)
                .ToPublishedEvent(summary.InterviewId));
            
            var report = reporter.GetReport(new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                Question = questionnaire.Find<SingleQuestion>(dwellingQuestion)
            });

            AssertReportHasTotal(report, Dwelling.Hole, 0);

            report = reporter.GetReport(new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                Question = questionnaire.Find<SingleQuestion>(relationQuestion)
            });

            AssertReportHasTotal(report, Relation.Child, 1);
        }

        [Test]
        public void when_answer_removed_should_not_generate_report_for_removed_answers()
        {
            var summary = new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed,
                ResponsibleName = "responsible",
                ResponsibleId = Id.gC,
                QuestionnaireId = questionnaire.PublicKey,
                QuestionnaireVersion = 1,
                TeamLeadId = Id.gE,
                TeamLeadName = "test"
            };

            StoreInterviewSummary(summary, new QuestionnaireIdentity(questionnaire.PublicKey, 1));
            UnitOfWork.Session.Flush();
            denormalizer.Update(summary, Create.PublishedEvent.SingleOptionQuestionAnswered(interviewId, dwellingQuestion, (decimal)Dwelling.Hole));
            denormalizer.Update(summary, Create.PublishedEvent.SingleOptionQuestionAnswered(interviewId, relationQuestion, (decimal)Relation.Child));

            denormalizer.Update(summary, new AnswersRemoved(new[] { Create.Identity(dwellingQuestion) }, DateTimeOffset.UtcNow)
                .ToPublishedEvent(summary.InterviewId));
            
            var report = reporter.GetReport(new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                Question = questionnaire.Find<SingleQuestion>(dwellingQuestion)
            });

            AssertReportHasTotal(report, Dwelling.Hole, 0);

            report = reporter.GetReport(new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                Question = questionnaire.Find<SingleQuestion>(relationQuestion)
            });

            AssertReportHasTotal(report, Relation.Child, 1);
        }

        [Test]
        public void when_numeric_answers_applied()
        {
            var summary = new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed,
                ResponsibleName = "responsible",
                ResponsibleId = Id.gC,
                QuestionnaireId = questionnaire.PublicKey,
                QuestionnaireVersion = 1,
                TeamLeadId = Id.gE,
                TeamLeadName = "test"
            };

            StoreInterviewSummary(summary, new QuestionnaireIdentity(questionnaire.PublicKey, 1));
            UnitOfWork.Session.Flush();

            denormalizer.Update(summary,
                new NumericIntegerQuestionAnswered(Guid.NewGuid(), numericIntQuestion, Array.Empty<decimal>(),
                        DateTimeOffset.UtcNow, 10)
                    .ToPublishedEvent(summary.InterviewId));

            denormalizer.Update(summary,
                new NumericRealQuestionAnswered(Guid.NewGuid(), numericRealQuestion, Array.Empty<decimal>(),
                        DateTimeOffset.UtcNow, 44)
                    .ToPublishedEvent(summary.InterviewId));
            
            var report = reporter.GetReport(new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                Question = questionnaire.Find<NumericQuestion>(numericIntQuestion)
            });

            AssertReportHasTotal(report, "Count", 1);
            AssertReportHasTotal(report, "Max", 10);

            report = reporter.GetReport(new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                Question = questionnaire.Find<NumericQuestion>(numericRealQuestion)
            });


            AssertReportHasTotal(report, "Count", 1);
            AssertReportHasTotal(report, "Max", 44);
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
                UnitOfWork.AcceptChanges();
                var rows = UnitOfWork.Session.Query<InterviewStatisticsReportRow>().ToList();
                Console.WriteLine(string.Join("\r\n", rows.Select(r => $"{r.InterviewId} | {r.RosterVector} | {r.EntityId} | {r.Type} | [{string.Join(", ", r.Answer)}] | {r.IsEnabled}")));
            }
        }
    }
}
