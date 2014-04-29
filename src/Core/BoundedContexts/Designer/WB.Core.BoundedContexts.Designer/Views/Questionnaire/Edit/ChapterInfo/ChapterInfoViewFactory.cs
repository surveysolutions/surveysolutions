using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class ChapterInfoViewFactory : IViewFactory<ChapterInfoViewInputModel, IQuestionnaireItem>
    {
        private readonly IReadSideRepositoryReader<GroupInfoView> readSideReader;

        public ChapterInfoViewFactory(IReadSideRepositoryReader<GroupInfoView> readSideReader)
        {
            this.readSideReader = readSideReader;
        }

        public IQuestionnaireItem Load(ChapterInfoViewInputModel input)
        {
            return
                this.readSideReader.GetById(input.QuestionnaireId)
                    .Items.Find(chapter => chapter.ItemId == input.ChapterId);
        }
    }
}