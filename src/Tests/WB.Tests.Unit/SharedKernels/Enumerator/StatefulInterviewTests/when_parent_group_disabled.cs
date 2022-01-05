using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_parent_group_disabled : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaire = Create.Entity.QuestionnaireDocument(
                  id: questionnaireId,
                  children: new IComposite[]
                  {
                    Create.Entity.Group(
                        groupId: parentGroupId,
                        children: new IComposite[]
                        {
                            Create.Entity.TextQuestion(childQuestionId),
                        })
                  });
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, Create.Entity.PlainQuestionnaire(questionnaire, 1));

            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository);

            BecauseOf();
        }
            
        private void BecauseOf() => statefulInterview.Apply(Create.Event.GroupsDisabled(new[] { Create.Entity.Identity(parentGroupId, RosterVector.Empty) }));

        [NUnit.Framework.Test] public void should_mark_child_questions_as_disabled () => 
            statefulInterview.IsEnabled(Create.Entity.Identity(childQuestionId, RosterVector.Empty)).Should().BeFalse();

        static StatefulInterview statefulInterview;

        static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid parentGroupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Guid childQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
