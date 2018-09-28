using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_on_a_question_with_answer_that_enables_dependent_group : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        public void BecauseOf() => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            SetUp.MockedServiceLocator();

            var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var dependentGroupId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            var interview = SetupInterviewWithExpressionStorage(questionnaireDocument: Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Abc.Create.Entity.NumericIntegerQuestion(answeredQuestionId, "q1"),
                Abc.Create.Entity.Group(dependentGroupId, "Group X", null, "q1 > 0", false, null),
            }));

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: answeredQuestionId, answer: 1));

                return new InvokeResults
                {
                    WasGroupsEnabledEventPublishedForDependentGroup =
                        eventContext.GetSingleEventOrNull<GroupsEnabled>()?.Groups.Any(group => group.Id == dependentGroupId) ?? false,
                };
            }
        });

        [NUnit.Framework.Test] public void should_enable_dependent_group () =>
            results.WasGroupsEnabledEventPublishedForDependentGroup.Should().BeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasGroupsEnabledEventPublishedForDependentGroup { get; set; }
        }
    }
}
