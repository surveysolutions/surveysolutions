﻿using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.BinaryFormatDataExportHandlerTests
{
    internal class when_export_all_binary_data: BinaryFormatDataExportHandlerTestContext
    {
        private Establish context = () =>
        {
            var questionnaireExportStructure =
                Create.Entity.QuestionnaireExportStructure(questionnaireId: questionnaireIdentity.QuestionnaireId,
                    version: questionnaireIdentity.Version);
            var headerToLevelMap = Create.Entity.HeaderStructureForLevel();

            var multiMediaQuestion = Create.Entity.ExportedHeaderItem();
            multiMediaQuestion.QuestionType = QuestionType.Multimedia;
            headerToLevelMap.HeaderItems.Add(multiMediaQuestion.PublicKey, multiMediaQuestion);

            questionnaireExportStructure.HeaderToLevelMap.Add(headerToLevelMap.LevelScopeVector, headerToLevelMap);

            var interviewSummary =
                Create.Entity.InterviewSummary(interviewId: interviewId, questionnaireId: questionnaireIdentity.QuestionnaireId,
                    questionnaireVersion: questionnaireIdentity.Version);

            var interviewDataStorage = new TestInMemoryWriter<InterviewData>();
            var interviewSummarytorage = new TestInMemoryWriter<InterviewSummary>();

            interviewSummarytorage.Store(interviewSummary, interviewId.FormatGuid());
            interviewDataStorage.Store(
                Create.Entity.InterviewData(Create.Entity.InterviewQuestion(questionId: multiMediaQuestion.PublicKey,
                    answer: "var.jpg")), interviewId.FormatGuid());

            var questionnaireStorage = new Mock<IQuestionnaireExportStructureStorage>();

            questionnaireStorage.Setup(x => x.GetQuestionnaireExportStructure(Moq.It.IsAny<QuestionnaireIdentity>()))
                .Returns(questionnaireExportStructure);

            plainInterviewFileStorageMock=new Mock<IPlainInterviewFileStorage>();
            plainInterviewFileStorageMock.Setup(x => x.GetBinaryFilesForInterview(interviewId))
                .Returns(new[] {Create.Entity.InterviewBinaryDataDescriptor()}.ToList());

            fileSystemAccessor=new Mock<IFileSystemAccessor>();
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>((p1, p2) => p2);

            var dataExportFileAccessor = CrerateDataExportFileAccessor(fileSystemAccessor.Object);

            binaryFormatDataExportHandler =
                CreateBinaryFormatDataExportHandler(
                    interviewSummaries: interviewSummarytorage,
                    interviewDatas: interviewDataStorage,
                    questionnaireExportStructureStorage: questionnaireStorage.Object,
                    plainFileRepository: plainInterviewFileStorageMock.Object,
                    fileSystemAccessor: fileSystemAccessor.Object,
                    dataExportFileAccessor: dataExportFileAccessor);
        };

        Because of = () => binaryFormatDataExportHandler.ExportData(Create.Entity.DataExportProcessDetails(questionnaireIdentity: questionnaireIdentity));

        It should_request_binary_data_for_answered_multimedia_question =
            () => plainInterviewFileStorageMock.Verify(x=>x.GetInterviewBinaryData(interviewId, "var.jpg"), Times.Once);

        It should_write_answered_multimedia_question =
          () => fileSystemAccessor.Verify(x => x.WriteAllBytes("var.jpg", Moq.It.IsAny<byte[]>()), Times.Once);

        private static BinaryFormatDataExportHandler binaryFormatDataExportHandler;
        private static QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);
        private static Guid interviewId = Guid.NewGuid();
        private static Mock<IPlainInterviewFileStorage> plainInterviewFileStorageMock;
        private static Mock<IFileSystemAccessor> fileSystemAccessor;
    }
}