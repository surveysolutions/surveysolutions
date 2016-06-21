using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Questionnaire.Documents
{
    public static class AnswerExtentions
    {
        public static CategoricalOption ToCategoricalOption(this Answer answer)
        {
            if (answer == null)
                return null;

            return new CategoricalOption()
            {
                Title = answer.AnswerText,
                Value = Convert.ToInt32(answer.AnswerCode),
                ParentValue = answer.ParentCode.HasValue ? Convert.ToInt32(answer.ParentCode): (int?)null
            };
        }
    }
}
