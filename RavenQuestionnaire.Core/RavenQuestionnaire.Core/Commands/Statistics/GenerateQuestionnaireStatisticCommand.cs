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

        public RavenQuestionnaire.Core.Entities.CompleteQuestionnaire CompleteQuestionnaire { get; private set; }


        public GenerateQuestionnaireStatisticCommand(RavenQuestionnaire.Core.Entities.CompleteQuestionnaire completeQuestionnaire, UserLight executor)
        {
            this.Executor = executor;
            this.CompleteQuestionnaire = completeQuestionnaire;
        }
    }
}
