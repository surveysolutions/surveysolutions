using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WB.Services.Export.Questionnaire
{
    public class QuestionnaireDocument : Group
    {
        private bool childrenWereConnected;

        public QuestionnaireDocument()
        {
            this.Children = new List<IQuestionnaireEntity>();
        }

        public string Id { get; set; }
    }
}
