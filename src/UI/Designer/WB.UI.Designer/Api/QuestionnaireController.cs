using System.Web.Http;
using Main.Core.View;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.UI.Designer.Api
{
    [Authorize]
    public class QuestionnaireController : ApiController
    {
        private readonly IViewFactory<ChapterInfoViewInputModel, ChapterInfoView> chapterInfoViewFactory;
        private readonly IViewFactory<QuestionnaireInfoViewInputModel, QuestionnaireInfoView> questionnaireInfoViewFactory;

        public QuestionnaireController(IViewFactory<ChapterInfoViewInputModel, ChapterInfoView> chapterInfoViewFactory,
                                       IViewFactory<QuestionnaireInfoViewInputModel, QuestionnaireInfoView> questionnaireInfoViewFactory)
        {
            this.chapterInfoViewFactory = chapterInfoViewFactory;
            this.questionnaireInfoViewFactory = questionnaireInfoViewFactory;
        }

        public QuestionnaireInfoView GetQuestionnaireInfo(QuestionnaireInfoViewInputModel input)
        {
            return questionnaireInfoViewFactory.Load(input);
        }

        public ChapterInfoView GetChapterInfo(ChapterInfoViewInputModel input)
        {
            return chapterInfoViewFactory.Load(input);
        }
        
    }
}
