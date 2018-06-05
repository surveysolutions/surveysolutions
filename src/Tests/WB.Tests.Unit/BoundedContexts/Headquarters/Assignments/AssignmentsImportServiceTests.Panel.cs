using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.Infrastructure.PlainStorage;
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
    }
}
