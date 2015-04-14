using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Versions
{
    internal class V1Parameters : IVersionParameters
    {
        public int Version
        {
            get { return 1; }
        }

        public string VersionPrefix
        {
            get { return string.Empty; }
        }

        public string[] Namespaces
        {
            get { return new string[0];}
        }

        public bool AreServiceVariablesPresent
        {
            get { return false; }
        }

        public bool IsIRosterLevelInherited
        {
            get { return false; }
        }

        public string GetRosterType(RosterTemplateModel rosterModel)
        {
            return string.Format("IEnumerable<{0}>", rosterModel.GeneratedTypeName);
        }

        public string GetRosterInitialization(RosterTemplateModel rosterModel)
        {
            return string.Format("rosters == null ? new List<{0}>() : rosters.Select(x => x as {0}).ToList()",
                rosterModel.GeneratedTypeName);
        }
    }
}
