﻿using System;
using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.TabularParaDataWriterTests
{
    internal class WhenMethodRemoveOfTabularParaDataWriterCalledAndCacheIsEnabled : TabularParaDataWriterTestContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("22222222222222222222222222222222");
            var questionnaireVersion = 2;
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true); 
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
            .Returns<string, string>(Path.Combine);


            var interviewSummaryWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            interviewSummaryWriterMock.Setup(x => x.GetById(Moq.It.IsAny<string>()))
                .Returns(new InterviewSummary() { QuestionnaireId = questionnaireId, QuestionnaireVersion = questionnaireVersion, InterviewId = interviewId });
            _tabularParaDataAccessor = CreateTabularParaDataWriter(interviewSummaryWriter: interviewSummaryWriterMock.Object, fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            _tabularParaDataAccessor.Remove(interviewId.FormatGuid());

        It should_delete_file_with_interview_history = () =>
            fileSystemAccessorMock.Verify(x => x.DeleteFile(Moq.It.Is<string>(_ => _.Contains(interviewId.FormatGuid()))), Times.Once);

        private static TabularParaDataAccessor _tabularParaDataAccessor;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
    }
}
