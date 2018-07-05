using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    internal class WarningsTests
    {
        [Test]
        public void no_shared_persons()
            => Enumerable
                .Empty<SharedPersonView>()
                .ExpectWarning("WB0227");

        [Test]
        public void shared_person()
            => new[]
                { Create.SharedPersonView() }
                .ExpectNoWarning("WB0227");

        [Test]
        public void no_current_time_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.DateTimeQuestion(),
                })
                .ExpectWarning("WB0221");

        [Test]
        public void current_time_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.DateTimeQuestion(isCurrentTime: true),
                })
                .ExpectNoWarning("WB0221");

        [Test]
        public void no_prefilled_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                })
                .ExpectWarning("WB0216");

        [Test]
        public void prefilled_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(isPrefilled: true),
                })
                .ExpectNoWarning("WB0216");

        [Test]
        public void variable_label_length_121()
            => Create
                .Question(variableLabel: new string(Enumerable.Repeat('a', 121).ToArray()))
                .ExpectWarning("WB0217");

        [Test]
        public void variable_label_length_120()
            => Create
                .Question(variableLabel: new string(Enumerable.Repeat('a', 120).ToArray()))
                .ExpectNoWarning("WB0217");

        [Test]
        public void no_gps_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                })
                .ExpectWarning("WB0211")
                .AndNoWarning("WB0264");

        [Test]
        public void gps_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.GpsCoordinateQuestion(),
                })
                .ExpectNoWarning("WB0211")
                .AndWarning("WB0264");

        [Test]
        public void no_single_option_prefilled_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                })
                .ExpectNoWarning("WB0222");

        [Test]
        public void single_option_prefilled_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.SingleOptionQuestion(isPrefilled: true),
                })
                .ExpectWarning("WB0222");

        [Test]
        public void no_barcode_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                })
                .ExpectNoWarning("WB0267");

        [Test]
        public void barcode_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.QRBarcodeQuestion(),
                })
                .ExpectWarning("WB0267");

        [Test]
        public void less_than_50_percent_questions_with_validations()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                    Create.Question(),
                    Create.Question(validationConditions: new[] { Create.ValidationCondition() }),
                })
                .ExpectWarning("WB0208");

        [Test]
        public void more_than_50_percent_questions_with_validations()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Question(),
                    Create.Question(validationConditions: new[] { Create.ValidationCondition() }),
                    Create.Question(validationConditions: new[] { Create.ValidationCondition() }),
                })
                .ExpectNoWarning("WB0208");

        [Test]
        public void more_than_30_percent_questions_are_text()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.TextQuestion(),
                    Create.TextQuestion(),
                    Create.NumericIntegerQuestion(),
                    Create.NumericIntegerQuestion(),
                })
                .ExpectWarning("WB0265");

        [Test]
        public void less_than_30_percent_questions_are_text()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.TextQuestion(),
                    Create.NumericIntegerQuestion(),
                    Create.NumericIntegerQuestion(),
                    Create.NumericIntegerQuestion(),
                })
                .ExpectNoWarning("WB0265");

        [Test]
        public void questions_with_same_title()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(title: "Question"),
                    Create.Question(title: "Question"),
                })
                .ExpectWarning("WB0266");

        [Test]
        public void questions_with_different_titles()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(title: "Question 1"),
                    Create.Question(title: "Question 2"),
                })
                .ExpectNoWarning("WB0266");

        [Test]
        public void non_consecutive_cascading_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(questionId: Guid.Parse("11111111111111111111111111111111")),
                    Create.Question(),
                    Create.SingleOptionQuestion(cascadeFromQuestionId: Guid.Parse("11111111111111111111111111111111")),
                })
                .ExpectWarning("WB0230");

        [Test]
        public void consecutive_cascading_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(questionId: Guid.Parse("11111111111111111111111111111111")),
                    Create.SingleOptionQuestion(cascadeFromQuestionId: Guid.Parse("11111111111111111111111111111111")),
                })
                .ExpectNoWarning("WB0230");

        [Test]
        public void two_single_option_non_cascading_questions()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.SingleOptionQuestion(),
                    Create.SingleOptionQuestion(),
                })
                .ExpectNoWarning("WB0230");

        [Test]
        public void cascading_questions_with_same_cascade_parent()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(questionId: Guid.Parse("11111111111111111111111111111111")),
                    Create.SingleOptionQuestion(cascadeFromQuestionId: Guid.Parse("11111111111111111111111111111111")),
                    Create.SingleOptionQuestion(cascadeFromQuestionId: Guid.Parse("11111111111111111111111111111111")),
                })
                .ExpectWarning("WB0226");

        [Test]
        public void cascading_questions_with_different_cascade_parents()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(questionId: Guid.Parse("11111111111111111111111111111111")),
                    Create.SingleOptionQuestion(questionId: Guid.Parse("22222222222222222222222222222222")),
                    Create.SingleOptionQuestion(cascadeFromQuestionId: Guid.Parse("11111111111111111111111111111111")),
                    Create.SingleOptionQuestion(cascadeFromQuestionId: Guid.Parse("22222222222222222222222222222222")),
                })
                .ExpectNoWarning("WB0226");

        [Test]
        public void static_text_validation_references_supervisor_question()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Question(variable: "x", scope: QuestionScope.Supervisor),
                    Create.StaticText(validationConditions: new[] { Create.ValidationCondition(expression: "x > 10") }),
                })
                .ExpectWarning("WB0229");

        [Test]
        public void question_validation_references_supervisor_question()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Question(variable: "x", scope: QuestionScope.Supervisor),
                    Create.Question(validationConditions: new[] { Create.ValidationCondition(expression: "x > 10") }),
                })
                .ExpectWarning("WB0229");

        [Test]
        public void question_validation_references_interviewer_question()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Question(variable: "x", scope: QuestionScope.Interviewer),
                    Create.Question(validationConditions: new[] { Create.ValidationCondition(expression: "x > 10") }),
                })
                .ExpectNoWarning("WB0229");

        [Test]
        public void two_questions_with_same_long_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: new string(Enumerable.Repeat('x', 100).ToArray())),
                    Create.Question(enablementCondition: new string(Enumerable.Repeat('x', 100).ToArray())),
                })
                .ExpectWarning("WB0235");

        [Test]
        public void one_question_with_same_long_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: new string(Enumerable.Repeat('x', 100).ToArray())),
                })
                .ExpectNoWarning("WB0235");

        [Test]
        public void two_questions_with_same_short_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: new string(Enumerable.Repeat('x', 10).ToArray())),
                    Create.Question(enablementCondition: new string(Enumerable.Repeat('x', 10).ToArray())),
                })
                .ExpectNoWarning("WB0235");

        [Test]
        public void two_questions_with_same_long_validation()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(validationConditions: new [] { Create.ValidationCondition(new string(Enumerable.Repeat('x', 100).ToArray())) }),
                    Create.Question(validationConditions: new [] { Create.ValidationCondition(new string(Enumerable.Repeat('x', 100).ToArray())) }),
                })
                .ExpectWarning("WB0236");

        [Test]
        public void question_with_two_same_long_validations()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(validationConditions: new []
                    {
                        Create.ValidationCondition(new string(Enumerable.Repeat('x', 100).ToArray())),
                        Create.ValidationCondition(new string(Enumerable.Repeat('x', 100).ToArray())),
                    }),
                })
                .ExpectNoWarning("WB0236");

        [Test]
        public void one_question_with_same_long_validation()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(validationConditions: new [] { Create.ValidationCondition(new string(Enumerable.Repeat('x', 100).ToArray())) }),
                })
                .ExpectNoWarning("WB0236");

        [Test]
        public void two_questions_with_same_short_validation()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(validationConditions: new [] { Create.ValidationCondition(new string(Enumerable.Repeat('x', 10).ToArray())) }),
                    Create.Question(validationConditions: new [] { Create.ValidationCondition(new string(Enumerable.Repeat('x', 10).ToArray())) }),
                })
                .ExpectNoWarning("WB0236");

        [Test]
        public void five_questions_with_same_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                })
                .ExpectWarning("WB0232");

        [Test]
        public void four_questions_with_same_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                })
                .ExpectNoWarning("WB0232");

        [Test]
        public void five_questions_with_same_empty_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: "   "),
                    Create.Question(enablementCondition: "   "),
                    Create.Question(enablementCondition: "   "),
                    Create.Question(enablementCondition: "   "),
                    Create.Question(enablementCondition: "   "),
                })
                .ExpectNoWarning("WB0232");

        [Test]
        public void consecutive_questions_with_same_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                })
                .ExpectWarning("WB0218");

        [Test]
        public void consecutive_questions_with_same_empty_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: "   "),
                    Create.Question(enablementCondition: "   "),
                    Create.Question(enablementCondition: "   "),
                })
                .ExpectNoWarning("WB0218");

        [Test]
        public void non_consecutive_questions_with_same_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(),
                    Create.Question(enablementCondition: "x > 10"),
                })
                .ExpectNoWarning("WB0218");

        [Test]
        public void consecutive_questions_with_different_enablements()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: "x > 10"),
                    Create.Question(enablementCondition: "y > 10"),
                    Create.Question(enablementCondition: "z > 10"),
                })
                .ExpectNoWarning("WB0218");

        [Test]
        public void consecutive_unconditional_single_option_questions_with_2_options()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 1, 2 }),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 10, 20 }),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 100, 200 }),
                })
                .ExpectWarning("WB0219");

        [Test]
        public void consecutive_unconditional_single_option_questions_with_3_options()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 1, 2, 3 }),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 10, 20, 30 }),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 100, 200, 300 }),
                })
                .ExpectNoWarning("WB0219");

        [Test]
        public void non_consecutive_unconditional_single_option_questions_with_2_options()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 1, 2 }),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 10, 20 }),
                    Create.Question(),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 100, 200 }),
                })
                .ExpectNoWarning("WB0219");

        [Test]
        public void consecutive_conditional_single_option_questions_with_2_options()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 1, 2 }),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 10, 20 }, enablementCondition: "x > 0"),
                    Create.SingleOptionQuestion(answerCodes: new decimal[] { 100, 200 }),
                })
                .ExpectNoWarning("WB0219");

        [Test]
        public void roster_inside_roster_with_same_source_question()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Roster(rosterSizeQuestionId: Guid.Parse("11111111111111111111111111111111"), children: new[]
                    {
                        Create.Roster(rosterSizeQuestionId: Guid.Parse("11111111111111111111111111111111")),
                    }),
                })
                .ExpectWarning("WB0234");

        [Test]
        public void roster_near_roster_with_same_source_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Roster(rosterSizeQuestionId: Guid.Parse("11111111111111111111111111111111")),
                    Create.Roster(rosterSizeQuestionId: Guid.Parse("11111111111111111111111111111111")),
                })
                .ExpectNoWarning("WB0234");

        [Test]
        public void roster_inside_another_roster_but_with_same_source_question()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Roster(children: new[]
                    {
                        Create.Roster(rosterSizeQuestionId: Guid.Parse("11111111111111111111111111111111")),
                    }),
                    Create.Roster(rosterSizeQuestionId: Guid.Parse("11111111111111111111111111111111")),
                })
                .ExpectNoWarning("WB0234");

        [Test]
        public void roster_inside_roster()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Roster(children: new[]
                    {
                        Create.Roster(),
                    }),
                })
                .ExpectNoWarning("WB0233");

        [Test]
        public void roster_inside_roster_inside_roster()
            => Create.QuestionnaireDocumentWithOneChapter(new[]
                {
                    Create.Roster(children: new[]
                    {
                        Create.Roster(children: new[]
                        {
                            Create.Roster(),
                        }),
                    }),
                })
                .ExpectWarning("WB0233");

        [Test]
        public void bitwise_and_operator_in_question_enablement()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Question(enablementCondition: "x & y"),
                })
                .ExpectWarning("WB0237");

        [Test]
        public void logical_and_operator_in_question_enablement()
            => Create
                .Question(enablementCondition: "x && y")
                .ExpectNoWarning("WB0237");

        [Test]
        public void bitwise_and_operator_in_string_in_question_enablement()
            => Create
                .Question(enablementCondition: "x == '&'")
                .ExpectNoWarning("WB0237");

        [Test]
        public void bitwise_and_operator_in_group_enablement()
            => Create
                .Group(enablementCondition: "x & y")
                .ExpectWarning("WB0237");

        [Test]
        public void bitwise_and_operator_in_question_validation()
            => Create
                .Question(validationConditions: new [] { Create.ValidationCondition("x & y") })
                .ExpectWarning("WB0237");

        [Test]
        public void logical_and_operator_in_question_validation()
            => Create
                .Question(validationConditions: new [] { Create.ValidationCondition("x && y") })
                .ExpectNoWarning("WB0237");

        [Test]
        public void bitwise_and_operator_in_static_text_validation()
            => Create
                .StaticText(validationConditions: new [] { Create.ValidationCondition("x & y") })
                .ExpectWarning("WB0237");

        [Test]
        public void bitwise_or_operator_in_question_enablement()
            => Create
                .Question(enablementCondition: "x | y")
                .ExpectWarning("WB0238");

        [Test]
        public void logical_or_operator_in_question_enablement()
            => Create
                .Question(enablementCondition: "x || y")
                .ExpectNoWarning("WB0238");

        [Test]
        public void bitwise_or_operator_in_string_in_question_enablement()
            => Create
                .Question(enablementCondition: "x == '|'")
                .ExpectNoWarning("WB0238");

        [Test]
        public void bitwise_or_operator_in_group_enablement()
            => Create
                .Group(enablementCondition: "x | y")
                .ExpectWarning("WB0238");

        [Test]
        public void bitwise_or_operator_in_question_validation()
            => Create
                .Question(validationConditions: new [] { Create.ValidationCondition("x | y") })
                .ExpectWarning("WB0238");

        [Test]
        public void logical_or_operator_in_question_validation()
            => Create
                .Question(validationConditions: new [] { Create.ValidationCondition("x || y") })
                .ExpectNoWarning("WB0238");

        [Test]
        public void bitwise_or_operator_in_static_text_validation()
            => Create
                .StaticText(validationConditions: new [] { Create.ValidationCondition("x | y") })
                .ExpectWarning("WB0238");

        [Test]
        public void used_rowindex_inside_multioption_based_roster()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.MultipleOptionsQuestion(questionId: Guid.Parse("11111111111111111111111111111111")),
                    Create.Roster(rosterSizeQuestionId: Guid.Parse("11111111111111111111111111111111"), rosterType: RosterSizeSourceType.Question, children: new []
                    {
                        Create.Question(enablementCondition: "@rowindex > 2"),
                    }),
                })
                .ExpectWarning("WB0220");

        [Test]
        public void used_rowcode_inside_multioption_based_roster()
            => Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.MultipleOptionsQuestion(questionId: Guid.Parse("11111111111111111111111111111111")),
                    Create.Roster(rosterSizeQuestionId: Guid.Parse("11111111111111111111111111111111"), children: new []
                    {
                        Create.Question(enablementCondition: "@rowcode > 2"),
                    }),
                })
                .ExpectNoWarning("WB0220");

        [Test]
        public void section_with_4_questions_and_other_big_section()
            => Create.QuestionnaireDocument(children: new IComposite[]
                {
                    Create.Section(children: new []
                    {
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                    }),
                    Create.Section(children: new []
                    {
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                    }),
                })
                .ExpectWarning("WB0223");

        [Test]
        public void section_with_5_questions_and_other_big_section()
            => Create.QuestionnaireDocument(children: new IComposite[]
                {
                    Create.Section(children: new []
                    {
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                    }),
                    Create.Section(children: new []
                    {
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                    }),
                })
                .ExpectNoWarning("WB0223");

        [Test]
        public void section_with_4_questions_and_no_other_sections()
            => Create.QuestionnaireDocument(children: new IComposite[]
                {
                    Create.Section(children: new []
                    {
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                    }),
                })
                .ExpectNoWarning("WB0223");

        [Test]
        public void section_with_10_subsections_at_one_level()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                })
                .ExpectWarning("WB0224");

        [Test]
        public void section_with_10_subsections_at_one_level_inside_other_subsection()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Subsection(children: new []
                    {
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                    }),
                })
                .ExpectWarning("WB0224");

        [Test]
        public void section_with_9_subsections_at_one_level_inside_other_subsection()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Subsection(children: new []
                    {
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                    }),
                })
                .ExpectNoWarning("WB0224");

        [Test]
        public void section_with_10_subsections_at_different_levels()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Subsection(children: new []
                    {
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                    }),
                    Create.Subsection(children: new []
                    {
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                        Create.Subsection(),
                    }),
                })
                .ExpectNoWarning("WB0224");

        [Test]
        public void section_with_9_subsections_at_one_level()
            => Create.QuestionnaireDocumentWithOneChapter(new []
                {
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                    Create.Subsection(),
                })
                .ExpectNoWarning("WB0224");

        [Test]
        public void multi_option_with_21_options()
            => Create
                .MultipleOptionsQuestion(answersList: Enumerable.Repeat(Create.Answer(), 21).ToList())
                .ExpectWarning("WB0231");

        [Test]
        public void multi_option_with_20_options()
            => Create
                .MultipleOptionsQuestion(answersList: Enumerable.Repeat(Create.Answer(), 20).ToList())
                .ExpectNoWarning("WB0231");

        [Test]
        public void single_option_with_9_options_in_combobox_mode()
            => Create
                .SingleOptionQuestion(
                    isComboBox: true,
                    answers: Enumerable.Repeat(Create.Answer(), 9).ToList())
                .ExpectWarning("WB0225");

        [Test]
        public void single_option_with_9_options_in_regular_mode()
            => Create
                .SingleOptionQuestion(
                    isComboBox: false,
                    answers: Enumerable.Repeat(Create.Answer(), 9).ToList())
                .ExpectNoWarning("WB0225");

        [Test]
        public void single_option_with_10_options_in_combobox_mode()
            => Create
                .SingleOptionQuestion(
                    isComboBox: true,
                    answers: Enumerable.Repeat(Create.Answer(), 10).ToList())
                .ExpectNoWarning("WB0225");

        [Test]
        public void multi_option_with_options_1_2_4_max_int()
            => Create
                .MultipleOptionsQuestion(answers: new decimal[] { 1, 2, 4, int.MaxValue })
                .ExpectWarning("WB0228");

        [Test]
        public void single_option_with_options_1_2_4_max_int()
            => Create
                .SingleOptionQuestion(answerCodes: new decimal[] { 1, 2, 4, int.MaxValue })
                .ExpectWarning("WB0228");

        [Test]
        public void single_option_with_options_1_4_5()
            => Create
                .SingleOptionQuestion(answerCodes: new decimal[] { 1, 4, 5 })
                .ExpectNoWarning("WB0228");

        [Test]
        public void single_option_with_options_1_2_3_4_5()
            => Create
                .SingleOptionQuestion(answerCodes: new decimal[] { 1, 2, 3, 4, 5 })
                .ExpectNoWarning("WB0228");
    }
}
