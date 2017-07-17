﻿using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewSynchronizationFileStorageTests
{
    [Subject(typeof(InterviewSynchronizationFileStorage))]
    internal class InterviewSynchronizationFileStorageTestContext
    {
        protected static InterviewSynchronizationFileStorage CreateFileSyncRepository(IImageQuestionFileStorage imageQuestionFileRepository = null, IFileSystemAccessor fileSystemAccessor = null)
        {
            return new InterviewSynchronizationFileStorage(imageQuestionFileRepository ?? Mock.Of<IImageQuestionFileStorage>(), fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(), "", "InterviewData");
        }

        protected static Mock<IFileSystemAccessor> CreateIFileSystemAccessorMock()
        {
            var result = new Mock<IFileSystemAccessor>();
            result.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            
            return result;
        }
    }
}
