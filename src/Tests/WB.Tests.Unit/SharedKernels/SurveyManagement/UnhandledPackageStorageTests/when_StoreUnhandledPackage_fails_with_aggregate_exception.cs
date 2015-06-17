using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UnhandledPackageStorageTests
{
    internal class when_StoreUnhandledPackage_fails_with_aggregate_exception : UnhandledPackageStorageTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            fileSystemAccessorMock.Setup(x => x.GetFileNameWithoutExtension(Moq.It.IsAny<string>()))
                .Returns(unhandledPackageName);

            fileSystemAccessorMock.Setup(x => x.WriteAllText(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Callback<string, string>((path, content) => exceptionFileContent = content);

            aggregateException =
                new AggregateException("aggregate exception message",new Exception[]
                {new NullReferenceException("null reference test"), new NotSupportedException("not supported")});

            brokenSyncPackagesStorage = CreateUnhandledPackageStorage(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            brokenSyncPackagesStorage.StoreUnhandledPackage(unhandledPackageName,null, aggregateException);

        It should_unwrap_aggregate_exception = () =>
            exceptionFileContent.ShouldEqual("aggregate exception message " + Environment.NewLine + "null reference test " + Environment.NewLine + "not supported ");
        
        private static BrokenSyncPackagesStorage brokenSyncPackagesStorage;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static AggregateException aggregateException;
        private static string unhandledPackageName = "test";
        private static string exceptionFileContent;
    }
}
