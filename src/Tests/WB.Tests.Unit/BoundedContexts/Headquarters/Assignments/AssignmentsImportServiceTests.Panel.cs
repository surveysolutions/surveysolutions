using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    internal partial class AssignmentsImportServiceTests
    {
        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_preloaded_file_has_unexpected_answer_should_return_error_and_not_save_preloading_assignments()
        {
            //arrange 
            var variableOfIntegerQuestion = "num";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.NumericIntegerQuestion(variable: variableOfIntegerQuestion)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variableOfIntegerQuestion, "not integer"))});

            var importAssignmentsProcessRepository = new Mock<IPlainStorageAccessor<AssignmentsImportProcess>>();
            var importAssignmentsRepository = new Mock<IPlainStorageAccessor<AssignmentToImport>>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsProcessRepository: importAssignmentsProcessRepository.Object,
                importAssignmentsRepository: importAssignmentsRepository.Object);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] {preloadedFile}, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Has.One.Items);

            importAssignmentsProcessRepository.Verify(x => x.Store(It.IsAny<AssignmentsImportProcess>(), It.IsAny<object>()), Times.Never);
            importAssignmentsRepository.Verify(x => x.Store(It.IsAny<IEnumerable<Tuple<AssignmentToImport, object>>>()), Times.Never);
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_preloaded_file_has_error_in_roster_should_return_error_and_not_save_preloading_assignments()
        {
            //arrange 
            var variableOfIntegerQuestion = "num";
            var variableOfIntegerInRosterQuestion = "numr";
            var variableOfRoster = "r";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(variable: variableOfIntegerQuestion),
                    Create.Entity.FixedRoster(variable: variableOfRoster, fixedTitles: Create.Entity.FixedTitles(10, 20, 30),
                        children: new[] {Create.Entity.NumericIntegerQuestion(variable: variableOfIntegerInRosterQuestion) })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "2"),
                    Create.Entity.PreloadingValue(variableOfIntegerQuestion, "1"))
            });

            var rosterFile = Create.Entity.PreloadedFile(variableOfRoster, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{variableOfRoster}__id", "10"),
                    Create.Entity.PreloadingValue(variableOfIntegerInRosterQuestion, "5"))
            });

            var importAssignmentsProcessRepository = new Mock<IPlainStorageAccessor<AssignmentsImportProcess>>();
            var importAssignmentsRepository = new Mock<IPlainStorageAccessor<AssignmentToImport>>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsProcessRepository: importAssignmentsProcessRepository.Object,
                importAssignmentsRepository: importAssignmentsRepository.Object);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Has.One.Items);

            importAssignmentsProcessRepository.Verify(x => x.Store(It.IsAny<AssignmentsImportProcess>(), It.IsAny<object>()), Times.Never);
            importAssignmentsRepository.Verify(x => x.Store(It.IsAny<IEnumerable<Tuple<AssignmentToImport, object>>>()), Times.Never);
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_preloaded_file_without_answers_should_return_PL0000_error()
        {
            //arrange 
            var variableOfIntegerQuestion = "num";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.NumericIntegerQuestion(variable: variableOfIntegerQuestion)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new PreloadingRow[0]);

            var service = Create.Service.AssignmentsImportService();

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { preloadedFile }, Guid.Empty, null, questionnaire).ToArray();

            //assert
            Assert.That(errors, Has.One.Items);
            Assert.That(errors[0].Code, Is.EqualTo("PL0000"));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_preloaded_file_has_2_assignments_with_1_answer_should_return_empty_errors_and_save_2_assignments_and_specified_preloading_process()
        {
            //arrange 
            var fileName = "original.zip";
            var defaultReponsibleId = Guid.Parse("11111111111111111111111111111111");
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("22222222222222222222222222222222"), 22);
            var variableOfIntegerQuestion = "num";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(null, questionnaireIdentity.QuestionnaireId,
                    Create.Entity.NumericIntegerQuestion(variable: variableOfIntegerQuestion)), questionnaireIdentity.Version);

            var preloadedFile = Create.Entity.PreloadedFile("questionnaire", null, rows: new[]
            {
                Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variableOfIntegerQuestion, "1")),
                Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variableOfIntegerQuestion, "2"))
            });

            var importAssignmentsProcessRepository = new Mock<IPlainStorageAccessor<AssignmentsImportProcess>>();
            var importAssignmentsRepository = new Mock<IPlainStorageAccessor<AssignmentToImport>>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsProcessRepository: importAssignmentsProcessRepository.Object,
                importAssignmentsRepository: importAssignmentsRepository.Object);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors(fileName, new[] { preloadedFile }, defaultReponsibleId, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            importAssignmentsProcessRepository.Verify(x => x.Store(It.Is<AssignmentsImportProcess>(
                    y => y.AssignedTo == defaultReponsibleId &&
                         y.FileName == fileName &&
                         y.Status == AssignmentsImportProcessStatus.Verification &&
                         y.TotalCount == 2 &&
                         y.QuestionnaireId == questionnaireIdentity.ToString()),
                    It.IsAny<object>()), Times.Once);

            importAssignmentsRepository.Verify(x => x.Store(It.Is<IEnumerable<Tuple<AssignmentToImport, object>>>(y =>
                y.Count() == 2 && y.All(z => z.Item1.Answers.Count == 1))), Times.Once);
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_by_list_question_has_2_roster_instances_should_be_saved_assignemnt_with_answer_on_roster_size_question()
        {
            //arrange 
            var questionId = Guid.Parse("99999999999999999999999999999999");
            var question = "q";
            var roster = "r";
            var txtInRoster = "txt";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(questionId, variable: question),
                    Create.Entity.ListRoster(variable: roster, rosterSizeQuestionId: questionId, children: new[]
                    {
                        Create.Entity.TextQuestion(variable: txtInRoster)
                    })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingCompositeValue(question,
                        Create.Entity.PreloadingValue("0", "a"),
                        Create.Entity.PreloadingValue("1", "b"),
                        Create.Entity.PreloadingValue("2", "c")))
            });

            var rosterFile = Create.Entity.PreloadedFile(roster, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster}__id", "10"),
                    Create.Entity.PreloadingValue(question, "text1"),
                    Create.Entity.PreloadingValue(txtInRoster, "")),
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster}__id", "20"),
                    Create.Entity.PreloadingValue(question, "text2"),
                    Create.Entity.PreloadingValue(txtInRoster, ""))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] {mainFile, rosterFile}, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<TextListAnswer>());
            Assert.That(((TextListAnswer)savedAssignments[0].Answers[0].Answer).Rows,
                Is.EquivalentTo(new[]
                {
                    new TextListAnswerRow(10, "text1"),
                    new TextListAnswerRow(20, "text2")
                }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_main_file_without_roster_files_with_answer_on_list_roster_size_question_should_be_saved_assignemnt_with_answer_on_roster_size_question_from_main_file()
        {
            //arrange 
            var questionId = Guid.Parse("99999999999999999999999999999999");
            var question = "q";
            var roster = "r";
            var txtInRoster = "txt";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(questionId, variable: question),
                    Create.Entity.ListRoster(variable: roster, rosterSizeQuestionId: questionId, children: new[]
                    {
                        Create.Entity.TextQuestion(variable: txtInRoster)
                    })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingCompositeValue(question,
                        Create.Entity.PreloadingValue("0", "a"),
                        Create.Entity.PreloadingValue("1", "b"),
                        Create.Entity.PreloadingValue("2", "c")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<TextListAnswer>());
            Assert.That(((TextListAnswer)savedAssignments[0].Answers[0].Answer).Rows,
                Is.EquivalentTo(new[]
                {
                    new TextListAnswerRow(0, "a"),
                    new TextListAnswerRow(1, "b"),
                    new TextListAnswerRow(2, "c")
                }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_by_multi_question_has_2_roster_instances_should_be_saved_assignemnt_with_answer_on_roster_size_question()
        {
            //arrange 
            var questionId = Guid.Parse("99999999999999999999999999999999");
            var question = "q";
            var roster = "r";
            var txtInRoster = "txt";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.MultyOptionsQuestion(questionId, variable: question, options: Create.Entity.Options(10, 20, 30)),
                    Create.Entity.MultiRoster(variable: roster, rosterSizeQuestionId: questionId, children: new[]
                    {
                        Create.Entity.TextQuestion(variable: txtInRoster)
                    })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingCompositeValue(question,
                        Create.Entity.PreloadingValue("10", "1"),
                        Create.Entity.PreloadingValue("20", "1"),
                        Create.Entity.PreloadingValue("30", "1")))
            });

            var rosterFile = Create.Entity.PreloadedFile(roster, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster}__id", "10"),
                    Create.Entity.PreloadingValue(txtInRoster, "")),
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster}__id", "30"),
                    Create.Entity.PreloadingValue(txtInRoster, ""))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<CategoricalFixedMultiOptionAnswer>());
            Assert.That(((CategoricalFixedMultiOptionAnswer)savedAssignments[0].Answers[0].Answer).CheckedValues, Is.EquivalentTo(new[] { 10, 30 }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_main_file_without_roster_files_with_answer_on_multi_roster_size_question_should_be_saved_assignemnt_with_answer_on_roster_size_question_from_main_file()
        {
            //arrange 
            var questionId = Guid.Parse("99999999999999999999999999999999");
            var question = "q";
            var roster = "r";
            var txtInRoster = "txt";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.MultyOptionsQuestion(questionId, variable: question, options: Create.Entity.Options(10, 20, 30)),
                    Create.Entity.MultiRoster(variable: roster, rosterSizeQuestionId: questionId, children: new[]
                    {
                        Create.Entity.TextQuestion(variable: txtInRoster)
                    })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingCompositeValue(question,
                        Create.Entity.PreloadingValue("10", "1"),
                        Create.Entity.PreloadingValue("20", ""),
                        Create.Entity.PreloadingValue("30", "1")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<CategoricalFixedMultiOptionAnswer>());
            Assert.That(((CategoricalFixedMultiOptionAnswer)savedAssignments[0].Answers[0].Answer).CheckedValues, Is.EquivalentTo(new[] { 10, 30 }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_by_numeric_question_has_2_roster_instances_should_be_saved_assignemnt_with_answer_on_roster_size_question()
        {
            //arrange 
            var questionId = Guid.Parse("99999999999999999999999999999999");
            var question = "q";
            var roster = "r";
            var txtInRoster = "txt";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(questionId, variable: question),
                    Create.Entity.NumericRoster(variable: roster, rosterSizeQuestionId: questionId, children: new[]
                    {
                        Create.Entity.TextQuestion(variable: txtInRoster)
                    })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(question, "10"))
            });

            var rosterFile = Create.Entity.PreloadedFile(roster, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster}__id", "1"),
                    Create.Entity.PreloadingValue(txtInRoster, "")),
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster}__id", "2"),
                    Create.Entity.PreloadingValue(txtInRoster, ""))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<NumericIntegerAnswer>());
            Assert.That(((NumericIntegerAnswer)savedAssignments[0].Answers[0].Answer).Value, Is.EqualTo(2));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_main_file_without_roster_files_with_answer_on_numeric_roster_size_question_should_be_saved_assignemnt_with_answer_on_roster_size_question_from_main_file()
        {
            //arrange 
            var questionId = Guid.Parse("99999999999999999999999999999999");
            var question = "q";
            var roster = "r";
            var txtInRoster = "txt";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(questionId, variable: question),
                    Create.Entity.NumericRoster(variable: roster, rosterSizeQuestionId: questionId, children: new[]
                    {
                        Create.Entity.TextQuestion(variable: txtInRoster)
                    })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(question, "10"))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<NumericIntegerAnswer>());
            Assert.That(((NumericIntegerAnswer)savedAssignments[0].Answers[0].Answer).Value, Is.EqualTo(10));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_integer_question_should_be_saved_assignemnt_with_specified_integer_answer()
        {
            //arrange 
            var integerQuestionId = Guid.Parse("11111111111111111111111111111111");
            var integerQuestion = "qInt";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.NumericIntegerQuestion(integerQuestionId, integerQuestion)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1"))});

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
                {Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"), 
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"), 
                    Create.Entity.PreloadingValue(integerQuestion, "555"))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<NumericIntegerAnswer>(savedAssignments[0], Identity.Create(integerQuestionId, Create.RosterVector(1))).Value, Is.EqualTo(555));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_double_question_should_be_saved_assignemnt_with_specified_double_answer()
        {
            //arrange 
            var doubleQuestionId = Guid.Parse("22222222222222222222222222222222");
            var doubleQuestion = "doubl";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.NumericRealQuestion(doubleQuestionId, doubleQuestion)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(doubleQuestion, "777.777"))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<NumericRealAnswer>(savedAssignments[0], Identity.Create(doubleQuestionId, Create.RosterVector(1))).Value, Is.EqualTo(777.777));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_text_question_should_be_saved_assignemnt_with_specified_text_answer()
        {
            //arrange 
            var textQuestionId = Guid.Parse("33333333333333333333333333333333");
            var textQuestion = "text";
            var answer = "some text";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.TextQuestion(textQuestionId, variable: textQuestion)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(textQuestion, answer))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<TextAnswer>(savedAssignments[0], Identity.Create(textQuestionId, Create.RosterVector(1))).Value, Is.EqualTo(answer));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_qrbarcode_question_should_be_saved_assignemnt_with_specified_qrbarcode_answer()
        {
            //arrange 
            var qrBarcodeQuestionId = Guid.Parse("44444444444444444444444444444444");
            var qrBarcodeQuestion = "qr";
            var answer = "scanned text";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.QRBarcodeQuestion(qrBarcodeQuestionId, variable: qrBarcodeQuestion)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(qrBarcodeQuestion, answer))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<QRBarcodeAnswer>(savedAssignments[0],
                Identity.Create(qrBarcodeQuestionId, Create.RosterVector(1))).DecodedText, Is.EqualTo(answer));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_datetime_question_should_be_saved_assignemnt_with_specified_datetime_answer()
        {
            //arrange 
            var dateTimeQuestionId = Guid.Parse("55555555555555555555555555555555");
            var dateTimeQuestion = "dt";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.DateTimeQuestion(dateTimeQuestionId, variable: dateTimeQuestion)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(dateTimeQuestion, "2008-09-22 14:01:54Z"))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<DateTimeAnswer>(savedAssignments[0],
                Identity.Create(dateTimeQuestionId, Create.RosterVector(1))).Value, Is.EqualTo(new DateTime(2008, 9, 22, 14, 1, 54, DateTimeKind.Utc)));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_datetime_question_in_user_defined_format_should_be_saved_assignemnt_with_specified_datetime_answer()
        {
            //arrange 
            var dateTimeQuestionId = Guid.Parse("55555555555555555555555555555555");
            var dateTimeQuestion = "dt";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.DateTimeQuestion(dateTimeQuestionId, variable: dateTimeQuestion)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(dateTimeQuestion, "02/22/18 11:35 AM"))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<DateTimeAnswer>(savedAssignments[0],
                Identity.Create(dateTimeQuestionId, Create.RosterVector(1))).Value, Is.EqualTo(new DateTime(2018, 2, 22, 11, 35, 0, DateTimeKind.Utc)));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_list_question_should_be_saved_assignemnt_with_specified_list_answers()
        {
            //arrange 
            var listQuestionId = Guid.Parse("66666666666666666666666666666666");
            var listQuestion = "list";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.TextListQuestion(listQuestionId, variable: listQuestion)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingCompositeValue(listQuestion,
                        Create.Entity.PreloadingValue("0", "john"),
                        Create.Entity.PreloadingValue("1", "jack"),
                        Create.Entity.PreloadingValue("2", "mike")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<TextListAnswer>(savedAssignments[0],
                Identity.Create(listQuestionId, Create.RosterVector(1))).Rows, Is.EquivalentTo(new[]
            {
                new TextListAnswerRow(0, "john"),
                new TextListAnswerRow(1, "jack"),
                new TextListAnswerRow(2, "mike")
            }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_gps_question_should_be_saved_assignemnt_with_specified_gps_answer()
        {
            //arrange 
            var gpsQuestionId = Guid.Parse("77777777777777777777777777777777");
            var gpsQuestion = "gps";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.GpsCoordinateQuestion(gpsQuestionId, gpsQuestion)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingCompositeValue(gpsQuestion,
                        Create.Entity.PreloadingValue("latitude", "90"),
                        Create.Entity.PreloadingValue("longitude", "180"),
                        Create.Entity.PreloadingValue("altitude", "100"),
                        Create.Entity.PreloadingValue("accuracy", "12"),
                        Create.Entity.PreloadingValue("timestamp", "2019-09-22 12:11:10")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);

            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<GpsAnswer>(savedAssignments[0],
                Identity.Create(gpsQuestionId, Create.RosterVector(1))).Value, Is.EqualTo(new GeoPosition(90, 180, 12, 100, new DateTime(2019, 9, 22, 12, 11, 10))));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_yesno_question_should_be_saved_assignemnt_with_specified_yesno_answers()
        {
            //arrange 
            var yesNoQuestionId = Guid.Parse("88888888888888888888888888888888");
            var yesNoQuestion = "yesno";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.YesNoQuestion(yesNoQuestionId, variable: yesNoQuestion,
                                answers: new[] {5, 6, 7})
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingCompositeValue(yesNoQuestion,
                        Create.Entity.PreloadingValue("5", "1"),
                        Create.Entity.PreloadingValue("6", ""),
                        Create.Entity.PreloadingValue("7", "0")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<YesNoAnswer>(savedAssignments[0],
                Identity.Create(yesNoQuestionId, Create.RosterVector(1))).CheckedOptions, Is.EquivalentTo(new[]
            {
                new CheckedYesNoAnswerOption(5, true),
                new CheckedYesNoAnswerOption(7, false)
            }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_ordered_yesno_question_should_be_saved_assignemnt_with_specified_ordered_yesno_answers()
        {
            //arrange 
            var yesNoQuestionId = Guid.Parse("88888888888888888888888888888888");
            var yesNoQuestion = "yesno";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.YesNoQuestion(yesNoQuestionId, variable: yesNoQuestion, ordered: true,
                                answers: new[] {5, 6, 7, 8, 9})
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingCompositeValue(yesNoQuestion,
                        Create.Entity.PreloadingValue("5", "3"),
                        Create.Entity.PreloadingValue("6", ""),
                        Create.Entity.PreloadingValue("7", "1"),
                        Create.Entity.PreloadingValue("8", "2"),
                        Create.Entity.PreloadingValue("9", "0")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<YesNoAnswer>(savedAssignments[0],
                Identity.Create(yesNoQuestionId, Create.RosterVector(1))).CheckedOptions, Is.EqualTo(new[]
            {
                new CheckedYesNoAnswerOption(9, false),
                new CheckedYesNoAnswerOption(7, true),
                new CheckedYesNoAnswerOption(8, true),
                new CheckedYesNoAnswerOption(5, true),
            }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_categorical_single_question_should_be_saved_assignemnt_with_specified_categorical_single_answer()
        {
            //arrange 
            var categoricalSingleQuestionId = Guid.Parse("10101010101010101010101010101010");
            var categoricalSingleQuestion = "single";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.SingleOptionQuestion(categoricalSingleQuestionId, categoricalSingleQuestion,
                                answerCodes: new[] {9m, 10m, 11m})
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(categoricalSingleQuestion, "10"))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<CategoricalFixedSingleOptionAnswer>(savedAssignments[0],
                Identity.Create(categoricalSingleQuestionId, Create.RosterVector(1))).SelectedValue, Is.EqualTo(10));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_categorical_multi_question_should_be_saved_assignemnt_with_specified_categorical_multi_answers()
        {
            //arrange 
            var categoricalMultiQuestionId = Guid.Parse("99999999999999999999999999999999");
            var categoricalMultiQuestion = "multi";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.MultyOptionsQuestion(categoricalMultiQuestionId,
                                variable: categoricalMultiQuestion,
                                options: Create.Entity.Options(11, 12, 13))
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingCompositeValue(categoricalMultiQuestion,
                        Create.Entity.PreloadingValue("11", "1"),
                        Create.Entity.PreloadingValue("12", "0"),
                        Create.Entity.PreloadingValue("13", "1")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<CategoricalFixedMultiOptionAnswer>(savedAssignments[0],
                Identity.Create(categoricalMultiQuestionId, Create.RosterVector(1))).CheckedValues, Is.EquivalentTo(new[] { 11, 13 }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_ordered_categorical_multi_question_should_be_saved_assignemnt_with_specified_ordered_categorical_multi_answers()
        {
            //arrange 
            var categoricalMultiQuestionId = Guid.Parse("99999999999999999999999999999999");
            var categoricalMultiQuestion = "multi";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.MultyOptionsQuestion(categoricalMultiQuestionId,
                                variable: categoricalMultiQuestion, areAnswersOrdered: true,
                                options: Create.Entity.Options(11, 12, 13))
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingCompositeValue(categoricalMultiQuestion,
                        Create.Entity.PreloadingValue("11", "2"),
                        Create.Entity.PreloadingValue("12", "3"),
                        Create.Entity.PreloadingValue("13", "1")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.Exactly(2).Items);
            Assert.That(GetAnswer<CategoricalFixedMultiOptionAnswer>(savedAssignments[0],
                Identity.Create(categoricalMultiQuestionId, Create.RosterVector(1))).CheckedValues, Is.EquivalentTo(new[] { 13, 11, 12 }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_empty_answer_on_integer_question_should_be_saved_assignemnt_with_roster_size_answer_only()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var rosterSizeQuestion = "list";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId, variable: rosterSizeQuestion),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.NumericIntegerQuestion(questionId, variable)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(rosterSizeQuestion, "item1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(variable, ""))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(GetAnswer<TextListAnswer>(savedAssignments[0],
                Identity.Create(listRosterId, RosterVector.Empty)).Rows, Is.EquivalentTo(new[] { new TextListAnswerRow(1, "item1") }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_empty_answer_on_double_question_should_be_saved_assignemnt_with_roster_size_answer_only()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var rosterSizeQuestion = "list";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId, variable: rosterSizeQuestion),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.NumericRealQuestion(questionId, variable)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(rosterSizeQuestion, "item1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(variable, ""))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(GetAnswer<TextListAnswer>(savedAssignments[0],
                Identity.Create(listRosterId, RosterVector.Empty)).Rows, Is.EquivalentTo(new[] { new TextListAnswerRow(1, "item1") }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_empty_answer_on_qrbarcode_question_should_be_saved_assignemnt_with_roster_size_answer_only()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var rosterSizeQuestion = "list";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId, variable: rosterSizeQuestion),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.QRBarcodeQuestion(questionId, variable: variable)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(rosterSizeQuestion, "item1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(variable, ""))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(GetAnswer<TextListAnswer>(savedAssignments[0],
                Identity.Create(listRosterId, RosterVector.Empty)).Rows, Is.EquivalentTo(new[] { new TextListAnswerRow(1, "item1") }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_empty_answer_on_categorical_signle_question_should_be_saved_assignemnt_with_roster_size_answer_only()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var rosterSizeQuestion = "list";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId, variable: rosterSizeQuestion),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.SingleOptionQuestion(questionId, variable: variable,
                                answerCodes: new[] {1m, 2m, 3m})
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(rosterSizeQuestion, "item1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(variable, ""))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(GetAnswer<TextListAnswer>(savedAssignments[0],
                Identity.Create(listRosterId, RosterVector.Empty)).Rows, Is.EquivalentTo(new[] { new TextListAnswerRow(1, "item1") }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_empty_answer_on_datetime_question_should_be_saved_assignemnt_with_roster_size_answer_only()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var rosterSizeQuestion = "list";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId, variable: rosterSizeQuestion),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId, children: new[] {
                    Create.Entity.DateTimeQuestion(questionId, variable: variable)
                    })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(rosterSizeQuestion, "item1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(variable, ""))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(GetAnswer<TextListAnswer>(savedAssignments[0],
                Identity.Create(listRosterId, RosterVector.Empty)).Rows, Is.EquivalentTo(new[] { new TextListAnswerRow(1, "item1") }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_empty_answer_on_gps_question_should_be_saved_assignemnt_with_roster_size_answer_only()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var rosterSizeQuestion = "list";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId, variable: rosterSizeQuestion),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.GpsCoordinateQuestion(questionId, variable)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(rosterSizeQuestion, "item1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingCompositeValue(variable,
                        Create.Entity.PreloadingValue("latitude", ""),
                        Create.Entity.PreloadingValue("longitude", ""),
                        Create.Entity.PreloadingValue("altitude", ""),
                        Create.Entity.PreloadingValue("accuracy", ""),
                        Create.Entity.PreloadingValue("timestamp", "")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(GetAnswer<TextListAnswer>(savedAssignments[0],
                Identity.Create(listRosterId, RosterVector.Empty)).Rows, Is.EquivalentTo(new[] { new TextListAnswerRow(1, "item1") }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_empty_answer_on_list_question_should_be_saved_assignemnt_with_roster_size_answer_only()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var rosterSizeQuestion = "list";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId, variable: rosterSizeQuestion),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.TextListQuestion(questionId, variable: variable)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(rosterSizeQuestion, "item1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingCompositeValue(variable,
                        Create.Entity.PreloadingValue("0", ""),
                        Create.Entity.PreloadingValue("1", ""),
                        Create.Entity.PreloadingValue("2", "")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(GetAnswer<TextListAnswer>(savedAssignments[0],
                Identity.Create(listRosterId, RosterVector.Empty)).Rows, Is.EquivalentTo(new[] { new TextListAnswerRow(1, "item1") }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_empty_answer_on_yesno_question_should_be_saved_assignemnt_with_roster_size_answer_only()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var rosterSizeQuestion = "list";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId, variable: rosterSizeQuestion),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.YesNoQuestion(questionId, variable: variable, answers: new[] {5, 6, 7})
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(rosterSizeQuestion, "item1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingCompositeValue(variable,
                        Create.Entity.PreloadingValue("5", ""),
                        Create.Entity.PreloadingValue("6", ""),
                        Create.Entity.PreloadingValue("7", "")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(GetAnswer<TextListAnswer>(savedAssignments[0],
                Identity.Create(listRosterId, RosterVector.Empty)).Rows, Is.EquivalentTo(new[] { new TextListAnswerRow(1, "item1") }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_empty_answer_on_categorical_multi_question_should_be_saved_assignemnt_with_roster_size_answer_only()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var rosterSizeQuestion = "list";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId, variable: rosterSizeQuestion),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.MultyOptionsQuestion(questionId, variable: variable,
                                options: Create.Entity.Options(11, 12, 13))
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(rosterSizeQuestion, "item1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingCompositeValue(variable,
                        Create.Entity.PreloadingValue("11", ""),
                        Create.Entity.PreloadingValue("12", ""),
                        Create.Entity.PreloadingValue("13", "")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(GetAnswer<TextListAnswer>(savedAssignments[0],
                Identity.Create(listRosterId, RosterVector.Empty)).Rows, Is.EquivalentTo(new[] { new TextListAnswerRow(1, "item1") }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_linked_to_question_categorical_multi_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.MultyOptionsQuestion(questionId, variable: variable,
                                linkedToQuestionId: Guid.NewGuid(),
                                options: Create.Entity.Options(11, 12, 13))
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingCompositeValue(variable,
                        Create.Entity.PreloadingValue("11", "1")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_linked_to_roster_categorical_multi_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.MultyOptionsQuestion(questionId, variable: variable,
                                linkedToRosterId: Guid.NewGuid(),
                                options: Create.Entity.Options(11, 12, 13))
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingCompositeValue(variable,
                        Create.Entity.PreloadingValue("11", "1")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_linked_to_question_categorical_signle_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.SingleOptionQuestion(questionId, variable: variable,
                                linkedToQuestionId: Guid.NewGuid(),
                                answerCodes: new[] {1m, 2m, 3m})
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(variable, "2"))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_answer_on_linked_to_roster_categorical_signle_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.SingleOptionQuestion(questionId, variable: variable,
                                linkedToRosterId: Guid.NewGuid(),
                                answerCodes: new[] {1m, 2m, 3m})
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(variable, "3"))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_has_empty_answer_on_text_question_should_be_saved_assignemnt_with_roster_size_answer_only()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var rosterVariable = "r";
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            var rosterSizeQuestion = "list";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listRosterId, variable: rosterSizeQuestion),
                    Create.Entity.ListRoster(variable: rosterVariable, rosterSizeQuestionId: listRosterId,
                        children: new[]
                        {
                            Create.Entity.TextQuestion(questionId, variable: variable)
                        })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[] { Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("interview__id", "1")) });

            var rosterFile = Create.Entity.PreloadedFile(rosterVariable, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(rosterSizeQuestion, "item1"),
                    Create.Entity.PreloadingValue($"{rosterVariable}__id", "1"),
                    Create.Entity.PreloadingValue(variable, ""))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(GetAnswer<TextListAnswer>(savedAssignments[0],
                Identity.Create(listRosterId, RosterVector.Empty)).Rows, Is.EquivalentTo(new[] { new TextListAnswerRow(1, "item1") }));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_numeric_roster_size_trigger_roster_and_nested_roster_should_be_saved_assignemnt_with_answers_in_roster_and_nested_roster()
        {
            //arrange 
            var rosterSizeQuestionId = Guid.Parse("99999999999999999999999999999999");
            var txtInRosterId = Guid.Parse("11111111111111111111111111111111");
            var txtInNestedRosterId = Guid.Parse("22222222222222222222222222222222");
            var rosterSizeQuestion = "q";
            var roster = "r";
            var nestedroster = "nr";
            var txtInRoster = "txt";
            var txtInNestedRoster = "ntxt";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(rosterSizeQuestionId, rosterSizeQuestion),
                    Create.Entity.NumericRoster(variable: roster, rosterSizeQuestionId: rosterSizeQuestionId, children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(txtInRosterId, variable: txtInRoster),
                        Create.Entity.NumericRoster(variable: nestedroster, rosterSizeQuestionId: rosterSizeQuestionId, children: new[]
                        {
                            Create.Entity.TextQuestion(txtInNestedRosterId, variable: txtInNestedRoster)
                        })
                    })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(rosterSizeQuestion, "3"))
            });

            var rosterFile = Create.Entity.PreloadedFile(roster, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster}__id", "1"),
                    Create.Entity.PreloadingValue(txtInRoster, "text 1")),
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster}__id", "2"),
                    Create.Entity.PreloadingValue(txtInRoster, "text 2"))
            });

            var nestedRosterFile = Create.Entity.PreloadedFile(nestedroster, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster}__id", "1"),
                    Create.Entity.PreloadingValue($"{nestedroster}__id", "1"),
                    Create.Entity.PreloadingValue(txtInNestedRoster, "text 3")),
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster}__id", "1"),
                    Create.Entity.PreloadingValue($"{nestedroster}__id", "2"),
                    Create.Entity.PreloadingValue(txtInNestedRoster, "text 4")),
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster}__id", "2"),
                    Create.Entity.PreloadingValue($"{nestedroster}__id", "1"),
                    Create.Entity.PreloadingValue(txtInNestedRoster, "text 5")),
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster}__id", "2"),
                    Create.Entity.PreloadingValue($"{nestedroster}__id", "2"),
                    Create.Entity.PreloadingValue(txtInNestedRoster, "text 6")),
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile, nestedRosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.Exactly(7).Items);
            Assert.That(GetAnswer<NumericIntegerAnswer>(savedAssignments[0], Identity.Create(rosterSizeQuestionId, RosterVector.Empty)).Value, Is.EqualTo(2));
            Assert.That(GetAnswer<TextAnswer>(savedAssignments[0], Identity.Create(txtInRosterId, Create.RosterVector(0))).Value, Is.EqualTo("text 1"));
            Assert.That(GetAnswer<TextAnswer>(savedAssignments[0], Identity.Create(txtInRosterId, Create.RosterVector(1))).Value, Is.EqualTo("text 2"));
            Assert.That(GetAnswer<TextAnswer>(savedAssignments[0], Identity.Create(txtInNestedRosterId, Create.RosterVector(0, 0))).Value, Is.EqualTo("text 3"));
            Assert.That(GetAnswer<TextAnswer>(savedAssignments[0], Identity.Create(txtInNestedRosterId, Create.RosterVector(0, 1))).Value, Is.EqualTo("text 4"));
            Assert.That(GetAnswer<TextAnswer>(savedAssignments[0], Identity.Create(txtInNestedRosterId, Create.RosterVector(1, 0))).Value, Is.EqualTo("text 5"));
            Assert.That(GetAnswer<TextAnswer>(savedAssignments[0], Identity.Create(txtInNestedRosterId, Create.RosterVector(1, 1))).Value, Is.EqualTo("text 6"));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_with_protected_variables_should_be_saved_assignemnt_with_specified_protected_variables()
        {
            //arrange 
            var numQuestion = "num";
            var multiQuestion = "multi";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(variable: numQuestion),
                    Create.Entity.MultyOptionsQuestion(variable: multiQuestion,
                        options: Create.Entity.Options(1, 2, 3))));

            var mainFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue(numQuestion, "1"),
                    Create.Entity.PreloadingCompositeValue(multiQuestion, Create.Entity.PreloadingValue("3", "1")))
            });

            var protectedVariablesFile = Create.Entity.PreloadedFile("protected__variables", rows: new[]
            {
                Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("variable__name", numQuestion)),
                Create.Entity.PreloadingRow(Create.Entity.PreloadingValue("variable__name", multiQuestion))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] {mainFile}, Guid.Empty,
                protectedVariablesFile, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].ProtectedVariables, Is.EquivalentTo(new[] {numQuestion, multiQuestion}));
        }

        [Test]
        public void when_VerifyPanelAndSaveIfNoErrors_and_roster_file_by_numeric_question_which_trigger_2_rosters_has_2_roster_instances_should_be_saved_assignemnt_with_answer_on_roster_size_question()
        {
            //arrange 
            var rosterSizeQuestionId = Guid.Parse("99999999999999999999999999999999");
            var rosterSizeQuestion = "q";
            var roster1 = "r1";
            var txtInRoster1 = "txt1";
            var roster2 = "r2";
            var txtInRoster2 = "txt2";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(rosterSizeQuestionId, variable: rosterSizeQuestion),
                    Create.Entity.NumericRoster(variable: roster1, rosterSizeQuestionId: rosterSizeQuestionId, children: new[]
                    {
                        Create.Entity.TextQuestion(variable: txtInRoster1)
                    }),
                    Create.Entity.NumericRoster(variable: roster2, rosterSizeQuestionId: rosterSizeQuestionId, children: new[]
                    {
                        Create.Entity.TextQuestion(variable: txtInRoster2)
                    })));

            var mainFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue(rosterSizeQuestion, "10"))
            });

            var rosterFile = Create.Entity.PreloadedFile(roster1, rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster1}__id", "1"),
                    Create.Entity.PreloadingValue(txtInRoster1, "")),
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("interview__id", "1"),
                    Create.Entity.PreloadingValue($"{roster1}__id", "2"),
                    Create.Entity.PreloadingValue(txtInRoster1, ""))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifyPanelAndSaveIfNoErrors("original.zip", new[] { mainFile, rosterFile }, Guid.Empty, null, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<NumericIntegerAnswer>());
            Assert.That(((NumericIntegerAnswer)savedAssignments[0].Answers[0].Answer).Value, Is.EqualTo(2));
        }

        private static T GetAnswer<T>(AssignmentToImport assignment, Identity questionIdentity) where T : AbstractAnswer
            => (T)assignment.Answers.FirstOrDefault(x => x.Identity == questionIdentity)?.Answer;
    }
}
