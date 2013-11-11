using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using System.Linq;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireEditView
    {
        public EditQuestionnaireView Questionnaire { get; private set; }
        public bool IsOwner { get; private set; }
        private readonly QuestionnaireSharedPersons questionnaireSharedPersons;
        public IEnumerable<string> SharedPersons {
            get
            {
                if (IsOwner && this.questionnaireSharedPersons != null)
                {
                    return this.questionnaireSharedPersons.SharedPersons.Select(x => x.Email);
                }

                return new string[0];
            }
        }

        public QuestionnaireEditView(EditQuestionnaireView questionaire, QuestionnaireSharedPersons questionnaireSharedPersons, bool isOwner)
        {
            this.Questionnaire = questionaire;
            this.questionnaireSharedPersons = questionnaireSharedPersons;
            this.IsOwner = isOwner;
        }
    }
}