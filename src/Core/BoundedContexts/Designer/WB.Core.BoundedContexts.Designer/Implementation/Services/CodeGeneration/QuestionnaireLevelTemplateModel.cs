using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnaireLevelTemplateModel : IRosterScope
    {
        public QuestionnaireLevelTemplateModel(QuestionnaireExecutorTemplateModel executorModel)
        {
            this.Questions = new List<QuestionTemplateModel>();
            this.Groups = new List<GroupTemplateModel>();
            this.Rosters = new List<RosterTemplateModel>();
            this.ExecutorModel = executorModel;
        }

        public List<QuestionTemplateModel> Questions { set; get; }
        public List<GroupTemplateModel> Groups { set; get; }
        public List<RosterTemplateModel> Rosters { set; get; }
        public QuestionnaireExecutorTemplateModel ExecutorModel { private set; get; }

        public string GeneratedRosterScopeName
        {
            set { }
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
            return this.GeneratedTypeName;
        }

        public IEnumerable<QuestionTemplateModel> GetAllQuestionsToTop()
        {
            return this.Questions;
        }

        public IEnumerable<RosterTemplateModel> GetAllRostersToTop()
        {
            return this.Rosters;
        }

        public List<Guid> GetRosterScope()
        {
            return new List<Guid>();
        }
    }
}