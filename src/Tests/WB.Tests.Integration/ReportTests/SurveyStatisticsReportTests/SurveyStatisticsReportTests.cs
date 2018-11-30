using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Integration.InterviewFactoryTests;

namespace WB.Tests.Integration.ReportTests.SurveyStatisticsReportTests
{
    internal class SurveyStatisticsReportTests : InterviewFactorySpecification
    {
        private QuestionnaireDocument questionnaire;
        private readonly Guid relationQuestion = Id.g1;
        private readonly Guid sexQuestion = Id.g2;
        private InterviewFactory factory;
        private SurveyStatisticsReport reporter;
        private const string teamLeadName = "teamLead";

        private enum Relation { Head = 1, Spouse, Child }
        private enum Sex { Male = 1, Female }

        [SetUp]
        public void SettingUp()
        {
            this.questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.Roster(Id.gA, variable: "hh_member", children: new[]
                {
                    Create.Entity.SingleOptionQuestion(relationQuestion, variable: "relation", answers: new List<Answer>
                    {
                        Create.Entity.Answer("Head", (int) Relation.Head),
                        Create.Entity.Answer("Spouse", (int) Relation.Spouse),
                        Create.Entity.Answer("Child", (int) Relation.Child)
                    }),

                    Create.Entity.SingleOptionQuestion(sexQuestion, variable: "sex", answers: new List<Answer>
                    {
                        Create.Entity.Answer("Male", (int) Sex.Male),
                        Create.Entity.Answer("Female", (int) Sex.Female)
                    })
                })
            );

            PrepareQuestionnaire(questionnaire, 1);

            this.factory = CreateInterviewFactory();

            Because();
            
            this.reporter = new SurveyStatisticsReport(new InterviewReportDataRepository(UnitOfWork));
        }

        private void Because()
        {
            // Creating 4 interviews with different members configuration

            CreateInterview(
                (Relation.Head, Sex.Male),
                (Relation.Spouse, Sex.Female));

            CreateInterview(
                (Relation.Head, Sex.Female),
                (Relation.Spouse, Sex.Male));

            CreateInterview(
                (Relation.Head, Sex.Male),
                (Relation.Spouse, Sex.Female),
                (Relation.Child, Sex.Male));

            CreateInterview(
                (Relation.Head, Sex.Female));

            // there is in total
            // 4 heads, 3 spouses and 1 child
            // 4 male and 4 female members
        }

        [Test]
        public void Categorical_report_by_sex()
        {
            var question = this.questionnaire.Find<SingleQuestion>(sexQuestion);

            var report = this.reporter.GetReport(new SurveyStatisticsReportInputModel
            {
                QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(questionnaire.PublicKey, 1),
                Question = question
            });

            // there is 4 males ,4 females in total 8
            Assert.That(report.Data[0], Is.EqualTo(new object[] { teamLeadName, null, 4, 4, 8 }));
        }

        [Test]
        public void Categorical_report_by_relation()
        {
            var question = this.questionnaire.Find<SingleQuestion>(relationQuestion);

            var report = this.reporter.GetReport(new SurveyStatisticsReportInputModel
            {
                QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(questionnaire.PublicKey, 1),
                Question = question
            });

            // there is 4 heads, 3 spouses and 1 child in total 8
            Assert.That(report.Data[0], Is.EqualTo(new object[] { teamLeadName, null, 4, 3, 1, 8 }));
        }

        [Test]
        public void Categorical_report_by_sex_with_condition_by_relation()
        {
            var question = this.questionnaire.Find<SingleQuestion>(sexQuestion);

            var report = this.reporter.GetReport(new SurveyStatisticsReportInputModel
            {
                QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(questionnaire.PublicKey, 1),
                ConditionalQuestion = this.questionnaire.Find<SingleQuestion>(relationQuestion),
                Condition = new[] { (long)Relation.Spouse },
                Question = question
            });

            // there is 1 male and 2 female spouses, in total there is 3 spouses
            Assert.That(report.Data[0], Is.EqualTo(new object[] { teamLeadName, null, 1, 2, 3 }));
        }

        [Test]
        public void PivotReport_report_proper_data()
        {
            var report = this.reporter.GetReport(new SurveyStatisticsReportInputModel
            {
                QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(questionnaire.PublicKey, 1),
                ConditionalQuestion = this.questionnaire.Find<SingleQuestion>(relationQuestion),
                Question = this.questionnaire.Find<SingleQuestion>(sexQuestion),
                Pivot = true
            });

            // there is 2 male and 2 female head members
            Assert.That(report.Data[0], Is.EqualTo(new object[] { Relation.Head.ToString(),   2, 2, 4}));

            // there is 1 male and 2 female spouse members
            Assert.That(report.Data[1], Is.EqualTo(new object[] { Relation.Spouse.ToString(), 1, 2, 3 }));

            // there is 1 male and 0 female child members
            Assert.That(report.Data[2], Is.EqualTo(new object[] { Relation.Child.ToString(),  1, 0, 1 }));
        }

        private void CreateInterview(params (Relation rel, Sex sex)[] members)
        {
            var interviewId = Guid.NewGuid();

            StoreInterviewSummary(new InterviewSummary(questionnaire)
            {
                InterviewId = interviewId,
                Status = InterviewStatus.Completed,
                ResponsibleName = "responsible",
                ResponsibleId = Id.gC,
                TeamLeadId = Id.gE,
                TeamLeadName = teamLeadName
            }, new QuestionnaireIdentity(questionnaire.PublicKey, 1));

            var state = Create.Entity.InterviewState(interviewId);

            for (var vector = 0; vector < members.Length; vector++)
            {
                var member = members[vector];
                var relation = InterviewStateIdentity.Create(relationQuestion, vector);
                var sex = InterviewStateIdentity.Create(sexQuestion, vector);
                state.Enablement[relation] = true;
                state.Enablement[sex] = true;
                state.Answers[relation] = new InterviewStateAnswer { AsInt = (int)member.rel };
                state.Answers[sex] = new InterviewStateAnswer { AsInt = (int)member.sex };
            }

            factory.Save(state);
        }
    }
}
