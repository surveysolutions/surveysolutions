using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_numeric_question_inside_roster_with_lookup_table_in_validation : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
                var questionA = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                var questionB = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
                var rosterId = Guid.Parse("cccccccccccccccccccccccccccccccc");
                var lookupId = Guid.Parse("dddddddddddddddddddddddddddddddd");
                var userId = Guid.NewGuid();

                var lookupTableContent = Create.LookupTableContent(new [] {"min", "max"},
                    Create.LookupTableRow(1, new decimal?[] { 1.15m, 10}),
                    Create.LookupTableRow(2, new decimal?[] { 1, 10}),
                    Create.LookupTableRow(3, new decimal?[] { 1, 10})
                );

                var lookupTableServiceMock = new Mock<ILookupTableService>();
                lookupTableServiceMock
                    .Setup(x => x.GetLookupTableContent(questionnaireId, lookupId))
                    .Returns(lookupTableContent);

                Setup.InstanceToMockedServiceLocator<ILookupTableService>(lookupTableServiceMock.Object);

                var assetsTitles = new[]
                {
                    Create.FixedRosterTitle(1, "TV"),
                    Create.FixedRosterTitle(2, "Microwave"),
                    Create.FixedRosterTitle(3, "Cleaner")
                };
                var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new[]
                {
                    Create.Chapter(children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(id: questionA, variable: "a", validationExpression: "a > price[1].min && a < price[1].max"),
                        Create.Roster(rosterId, variable: "assets", rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: assetsTitles,
                            children: new []
                            {
                                Create.NumericRealQuestion(id: questionB, variable: "p", validationExpression: "p.InRange(price[@rowcode].min, price[@rowcode].max)")
                            })
                    }),
                });

                questionnaire.LookupTables.Add(lookupId, Create.LookupTable("price"));

                var interview = SetupInterview(questionnaire);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionA, Empty.RosterVector, DateTime.Now, 1);
                    interview.AnswerNumericRealQuestion(userId, questionB, new decimal[] { 1 }, DateTime.Now, -30);
                    interview.AnswerNumericRealQuestion(userId, questionB, new decimal[] { 2 }, DateTime.Now, 35);
                    interview.AnswerNumericRealQuestion(userId, questionB, new decimal[] { 3 }, DateTime.Now, 300);

                    return new InvokeResult
                    {
                        IsQuestionAInValid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == questionA)),
                        IsQuestionB1InValid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == questionB && q.RosterVector.SequenceEqual(new[] { 1m } ))),
                        IsQuestionB2InValid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == questionB && q.RosterVector.SequenceEqual(new[] { 2m } ))),
                        IsQuestionB3InValid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == questionB && q.RosterVector.SequenceEqual(new[] { 3m } ))),
                    };
                }
            });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_raise_AnswersDeclaredValid_event_for_questiona_a = () =>
            result.IsQuestionAInValid.ShouldBeTrue();

        It should_raise_AnswersDeclaredInvalid_event_for_questiona_b_1 = () =>
            result.IsQuestionB1InValid.ShouldBeTrue();

        It should_raise_AnswersDeclaredInvalid_event_for_questiona_b_2 = () =>
            result.IsQuestionB2InValid.ShouldBeTrue();

        It should_raise_AnswersDeclaredInvalid_event_for_questiona_b_3 = () =>
            result.IsQuestionB3InValid.ShouldBeTrue();

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