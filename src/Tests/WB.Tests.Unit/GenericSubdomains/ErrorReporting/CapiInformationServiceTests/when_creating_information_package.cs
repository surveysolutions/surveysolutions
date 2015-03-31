using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.CapiInformation;
using WB.Core.Infrastructure.FileSystem;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.GenericSubdomains.ErrorReporting.CapiInformationServiceTests
{
    internal class when_creating_information_package : CapiInformationServiceTestContext
    {
        Establish context = () =>
        {
            archiveUtils=new Mock<IArchiveUtils>();
            filesAddedToArchive=new List<string>();
            capiInformationService = CreateCapiInformationService(new string[] { "1", "2", "3" },
                (fileToAdd) => filesAddedToArchive.Add(fileToAdd), archiveUtils.Object);
        };

        Because of = () => pathToResultArchive = capiInformationService.CreateInformationPackage(new CancellationTokenSource().Token).Result;

        It should_result_ends_with_zip_extension = () => pathToResultArchive.ShouldEndWith(".zip");
        
        It should_result_contain_first_file = () => filesAddedToArchive.ShouldContain("1");
        It should_result_contain_second_file = () => filesAddedToArchive.ShouldContain("2");
        It should_result_contain_third_file = () => filesAddedToArchive.ShouldContain("3");

        private It should_directory_beZipped =
            () => archiveUtils.Verify(x => x.ZipDirectory(Moq.It.IsAny<string>(), pathToResultArchive), Times.Once);

        private static CapiInformationService capiInformationService;
        private static string pathToResultArchive;
        private static List<string> filesAddedToArchive;
        private static Mock<IArchiveUtils> archiveUtils;
    }
}
