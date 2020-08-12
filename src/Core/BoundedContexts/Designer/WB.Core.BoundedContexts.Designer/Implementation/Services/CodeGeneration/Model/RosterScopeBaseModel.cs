using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public abstract class RosterScopeBaseModel
    {
        protected RosterScopeBaseModel(
            string rosterScopeName, 
            string typeName, 
            List<GroupTemplateModel> groups, 
            List<QuestionTemplateModel> questions,
            List<StaticTextTemplateModel> staticTexts,
            List<RosterTemplateModel> rosters, 
            List<Guid> rosterScope,
            List<VariableTemplateModel> variables,
            string? parentTypeName)
        {
            //this.Questions = new List<QuestionTemplateModel>();
            //this.StaticTexts = new List<StaticTextTemplateModel>();
            //this.Groups = new List<GroupTemplateModel>();
            //this.Rosters = new List<RosterTemplateModel>();
            //this.Variables = new List<VariableTemplateModel>();
            
            RosterScopeName = rosterScopeName;
            TypeName = typeName;
            Groups = groups;
            Questions = questions;
            StaticTexts = staticTexts;
            Rosters = rosters;
            RosterScope = rosterScope;
            Variables = variables;
            ParentTypeName = parentTypeName;
        }

        public string RosterScopeName { get;}

        public string TypeName { get; }

        public string? ParentTypeName { get; }

        public List<QuestionTemplateModel> Questions { get; }

        public List<VariableTemplateModel> Variables { get; }

        public List<StaticTextTemplateModel> StaticTexts { get; }

        public List<GroupTemplateModel> Groups { get; }

        public List<RosterTemplateModel> Rosters { get;}

        public List<Guid> RosterScope { set; get; }

        public List<string> LinkedQuestionsIdNames { get; set; } = new List<string>();
    }
}
