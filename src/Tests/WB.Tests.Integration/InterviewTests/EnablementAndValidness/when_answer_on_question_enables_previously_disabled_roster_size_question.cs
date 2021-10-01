using System;
using Main.Core.Documents;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    [TestOf(typeof(Interview))]
    internal class when_answer_on_question_enables_previously_disabled_roster_size_question : InterviewTestsContext
    {
        [SetUp]
        public void Context()
        {
            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(id: questionWhichDisablesRosterSizeQuestion, variable: "num_disable"),
                Create.Entity.NumericIntegerQuestion(id: rosterSizeId, variable: "num_trigger", enablementCondition: "num_disable == 2"),
                Create.Entity.NumericRoster(rosterId: rosterId, variable: "ros", rosterSizeQuestionId: rosterSizeId)
            );
            appDomainContext = AppDomainContext.Create();

            interview = SetupStatefullInterview(appDomainContext.AssemblyLoadContext, questionnaire);

            interview.AnswerNumericIntegerQuestion(userId, questionWhichDisablesRosterSizeQuestion, RosterVector.Empty, DateTime.Now, 2);
            interview.AnswerNumericIntegerQuestion(userId, rosterSizeId, RosterVector.Empty, DateTime.Now, 2);
            interview.AnswerNumericIntegerQuestion(userId, questionWhichDisablesRosterSizeQuestion, RosterVector.Empty, DateTime.Now, 1);

            interview.AnswerNumericIntegerQuestion(userId, questionWhichDisablesRosterSizeQuestion, RosterVector.Empty, DateTime.Now, 2);
        }

        [TearDown]
        public void Cleanup()
        {
            interview = null;
            appDomainContext.Dispose();
        }

        [Test]
        public void should_enable_first_roster_row()
        {
            var roster = interview.GetRoster(Create.Identity(rosterId, 0));
            Assert.That(roster.IsDisabled(), Is.False);
        }

        [Test]
        public void should_enable_second_roster_row()
        {
            var roster = interview.GetRoster(Create.Identity(rosterId, 1));
            Assert.That(roster.IsDisabled(), Is.False);
        }

        private static AppDomainContext appDomainContext;
        private static StatefulInterview interview;
        private static readonly Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static readonly Guid rosterSizeId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid questionWhichDisablesRosterSizeQuestion = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid rosterId = Guid.Parse("11111111111111111111111111111111");
    }
}
