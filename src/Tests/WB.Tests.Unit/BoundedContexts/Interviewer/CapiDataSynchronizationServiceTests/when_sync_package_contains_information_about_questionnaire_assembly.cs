﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.GenericSubdomains.Native;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_questionnaire_assembly : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            var meta = new QuestionnaireAssemblyMetadata(questionnaireId, version);

            var jsonUtils = new NewtonJsonUtils();
            var compressor = new JsonCompressor(jsonUtils);

            received = new QuestionnaireSyncPackageDto
            {
                Content = assemblyAsBase64,
                MetaInfo = GetItemAsContent(meta)
            };

            changeLogManipulator = new Mock<IChangeLogManipulator>();

            questionnareAssemblyFileAccessor = new Mock<IQuestionnaireAssemblyFileAccessor>();

            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, stringCompressor: compressor,
                jsonUtils: jsonUtils, questionnareAssemblyFileAccessor: questionnareAssemblyFileAccessor.Object);
        };

        Because of = () => capiDataSynchronizationService.ProcessDownloadedPackage(received, SyncItemType.QuestionnaireAssembly);

        It should_call_StoreAssembly_once =
            () => questionnareAssemblyFileAccessor.Verify(x => x.StoreAssembly(questionnaireId, version, assemblyAsBase64), Times.Once);

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long version = 3;
        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static QuestionnaireSyncPackageDto received;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static string assemblyAsBase64 = "some_content";
        private static Mock<IQuestionnaireAssemblyFileAccessor> questionnareAssemblyFileAccessor;
    }

}
