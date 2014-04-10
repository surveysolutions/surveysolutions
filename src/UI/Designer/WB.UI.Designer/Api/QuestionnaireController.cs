using System.Web.Http;
using Main.Core.View;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.UI.Designer.Api
{
    [Authorize]
    public class QuestionnaireController : ApiController
    {
        private readonly IViewFactory<ChapterInfoViewInputModel, ChapterInfoView> chapterInfoViewFactory;

        public QuestionnaireController(IViewFactory<ChapterInfoViewInputModel, ChapterInfoView> chapterInfoViewFactory)
        {
            this.chapterInfoViewFactory = chapterInfoViewFactory;
        }

        public ChapterInfoView GetChapterInfo(ChapterInfoViewInputModel input)
        {
            return chapterInfoViewFactory.Load(input);
        }
        
    }
}
