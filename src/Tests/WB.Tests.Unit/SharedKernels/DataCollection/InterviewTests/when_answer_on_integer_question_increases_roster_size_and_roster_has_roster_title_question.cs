using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_integer_question_increases_roster_size_and_roster_has_roster_title_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(SectionId, QuestionnaireId,
                  Create.Entity.NumericIntegerQuestion(questionWhichIncreasesRosterSizeId, variable: "num"),
                  Create.Entity.Roster(rosterGroupId, variable: "r",
                      rosterSizeSourceType: RosterSizeSourceType.Question,
                      rosterSizeQuestionId: questionWhichIncreasesRosterSizeId,
                      rosterTitleQuestionId: rosterTitleId,
                      children: new IComposite[]
                      {
                          Create.Entity.NumericIntegerQuestion(rosterTitleId, variable: "title"),
                      })));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(QuestionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: QuestionnaireId, questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[] { }, DateTime.Now, 3);

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_event_for_that_roster () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.All(instance => instance.GroupId == rosterGroupId));

        [NUnit.Framework.Test] public void should_not_raise_RosterInstancesTitleChanged_event () =>
            eventContext.ShouldNotContainEvent<RosterInstancesTitleChanged>();

        [NUnit.Framework.Test] public void should_not_raise_RosterInstancesRemoved_event () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>();

        private static EventContext eventContext;
        private static Interview interview;
        private static readonly Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static readonly Guid questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid rosterGroupId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid SectionId = Guid.Parse("FFFFFFFF0000000000000000DDDDDDDD");
        private static readonly Guid  rosterTitleId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid QuestionnaireId = Guid.Parse("10000000000000000000000000000000");
    }
}
