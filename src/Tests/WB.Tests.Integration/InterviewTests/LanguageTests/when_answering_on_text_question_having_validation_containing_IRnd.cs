using System;

using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_on_text_question_having_validation_containing_IRnd : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var id = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(id, children: new[]
                {
                    Create.Chapter(children: new IComposite[]
                    {
                        Create.TextQuestion(variable: "test", id :questionId, validationExpression: "Quest.IRnd() > 0.5"),
                    }),
                });

                var interview = SetupInterview(questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextQuestion(Guid.NewGuid(), questionId, Empty.RosterVector, DateTime.Now, "test");

                    return new InvokeResult
                    {
                        AnswerDeclaredInvalidEventCount = eventContext.Count<AnswersDeclaredInvalid>()
                    };
                }
            });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };
        
        It should_raise_AnswerDeclaredInvalidEvent_event = () =>
            result.AnswerDeclaredInvalidEventCount.ShouldEqual(0);

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResult result;
        private static readonly Guid questionId = Guid.Parse("11111111111111111111111111111111");

        [Serializable]
        private class InvokeResult
        {
            public int AnswerDeclaredInvalidEventCount { get; set; }
        }
    }
}