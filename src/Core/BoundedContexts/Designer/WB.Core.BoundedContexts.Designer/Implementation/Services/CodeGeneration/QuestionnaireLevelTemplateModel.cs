using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnaireLevelTemplateModel : IParent {
        
        public List<QuestionTemplateModel> Questions { private set; get; }
        public List<GroupTemplateModel> Groups { private set; get; }

        public string GeneratedTypeName {
            get { return "QuestionnaireLevel"; }
        }

        public IParent GetParent()
        {
            return null;
        }

        public string GetTypeName()
        {
            return GeneratedTypeName;
        }

        public IEnumerable<QuestionTemplateModel> GetQuestions()
        {
            return Questions;
        }
    }
}