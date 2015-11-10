﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.FilebasedExportedDataAccessorTests
{
    [Subject(typeof(FilebasedExportedDataAccessor))]
    internal class FilebasedExportedDataAccessorTestContext
    {
        protected static FilebasedExportedDataAccessor CreateFilebasedExportedDataAccessor(
            string[] dataFiles=null, 
            string[] environmentFiles=null, 
            Action<IEnumerable<string>> zipCallback=null,
            IFileSystemAccessor fileSystemAccessor=null)
        {
            if (fileSystemAccessor == null)
            {
                var fileSystemAccessorMock = CreateFileSystemAccessorMock();
                fileSystemAccessorMock.Setup(x => x.CombinePath(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string>(Path.Combine);
                fileSystemAccessor = fileSystemAccessorMock.Object;
            }
            var archiveUtilsMock = new Mock<IArchiveUtils>();
            if (zipCallback != null)
                archiveUtilsMock.Setup(x => x.ZipFiles(It.IsAny<IEnumerable<string>>(), It.IsAny<string>()))
                    .Callback<IEnumerable<string>, string>((f, n) => zipCallback(f));

            return new FilebasedExportedDataAccessor(fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(), new InterviewDataExportSettings(), 
                Mock.Of<IMetadataExportService>(),
                Mock.Of<ILogger>(), archiveUtilsMock.Object);
        }

        protected static Mock<IFileSystemAccessor> CreateFileSystemAccessorMock()
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            fileSystemAccessorMock.Setup(x => x.GetFileName(It.IsAny<string>())).Returns<string>(n => n);
            fileSystemAccessorMock.Setup(x => x.IsDirectoryExists(It.IsAny<string>())).Returns(true);
            return fileSystemAccessorMock;
        }
    }
}
