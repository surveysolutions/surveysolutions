using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_on_a_question_with_answer_that_enables_dependent_group : in_standalone_app_domain
    {
        Because of = () => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            Setup.MockedServiceLocator();

            var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var dependentGroupId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            var interview = SetupInterview(questionnaireDocument: Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.NumericIntegerQuestion(answeredQuestionId, "q1"),
                Create.Group(dependentGroupId, enablementCondition: "q1 > 0"),
            }));

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestion(questionId: answeredQuestionId, answer: 1));

                return new InvokeResults
                {
                    WasGroupsEnabledEventPublishedForDependentGroup =
                        eventContext.GetSingleEventOrNull<GroupsEnabled>()?.Groups.Any(group => group.Id == dependentGroupId) ?? false,
                };
            }
        });

        It should_enable_dependent_group = () =>
            results.WasGroupsEnabledEventPublishedForDependentGroup.ShouldBeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasGroupsEnabledEventPublishedForDependentGroup { get; set; }
        }
    }
}