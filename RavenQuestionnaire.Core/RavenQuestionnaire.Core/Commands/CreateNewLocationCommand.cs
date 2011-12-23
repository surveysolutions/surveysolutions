using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands
{
    public class CreateNewLocationCommand: ICommand
    {
        public string Location
        {
            get;
            private set;
        }

        public UserLight Executor { get; set; }

        public CreateNewLocationCommand(string location, UserLight executor)
        {
            this.Location = location;
            Executor = executor;

        }
    }
}
