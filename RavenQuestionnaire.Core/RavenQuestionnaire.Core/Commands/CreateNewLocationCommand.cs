using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Commands
{
    public class CreateNewLocationCommand: ICommand
    {
        public string Location
        {
            get;
            private set;
        }


        public CreateNewLocationCommand(string location)
        {
            this.Location = location;
        }
    }
}
