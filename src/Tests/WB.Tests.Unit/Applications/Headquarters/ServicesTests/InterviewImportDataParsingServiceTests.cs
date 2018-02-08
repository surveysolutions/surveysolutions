using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.ServicesTests
{
    [TestOf(typeof(InterviewImportDataParsingService))]
    internal class InterviewImportDataParsingServiceTests
    {
        private static InterviewImportDataParsingService CreateInterviewImportDataParsingService(
            IPreloadedDataRepository preloadedDataRepository = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage = null,
            IUserViewFactory userViewFactory = null,
            IQuestionDataParser dataParser = null)
            => new InterviewImportDataParsingService(
                preloadedDataRepository ?? Mock.Of<IPreloadedDataRepository>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                questionnaireExportStructureStorage ?? Mock.Of<IQuestionnaireExportStructureStorage>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                dataParser ?? Mock.Of<IQuestionDataParser>());

        [Test]
        public void when_GetAssignmentsData_in_assignments_mode_and_preload_file_name_is_not_questionnaire_title_name()
        {
            //arrange
            var importProcessId = "processid";
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
            var preloadedFileName = "test";
            var questionVariable = "qVar";

            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new []{ Create.Entity.TextQuestion(variable: questionVariable)});
            var questionnaireStorage = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, questionnaireDocument);

            var questionnaireExportStructure = Mock.Of<IQuestionnaireExportStructureStorage>(x =>
                x.GetQuestionnaireExportStructure(questionnaireIdentity) ==
                Create.Entity.QuestionnaireExportStructure(questionnaireDocument));

            var preloadedDataRepository = Mock.Of<IPreloadedDataRepository>(x =>
                x.GetPreloadedDataOfSample() == new PreloadedDataByFile(
                    preloadedFileName, new[] {questionVariable}, new[] {new[] {"text"}}));

            var service = CreateInterviewImportDataParsingService(questionnaireStorage: questionnaireStorage,
                preloadedDataRepository: preloadedDataRepository, questionnaireExportStructureStorage: questionnaireExportStructure);

            //act
            var result = service.GetAssignmentsData(questionnaireIdentity, AssignmentImportType.Assignments);
            //assert
            Assert.That(result, Is.Not.Empty);
        }
    }
}
