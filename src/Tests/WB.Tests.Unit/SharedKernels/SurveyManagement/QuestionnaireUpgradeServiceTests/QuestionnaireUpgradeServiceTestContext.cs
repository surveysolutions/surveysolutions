using System.Collections.Generic;
using System.Collections.ObjectModel;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireUpgradeServiceTests
{
    [Subject(typeof(QuestionnaireUpgradeService))]
    internal class QuestionnaireUpgradeServiceTestContext
    {
        protected static QuestionnaireUpgradeService CreateQuestionnaireUpgradeService()
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.MakeStataCompatibleFileName(Moq.It.IsAny<string>())).Returns<string>(s => s);
            return new QuestionnaireUpgradeService(fileSystemAccessorMock.Object);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] children)
        {
            var result = new QuestionnaireDocument()
            {
                Children = new IComposite[] { new Group("Chapter")
                {
                    Children = children?.ToReadOnlyCollection()?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
                } }.ToReadOnlyCollection()
            };

            return result;
        }
    }
}
