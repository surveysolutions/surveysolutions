using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;

namespace WB.Core.SharedKernels.QuestionnaireUpgrader.Tests.QuestionnaireUpgradeServiceTests
{
    [Subject(typeof(QuestionnaireUpgradeService))]
    internal class QuestionnaireUpgradeServiceTestContext
    {
        protected static QuestionnaireUpgradeService CreateQuestionnaireUpgradeService()
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.MakeValidFileName(Moq.It.IsAny<string>())).Returns<string>(s => s);
            return new QuestionnaireUpgradeService(fileSystemAccessorMock.Object);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] children)
        {
            var result = new QuestionnaireDocument();
            var chapter = new Group("Chapter");
            result.Children.Add(chapter);

            foreach (var child in children)
            {
                chapter.Children.Add(child);
            }

            return result;
        }
    }
}
