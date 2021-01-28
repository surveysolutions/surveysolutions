using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_numeric_question_inside_roster_with_lookup_table_in_validation : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireId = Id.gF;
                var questionA = Id.gA;
                var questionB = Id.gB;
                var questionC = Id.gC;
                var rosterId = Id.g10;
                var lookupId = Id.g6;
                var variableA = Id.g9;
                var userId = Guid.NewGuid();

                var lookupTableContent = IntegrationCreate.LookupTableContent(new[] { "min", "max" },
                    IntegrationCreate.LookupTableRow(1, new decimal?[] { 1.15m, 10 }),
                    IntegrationCreate.LookupTableRow(2, new decimal?[] { 1, 10 }),
                    IntegrationCreate.LookupTableRow(3, new decimal?[] { 1, 10 }),
                    IntegrationCreate.LookupTableRow(4, new decimal?[] { null, 10.1212m })
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
                    Create.Entity.NumericIntegerQuestion(id: questionA, variable: "a",
                        validationExpression: "a > price[1].min && a < price[1].max"),

                    Create.Entity.FixedRoster(rosterId, variable: "assets",fixedTitles: assetsTitles, children: new[]
                    {
                        Create.Entity.NumericRealQuestion(id: questionB, variable: "p", validationExpression: "p.InRange(price[@rowcode].min, price[@rowcode].max)")
                    }),

                    Create.Entity.NumericRealQuestion(id: questionC, variable: "qc", validationExpression: "!(price[4].min == null && price[4].max == 10.1212 )") ,

                    Create.Entity.Variable(variableA, VariableType.String, "v1", "qc.ToString()")
                });

                questionnaire.LookupTables.Add(lookupId, Create.Entity.LookupTable("price"));
                var culture = CultureInfo.GetCultureInfo("ru");
                var interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaire);

                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                double? res = null;
                if (double.TryParse("3.44", NumberStyles.Float, CultureInfo.InvariantCulture, out var _res)) res = _res;

                using var eventContext = new EventContext();

                interview.AnswerNumericIntegerQuestion(userId, questionA, RosterVector.Empty, DateTime.Now, 1);
                interview.AnswerNumericRealQuestion(userId, questionB, Create.RosterVector(1), DateTime.Now, -30);
                interview.AnswerNumericRealQuestion(userId, questionB, Create.RosterVector(2), DateTime.Now, 35);
                interview.AnswerNumericRealQuestion(userId, questionB, Create.RosterVector(3), DateTime.Now, 300);
                interview.AnswerNumericRealQuestion(userId, questionC, Create.RosterVector(), DateTime.Now, 0.3);

                bool HasInvalidAnswerAt(Guid question, params int[] vector )
                {
                    return eventContext.AnyEvent<AnswersDeclaredInvalid>(x =>
                        x.Questions.Any(q => q.Id == question && q.RosterVector.Identical(vector ?? Create.RosterVector(vector))));
                }

                return new InvokeResult
                {
                    IsQuestionAInvalid = HasInvalidAnswerAt(questionA),
                    IsQuestionB1Invalid = HasInvalidAnswerAt(questionB, 1),
                    IsQuestionB2Invalid = HasInvalidAnswerAt(questionB, 2),
                    IsQuestionB3Invalid = HasInvalidAnswerAt(questionB, 3),
                    IsQuestionCInvalid = HasInvalidAnswerAt(questionC),
                    VariableAHasExpectedValue = eventContext.AnyEvent<VariablesChanged>(x => x.ChangedVariables.Any(v => (string)v.NewValue == "0,3"))
                };
            });

        [OneTimeTearDown]
        public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void should_raise_AnswersDeclaredValid_event_for_questiona_a() => result.IsQuestionAInvalid.Should().BeTrue();

        [Test]
        public void should_raise_AnswersDeclaredInvalid_event_for_questiona_b_1() => result.IsQuestionB1Invalid.Should().BeTrue();

        [Test]
        public void should_raise_AnswersDeclaredInvalid_event_for_questiona_b_2() => result.IsQuestionB2Invalid.Should().BeTrue();

        [Test]
        public void should_raise_AnswersDeclaredInvalid_event_for_questiona_c() => result.IsQuestionCInvalid.Should().BeTrue();

        [Test]
        public void should_has_expected_localized_variable_name() => result.VariableAHasExpectedValue.Should().BeTrue();

        [Test]
        public void should_raise_AnswersDeclaredInvalid_event_for_variable_3() => result.IsQuestionB3Invalid.Should().BeTrue();

        private static AppDomainContext appDomainContext;
        private static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public bool IsQuestionAInvalid { get; set; }
            public bool IsQuestionB1Invalid { get; set; }
            public bool IsQuestionB2Invalid { get; set; }
            public bool IsQuestionB3Invalid { get; set; }
            public bool IsQuestionCInvalid { get; set; }
            public bool VariableAHasExpectedValue { get; set; }
        }
    }
}
