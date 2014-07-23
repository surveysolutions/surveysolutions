using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class RosterTemplateModel : IParent
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }
        public string Conditions { set; get; }
        public string RosterGeneratedTypeName { set; get; }

        public List<QuestionTemplateModel> Questions { private set; get; }
        public List<GroupTemplateModel> Groups { private set; get; }

        public IParent ParentRoster { set; get; }

        public IParent GetParent()
        {
            return this.ParentRoster;
        }

        public string GetTypeName()
        {
            return this.RosterGeneratedTypeName;
        }

        public IEnumerable<QuestionTemplateModel> GetQuestions()
        {
            return this.Questions;
        }
    }
}