using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class ChapterInfoViewFactory : IChapterInfoViewFactory
    {
        private readonly IReadSideKeyValueStorage<GroupInfoView> readSideReader;

        public ChapterInfoViewFactory(IReadSideKeyValueStorage<GroupInfoView> readSideReader)
        {
            this.readSideReader = readSideReader;
        }

        public IQuestionnaireItem Load(string questionnaireId, string groupId)
        {
            return
                this.readSideReader.GetById(questionnaireId)
                                   .Items
                                   .Find(chapter => chapter.ItemId == groupId);
        }
    }
}