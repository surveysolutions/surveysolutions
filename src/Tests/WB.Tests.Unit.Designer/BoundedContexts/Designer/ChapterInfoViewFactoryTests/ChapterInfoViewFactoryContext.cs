using System.Collections.Generic;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.BoundedContexts.Designer.ChapterInfoViewFactoryTests
{
    internal class ChapterInfoViewFactoryContext
    {
        protected static GroupInfoView CreateChapterInfoView(string questionnaireId, string chapterId)
        {
            return new GroupInfoView()
            {
                ItemId = questionnaireId,
                Items = new List<IQuestionnaireItem>() {new GroupInfoView() {ItemId = chapterId}}
            };
        }

        protected static GroupInfoView CreateChapterInfoViewWithoutChapters(string questionnaireId, string chapterId)
        {
            return new GroupInfoView()
            {
                ItemId = questionnaireId,
                Items = new List<IQuestionnaireItem>()
            };
        }

        protected static ChapterInfoViewFactory CreateChapterInfoViewFactory(
            IReadSideKeyValueStorage<GroupInfoView> repository = null)
        {
            return
                new ChapterInfoViewFactory(repository ??
                                                 Mock.Of<IReadSideKeyValueStorage<GroupInfoView>>());
        }
    }
}