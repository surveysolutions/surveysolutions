using Main.Core.Documents;
using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public interface IQuestionnaireChangeHistoryFactory
    {
        QuestionnaireChangeHistory Load(Guid id, int page, int pageSize);

    }
}
