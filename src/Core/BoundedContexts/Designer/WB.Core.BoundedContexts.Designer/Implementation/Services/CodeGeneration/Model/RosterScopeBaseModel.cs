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
            List<VariableTemplateModel> variabels,
            string parentTypeName)
        {
            this.RosterScopeName = rosterScopeName;
            this.TypeName = typeName;
            Groups = groups;
            Questions = questions;
            StaticTexts = staticTexts;
            Rosters = rosters;
            RosterScope = rosterScope;
            Variables = variabels;
            this.ParentTypeName = parentTypeName;
        }

        protected RosterScopeBaseModel()
        {
            this.Questions = new List<QuestionTemplateModel>();
            this.StaticTexts = new List<StaticTextTemplateModel>();
            this.Groups = new List<GroupTemplateModel>();
            this.Rosters = new List<RosterTemplateModel>();
            this.Variables=new List<VariableTemplateModel>();
        }

        public string RosterScopeName { get; set; }

        public string TypeName { set; get; }

        public string ParentTypeName { set; get; }

        public List<QuestionTemplateModel> Questions { get; set; }

        public List<VariableTemplateModel> Variables { get; set; }

        public List<StaticTextTemplateModel> StaticTexts { get; set; }

        public List<GroupTemplateModel> Groups { get; set; }

        public List<RosterTemplateModel> Rosters { get; set; }

        public List<Guid> RosterScope { set; get; }
    }
}