using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands.Statistics;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Services
{
    public class ValildationService : IValildationService
    {
        public ValildationService(ICommandInvokerAsync asyncInvocker)
        {
            this._asyncInvocker = asyncInvocker;
        }

        #region Implementation of IValildationService
        private ICommandInvokerAsync _asyncInvocker;
        public bool Validate(CompleteQuestionnaire entity, Guid? groupKey, Guid? propagationKey)
        {
            bool result = true;
          
            if (!groupKey.HasValue)
            {
                CompleteQuestionnaireValidationExecutor validator =
              new CompleteQuestionnaireValidationExecutor(new GroupHash(entity.GetInnerDocument()));

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

            var command = new GenerateQuestionnaireStatisticCommand(entity, null);
            _asyncInvocker.Execute(command);

            return result;
        }

        #endregion
    }
}
