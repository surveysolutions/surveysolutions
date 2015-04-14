using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Versions
{
    public interface IVersionParameters
    {
        int Version { get; }
        string VersionPrefix { get; }
        string[] Namespaces { get; }
        bool AreServiceVariablesPresent { get; }
        bool IsIRosterLevelInherited { get; }
        string GetRosterType(RosterTemplateModel rosterModel);
        string GetRosterInitialization(RosterTemplateModel rosterModel);
    }
}
