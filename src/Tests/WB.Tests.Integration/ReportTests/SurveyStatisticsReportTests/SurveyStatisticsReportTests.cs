using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Workspaces;
using WB.Tests.Abc;
using WB.Tests.Integration.InterviewFactoryTests;

namespace WB.Tests.Integration.ReportTests.SurveyStatisticsReportTests
{
    internal class SurveyStatisticsReportTests : InterviewFactorySpecification
    {
        private QuestionnaireDocument questionnaire;
        private IQuestionnaire plainQuestionnaire;
        private readonly Guid relationQuestion = Id.g1;
        private readonly Guid sexQuestion = Id.g2;
        private readonly Guid dwellingQuestion = Id.g3;

        private SurveyStatisticsReport reporter;
        private const string teamLeadName = "teamLead";

        internal enum Relation { Head = 1, Spouse, Child }
        internal enum Sex { Male = 1, Female }
        internal enum Dwelling { House = 1, Barrack, Hole }

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
            this.plainQuestionnaire = Create.Entity.PlainQuestionnaire(this.questionnaire, 1);

            PrepareQuestionnaire(questionnaire, 1);

            Because();
            
            this.reporter = new SurveyStatisticsReport(new InterviewReportDataRepository(UnitOfWork));
        }

        private void Because()
        {
            // Creating 4 interviews with different members configuration

            CreateInterview(Dwelling.House,  workspace,
                (Relation.Head, Sex.Male),
                (Relation.Spouse, Sex.Female));

            CreateInterview(Dwelling.Barrack,  workspace,
                (Relation.Head, Sex.Female),
                (Relation.Spouse, Sex.Male));

            CreateInterview(Dwelling.Hole, workspace,
                (Relation.Head, Sex.Male),
                (Relation.Spouse, Sex.Female),
                (Relation.Child, Sex.Male));

            CreateInterview(Dwelling.House, workspace,
                (Relation.Head, Sex.Female));

            // there is in total 8 members in survey
            // 4 heads, 3 spouses and 1 child
            // 4 male and 4 female members
            //  3 people live in houses, 2 ion barracks and 3 in a hole in the ground
        }

        [Test]
        public void Categorical_report_by_sex()
        {
            var report = this.reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
                {
                    QuestionnaireId = questionnaire.PublicKey.FormatGuid(), QuestionnaireVersion = 1,
                    QuestionId = sexQuestion
                });

            // there is 4 males ,4 females in total 8
            Assert.That(report.Data[0], Is.EqualTo(new object[] { teamLeadName, null, 4, 4, 8 }));
        }

        [Test]
        public void Categorical_report_by_relation()
        {
            var report = this.reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionnaireVersion = 1,
                QuestionId = relationQuestion
            });

            // there is 4 heads, 3 spouses and 1 child in total 8
            Assert.That(report.Data[0], Is.EqualTo(new object[] { teamLeadName, null, 4, 3, 1, 8 }));
        }

        [Test]
        public void Categorical_report_by_sex_with_condition_by_relation()
        {
            var report = this.reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionnaireVersion = 1,
                ConditionalQuestionId = relationQuestion,
                Condition = new[] { (long)Relation.Spouse },
                QuestionId = sexQuestion
                });

            // there is 1 male and 2 female spouses, in total there is 3 spouses
            Assert.That(report.Data[0], Is.EqualTo(new object[] { teamLeadName, null, 1, 2, 3 }));
        }

        [Test]
        public void PivotReport_report_proper_data()
        {
            var report = this.reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionnaireVersion = 1,
                ConditionalQuestionId = relationQuestion,
                QuestionId = sexQuestion,
                Pivot = true
            });

            // there is 2 male and 2 female head members
            Assert.That(report.Data[0], Is.EqualTo(new object[] { Relation.Head.ToString(),   2, 2, 4}));

            // there is 1 male and 2 female spouse members
            Assert.That(report.Data[1], Is.EqualTo(new object[] { Relation.Spouse.ToString(), 1, 2, 3 }));

            // there is 1 male and 0 female child members
            Assert.That(report.Data[2], Is.EqualTo(new object[] { Relation.Child.ToString(),  1, 0, 1 }));
        }

        //                                                            object[] { Male, Female, Total }
        [TestCase(Dwelling.House,                ExpectedResult = new object[] {    1,      2,     3 })]
        [TestCase(Dwelling.Hole, Dwelling.House, ExpectedResult = new object[] {    3,      3,     6 })]
        [TestCase(Dwelling.Barrack,              ExpectedResult = new object[] {    1,      1,     2 })]
        [TestCase(Dwelling.Barrack, Dwelling.Hole, Dwelling.House, Description = "Should return all members", 
                                                 ExpectedResult = new object[] {    4,      4,     8 })]
        public object[] Should_be_able_to_condition_report_by_non_roster_variable(params Dwelling[] condition)
        {
            var report = this.reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionnaireVersion = 1,
                ConditionalQuestionId = dwellingQuestion,
                QuestionId = sexQuestion,
                Condition = condition.Select(c => (long) c).ToArray()
            });

            return report.Data[0].Skip(2).ToArray();
        }

        [Test]
        public void Should_be_able_to_build_pivot_report_by_non_roster_variable()
        {
            var report = this.reporter.GetReport(plainQuestionnaire,
                new SurveyStatisticsReportInputModel
            {
                QuestionnaireId = questionnaire.PublicKey.FormatGuid(),
                QuestionnaireVersion = 1,
                ConditionalQuestionId = dwellingQuestion,
                QuestionId = sexQuestion,
                Pivot = true
            });

            // there is 1 male and 2 females in houses
            Assert.That(report.Data[0], Is.EqualTo(new object[] { Dwelling.House.ToString(),   1, 2, 3 }));

            // there is 1 male and 1 female in barracks
            Assert.That(report.Data[1], Is.EqualTo(new object[] { Dwelling.Barrack.ToString(), 1, 1, 2 }));

            // there is 2 male and 1 females in holes
            Assert.That(report.Data[2], Is.EqualTo(new object[] { Dwelling.Hole.ToString(),    2, 1, 3 }));

            // total 8 members are 4 male 4 female 
            Assert.That(report.Totals, Is.EqualTo(new object[] { "Total", 4, 4, 8}));
        }

        private void CreateInterview(Dwelling dwelling, IWorkspaceContextAccessor workspaceContextAccessor, params (Relation rel, Sex sex)[] members)
        {
            var interviewId = Guid.NewGuid();

               var summary = Create.Entity.InterviewSummary(
                                            interviewId: interviewId,
                                            status: InterviewStatus.Completed,
                                            responsibleName: "responsible",
                                            responsibleId: Id.gC,
                                            teamLeadId: Id.gE,
                                            teamLeadName: teamLeadName,
                                            questionnaireVariable: questionnaire.VariableName);

            StoreInterviewSummary(summary, new QuestionnaireIdentity(questionnaire.PublicKey, 1));
            
            SetIntAnswer(dwellingQuestion, (int) dwelling);

            for (var vector = 0; vector < members.Length; vector++)
            {
                var member = members[vector];

                SetIntAnswer(relationQuestion, (int) member.rel, vector);
                SetIntAnswer(sexQuestion, (int) member.sex, vector);
            }

            void SetIntAnswer(Guid questionId, int answer, params int[] rosterVector)
            {
                this.UnitOfWork.Session.Connection.Execute(
                    $"INSERT INTO report_statistics(interview_id, entity_id, rostervector, answer, \"type\", " +
                    "is_enabled) VALUES(@interviewId, @entityId, @rosterVector, @answer, 0, @enabled); ", new
                    {
                        interviewId = summary.Id,
                        rosterVector = new RosterVector(rosterVector).AsString(),
                        answer = new [] {answer},
                        enabled = true,
                        entityId = questionnaire.EntitiesIdMap[questionId]
                    });
            }
        }
    }
}
