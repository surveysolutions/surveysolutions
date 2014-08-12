using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public interface IRosterScope
    {
        IRosterScope GetParentScope();
        string GetTypeName();

        IEnumerable<QuestionTemplateModel> GetAllQuestionsToTop();
        IEnumerable<RosterTemplateModel> GetAllRostersToTop();

        List<Guid> GetRosterScope();
        

        List<QuestionTemplateModel> Questions { set; get; }
        List<GroupTemplateModel> Groups { set; get; }
        List<RosterTemplateModel> Rosters { set; get; }

        string GeneratedRosterScopeName { set; get; }
    }
}