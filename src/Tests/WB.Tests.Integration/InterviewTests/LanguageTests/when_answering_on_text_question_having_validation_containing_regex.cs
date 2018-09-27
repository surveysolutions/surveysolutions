using System;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_on_text_question_having_validation_containing_regex : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var id = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id, children: new[]
                {
                    Abc.Create.Entity.Group(children: new IComposite[]
                    {
                        Abc.Create.Entity.TextQuestion(questionId: questionId,
                            variable: "test",
                            validationExpression: "Regex.IsMatch(self, @\"^\\d{8}_\\d{1,4}$\")", 
                            text: null),
                    }),
                });

                var interview = SetupInterview(questionnaireDocument);
                interview.AnswerTextQuestion(Guid.NewGuid(), questionId, RosterVector.Empty, DateTime.Now, "1");

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextQuestion(Guid.NewGuid(), questionId, RosterVector.Empty, DateTime.Now, "12345678_1234");

                    return new InvokeResult
                    {
                        AnswerDeclaredValidEventCount = eventContext.Count<AnswersDeclaredValid>(),
                        AnswerDeclaredInvalidEventCount = eventContext.Count<AnswersDeclaredInvalid>()
                    };
                }
            });

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }
        
        [NUnit.Framework.Test] public void should_raise_AnswerDeclaredValidEvent_event () =>
            result.AnswerDeclaredValidEventCount.Should().Be(1);

        [NUnit.Framework.Test] public void should_raise_AnswerDeclaredInvalidEvent_event () =>
            result.AnswerDeclaredInvalidEventCount.Should().Be(0);

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
