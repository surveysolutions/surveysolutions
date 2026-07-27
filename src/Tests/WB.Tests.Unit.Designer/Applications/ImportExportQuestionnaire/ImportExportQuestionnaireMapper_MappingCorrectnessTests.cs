using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.Applications.ImportExportQuestionnaire
{
    /// <summary>
    /// Focused tests for ImportExportQuestionnaireMapper corner cases,
    /// context-passing (variable-name ↔ ID resolution), and null-safety.
    /// Assertions test the mapping output, not AutoMapper mechanics,
    /// so they remain valid when AutoMapper is removed.
    /// Only CreateMapper() needs updating after that change.
    /// </summary>
    [TestOf(typeof(ImportExportQuestionnaireMapper))]
    public class ImportExportQuestionnaireMapper_MappingCorrectnessTests
    {
        // ── Helpers ──────────────────────────────────────────────────────────────

        private static ImportExportQuestionnaireMapper CreateMapper()
        {
            return new ImportExportQuestionnaireMapper();
        }

        private static QuestionnaireDocument RoundTrip(QuestionnaireDocument doc)
        {
            var mapper = CreateMapper();
            var questionnaire = mapper.Map(doc);
            return mapper.Map(questionnaire);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Basic structure
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void Map_QuestionnaireDocument_with_single_chapter_roundtrips_correctly()
        {
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1, Create.Chapter());
            doc.Title = "Simple";
            var result = RoundTrip(doc);
            Assert.That(result.Title, Is.EqualTo("Simple"));
        }

        [Test]
        public void Map_QuestionnaireDocument_title_is_preserved()
        {
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1, Create.Chapter());
            doc.Title = "Survey 2024 — Households";
            var result = RoundTrip(doc);
            Assert.That(result.Title, Is.EqualTo("Survey 2024 — Households"));
        }

        [Test]
        public void Map_QuestionnaireDocument_HideIfDisabled_flag_roundtrips()
        {
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1, Create.Chapter());
            doc.HideIfDisabled = true;
            var result = RoundTrip(doc);
            Assert.That(result.HideIfDisabled, Is.True);
        }

        [Test]
        public void Map_QuestionnaireDocument_HideIfDisabled_false_roundtrips()
        {
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1, Create.Chapter());
            doc.HideIfDisabled = false;
            var result = RoundTrip(doc);
            Assert.That(result.HideIfDisabled, Is.False);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Variable-name ↔ ID context (the key AutoMapper context usage)
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void Map_linked_question_linked_to_is_variable_name_not_guid()
        {
            var sourceId = Id.g3;
            var linkedId = Id.g4;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextListQuestion(questionId: sourceId, variable: "source_list"),
                    Create.SingleQuestion(id: linkedId, variable: "linked_q",
                        linkedToQuestionId: sourceId)
                }));

            var mapper = CreateMapper();
            var questionnaire = mapper.Map(doc);

            var linkedQuestion = questionnaire.Children
                .SelectMany(c => c.Children)
                .OfType<WB.Core.BoundedContexts.Designer.ImportExport.Models.Question.SingleQuestion>()
                .FirstOrDefault(q => q.VariableName == "linked_q");

            Assert.That(linkedQuestion, Is.Not.Null);
            // The LinkedTo should be the variable name, not the raw GUID
            Assert.That(linkedQuestion.LinkedTo, Is.EqualTo("source_list"));
        }

        [Test]
        public void Map_linked_question_import_restores_guid_from_variable_name()
        {
            var sourceId = Id.g3;
            var linkedId = Id.g4;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextListQuestion(questionId: sourceId, variable: "source_list"),
                    Create.SingleQuestion(id: linkedId, variable: "linked_q",
                        linkedToQuestionId: sourceId)
                }));

            var result = RoundTrip(doc);

            var linkedQuestion = result.Find<Main.Core.Entities.SubEntities.Question.SingleQuestion>(linkedId);
            Assert.That(linkedQuestion.LinkedToQuestionId, Is.EqualTo(sourceId));
        }

        [Test]
        public void Map_cascading_question_CascadeFromQuestion_is_variable_name()
        {
            var parentId = Id.g3;
            var childId = Id.g4;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.SingleQuestion(id: parentId, variable: "parent_q",
                        options: new List<Answer> { Create.Answer("A", 1) }),
                    Create.SingleQuestion(id: childId, variable: "cascade_q",
                        cascadeFromQuestionId: parentId,
                        options: new List<Answer> { Create.Answer("A1", 1) })
                }));

            var mapper = CreateMapper();
            var questionnaire = mapper.Map(doc);

            var cascadeQ = questionnaire.Children
                .SelectMany(c => c.Children)
                .OfType<WB.Core.BoundedContexts.Designer.ImportExport.Models.Question.SingleQuestion>()
                .FirstOrDefault(q => q.VariableName == "cascade_q");

            Assert.That(cascadeQ, Is.Not.Null);
            Assert.That(cascadeQ.CascadeFromQuestion, Is.EqualTo("parent_q"));
        }

        [Test]
        public void Map_cascading_question_roundtrip_restores_parent_guid()
        {
            var parentId = Id.g3;
            var childId = Id.g4;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.SingleQuestion(id: parentId, variable: "parent_q",
                        options: new List<Answer> { Create.Answer("A", 1) }),
                    Create.SingleQuestion(id: childId, variable: "cascade_q",
                        cascadeFromQuestionId: parentId,
                        options: new List<Answer> { Create.Answer("A1", 1) })
                }));

            var result = RoundTrip(doc);
            var cascadeQ = result.Find<Main.Core.Entities.SubEntities.Question.SingleQuestion>(childId);
            Assert.That(cascadeQ.CascadeFromQuestionId, Is.EqualTo(parentId));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Question types — forward mapping produces correct model type
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void Map_TextQuestion_produces_correct_export_model_type()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextQuestion(questionId: qId, variable: "txt")
                }));

            var mapper = CreateMapper();
            var questionnaire = mapper.Map(doc);
            var q = questionnaire.Children.SelectMany(c => c.Children).First();

            Assert.That(q, Is.InstanceOf<WB.Core.BoundedContexts.Designer.ImportExport.Models.Question.TextQuestion>());
        }

        [Test]
        public void Map_NumericIntegerQuestion_produces_correct_export_model_type()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(id: qId, variable: "num")
                }));

            var mapper = CreateMapper();
            var questionnaire = mapper.Map(doc);
            var q = questionnaire.Children.SelectMany(c => c.Children).First();

            Assert.That(q, Is.InstanceOf<WB.Core.BoundedContexts.Designer.ImportExport.Models.Question.NumericQuestion>());
        }

        [Test]
        public void Map_NumericRealQuestion_produces_numeric_model_with_decimal_type()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.NumericRealQuestion(id: qId, variable: "dbl")
                }));

            var mapper = CreateMapper();
            var questionnaire = mapper.Map(doc);
            var q = questionnaire.Children.SelectMany(c => c.Children).First()
                as WB.Core.BoundedContexts.Designer.ImportExport.Models.Question.NumericQuestion;

            Assert.That(q, Is.Not.Null);
            Assert.That(q.IsInteger, Is.False);
        }

        [Test]
        public void Map_NumericIntegerQuestion_has_IsInteger_true()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(id: qId, variable: "num")
                }));

            var mapper = CreateMapper();
            var questionnaire = mapper.Map(doc);
            var q = questionnaire.Children.SelectMany(c => c.Children).First()
                as WB.Core.BoundedContexts.Designer.ImportExport.Models.Question.NumericQuestion;

            Assert.That(q, Is.Not.Null);
            Assert.That(q.IsInteger, Is.True);
        }

        [Test]
        public void Map_SingleOptionQuestion_produces_correct_export_model_type()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.SingleQuestion(id: qId, variable: "so",
                        options: new List<Answer> { Create.Answer("A", 1) })
                }));

            var mapper = CreateMapper();
            var questionnaire = mapper.Map(doc);
            var q = questionnaire.Children.SelectMany(c => c.Children).First();

            Assert.That(q, Is.InstanceOf<WB.Core.BoundedContexts.Designer.ImportExport.Models.Question.SingleQuestion>());
        }

        [Test]
        public void Map_MultipleOptionsQuestion_produces_correct_export_model_type()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.MultyOptionsQuestion(id: qId, variable: "mo",
                        options: new Answer[] { Create.Answer("A", 1), Create.Answer("B", 2) })
                }));

            var mapper = CreateMapper();
            var questionnaire = mapper.Map(doc);
            var q = questionnaire.Children.SelectMany(c => c.Children).First();

            Assert.That(q, Is.InstanceOf<WB.Core.BoundedContexts.Designer.ImportExport.Models.Question.MultiOptionsQuestion>());
        }

        [Test]
        public void Map_DateTimeQuestion_produces_correct_export_model_type()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.DateTimeQuestion(questionId: qId, variable: "dt")
                }));

            var mapper = CreateMapper();
            var questionnaire = mapper.Map(doc);
            var q = questionnaire.Children.SelectMany(c => c.Children).First();

            Assert.That(q, Is.InstanceOf<WB.Core.BoundedContexts.Designer.ImportExport.Models.Question.DateTimeQuestion>());
        }

        [Test]
        public void Map_GpsQuestion_produces_correct_export_model_type()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.GpsCoordinateQuestion(questionId: qId, variable: "gps")
                }));

            var mapper = CreateMapper();
            var questionnaire = mapper.Map(doc);
            var q = questionnaire.Children.SelectMany(c => c.Children).First();

            Assert.That(q, Is.InstanceOf<WB.Core.BoundedContexts.Designer.ImportExport.Models.Question.GpsCoordinateQuestion>());
        }

        [Test]
        public void Map_TextListQuestion_produces_correct_export_model_type()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextListQuestion(questionId: qId, variable: "tl")
                }));

            var mapper = CreateMapper();
            var questionnaire = mapper.Map(doc);
            var q = questionnaire.Children.SelectMany(c => c.Children).First();

            Assert.That(q, Is.InstanceOf<WB.Core.BoundedContexts.Designer.ImportExport.Models.Question.TextListQuestion>());
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Answer options roundtrip
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void Map_SingleQuestion_answer_options_roundtrip_preserves_count()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.SingleQuestion(id: qId, variable: "so",
                        options: new List<Answer>
                        {
                            Create.Answer("Option A", 1),
                            Create.Answer("Option B", 2),
                            Create.Answer("Option C", 3),
                        })
                }));

            var result = RoundTrip(doc);
            var q = result.Find<Main.Core.Entities.SubEntities.Question.SingleQuestion>(qId);
            Assert.That(q.Answers, Has.Count.EqualTo(3));
        }

        [Test]
        public void Map_SingleQuestion_answer_options_roundtrip_preserves_text()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.SingleQuestion(id: qId, variable: "so",
                        options: new List<Answer> { Create.Answer("My Answer", 7) })
                }));

            var result = RoundTrip(doc);
            var q = result.Find<Main.Core.Entities.SubEntities.Question.SingleQuestion>(qId);
            Assert.That(q.Answers[0].AnswerText, Is.EqualTo("My Answer"));
        }

        [Test]
        public void Map_SingleQuestion_answer_options_roundtrip_preserves_value()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.SingleQuestion(id: qId, variable: "so",
                        options: new List<Answer> { Create.Answer("Answer", 42) })
                }));

            var result = RoundTrip(doc);
            var q = result.Find<Main.Core.Entities.SubEntities.Question.SingleQuestion>(qId);
            Assert.That(q.Answers[0].GetParsedValue(), Is.EqualTo(42));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Validation conditions
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void Map_question_with_validation_conditions_roundtrip_preserves_count()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextQuestion(questionId: qId, variable: "txt",
                        validationConditions: new[]
                        {
                            Create.ValidationCondition("self != \"\"", "Must not be empty"),
                            Create.ValidationCondition("len(self) < 100", "Too long")
                        })
                }));

            var result = RoundTrip(doc);
            var q = result.Find<IComposite>(qId) as Main.Core.Entities.SubEntities.Question.TextQuestion;
            Assert.That(q?.ValidationConditions, Has.Count.EqualTo(2));
        }

        [Test]
        public void Map_question_with_validation_condition_roundtrip_preserves_expression()
        {
            var qId = Id.g3;
            var expression = "self != \"bad\"";
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextQuestion(questionId: qId, variable: "txt",
                        validationConditions: new[]
                        {
                            Create.ValidationCondition(expression, "Error message")
                        })
                }));

            var result = RoundTrip(doc);
            var q = result.Find<IComposite>(qId) as Main.Core.Entities.SubEntities.Question.TextQuestion;
            Assert.That(q?.ValidationConditions[0].Expression, Is.EqualTo(expression));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Roster roundtrip
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void Map_NumericRoster_roundtrip_preserves_variable_name()
        {
            var rosterId = Id.g3;
            var triggerId = Id.g4;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(id: triggerId, variable: "trig"),
                    Create.NumericRoster(rosterId: rosterId, rosterSizeQuestionId: triggerId,
                        variable: "ros1", children: new IComposite[] { Create.TextQuestion() })
                }));

            var result = RoundTrip(doc);
            var roster = result.Find<Main.Core.Entities.SubEntities.Group>(rosterId);
            Assert.That(roster.VariableName, Is.EqualTo("ros1"));
        }

        [Test]
        public void Map_FixedRoster_roundtrip_preserves_fixed_titles_count()
        {
            var rosterId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.FixedRoster(rosterId: rosterId, variable: "fix_r",
                        fixedRosterTitles: new[]
                        {
                            Create.FixedRosterTitle(1, "Row 1"),
                            Create.FixedRosterTitle(2, "Row 2"),
                            Create.FixedRosterTitle(3, "Row 3"),
                        })
                }));

            var result = RoundTrip(doc);
            Assert.That(result.Find<IComposite>(rosterId), Is.Not.Null);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Null / edge cases
        // ══════════════════════════════════════════════════════════════════════════

        [Test]
        public void Map_question_with_null_enablement_condition_roundtrips_without_error()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextQuestion(questionId: qId, variable: "txt", enablementCondition: null)
                }));

            Assert.DoesNotThrow(() => RoundTrip(doc));
        }

        [Test]
        public void Map_question_with_empty_instructions_roundtrips_correctly()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextQuestion(questionId: qId, variable: "txt", instruction: "")
                }));

            var result = RoundTrip(doc);
            var q = result.Find<IComposite>(qId) as Main.Core.Entities.SubEntities.Question.TextQuestion;
            Assert.That(q?.Instructions, Is.Null.Or.Empty);
        }

        [Test]
        public void Map_empty_questionnaire_children_roundtrips_without_error()
        {
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1, Create.Chapter());
            Assert.DoesNotThrow(() => RoundTrip(doc));
        }

        [Test]
        public void Map_questionnaire_with_multiple_chapters_roundtrip_preserves_chapter_count()
        {
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(title: "ch1"),
                Create.Chapter(title: "ch2"),
                Create.Chapter(title: "ch3"));

            var result = RoundTrip(doc);
            var nonCoverChapters = result.Children
                .OfType<Main.Core.Entities.SubEntities.Group>()
                .Where(g => g.PublicKey != doc.CoverPageSectionId);
            Assert.That(nonCoverChapters.Count(), Is.EqualTo(3));
        }

        [Test]
        public void Map_questionnaire_translation_roundtrips_correctly()
        {
            var translationId = Guid.NewGuid();
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1, Create.Chapter());
            doc.Translations = new List<WB.Core.SharedKernels.SurveySolutions.Documents.Translation>
            {
                new WB.Core.SharedKernels.SurveySolutions.Documents.Translation
                {
                    Id = translationId,
                    Name = "French"
                }
            };

            var result = RoundTrip(doc);
            Assert.That(result.Translations, Has.Exactly(1).Items);
            Assert.That(result.Translations[0].Name, Is.EqualTo("French"));
        }

        [Test]
        public void Map_question_variable_name_roundtrip_is_preserved()
        {
            var qId = Id.g3;
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextQuestion(questionId: qId, variable: "my_unique_variable")
                }));

            var result = RoundTrip(doc);
            var q = result.Find<IComposite>(qId) as Main.Core.Entities.SubEntities.AbstractQuestion;
            Assert.That(q?.VariableName, Is.EqualTo("my_unique_variable"));
        }

        [Test]
        public void Map_question_enablement_condition_roundtrip_is_preserved()
        {
            var qId = Id.g3;
            var condition = "var1 > 5";
            var doc = Create.QuestionnaireDocumentWithEmptyCoverPage(Id.g1,
                Create.Chapter(children: new IComposite[]
                {
                    Create.TextQuestion(questionId: qId, variable: "txt", enablementCondition: condition)
                }));

            var result = RoundTrip(doc);
            var q = result.Find<IComposite>(qId) as Main.Core.Entities.SubEntities.AbstractQuestion;
            Assert.That(q?.ConditionExpression, Is.EqualTo(condition));
        }
    }
}
