using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class CodeGenerationSettings
    {
        public CodeGenerationSettings(
            string[] additionInterfaces, 
            string[] namespaces, 
            bool areRosterServiceVariablesPresent, string rosterType)
        {
            AdditionInterfaces = additionInterfaces;
            Namespaces = namespaces;
            AreRosterServiceVariablesPresent = areRosterServiceVariablesPresent;
            RosterType = rosterType;
        }

        public string[] AdditionInterfaces { get; private set; }

        public string[] Namespaces { get; private set; }

        public bool AreRosterServiceVariablesPresent { get; private set; }

        public string RosterType { get; private set; }
    }
}
