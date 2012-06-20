using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Services
{
    public class ValildationService : IValildationService
    {
        public ValildationService()
        {
        }

        #region Implementation of IValildationService
        public bool Validate(CompleteQuestionnaire entity, Guid? groupKey, Guid? propagationKey)
        {
            bool result = true;
          
            if (!groupKey.HasValue)
            {
                CompleteQuestionnaireValidationExecutor validator =
              new CompleteQuestionnaireValidationExecutor(entity.GetInnerDocument().QuestionHash);

                result = validator.Execute();

            
            }
            else
            {
                var group = entity.GetInnerDocument().FindGroupByKey(groupKey.Value, propagationKey);
                if (group == null)
                    throw new ArgumentException(string.Format("group with publick key {0} doesn't exist", groupKey));
                CompleteQuestionnaireValidationExecutor validator =
              new CompleteQuestionnaireValidationExecutor(new GroupHash(group));

                result = validator.Execute();
            }


            return result;
        }

        #endregion
    }
}
