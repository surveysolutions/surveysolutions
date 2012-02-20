using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands
{
    public class CreateNewFlowGraphCommand: ICommand
    {
        public Questionnaire Questionnaire
        {
            get;
            private set;
        }

        public UserLight Executor { get; set; }

        public CreateNewFlowGraphCommand(Questionnaire questionnaire, UserLight executor)
        {
            this.Questionnaire = questionnaire;
            Executor = executor;

        }
    }
}
