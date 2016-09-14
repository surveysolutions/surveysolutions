using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_numeric_question_with_valid_answer_and_question_and_static_text_were_valid_before_answer : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionA = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                var staticTextB = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

                var interview = SetupInterview(
                    Create.QuestionnaireDocumentWithOneChapter(children: new[]
                    {
                        Create.Chapter(children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(id: questionA, variable: "a", validationConditions: new List<ValidationCondition>() {new ValidationCondition("a > 0", "err") }),
                            Create.StaticText(id: staticTextB, validationConditions: new List<ValidationCondition>() {new ValidationCondition("a > 0", "err") }),
                        }),
                    }),
                    events: new object[]
                    {
                        Create.Event.NumericIntegerQuestionAnswered(questionId: questionA, answer: 1),

                        Create.Event.AnswersDeclaredValid(new []{  Create.Identity(questionA)}),
                        Create.Event.StaticTextsDeclaredValid(new []{  Create.Identity(staticTextB) })
                    });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), questionA, Empty.RosterVector, DateTime.Now, 3);

                    return new InvokeResult
                    {
                        AnswersDeclaredValidEventCount = eventContext.Count<AnswersDeclaredValid>(),
                        AnswersDeclaredInvalidEventCount = eventContext.Count<AnswersDeclaredInvalid>(),
                        StaticTextsDeclaredInvalidCount = eventContext.Count<StaticTextsDeclaredInvalid>(),
                        StaticTextsDeclaredValidCount = eventContext.Count<StaticTextsDeclaredValid>()
                    };
                }
            });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_not_raise_AnswersDeclaredValid_event = () =>
            result.AnswersDeclaredValidEventCount.ShouldEqual(0);

        It should_raise_AnswersDeclaredInvalid_event = () =>
            result.AnswersDeclaredInvalidEventCount.ShouldEqual(0);

        It should_not_raise_StaticTextsDeclaredInvalid_event = () =>
            result.StaticTextsDeclaredInvalidCount.ShouldEqual(0);

        It should_raise_StaticTextsDeclaredInvalid_event = () =>
            result.StaticTextsDeclaredValidCount.ShouldEqual(0);

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public int AnswersDeclaredValidEventCount { get; set; }
            public int AnswersDeclaredInvalidEventCount { get; set; }

            public int StaticTextsDeclaredValidCount { set; get; }
            public int StaticTextsDeclaredInvalidCount { set; get; }
        }
    }
}