using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionnaireLevelTemplateModel : RosterScopeBaseModel
    {
        public QuestionnaireLevelTemplateModel(QuestionnaireExecutorTemplateModel executorModel,
            bool areRowSpecificVariablesPresent, 
            bool isIRosterLevelInherited, 
            string rosterType)
            : base(
            null, 
            "@__questionnaire_scope", 
            "QuestionnaireTopLevel",
            new List<GroupTemplateModel>(), 
            new List<QuestionTemplateModel>(), 
            new List<RosterTemplateModel>(), 
            new List<Guid>(),
            areRowSpecificVariablesPresent,
            isIRosterLevelInherited,
            rosterType)
        {
            ExecutorModel = executorModel;
        }

        public QuestionnaireExecutorTemplateModel ExecutorModel { private set; get; }
    }
}