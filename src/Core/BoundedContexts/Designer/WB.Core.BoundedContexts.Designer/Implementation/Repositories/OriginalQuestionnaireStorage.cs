using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;

namespace WB.Core.BoundedContexts.Designer.Implementation.Repositories
{
    public class OriginalQuestionnaireStorage
    {
        private string json;

        public void Push(string questionnaireJson)
        {
            this.json = questionnaireJson;
        }

        public string Pop()
        {
            var result = this.json;
            this.json = null;
            return result;
        }
    }
}
