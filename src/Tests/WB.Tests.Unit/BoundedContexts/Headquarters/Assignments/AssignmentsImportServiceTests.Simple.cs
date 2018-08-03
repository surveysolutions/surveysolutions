using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(AssignmentsImportService))]
    internal partial class AssignmentsImportServiceTests
    {
        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_unexpected_answer_should_return_error_and_not_save_preloading_assignments()
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
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Has.One.Items);

            importAssignmentsProcessRepository.Verify(x => x.Store(It.IsAny<AssignmentsImportProcess>(), It.IsAny<object>()), Times.Never);
            importAssignmentsRepository.Verify(x => x.Store(It.IsAny<IEnumerable<Tuple<AssignmentToImport, object>>>()), Times.Never);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_without_answers_should_return_PL0000_error()
        {
            //arrange 
            var variableOfIntegerQuestion = "num";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.NumericIntegerQuestion(variable: variableOfIntegerQuestion)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new PreloadingRow[0]);

            var service = Create.Service.AssignmentsImportService();

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire).ToArray();

            //assert
            Assert.That(errors, Has.One.Items);
            Assert.That(errors[0].Code, Is.EqualTo("PL0000"));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_2_assignments_with_1_answer_should_return_empty_errors_and_save_2_assignments_and_specified_preloading_process()
        {
            //arrange 
            var fileName = "main.tab";
            var defaultReponsibleId = Guid.Parse("11111111111111111111111111111111");
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("22222222222222222222222222222222"), 22);
            var variableOfIntegerQuestion = "num";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(null, questionnaireIdentity.QuestionnaireId,
                    Create.Entity.NumericIntegerQuestion(variable: variableOfIntegerQuestion)), questionnaireIdentity.Version);

            var preloadedFile = Create.Entity.PreloadedFile(fileName, null, rows: new[]
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
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, defaultReponsibleId, questionnaire);

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
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_integer_question_should_be_saved_assignemnt_with_specified_integer_answer()
        {
            //arrange 
            var integerQuestionId =  Guid.Parse("11111111111111111111111111111111");
            var integerQuestion = "qInt";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(integerQuestionId, integerQuestion)
                ));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(integerQuestion, "555"))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<NumericIntegerAnswer>());
            Assert.That(((NumericIntegerAnswer)savedAssignments[0].Answers[0].Answer).Value, Is.EqualTo(555));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_double_question_should_be_saved_assignemnt_with_specified_double_answer()
        {
            //arrange 
            var doubleQuestionId = Guid.Parse("22222222222222222222222222222222");
            var doubleQuestion = "doubl";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericRealQuestion(doubleQuestionId, doubleQuestion)
                ));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(doubleQuestion, "777.777"))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<NumericRealAnswer>());
            Assert.That(((NumericRealAnswer)savedAssignments[0].Answers[0].Answer).Value, Is.EqualTo(777.777));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_text_question_should_be_saved_assignemnt_with_specified_text_answer()
        {
            //arrange 
            var textQuestionId = Guid.Parse("33333333333333333333333333333333");
            var textQuestion = "text";
            var answer = "some text";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextQuestion(textQuestionId, variable: textQuestion)
                ));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(textQuestion, answer))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<TextAnswer>());
            Assert.That(((TextAnswer)savedAssignments[0].Answers[0].Answer).Value, Is.EqualTo(answer));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_qrbarcode_question_should_be_saved_assignemnt_with_specified_qrbarcode_answer()
        {
            //arrange 
            var qrBarcodeQuestionId = Guid.Parse("44444444444444444444444444444444");
            var qrBarcodeQuestion = "qr";
            var answer = "scanned text";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.QRBarcodeQuestion(qrBarcodeQuestionId, variable: qrBarcodeQuestion)
                ));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(qrBarcodeQuestion, answer))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<QRBarcodeAnswer>());
            Assert.That(((QRBarcodeAnswer)savedAssignments[0].Answers[0].Answer).DecodedText, Is.EqualTo(answer));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_datetime_question_should_be_saved_assignemnt_with_specified_datetime_answer()
        {
            //arrange 
            var dateTimeQuestionId = Guid.Parse("55555555555555555555555555555555");
            var dateTimeQuestion = "dt";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.DateTimeQuestion(dateTimeQuestionId, variable: dateTimeQuestion)
                ));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(dateTimeQuestion, "2008-09-22 14:01:54Z"))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<DateTimeAnswer>());
            Assert.That(((DateTimeAnswer)savedAssignments[0].Answers[0].Answer).Value, Is.EqualTo(new DateTime(2008, 9, 22, 14, 1, 54, DateTimeKind.Utc)));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_datetime_question_in_user_defined_format_should_be_saved_assignemnt_with_specified_datetime_answer()
        {
            //arrange 
            var dateTimeQuestionId = Guid.Parse("55555555555555555555555555555555");
            var dateTimeQuestion = "dt";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.DateTimeQuestion(dateTimeQuestionId, variable: dateTimeQuestion)
                ));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(dateTimeQuestion, "02/22/18 11:35 AM"))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<DateTimeAnswer>());
            Assert.That(((DateTimeAnswer)savedAssignments[0].Answers[0].Answer).Value, Is.EqualTo(new DateTime(2018, 2, 22, 11, 35, 0, DateTimeKind.Utc)));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_list_question_should_be_saved_assignemnt_with_specified_list_answers()
        {
            //arrange 
            var listQuestionId = Guid.Parse("66666666666666666666666666666666");
            var listQuestion = "list";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(listQuestionId, variable: listQuestion)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(Create.Entity.PreloadingCompositeValue(listQuestion,
                    Create.Entity.PreloadingValue("0", "john"), 
                    Create.Entity.PreloadingValue("1", "jack"),
                    Create.Entity.PreloadingValue("2", "mike")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<TextListAnswer>());
            Assert.That(((TextListAnswer) savedAssignments[0].Answers[0].Answer).Rows,
                Is.EquivalentTo(new[]
                {
                    new TextListAnswerRow(0, "john"),
                    new TextListAnswerRow(1, "jack"),
                    new TextListAnswerRow(2, "mike")
                }));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_gps_question_should_be_saved_assignemnt_with_specified_gps_answer()
        {
            //arrange 
            var gpsQuestionId = Guid.Parse("77777777777777777777777777777777");
            var gpsQuestion = "gps";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.GpsCoordinateQuestion(gpsQuestionId, gpsQuestion)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(Create.Entity.PreloadingCompositeValue(gpsQuestion,
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
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<GpsAnswer>());
            Assert.That(((GpsAnswer) savedAssignments[0].Answers[0].Answer).Value,
                Is.EqualTo(new GeoPosition(90, 180, 12, 100, new DateTime(2019, 9, 22, 12, 11, 10))));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_yesno_question_should_be_saved_assignemnt_with_specified_yesno_answers()
        {
            //arrange 
            var yesNoQuestionId = Guid.Parse("88888888888888888888888888888888");
            var yesNoQuestion = "yesno";
            
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.YesNoQuestion(yesNoQuestionId, variable: yesNoQuestion, answers: new[] { 5, 6, 7 })));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(Create.Entity.PreloadingCompositeValue(yesNoQuestion,
                    Create.Entity.PreloadingValue("5", "1"), 
                    Create.Entity.PreloadingValue("6", ""),
                    Create.Entity.PreloadingValue("7", "0")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<YesNoAnswer>());
            Assert.That(((YesNoAnswer)savedAssignments[0].Answers[0].Answer).CheckedOptions,
                Is.EquivalentTo(new[]
                {
                    new CheckedYesNoAnswerOption(5, true),
                    new CheckedYesNoAnswerOption(7, false)
                }));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_ordered_yesno_question_should_be_saved_assignemnt_with_specified_ordered_yesno_answers()
        {
            //arrange 
            var yesNoQuestionId = Guid.Parse("88888888888888888888888888888888");
            var yesNoQuestion = "yesno";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.YesNoQuestion(yesNoQuestionId, variable: yesNoQuestion, ordered: true, answers: new[] { 5, 6, 7, 8, 9 })));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(Create.Entity.PreloadingCompositeValue(yesNoQuestion,
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
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<YesNoAnswer>());
            Assert.That(((YesNoAnswer)savedAssignments[0].Answers[0].Answer).CheckedOptions,
                Is.EqualTo(new[]
                {
                    new CheckedYesNoAnswerOption(9, false),
                    new CheckedYesNoAnswerOption(7, true),
                    new CheckedYesNoAnswerOption(8, true),
                    new CheckedYesNoAnswerOption(5, true),
                }));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_categorical_single_question_should_be_saved_assignemnt_with_specified_categorical_single_answer()
        {
            //arrange 
            var categoricalSingleQuestionId = Guid.Parse("10101010101010101010101010101010");
            var categoricalSingleQuestion = "single";


            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.SingleOptionQuestion(categoricalSingleQuestionId, categoricalSingleQuestion,
                        answerCodes: new[] {9m, 10m, 11m})));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(categoricalSingleQuestion, "10"))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<CategoricalFixedSingleOptionAnswer>());
            Assert.That(((CategoricalFixedSingleOptionAnswer)savedAssignments[0].Answers[0].Answer).SelectedValue, Is.EqualTo(10));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_categorical_multi_question_should_be_saved_assignemnt_with_specified_categorical_multi_answers()
        {
            //arrange 
            var categoricalMultiQuestionId = Guid.Parse("99999999999999999999999999999999");
            var categoricalMultiQuestion = "multi";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.MultyOptionsQuestion(categoricalMultiQuestionId, variable: categoricalMultiQuestion,
                        options: Create.Entity.Options(11, 12, 13))));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingCompositeValue(categoricalMultiQuestion,
                        Create.Entity.PreloadingValue("11", "1"), 
                        Create.Entity.PreloadingValue("12", "0"),
                        Create.Entity.PreloadingValue("13", "1")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<CategoricalFixedMultiOptionAnswer>());
            Assert.That(((CategoricalFixedMultiOptionAnswer) savedAssignments[0].Answers[0].Answer).CheckedValues, Is.EquivalentTo(new[] {11, 13}));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_ordered_categorical_multi_question_should_be_saved_assignemnt_with_specified_ordered_categorical_multi_answers()
        {
            //arrange 
            var categoricalMultiQuestionId = Guid.Parse("99999999999999999999999999999999");
            var categoricalMultiQuestion = "multi";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.MultyOptionsQuestion(categoricalMultiQuestionId, variable: categoricalMultiQuestion, areAnswersOrdered: true,
                        options: Create.Entity.Options(11, 12, 13))));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingCompositeValue(categoricalMultiQuestion,
                        Create.Entity.PreloadingValue("11", "2"),
                        Create.Entity.PreloadingValue("12", "3"),
                        Create.Entity.PreloadingValue("13", "1")))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Has.One.Items);
            Assert.That(savedAssignments[0].Answers[0].Answer, Is.TypeOf<CategoricalFixedMultiOptionAnswer>());
            Assert.That(((CategoricalFixedMultiOptionAnswer)savedAssignments[0].Answers[0].Answer).CheckedValues, Is.EqualTo(new[] { 13, 11, 12 }));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_empty_answer_on_text_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextQuestion(questionId, variable)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, ""))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_empty_answer_on_integer_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(questionId, variable)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, ""))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_empty_answer_on_double_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericRealQuestion(questionId, variable)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, ""))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_empty_answer_on_qrbarcode_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.QRBarcodeQuestion(questionId, variable: variable)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, ""))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_empty_answer_on_categorical_signle_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.SingleOptionQuestion(questionId, variable: variable,
                        answerCodes: new[] {1m, 2m, 3m})));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, ""))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_empty_answer_on_datetime_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.DateTimeQuestion(questionId, variable: variable)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, ""))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_empty_answer_on_gps_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.GpsCoordinateQuestion(questionId, variable)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingCompositeValue(variable,
                    Create.Entity.PreloadingValue("latitude", ""),
                    Create.Entity.PreloadingValue("longitude", ""),
                    Create.Entity.PreloadingValue("altitude", ""),
                    Create.Entity.PreloadingValue("accuracy", ""),
                    Create.Entity.PreloadingValue("timestamp", "")))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_empty_answer_on_list_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.TextListQuestion(questionId, variable: variable)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {Create.Entity.PreloadingRow(Create.Entity.PreloadingCompositeValue(variable,
                Create.Entity.PreloadingValue("0", ""),
                Create.Entity.PreloadingValue("1", ""),
                Create.Entity.PreloadingValue("2", "")))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_empty_answer_on_yesno_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.YesNoQuestion(questionId, variable: variable, answers: new[] {5, 6, 7})));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {Create.Entity.PreloadingRow(Create.Entity.PreloadingCompositeValue(variable,
                Create.Entity.PreloadingValue("5", ""),
                Create.Entity.PreloadingValue("6", ""),
                Create.Entity.PreloadingValue("7", "")))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_empty_answer_on_categorical_multi_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";


            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.MultyOptionsQuestion(questionId, variable: variable,
                        options: Create.Entity.Options(11, 12, 13))));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {Create.Entity.PreloadingRow(
                Create.Entity.PreloadingCompositeValue(variable,
                    Create.Entity.PreloadingValue("11", ""),
                    Create.Entity.PreloadingValue("12", ""),
                    Create.Entity.PreloadingValue("13", "")))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_linked_to_question_categorical_multi_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.MultyOptionsQuestion(questionId, variable: variable, linkedToQuestionId: Guid.NewGuid(),
                        options: Create.Entity.Options(11, 12, 13))));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {Create.Entity.PreloadingRow(
                Create.Entity.PreloadingCompositeValue(variable,
                    Create.Entity.PreloadingValue("11", "1")))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_linked_to_roster_categorical_multi_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.MultyOptionsQuestion(questionId, variable: variable, linkedToRosterId: Guid.NewGuid(),
                        options: Create.Entity.Options(11, 12, 13))));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {Create.Entity.PreloadingRow(
                Create.Entity.PreloadingCompositeValue(variable,
                    Create.Entity.PreloadingValue("11", "1")))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_linked_to_question_categorical_signle_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.SingleOptionQuestion(questionId, variable: variable, linkedToQuestionId: Guid.NewGuid(),
                        answerCodes: new[] { 1m, 2m, 3m })));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, "2"))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_answer_on_linked_to_roster_categorical_signle_question_should_be_saved_assignemnt_with_0_answers()
        {
            //arrange 
            var questionId = Guid.Parse("10101010101010101010101010101010");
            var variable = "myvar";

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.SingleOptionQuestion(questionId, variable: variable, linkedToRosterId: Guid.NewGuid(),
                        answerCodes: new[] { 1m, 2m, 3m })));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
                {Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(variable, "3"))});

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Answers, Is.Empty);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_assignment_with_limit_by_quantity_should_return_empty_errors_and_save_assignment_with_specified_quantity()
        {
            //arrange 
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("_quantity", "5"))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Quantity, Is.EqualTo(5));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_assignment_with_empty_by_quantity_should_return_empty_errors_and_save_assignment_with_quantity_equals_to_1()
        {
            //arrange 
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("_quantity", ""))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Quantity, Is.EqualTo(1));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_assignment_without_quantity_should_return_empty_errors_and_save_assignment_with_quantity_equals_to_1()
        {
            //arrange 
            var textQuestion = "txt";
            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion(variable: textQuestion)));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue(textQuestion, "text"))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Quantity, Is.EqualTo(1));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_assignment_infinity_quantity_should_return_empty_errors_and_save_assignment_with_quantity_equals_null()
        {
            //arrange 
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("_quantity", "-1"))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Quantity, Is.Null);
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_responsible_interviewer_should_return_empty_errors_and_save_assignment_assigner_to_interviewer()
        {
            //arrange 
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));
            var interviewerId = Guid.Parse("11111111111111111111111111111111");
            var supervisorId = Guid.Parse("22222222222222222222222222222222");
            var interviewerName = "int1";

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("_responsible", interviewerName))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();
            var userViewFactory = Create.Storage.UserViewFactory(
                Create.Entity.HqUser(interviewerId, supervisorId, userName: interviewerName));

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository,
                userViewFactory: userViewFactory);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Interviewer, Is.EqualTo(interviewerId));
            Assert.That(savedAssignments[0].Supervisor, Is.EqualTo(supervisorId));
        }

        [Test]
        public void when_VerifySimpleAndSaveIfNoErrors_and_preloaded_file_has_responsible_supervisorr_should_return_empty_errors_and_save_assignment_assigner_to_supervisor()
        {
            //arrange 
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion()));
            var supervisorId = Guid.Parse("22222222222222222222222222222222");
            var supervisorName = "super";

            var preloadedFile = Create.Entity.PreloadedFile(rows: new[]
            {
                Create.Entity.PreloadingRow(
                    Create.Entity.PreloadingValue("_responsible", supervisorName))
            });

            var importAssignmentsRepository = Create.Storage.InMemoryPlainStorage<AssignmentToImport>();
            var userViewFactory = Create.Storage.UserViewFactory(
                Create.Entity.HqUser(supervisorId, userName: supervisorName, role: UserRoles.Supervisor));

            var service = Create.Service.AssignmentsImportService(
                importAssignmentsRepository: importAssignmentsRepository,
                userViewFactory: userViewFactory);

            //act
            var errors = service.VerifySimpleAndSaveIfNoErrors(preloadedFile, Guid.Empty, questionnaire);

            //assert
            Assert.That(errors, Is.Empty);

            var savedAssignments = importAssignmentsRepository.Query(x => x.ToArray());

            Assert.That(savedAssignments, Has.One.Items);
            Assert.That(savedAssignments[0].Interviewer, Is.Null);
            Assert.That(savedAssignments[0].Supervisor, Is.EqualTo(supervisorId));
        }
    }
}
