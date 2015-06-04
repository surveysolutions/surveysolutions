using System;
using System.Collections.Generic;

using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class QuestionnaireModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public Dictionary<Guid, GroupModel> GroupsWithoutNestedChildren { set; get; }

        public Dictionary<Guid, BaseQuestionModel> Questions { get; set; }

        public Dictionary<Guid, StaticTextModel> StaticTexts { get; set; }

        public List<QuestionnaireReferenceModel> PrefilledQuestionsIds { get; set; }

        public Dictionary<Guid, List<QuestionnaireReferenceModel>> Parents { get; set; }

        public Dictionary<string, BaseQuestionModel> QuestionsByVariableNames { get; set; }

        public List<GroupsHierarchyModel> GroupsHierarchy { get; set; }
    }
}
