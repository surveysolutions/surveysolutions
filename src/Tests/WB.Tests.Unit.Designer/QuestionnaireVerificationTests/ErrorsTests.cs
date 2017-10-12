﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    internal class ErrorsTests
    {
        private static readonly Guid Id1 = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid Id2 = Guid.Parse("22222222222222222222222222222222");

        [TestCase("variableЙФЪ", "WB0122")]
        [TestCase("1variable", "WB0123")]
        [TestCase("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "WB0121")]
        [TestCase("_variable", "WB0123")]
        [TestCase("variable_", "WB0124")]
        [TestCase("vari__able", "WB0125")]
        [TestCase("a23456789012345678901", "WB0121")]
        public void variable_variable_name_is_invalid(string variable, string errorCode)
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Variable(Id1, variableName: variable)
                })
                .ExpectError(errorCode);

        [TestCase("variableЙФЪ", "WB0122")]
        [TestCase("1variable", "WB0123")]
        [TestCase("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "WB0121")]
        [TestCase("_variable", "WB0123")]
        [TestCase("variable_", "WB0124")]
        [TestCase("vari__able", "WB0125")]
        [TestCase("a23456789012345678901", "WB0121")]
        public void roster_variable_name_is_invalid(string variable, string errorCode)
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.FixedRoster(Id1, variable: variable)
                })
                .ExpectError(errorCode);

        [TestCase("variableЙФЪ", "WB0122")]
        [TestCase("1variable", "WB0123")]
        [TestCase("_variable", "WB0123")]
        [TestCase("variable_", "WB0124")]
        [TestCase("vari__able", "WB0125")]
        [TestCase("a23456789012345678901", "WB0121")]
        [TestCase("rowcode", "WB0058")]
        [TestCase("rowname", "WB0058")]
        [TestCase("rowindex", "WB0058")]
        [TestCase("roster", "WB0058")]
        [TestCase("Id", "WB0058")]
        public void question_variable_name_is_invalid(string variable, string errorCode)
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.GpsCoordinateQuestion(Id1, variable: variable)
                })
                .ExpectError(errorCode);

        [Test]
        public void categorical_question_with_long_option()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.SingleQuestion(Id1, variable: "q2", options: new List<Answer>{ Create.Option(1, "A".PadLeft(300, 'A'))})
                })
                .ExpectError("WB0129");

        [TestCase(0)]
        [TestCase(16)]
        public void real_question_with_decimal_places_not_in_range_1_15(int countOfDecimalPlaces)
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.NumericRealQuestion(Id2, variable: "q1", decimalPlaces: countOfDecimalPlaces),
                })
                .ExpectError("WB0128");

        [Test]
        public void identifying_question_with_sunstitution()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.NumericIntegerQuestion(Id1, variable: "q2"),
                    Create.NumericIntegerQuestion(Id2, variable: "q1", variableLabel: "%q2%"),
                })
                .ExpectError("WB0008");

        [Test]
        public void question_with_validation_uses_forbidden_DateTime_properties()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.TextQuestion(validationConditions: new[] { Create.ValidationCondition("System.DateTime.UtcNow.ToString() == self") })
                })
                .ExpectError("WB0118");

        [Test]
        public void question_with_condition_uses_forbidden_DateTime_properties()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.TextQuestion(enablementCondition: "DateTime.Now.AddMonth(1) > System.DateTime.Today")
                })
                .ExpectError("WB0118");

        [Test]
        public void question_with_option_filter_uses_forbidden_DateTime_properties()
           => Create.QuestionnaireDocumentWithOneChapter(new[]
               {
                    Create.SingleQuestion(optionsFilter: "(System.DateTime.Today.AddMonth(1) - System.DateTime.Today).TotalSeconds > 10")
               })
               .ExpectError("WB0118");

        [Test]
        public void question_with_linked_filter_uses_forbidden_DateTime_properties()
           => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
               {
                    Create.TextListQuestion(questionId: Id1),
                    Create.SingleQuestion(linkedToQuestionId: Id1, linkedFilter: "(System.DateTime.Today.AddMonth(1) - DateTime.Today).TotalSeconds > 10")
               })
               .ExpectError("WB0118");

        [Test]
        public void static_text_with_validation_uses_forbidden_DateTime_properties()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.StaticText(validationConditions: new[] { Create.ValidationCondition("System.DateTime.UtcNow.ToString() == self") })
                })
                .ExpectError("WB0118");

        [Test]
        public void static_text_condition_uses_forbidden_DateTime_properties()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.StaticText(enablementCondition: "DateTime.Now.AddMonth(1) > System.DateTime.Today")
                })
                .ExpectError("WB0118");

        [Test]
        public void group_condition_uses_forbidden_DateTime_properties()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Group(enablementCondition: "DateTime.Now.AddMonth(1) > DateTime.Today")
                })
                .ExpectError("WB0118");

        [Test]
        public void variable_condition_uses_forbidden_DateTime_properties()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Variable(expression: "(System.DateTime.UtcNow.AddMonth(1) - System.DateTime.Today).TotalSeconds")
                })
                .ExpectError("WB0118");

        [Test]
        public void circular_reference_in_enablings()
            => Create.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Question(variable: "x", enablementCondition: "y > 0"),
                    Create.Question(variable: "y", enablementCondition: "x > 0"),
                })
                .ExpectError("WB0056");

        [Test]
        public void circular_reference_in_enablings_hidden_by_macro()
            => Create.QuestionnaireDocumentWithOneChapter(
                    macros: new[]
                    {
                        Create.Macro(name: "m", content: "x > 0"),
                    },
                    children: new[]
                    {
                        Create.Question(variable: "x", enablementCondition: "y > 0"),
                        Create.Question(variable: "y", enablementCondition: "$m"),
                    })
                .ExpectError("WB0056");

        [Test]
        public void linked_question_reference_on_TextList_question_from_wrong_scope()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.Roster(children: new IComposite[]
                    {
                        Create.TextListQuestion(Id1),
                    }),
                    Create.SingleQuestion(linkedToQuestionId: Id1)
                })
                .ExpectError("WB0116");


        [Test]
        public void linked_question_reference_on_TextList_question_from_current_and_child_scope()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.TextListQuestion(Id1),
                    Create.Roster(children: new IComposite[]
                    {
                        Create.TextListQuestion(Id2),
                        Create.SingleQuestion(linkedToQuestionId: Id1),
                        Create.SingleQuestion(linkedToQuestionId: Id2)
                    }),
                })
                .ExpectNoError("WB0116");

        [Test]
        public void linked_question_reference_on_TextList_question()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.TextListQuestion(Id1),
                    Create.SingleQuestion(linkedToQuestionId: Id1)
                })
                .ExpectNoError("WB0012");

        [Test]
        public void linked_to_TextList_question_has_options_filter()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.TextListQuestion(Id1),
                    Create.SingleQuestion(linkedToQuestionId: Id1, optionsFilter: "something")
                })
                .ExpectCritical("WB0117");

        [Test]
        public void linked_to_TextList_question_has_linked_filter()
            => Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.TextListQuestion(Id1),
                    Create.SingleQuestion(linkedToQuestionId: Id1, linkedFilter: "something")
                })
                .ExpectCritical("WB0117");

        [Test]
        public void lookup_table_has_reserved_word_in_header()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextListQuestion(Id1, variable: "q1")
            });

            Guid tableId = Guid.Parse("11111111111111111111111111111111");
            questionnaire.LookupTables.Add(tableId, Create.LookupTable("hello"));

            var lookupTableContent = Create.LookupTableContent(new[] { "int", "long"},
            Create.LookupTableRow(1, new decimal?[] { 1.15m, 10 }));
            Mock<ILookupTableService> lookupTableServiceMock = new Mock<ILookupTableService>();

            lookupTableServiceMock
                .Setup(x => x.GetLookupTableContent(questionnaire.PublicKey, tableId))
                .Returns(lookupTableContent);

            var verifier = Create.QuestionnaireVerifier(lookupTableService: lookupTableServiceMock.Object);

            var result = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

            result.Single().Code.ShouldEqual("WB0031");
        }
        
    }
}