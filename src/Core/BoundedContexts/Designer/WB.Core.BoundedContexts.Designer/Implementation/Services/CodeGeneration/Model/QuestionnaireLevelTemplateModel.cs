using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionnaireLevelTemplateModel : RosterScopeBaseModel
    {
        public QuestionnaireLevelTemplateModel(QuestionnaireExecutorTemplateModel executorModel,bool generateEmbeddedExpressionMethods)
            : base(generateEmbeddedExpressionMethods, null, "@__questionnaire_scope", "QuestionnaireTopLevel",
                new List<GroupTemplateModel>(), new List<QuestionTemplateModel>(), new List<RosterTemplateModel>(), new List<Guid>())
        {
            ExecutorModel = executorModel;
            Version = executorModel.Version;
        }

        public QuestionnaireExecutorTemplateModel ExecutorModel { private set; get; }
    }
}