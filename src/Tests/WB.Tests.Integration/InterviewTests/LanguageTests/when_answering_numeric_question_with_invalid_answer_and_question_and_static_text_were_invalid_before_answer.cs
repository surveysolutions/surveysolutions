using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_numeric_question_with_invalid_answer_and_question_and_static_text_were_invalid_before_answer : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionA = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                var staticTextB = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

                var interview = SetupInterview(
                    Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {
                        Abc.Create.Entity.Group(children: new IComposite[]
                        {
                            Abc.Create.Entity.NumericIntegerQuestion(id: questionA, variable: "a", validationConditions: new List<ValidationCondition>() {new ValidationCondition("a > 0", "err") }),
                            Abc.Create.Entity.StaticText(publicKey: staticTextB, validationConditions: new List<ValidationCondition>() {new ValidationCondition("a > 0", "err") }),
                        }),
                    }),
                    events: new object[]
                    {
                        Abc.Create.Event.NumericIntegerQuestionAnswered(
                            questionA, null, -1, null, null
                        ),
                        
                        Abc.Create.Event.AnswersDeclaredInvalid(new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>()
                        {
                            {
                                Abc.Create.Identity(questionA),
                                new List<FailedValidationCondition>() {new FailedValidationCondition(0)}
                            }
                        }),

                        Abc.Create.Event.StaticTextsDeclaredInvalid(new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>()
                        {
                            {
                                Abc.Create.Identity(staticTextB),
                                new List<FailedValidationCondition>() {new FailedValidationCondition(0)}
                            }
                        }.ToList())
                    });

            using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), questionA, RosterVector.Empty, DateTime.Now, -3);

                    return new InvokeResult
                    {
                        AnswersDeclaredValidEventCount = eventContext.Count<AnswersDeclaredValid>(),
                        AnswersDeclaredInvalidEventCount = eventContext.Count<AnswersDeclaredInvalid>(),
                        StaticTextsDeclaredInvalidCount = eventContext.Count<StaticTextsDeclaredInvalid>(),
                        StaticTextsDeclaredValidCount = eventContext.Count<StaticTextsDeclaredValid>()
                    };
                }
            });

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [NUnit.Framework.Test] public void should_not_raise_AnswersDeclaredValid_event () =>
            result.AnswersDeclaredValidEventCount.Should().Be(0);

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredInvalid_event () =>
            result.AnswersDeclaredInvalidEventCount.Should().Be(0);

        [NUnit.Framework.Test] public void should_not_raise_StaticTextsDeclaredInvalid_event () =>
            result.StaticTextsDeclaredInvalidCount.Should().Be(0);

        [NUnit.Framework.Test] public void should_raise_StaticTextsDeclaredInvalid_event () =>
            result.StaticTextsDeclaredValidCount.Should().Be(0);


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
