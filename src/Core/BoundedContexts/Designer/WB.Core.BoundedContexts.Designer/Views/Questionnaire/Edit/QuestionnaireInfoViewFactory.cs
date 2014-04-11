using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionnaireInfoViewFactory : IViewFactory<QuestionnaireInfoViewInputModel, QuestionnaireInfoView>
    {
        private readonly IReadSideRepositoryReader<QuestionnaireDocument> questionnaireStorage;
        private readonly IViewFactory<ChapterInfoViewInputModel, ChapterInfoView> chapterInfoViewFactory;

        public QuestionnaireInfoViewFactory(IReadSideRepositoryReader<QuestionnaireDocument> questionnaireStorage,
                                       IViewFactory<ChapterInfoViewInputModel, ChapterInfoView> chapterInfoViewFactory)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.chapterInfoViewFactory = chapterInfoViewFactory;
        }

        public QuestionnaireInfoView Load(QuestionnaireInfoViewInputModel input)
        {
            var questionnaire = this.questionnaireStorage.GetById(input.QuestionnaireId);
            if (questionnaire == null) return null;

            var chapters =
                questionnaire.Children.OfType<IGroup>().Select(
                    group =>
                        chapterInfoViewFactory.Load(new ChapterInfoViewInputModel()
                        {
                            QuestionnaireId = input.QuestionnaireId,
                            ChapterId = group.PublicKey.FormatGuid()
                        }));


            return new QuestionnaireInfoView(chapters) {Title = questionnaire.Title};
        }
    }
}