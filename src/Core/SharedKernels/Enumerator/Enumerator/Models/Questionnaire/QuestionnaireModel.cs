using System;
using System.Collections.Generic;

using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;

namespace WB.Core.BoundedContexts.Tester.Implementation.Entities
{
    public class QuestionnaireModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public Dictionary<Guid, GroupModel> GroupsWithFirstLevelChildrenAsReferences { set; get; }

        public Dictionary<Guid, BaseQuestionModel> Questions { get; set; }

        public Dictionary<Guid, StaticTextModel> StaticTexts { get; set; }

        public List<QuestionnaireReferenceModel> PrefilledQuestionsIds { get; set; }

        public Dictionary<Guid, List<GroupReferenceModel>> Parents { get; set; }

        public Dictionary<string, BaseQuestionModel> QuestionsByVariableNames { get; set; }

        public List<GroupsHierarchyModel> GroupsHierarchy { get; set; }

        public Dictionary<Guid, int> GroupsRosterLevelDepth { get; set; }

        public Dictionary<Guid, Guid?> GroupsParentIdMap { get; set; }

        public Dictionary<Guid, Guid?> QuestionsNearestRosterIdMap { get; set; }

        public IntegerNumericQuestionModel GetIntegerNumericQuestion(Guid questionId)
        {
            return this.GetQuestion<IntegerNumericQuestionModel>(questionId);
        }

        public RealNumericQuestionModel GetRealNumericQuestion(Guid questionId)
        {
            return this.GetQuestion<RealNumericQuestionModel>(questionId);
        }

        public TextQuestionModel GetTextQuestion(Guid questionId)
        {
            return this.GetQuestion<TextQuestionModel>(questionId);
        }

        public TextListQuestionModel GetTextListQuestion(Guid questionId)
        {
            return this.GetQuestion<TextListQuestionModel>(questionId);
        }

        public SingleOptionQuestionModel GetSingleOptionQuestion(Guid questionId)
        {
            return this.GetQuestion<SingleOptionQuestionModel>(questionId);
        }

        public MultiOptionQuestionModel GetMultiOptionQuestion(Guid questionId)
        {
            return this.GetQuestion<MultiOptionQuestionModel>(questionId);
        }

        public MultimediaQuestionModel GetMultimediaQuestion(Guid questionId)
        {
            return this.GetQuestion<MultimediaQuestionModel>(questionId);
        }
        
        public LinkedSingleOptionQuestionModel GetLinkedSingleOptionQuestion(Guid questionId)
        {
            return this.GetQuestion<LinkedSingleOptionQuestionModel>(questionId);
        }

        public LinkedMultiOptionQuestionModel GetLinkedMultiOptionQuestion(Guid questionId)
        {
            return this.GetQuestion<LinkedMultiOptionQuestionModel>(questionId);
        }

        public T GetQuestion<T>(Guid questionId) where T : BaseQuestionModel
        {
            return (T)this.Questions[questionId];
        }
    }
}
