using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Versions
{
    internal class V2Parameters : IVersionParameters
    {
        public int Version
        {
            get { return 2; }
        }

        public string VersionPrefix
        {
            get { return "V2"; }
        }

        public string[] Namespaces
        {
            get
            {
                return new[]
                {
                    string.Format("WB.Core.SharedKernels.DataCollection.{0}", VersionPrefix),
                    string.Format("WB.Core.SharedKernels.DataCollection.{0}.CustomFunctions", VersionPrefix)
                };
            }
        }

        public bool AreServiceVariablesPresent
        {
            get { return true; }
        }

        public bool IsIRosterLevelInherited
        {
            get { return true; }
        }

        public string GetRosterType(RosterTemplateModel rosterModel)
        {
            return string.Format("RosterRowList<{0}>", rosterModel.GeneratedTypeName);
        }

        public string GetRosterInitialization(RosterTemplateModel rosterModel)
        {
            return string.Format("new RosterRowList<{0}>(rosters)", rosterModel.GeneratedTypeName);
        }
    }
}
