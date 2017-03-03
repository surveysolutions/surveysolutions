using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    public class LevelModel
    {
        public Guid Id { set; get; }
        public string Variable { set; get; }
        public string ClassName { get; set; }
        public RosterScope RosterScope { get; set; }

        public List<QuestionModel> Questions { get; private set; } = new List<QuestionModel>();
        public List<RosterModel> Rosters { get; private set; } = new List<RosterModel>();
        public List<VariableModel> Variables { get; private set; } = new List<VariableModel>();

    }
}