using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionnaireLevelTemplateModel : RosterScopeBaseModel
    {
        public QuestionnaireLevelTemplateModel(QuestionnaireExecutorTemplateModel executorModel)
        {
            this.GeneratedTypeName = "QuestionnaireTopLevel";
            this.GeneratedRosterScopeName = "@__questionnaire_scope";

            this.Questions = new List<QuestionTemplateModel>();
            this.Groups = new List<GroupTemplateModel>();
            this.Rosters = new List<RosterTemplateModel>();

            this.ParentScope = null;
            this.RosterScope = new List<Guid>();

            this.ExecutorModel = executorModel;
        }

        public QuestionnaireExecutorTemplateModel ExecutorModel { private set; get; }
    }
}