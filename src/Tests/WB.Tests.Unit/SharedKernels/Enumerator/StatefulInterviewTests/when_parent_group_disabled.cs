using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_parent_group_disabled : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.QuestionnaireDocument(
                  id: questionnaireId,
                  children: new IComposite[]
                  {
                    Create.Entity.Group(
                        groupId: parentGroupId,
                        children: new IComposite[]
                        {
                            Create.Entity.Question(childQuestionId),
                        })
                  });
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, Create.Entity.PlainQuestionnaire(questionnaire, 1));

            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository);

        };
            
        Because of = () => statefulInterview.Apply(Create.Event.GroupsDisabled(new[] { Create.Entity.Identity(parentGroupId, RosterVector.Empty) }));

        It should_mark_child_questions_as_disabled = () => 
            statefulInterview.IsEnabled(Create.Entity.Identity(childQuestionId, RosterVector.Empty)).ShouldBeFalse();

        static StatefulInterview statefulInterview;

        static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid parentGroupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Guid childQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}