using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services
{
    /// <summary>
    /// Tests for WebInterviewInterviewEntityFactory.GetEntityDetails().
    /// Tests are written against behavior (field values) rather than AutoMapper internals,
    /// so they remain valid after AutoMapper is replaced with manual mapping.
    /// Only CreateWebInterviewInterviewEntityFactory() needs to change when AutoMapper is removed.
    /// </summary>
    [TestOf(typeof(WebInterviewInterviewEntityFactory))]
    public class WebInterviewInterviewEntityFactory_GetEntityDetails_Tests
    {
        // ── Helpers ─────────────────────────────────────────────────────────────

        private WebInterviewInterviewEntityFactory CreateFactory()
        {
            var navService = new Mock<IWebNavigationService>();
            navService.Setup(s => s.MakeNavigationLinks(It.IsAny<string>(), It.IsAny<Identity>(),
                    It.IsAny<IQuestionnaire>(), It.IsAny<IStatefulInterview>(), It.IsAny<string>()))
                .Returns((string text, Identity id, IQuestionnaire q, IStatefulInterview i, string dir) => text);
            return new WebInterviewInterviewEntityFactory(
                Create.Service.EnumeratorGroupGroupStateCalculationStrategy(),
                Create.Service.SupervisorGroupStateCalculationStrategy(),
                navService.Object,
                Create.Service.SubstitutionTextFactory());
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TEXT QUESTION
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_text_question_unanswered_Then_answer_is_null()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewTextQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.Null);
            Assert.That(result.IsAnswered, Is.False);
        }

        [Test]
        public void GetEntityDetails_text_question_answered_Then_answer_is_mapped()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.TextQuestionAnswered(questionId: questionId, answer: "hello"));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewTextQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.EqualTo("hello"));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void GetEntityDetails_text_question_with_mask_Then_mask_is_set()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt", mask: "##-##")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewTextQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Mask, Is.EqualTo("##-##"));
        }

        [Test]
        public void GetEntityDetails_text_question_without_mask_Then_mask_is_null()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewTextQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Mask, Is.Null.Or.Empty);
        }

        [Test]
        public void GetEntityDetails_text_question_Then_id_is_identity_string()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt", text: "My Title")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Id, Is.EqualTo(identity.ToString()));
        }

        [Test]
        public void GetEntityDetails_text_question_disabled_Then_IsDisabled_is_true()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.QuestionsDisabled(questionId));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.IsDisabled, Is.True);
        }

        [Test]
        public void GetEntityDetails_text_question_disabled_with_html_title_Then_title_is_stripped()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt", text: "<b>Bold</b>")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.QuestionsDisabled(questionId));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Title, Does.Not.Contain("<b>"));
            Assert.That(result.Title, Is.EqualTo("Bold"));
        }

        [Test]
        public void GetEntityDetails_text_question_includeVariableName_Then_Name_is_variable()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "my_var")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false, includeVariableName: true);

            Assert.That(result.Name, Is.EqualTo("my_var"));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // INTEGER QUESTION
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_integer_question_unanswered_Then_answer_is_null()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: questionId, variable: "num")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewIntegerQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.Null);
            Assert.That(result.IsAnswered, Is.False);
        }

        [Test]
        public void GetEntityDetails_integer_question_answered_Then_answer_is_mapped()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: questionId, variable: "num")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.NumericIntegerQuestionAnswered(questionId: questionId, answer: 42));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewIntegerQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.EqualTo(42));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void GetEntityDetails_integer_roster_size_question_Then_AnswerMaxValue_is_MaxRosterRowCount()
        {
            var questionId = Id.g1;
            var rosterId = Id.g2;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: questionId, variable: "rsize"),
                Create.Entity.NumericRoster(rosterId: rosterId, rosterSizeQuestionId: questionId,
                    variable: "ros", children: new IComposite[] { Create.Entity.TextQuestion() })
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewIntegerQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.IsRosterSize, Is.True);
            Assert.That(result.AnswerMaxValue, Is.EqualTo(Constants.MaxRosterRowCount));
        }

        [Test]
        public void GetEntityDetails_integer_non_roster_size_question_Then_AnswerMaxValue_is_null()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: questionId, variable: "num")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewIntegerQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.IsRosterSize, Is.False);
            Assert.That(result.AnswerMaxValue, Is.Null);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // DOUBLE QUESTION
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_double_question_unanswered_Then_answer_is_null()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericRealQuestion(id: questionId, variable: "dbl")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewDoubleQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.Null);
            Assert.That(result.IsAnswered, Is.False);
        }

        [Test]
        public void GetEntityDetails_double_question_answered_Then_answer_is_mapped()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericRealQuestion(id: questionId, variable: "dbl")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.NumericRealQuestionAnswered(identity: Identity.Create(questionId, RosterVector.Empty), answer: (decimal)3.14));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewDoubleQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.EqualTo(3.14).Within(0.001));
            Assert.That(result.IsAnswered, Is.True);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // SINGLE-OPTION QUESTION
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_single_option_question_unanswered_Then_answer_is_null()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.SingleOptionQuestion(questionId: questionId, variable: "so",
                    answers: new[] { Create.Entity.Answer("A", 1), Create.Entity.Answer("B", 2) }.ToList())
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewSingleOptionQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.Null);
            Assert.That(result.IsAnswered, Is.False);
        }

        [Test]
        public void GetEntityDetails_single_option_question_answered_Then_answer_is_selected_value()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.SingleOptionQuestion(questionId: questionId, variable: "so",
                    answers: new[] { Create.Entity.Answer("A", 1), Create.Entity.Answer("B", 2) }.ToList())
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.SingleOptionQuestionAnswered(questionId: questionId, rosterVector: RosterVector.Empty, answer: 2));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewSingleOptionQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.EqualTo(2));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void GetEntityDetails_filtered_combobox_single_option_Then_result_is_InterviewFilteredQuestion()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.SingleOptionQuestion(questionId: questionId, variable: "so",
                    isFilteredCombobox: true,
                    answers: new[] { Create.Entity.Answer("A", 1), Create.Entity.Answer("B", 2) }.ToList())
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result, Is.InstanceOf<InterviewFilteredQuestion>());
        }

        [Test]
        public void GetEntityDetails_non_combobox_single_option_Then_result_is_InterviewSingleOptionQuestion()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.SingleOptionQuestion(questionId: questionId, variable: "so",
                    answers: new[] { Create.Entity.Answer("A", 1) }.ToList())
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result, Is.InstanceOf<InterviewSingleOptionQuestion>());
        }

        // ══════════════════════════════════════════════════════════════════════════
        // MULTI-OPTION QUESTION
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_multi_option_question_unanswered_Then_answer_is_empty()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: questionId, variable: "mo",
                    textAnswers: new[] { Create.Entity.Answer("A", 1), Create.Entity.Answer("B", 2), Create.Entity.Answer("C", 3) })
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewMutliOptionQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.Empty);
            Assert.That(result.IsAnswered, Is.False);
        }

        [Test]
        public void GetEntityDetails_multi_option_question_answered_Then_answer_contains_checked_values()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: questionId, variable: "mo",
                    textAnswers: new[] { Create.Entity.Answer("A", 1), Create.Entity.Answer("B", 2), Create.Entity.Answer("C", 3) })
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.MultipleOptionsQuestionAnswered(questionId: questionId, selectedOptions: new decimal[] { 1, 3 }));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewMutliOptionQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.EquivalentTo(new[] { 1, 3 }));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void GetEntityDetails_multi_option_is_roster_size_Then_IsRosterSize_is_true()
        {
            var questionId = Id.g1;
            var rosterId = Id.g2;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: questionId, variable: "mo",
                    textAnswers: new[] { Create.Entity.Answer("A", 1), Create.Entity.Answer("B", 2) }),
                Create.Entity.NumericRoster(rosterId: rosterId, rosterSizeQuestionId: questionId,
                    variable: "ros", children: new IComposite[] { Create.Entity.TextQuestion() })
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewMutliOptionQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.IsRosterSize, Is.True);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // YES/NO QUESTION
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_yesno_question_unanswered_Then_answer_is_empty()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: questionId, variable: "yn", isYesNo: true,
                    textAnswers: new[] { Create.Entity.Answer("A", 1), Create.Entity.Answer("B", 2) })
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewYesNoQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.Empty);
            Assert.That(result.IsAnswered, Is.False);
        }

        [Test]
        public void GetEntityDetails_yesno_question_answered_Then_answer_contains_yes_no_options()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: questionId, variable: "yn", isYesNo: true,
                    textAnswers: new[] { Create.Entity.Answer("A", 1), Create.Entity.Answer("B", 2) })
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.YesNoQuestionAnswered(questionId: questionId, answeredOptions: new[]
            {
                new AnsweredYesNoOption(1, true),
                new AnsweredYesNoOption(2, false)
            }));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewYesNoQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Has.Exactly(2).Items);
            Assert.That(result.Answer.First(a => a.Value == 1).Yes, Is.True);
            Assert.That(result.Answer.First(a => a.Value == 2).Yes, Is.False);
            Assert.That(result.IsAnswered, Is.True);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // DATETIME QUESTION
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_datetime_question_unanswered_Then_answer_is_null()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.DateTimeQuestion(questionId: questionId, variable: "dt")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewDateQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.Null);
            Assert.That(result.IsAnswered, Is.False);
        }

        [Test]
        public void GetEntityDetails_datetime_question_answered_Then_answer_is_mapped()
        {
            var questionId = Id.g1;
            var expected = new DateTime(2024, 6, 15, 10, 30, 0);
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.DateTimeQuestion(questionId: questionId, variable: "dt")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.DateTimeQuestionAnswered(questionId: questionId, answer: expected));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewDateQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.EqualTo(expected));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void GetEntityDetails_timestamp_question_Then_IsTimestamp_is_true()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.DateTimeQuestion(questionId: questionId, variable: "ts", isTimestamp: true)
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewDateQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.IsTimestamp, Is.True);
        }

        [Test]
        public void GetEntityDetails_non_timestamp_question_Then_IsTimestamp_is_false()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.DateTimeQuestion(questionId: questionId, variable: "dt")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewDateQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.IsTimestamp, Is.False);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TEXT LIST QUESTION
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_textlist_question_unanswered_Then_rows_are_empty()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextListQuestion(questionId: questionId, variable: "tl")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewTextListQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Rows, Is.Empty);
            Assert.That(result.IsAnswered, Is.False);
        }

        [Test]
        public void GetEntityDetails_textlist_question_answered_Then_rows_are_mapped()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextListQuestion(questionId: questionId, variable: "tl")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.TextListQuestionAnswered(questionId: questionId,
                answers: new[] { Tuple.Create(1m, "First"), Tuple.Create(2m, "Second") }));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewTextListQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Rows, Has.Exactly(2).Items);
            Assert.That(result.Rows[0].Text, Is.EqualTo("First"));
            Assert.That(result.Rows[0].Value, Is.EqualTo(1m));
            Assert.That(result.Rows[1].Text, Is.EqualTo("Second"));
            Assert.That(result.Rows[1].Value, Is.EqualTo(2m));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void GetEntityDetails_textlist_with_max_answers_Then_MaxAnswersCount_is_set()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextListQuestion(questionId: questionId, variable: "tl", maxAnswerCount: 5)
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewTextListQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.MaxAnswersCount, Is.EqualTo(5));
        }

        [Test]
        public void GetEntityDetails_textlist_without_max_answers_Then_MaxAnswersCount_defaults_to_200()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextListQuestion(questionId: questionId, variable: "tl")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewTextListQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.MaxAnswersCount, Is.EqualTo(200));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // GPS QUESTION
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_gps_question_unanswered_Then_answer_is_null()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.GpsCoordinateQuestion(questionId: questionId, variable: "gps")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewGpsQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.Null);
        }

        [Test]
        public void GetEntityDetails_gps_question_answered_Then_lat_lon_accuracy_altitude_are_mapped()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.GpsCoordinateQuestion(questionId: questionId, variable: "gps")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.GeoLocationQuestionAnswered(
                question: Identity.Create(questionId, RosterVector.Empty),
                latitude: 10.5,
                longitude: 20.3));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewGpsQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.Not.Null);
            Assert.That(result.Answer.Latitude, Is.EqualTo(10.5).Within(0.001));
            Assert.That(result.Answer.Longitude, Is.EqualTo(20.3).Within(0.001));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // BARCODE QUESTION
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_barcode_question_unanswered_Then_answer_is_null()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.QRBarcodeQuestion(questionId: questionId, variable: "qr")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewBarcodeQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.Null);
        }

        [Test]
        public void GetEntityDetails_barcode_question_answered_Then_answer_is_mapped()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.QRBarcodeQuestion(questionId: questionId, variable: "qr")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.QRBarcodeQuestionAnswered(questionId: questionId, answer: "BARCODE-123"));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewBarcodeQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Is.EqualTo("BARCODE-123"));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // MULTIMEDIA QUESTION
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_multimedia_question_answered_Then_answer_contains_interview_and_question_ids()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.MultimediaQuestion(questionId: questionId, variable: "photo")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.PictureQuestionAnswered(questionId: questionId, answer: "photo.jpg"));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (InterviewMultimediaQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Answer, Does.Contain("interviewId="));
            Assert.That(result.Answer, Does.Contain("questionId="));
            Assert.That(result.Answer, Does.Contain("filename=photo.jpg"));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // STATIC TEXT
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_static_text_Then_result_is_InterviewStaticText()
        {
            var staticTextId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextId, text: "Hello World")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(staticTextId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result, Is.InstanceOf<InterviewStaticText>());
        }

        [Test]
        public void GetEntityDetails_static_text_Then_id_is_identity_string()
        {
            var staticTextId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextId, text: "Some text")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(staticTextId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Id, Is.EqualTo(identity.ToString()));
        }

        [Test]
        public void GetEntityDetails_static_text_disabled_Then_IsDisabled_is_true()
        {
            var staticTextId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextId)
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.StaticTextsDisabled(Identity.Create(staticTextId, RosterVector.Empty)));
            var factory = CreateFactory();

            var identity = Identity.Create(staticTextId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.IsDisabled, Is.True);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // VARIABLE
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_variable_Then_result_is_InterviewVariable()
        {
            var variableId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.Variable(id: variableId, variableName: "v1")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(variableId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result, Is.InstanceOf<InterviewVariable>());
        }

        [Test]
        public void GetEntityDetails_variable_Then_Name_is_variable_name_from_questionnaire()
        {
            var variableId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.Variable(id: variableId, variableName: "score_var")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(variableId, RosterVector.Empty);
            var result = (InterviewVariable)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Name, Is.EqualTo("score_var"));
        }

        [Test]
        public void GetEntityDetails_variable_with_computed_value_Then_Value_is_set()
        {
            var variableId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.Variable(id: variableId, variableName: "v1")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.VariablesChanged(Create.Event.ChangedVariable(
                Identity.Create(variableId, RosterVector.Empty), "computed_value")));
            var factory = CreateFactory();

            var identity = Identity.Create(variableId, RosterVector.Empty);
            var result = (InterviewVariable)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Value, Is.Not.Null.And.Not.Empty);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // GROUP / ROSTER
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_group_Then_result_is_InterviewGroupOrRosterInstance()
        {
            var sectionId = Id.g1;
            var groupId = Id.g2;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, children: new IComposite[]
            {
                Create.Entity.Group(groupId, variable: "grp")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(groupId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result, Is.InstanceOf<InterviewGroupOrRosterInstance>());
        }

        [Test]
        public void GetEntityDetails_group_Then_IsRoster_is_false()
        {
            var sectionId = Id.g1;
            var groupId = Id.g2;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, children: new IComposite[]
            {
                Create.Entity.Group(groupId, variable: "grp")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(groupId, RosterVector.Empty);
            var result = (InterviewGroupOrRosterInstance)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.IsRoster, Is.False);
        }

        [Test]
        public void GetEntityDetails_roster_instance_Then_IsRoster_is_true()
        {
            var rosterId = Id.g2;
            var triggerId = Id.g3;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: triggerId, variable: "trig"),
                Create.Entity.NumericRoster(rosterId: rosterId, rosterSizeQuestionId: triggerId,
                    variable: "r1", children: new IComposite[] { Create.Entity.TextQuestion() })
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.NumericIntegerQuestionAnswered(questionId: triggerId, answer: 1));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterId, new decimal[] { 0 }));
            var factory = CreateFactory();

            var identity = Identity.Create(rosterId, Create.RosterVector(0));
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result, Is.InstanceOf<InterviewGroupOrRosterInstance>());
            Assert.That(((InterviewGroupOrRosterInstance)result).IsRoster, Is.True);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // VALIDITY (validation messages)
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_question_declared_invalid_Then_Validity_IsValid_is_false()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt",
                    validationConditions: new[] { Create.Entity.ValidationCondition("self != \"bad\"", "Must not be bad") })
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.TextQuestionAnswered(questionId: questionId, answer: "bad"));
            interview.Apply(Create.Event.AnswersDeclaredInvalid(Identity.Create(questionId, RosterVector.Empty)));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (GenericQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Validity.IsValid, Is.False);
        }

        [Test]
        public void GetEntityDetails_question_with_no_validation_issues_Then_Validity_IsValid_is_true()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (GenericQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Validity.IsValid, Is.True);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // COMMENTS
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_question_with_comment_Then_Comments_are_populated()
        {
            var questionId = Id.g1;
            var userId = Id.gA;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            interview.Apply(Create.Event.AnswerCommented(questionId: questionId, comment: "Supervisor note"));
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (GenericQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Comments, Has.Exactly(1).Items);
            Assert.That(result.Comments[0].Text, Is.EqualTo("Supervisor note"));
        }

        [Test]
        public void GetEntityDetails_question_without_comments_Then_Comments_are_empty()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (GenericQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.Comments, Is.Empty);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // HideIfDisabled behavior on questions
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_question_with_hide_if_disabled_Then_HideIfDisabled_is_true()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt", hideIfDisabled: true)
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.HideIfDisabled, Is.True);
        }

        [Test]
        public void GetEntityDetails_question_without_hide_if_disabled_Then_HideIfDisabled_is_false()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.HideIfDisabled, Is.False);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // AcceptAnswer
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_any_question_Then_AcceptAnswer_is_true()
        {
            var questionId = Id.g1;
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: questionId, variable: "txt")
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(questionId, RosterVector.Empty);
            var result = (GenericQuestion)factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result.AcceptAnswer, Is.True);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Non-existent entity
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetEntityDetails_non_existent_identity_Then_returns_null()
        {
            var doc = Create.Entity.QuestionnaireDocumentWithOneChapter();
            var questionnaire = Create.Entity.PlainQuestionnaire(doc);
            var interview = Abc.SetUp.StatefulInterview(doc);
            var factory = CreateFactory();

            var identity = Identity.Create(Guid.NewGuid(), RosterVector.Empty);
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            Assert.That(result, Is.Null);
        }
    }
}
