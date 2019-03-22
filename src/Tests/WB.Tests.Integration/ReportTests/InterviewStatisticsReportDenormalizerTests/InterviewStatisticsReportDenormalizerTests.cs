using System;
using System.Linq;
using Main.Core.Documents;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
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
        private readonly Guid interviewId = Guid.NewGuid();

        [SetUp]
        public void SettingUp()
        {
            this.questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(dwellingQuestion, "dwelling", answers: GetAnswersFromEnum<Dwelling>()),
                Create.Entity.Roster(Id.gA, variable: "hh_member", children: new[]
                {
                    Create.Entity.SingleOptionQuestion(relationQuestion, variable: "relation", answers: GetAnswersFromEnum<Relation>()),
                    Create.Entity.SingleOptionQuestion(sexQuestion, variable: "sex", answers:  GetAnswersFromEnum<Sex>())
                })
            );

            PrepareQuestionnaire(questionnaire, 1);

            this.denormalizer = new InterviewStatisticsReportDenormalizer(UnitOfWork, questionnaireStorage);
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

            denormalizer.Update(summary, Create.PublishedEvent.SingleOptionQuestionAnswered(interviewId, dwellingQuestion, 100));

            var row = UnitOfWork.Session
                .Query<InterviewStatisticsReportRow>()
                .SingleOrDefault(i => i.InterviewId == summary.Id && i.EntityId == questionnaire.EntitiesIdMap[dwellingQuestion] && i.RosterVector == "");

            Assert.NotNull(row);

            Assert.That(row.Answer, Is.EqualTo(new []{ 100 }));
            Assert.That(row.IsEnabled, Is.EqualTo(true));
        }
    }
}
