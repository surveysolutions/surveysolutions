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
            string versionPrefix, 
            string[] namespaces, 
            bool areRowSpecificVariablesPresent, 
            bool isIRosterLevelInherited, 
            bool shouldGenerateUpdateRosterTitleMethods,
            string rosterType)
        {
            VersionPrefix = versionPrefix;
            Namespaces = namespaces;
            AreRowSpecificVariablesPresent = areRowSpecificVariablesPresent;
            IsIRosterLevelInherited = isIRosterLevelInherited;
            RosterType = rosterType;
            ShouldGenerateUpdateRosterTitleMethods = shouldGenerateUpdateRosterTitleMethods;
        }

        public string VersionPrefix { get; private set; }

        public string[] Namespaces { get; private set; }

        public bool AreRowSpecificVariablesPresent { get; private set; }

        public bool IsIRosterLevelInherited { get; private set; }

        public bool ShouldGenerateUpdateRosterTitleMethods { get; private set; }

        public string RosterType { get; private set; }
    }
}
