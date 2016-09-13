using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_on_text_question_having_validation_containing_regex : InterviewTestsContext
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
                        Create.TextQuestion(variable: "test", id :questionId, validationExpression: "Regex.IsMatch(self, @\"^\\d{8}_\\d{1,4}$\")"),
                    }),
                });

                var interview = SetupInterview(questionnaireDocument);
                interview.AnswerTextQuestion(Guid.NewGuid(), questionId, Empty.RosterVector, DateTime.Now, "1");

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextQuestion(Guid.NewGuid(), questionId, Empty.RosterVector, DateTime.Now, "12345678_1234");

                    return new InvokeResult
                    {
                        AnswerDeclaredValidEventCount = eventContext.Count<AnswersDeclaredValid>(),
                        AnswerDeclaredInvalidEventCount = eventContext.Count<AnswersDeclaredInvalid>()

                    };
                }
            });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };
        
        It should_raise_AnswerDeclaredValidEvent_event = () =>
            result.AnswerDeclaredValidEventCount.ShouldEqual(1);

        It should_raise_AnswerDeclaredInvalidEvent_event = () =>
            result.AnswerDeclaredInvalidEventCount.ShouldEqual(0);

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResult result;
        private static readonly Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid groupId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid rosterId = Guid.Parse("33333333333333333333333333333333");

        [Serializable]
        private class InvokeResult
        {
            public int AnswerDeclaredValidEventCount { get; set; }
            public int AnswerDeclaredInvalidEventCount { get; set; }
            
        }
    }
}