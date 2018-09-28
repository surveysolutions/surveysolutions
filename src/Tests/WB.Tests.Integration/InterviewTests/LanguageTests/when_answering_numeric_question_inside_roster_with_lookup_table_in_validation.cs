using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_numeric_question_inside_roster_with_lookup_table_in_validation : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
                var questionA = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                var questionB = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
                var rosterId = Guid.Parse("cccccccccccccccccccccccccccccccc");
                var lookupId = Guid.Parse("dddddddddddddddddddddddddddddddd");
                var userId = Guid.NewGuid();

                var lookupTableContent = IntegrationCreate.LookupTableContent(new [] {"min", "max"},
                    IntegrationCreate.LookupTableRow(1, new decimal?[] { 1.15m, 10}),
                    IntegrationCreate.LookupTableRow(2, new decimal?[] { 1, 10}),
                    IntegrationCreate.LookupTableRow(3, new decimal?[] { 1, 10})
                );

                var lookupTableServiceMock = new Mock<ILookupTableService>();
                lookupTableServiceMock.SetReturnsDefault(lookupTableContent);

                SetUp.InstanceToMockedServiceLocator<ILookupTableService>(lookupTableServiceMock.Object);

                var assetsTitles = new[]
                {
                    Create.Entity.FixedTitle(1, "TV"),
                    Create.Entity.FixedTitle(2, "Microwave"),
                    Create.Entity.FixedTitle(3, "Cleaner")
                };
                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: questionA, variable: "a", validationExpression: "a > price[1].min && a < price[1].max"),
                    Create.Entity.FixedRoster(rosterId, variable: "assets",fixedTitles: assetsTitles, children: new[]
                    {
                        Create.Entity.NumericRealQuestion(id: questionB, variable: "p", validationExpression: "p.InRange(price[@rowcode].min, price[@rowcode].max)")
                    })
                });

                questionnaire.LookupTables.Add(lookupId, Create.Entity.LookupTable("price"));

                var interview = SetupInterview(questionnaire);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionA, RosterVector.Empty, DateTime.Now, 1);
                    interview.AnswerNumericRealQuestion(userId, questionB, Create.RosterVector(1), DateTime.Now, -30);
                    interview.AnswerNumericRealQuestion(userId, questionB, Create.RosterVector(2), DateTime.Now, 35);
                    interview.AnswerNumericRealQuestion(userId, questionB, Create.RosterVector(3), DateTime.Now, 300);

                    return new InvokeResult
                    {
                        IsQuestionAInValid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == questionA)),
                        IsQuestionB1InValid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == questionB && q.RosterVector.Identical(Create.RosterVector(1)))),
                        IsQuestionB2InValid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == questionB && q.RosterVector.Identical(Create.RosterVector(2)))),
                        IsQuestionB3InValid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == questionB && q.RosterVector.Identical(Create.RosterVector(3)))),
                    };
                }
            });

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredValid_event_for_questiona_a () =>
            result.IsQuestionAInValid.Should().BeTrue();

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredInvalid_event_for_questiona_b_1 () =>
            result.IsQuestionB1InValid.Should().BeTrue();

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredInvalid_event_for_questiona_b_2 () =>
            result.IsQuestionB2InValid.Should().BeTrue();

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredInvalid_event_for_questiona_b_3 () =>
            result.IsQuestionB3InValid.Should().BeTrue();

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public bool IsQuestionAInValid { get; set; }
            public bool IsQuestionB1InValid { get; set; }
            public bool IsQuestionB2InValid { get; set; }
            public bool IsQuestionB3InValid { get; set; }
        }
    }
}
