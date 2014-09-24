using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core;
using Main.Core.Events;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.IncomePackagesRepositoryTests
{
    internal class when_Processing_Stored_Item_with_zero_events_inside : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            var jsonMock = new Mock<IJsonUtils>();
            jsonMock.Setup(x => x.Deserrialize<AggregateRootEvent[]>(Moq.It.IsAny<string>())).Returns(new AggregateRootEvent[0]);

            fileSystemAccessorMock = CreateDefaultFileSystemAccessorMock();
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.Is<string>(name => name.Contains(interviewId.ToString()))))
                .Returns(true);
            fileSystemAccessorMock.Setup(x => x.ReadAllText(Moq.It.Is<string>(name => name.Contains(interviewId.ToString()))))
                .Returns(packageContent);

            incomePackagesRepository = CreateIncomePackagesRepository(fileSystemAccessor: fileSystemAccessorMock.Object, jsonUtils:jsonMock.Object,
                interviewSummaryStorage:
                    Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(_ => _.GetById(interviewId.FormatGuid()) == new InterviewSummary()));
        };

        Because of = () =>
            incomePackagesRepository.ProcessItem(interviewId);

        It should_content_of_package_not_be_written_at_error_folder = () =>
            fileSystemAccessorMock.Verify(
                x => x.WriteAllText(Moq.It.Is<string>(name => name.Contains(interviewId.ToString()) && name.Contains("IncomingDataWithErrors")), packageContent), Times.Never);

        It should_file_be_deleted_from_package_folder = () =>
            fileSystemAccessorMock.Verify(
             x => x.DeleteFile(Moq.It.Is<string>(name => name.Contains(interviewId.ToString()) && name.Contains("IncomingData"))), Times.Once);

        private static IncomePackagesRepository incomePackagesRepository;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Guid interviewId = Guid.NewGuid();
        private static string packageContent = PackageHelper.CompressString("random content");
    }
}
