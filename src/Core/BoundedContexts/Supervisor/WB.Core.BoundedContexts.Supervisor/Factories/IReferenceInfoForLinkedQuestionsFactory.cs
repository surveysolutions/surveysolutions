using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Factories
{
    public interface IReferenceInfoForLinkedQuestionsFactory
    {
        ReferenceInfoForLinkedQuestions CreateReferenceInfoForLinkedQuestions(QuestionnaireDocument questionnaire, long version);
    }
}
