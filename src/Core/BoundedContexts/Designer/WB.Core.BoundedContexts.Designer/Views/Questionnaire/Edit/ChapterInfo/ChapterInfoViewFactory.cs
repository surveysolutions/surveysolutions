using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo
{
    public class ChapterInfoViewFactory : IViewFactory<ChapterInfoViewInputModel, GroupInfoView>
    {
        private readonly IReadSideRepositoryReader<GroupInfoView> readSideReader;

        public ChapterInfoViewFactory(IReadSideRepositoryReader<GroupInfoView> readSideReader)
        {
            this.readSideReader = readSideReader;
        }

        public GroupInfoView Load(ChapterInfoViewInputModel input)
        {
            return
                this.readSideReader.GetById(input.QuestionnaireId)
                    .Groups.Find(chapter => chapter.GroupId == input.ChapterId);
        }
    }
}