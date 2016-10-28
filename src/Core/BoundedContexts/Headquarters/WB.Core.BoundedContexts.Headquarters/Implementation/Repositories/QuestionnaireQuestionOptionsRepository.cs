using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public class QuestionnaireQuestionOptionsRepository : IQuestionOptionsRepository
    {
       
        public IEnumerable<CategoricalOption> GetOptionsForQuestion(QuestionnaireIdentity qestionnaireIdentity, 
            IQuestion question, int? parentQuestionValue, string filter, Guid? translationId)
        {
            return AnswerUtils.GetCategoricalOptionsFromQuestion(question, parentQuestionValue, filter);
        }

        public CategoricalOption GetOptionForQuestionByOptionText(QuestionnaireIdentity qestionnaireIdentity,
             IQuestion question, string optionText, Guid? translationId)
        {
            return AnswerUtils.GetOptionForQuestionByOptionText(question, optionText);
        }

        public CategoricalOption GetOptionForQuestionByOptionValue(QuestionnaireIdentity qestionnaireIdentity,
             IQuestion question, decimal optionValue, Guid? translationId)
        {
            return AnswerUtils.GetOptionForQuestionByOptionValue(question, optionValue);
        }
    }
}
