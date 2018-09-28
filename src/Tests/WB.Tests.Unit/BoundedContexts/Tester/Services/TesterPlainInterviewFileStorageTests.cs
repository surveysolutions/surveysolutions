using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Tester.Services
{
    [TestOf(typeof(TesterImageFileStorage))]
    [TestFixture]
    public class TesterPlainInterviewFileStorageTests
    {
        [Test]
        public void When_StoreInterviewBinaryData_with_image_data_Then_should_store_specified_file_to_file_system()
        {
            //arrange
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            var interviewerPlainInterviewFileStorage = Create.Service.TesterPlainInterviewFileStorage(
                fileSystemAccessor: fileSystemAccessor.Object,
                rootDirectory: "temp");
            byte[] imageFileBytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

            //act
            interviewerPlainInterviewFileStorage.StoreInterviewBinaryData(Guid.NewGuid(), "imageFileName", imageFileBytes, null);

            //assert
            fileSystemAccessor.Verify(x => x.WriteAllBytes(Moq.It.IsAny<string>(), imageFileBytes), Times.Once);
        }

        [Test]
        public void When_StoreInterviewBinaryData_with_image_data_for_one_interview_Then_should_we_get_this_image_by_name_for_different_interview()
        {
            //arrange
            var interviewId1 = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var interviewId2 = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var imageName = "imageFileName";
            var filePathToImage = "filePathToImage";
            byte[] imageFileBytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

            var fileSystemAccessor = Mock.Of<IFileSystemAccessor>(a 
                => a.ReadAllBytes(filePathToImage, null, null) == imageFileBytes
                && a.IsFileExists(filePathToImage) == true
                && a.CombinePath(It.IsAny<string>(), imageName) == filePathToImage);
            var interviewerPlainInterviewFileStorage = Create.Service.TesterPlainInterviewFileStorage(
                fileSystemAccessor: fileSystemAccessor,
                rootDirectory: "temp");
            interviewerPlainInterviewFileStorage.StoreInterviewBinaryData(interviewId1, imageName, imageFileBytes, null);

            //act
            var resImage = interviewerPlainInterviewFileStorage.GetInterviewBinaryData(interviewId2, imageName);

            //assert
            Mock.Get(fileSystemAccessor).Verify(x => x.WriteAllBytes(filePathToImage, imageFileBytes), Times.Once);
            Assert.That(resImage, Is.EqualTo(imageFileBytes));
        }
    }
}
