using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Statistics
{
    public class GenerateQuestionnaireStatisticCommand : ICommand
    {
        #region Implementation of ICommand

        public UserLight Executor { get; set; }

        #endregion

        public string CompleteQuestionnaireId { get; private set; }


        public GenerateQuestionnaireStatisticCommand(string completeQuestionanireId, UserLight executor)
        {
            this.Executor = executor;
            this.CompleteQuestionnaireId = completeQuestionanireId;
        }
    }
}
