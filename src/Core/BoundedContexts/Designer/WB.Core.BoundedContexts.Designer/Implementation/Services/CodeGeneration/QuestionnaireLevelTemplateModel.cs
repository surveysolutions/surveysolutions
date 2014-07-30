using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnaireLevelTemplateModel : IRosterScope
    {

        public QuestionnaireLevelTemplateModel()
        {
            this.Questions = new List<QuestionTemplateModel>();
            this.Groups = new List<GroupTemplateModel>();
            this.Rosters = new List<RosterTemplateModel>();
        }

        public List<QuestionTemplateModel> Questions { set; get; }
        public List<GroupTemplateModel> Groups {  set; get; }
        public List<RosterTemplateModel> Rosters { set; get; }

        public string GeneratedRosterScopeName {
            set {} 
            get { return "@__questionnaire_scope"; }
            
        }

        public string GeneratedTypeName 
        {
            get { return "QuestionnaireTopLevel"; }
        }

        public IRosterScope GetParentScope()
        {
            return null;
        }

        public string GetTypeName()
        {
            return GeneratedTypeName;
        }

        public IEnumerable<QuestionTemplateModel> GetQuestions()
        {
            return this.Questions;
        }

        public IEnumerable<RosterTemplateModel> GetRosters()
        {
            return this.Rosters;
        }

        public List<Guid> GetRosterScope()
        {
            return new List<Guid>();
        }
    }
}