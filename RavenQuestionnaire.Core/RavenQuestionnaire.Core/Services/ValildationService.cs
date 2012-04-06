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
        #region Implementation of IValildationService

        public void Validate(CompleteQuestionnaire entity, Guid? groupKey, Guid? propagationKey)
        {
            CompleteQuestionnaireValidationExecutor validator =
                new CompleteQuestionnaireValidationExecutor(entity.GetInnerDocument());
            if (!groupKey.HasValue)
            {
                validator.Execute(entity.GetInnerDocument());
                return;
            }
            var group = entity.GetInnerDocument().FindGroupByKey(groupKey.Value, propagationKey);
            if (group == null)
                throw new ArgumentException(string.Format("group with publick key {0} doesn't exist", groupKey));
            validator.Execute(group);
        }

        #endregion
    }
}
