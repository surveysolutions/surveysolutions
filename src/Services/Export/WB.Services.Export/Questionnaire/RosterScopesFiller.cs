using System;
using System.Collections.Generic;
using System.Linq;
using SoftCircuits.Collections;
using WB.Services.Export.Interview;

namespace WB.Services.Export.Questionnaire
{
     public class RosterScopesFiller
    {
        private readonly QuestionnaireDocument document;
        
        public RosterScopesFiller(QuestionnaireDocument document)
        {
            this.document = document;
        }
        
        public Dictionary<ValueVector<Guid>, RosterScopeDescription> FillRosterScopes()
        {
            Dictionary<ValueVector<Guid>, RosterScopeDescription> result = new Dictionary<ValueVector<Guid>, RosterScopeDescription>();

            var groupsByRosterSizeSourceLookup = this.document.FindInDepth<Group>().Where(g => g.IsRoster).ToLookup(r => r.RosterSizeSource);
            var groupsMappedOnPropagatableQuestion = this.GetAllRosterScopesGroupedByRosterId(groupsByRosterSizeSourceLookup);
            var questionsByPublicKeyDictionary = this.document.FindInDepth<Question>().ToDictionary(q => q.PublicKey);
            var rosterGroups = groupsByRosterSizeSourceLookup[RosterSizeSourceType.Question].ToLookup(@group => @group.RosterSizeQuestionId);
            var rosterSizeQuestions = this.document.Find<Question>(question => rosterGroups[question.PublicKey].Any()).ToList();
            
            foreach (var rosterSizeQuestion in rosterSizeQuestions)
            {
                var scopeVectorsOfTriggers = rosterGroups[rosterSizeQuestion.PublicKey]
                    .Select(x => this.GetScopeOfQuestionnaireItem(groupsMappedOnPropagatableQuestion, x))
                    .GroupBy(k => k)
                    .ToList();

                foreach (var scopeVectorsOfTrigger in scopeVectorsOfTriggers)
                {
                    var rosterIdWithTitleQuestionIds =
                        this.GetRosterIdToRosterTitleQuestionIdMapByRostersInScope(
                            questionsByPublicKeyDictionary,
                            rosterGroups[rosterSizeQuestion.PublicKey]
                                .Where(group => this.GetScopeOfQuestionnaireItem(groupsMappedOnPropagatableQuestion, group)
                                    .SequenceEqual(scopeVectorsOfTrigger.Key)));

                    var rosterDescription = new RosterScopeDescription(scopeVectorsOfTrigger.Key, rosterSizeQuestion.VariableName,
                        this.GetRosterScopeTypeByQuestionType(rosterSizeQuestion.QuestionType), rosterIdWithTitleQuestionIds);

                    result.Add(new ValueVector<Guid>(scopeVectorsOfTrigger.Key), rosterDescription);
                }
            }

            foreach (var fixedRosterGroup in groupsByRosterSizeSourceLookup[RosterSizeSourceType.FixedTitles])
            {
                var scopeVector = this.GetScopeOfQuestionnaireItem(groupsMappedOnPropagatableQuestion, fixedRosterGroup);

                result[scopeVector] =
                    new RosterScopeDescription(scopeVector,
                        string.Empty, RosterScopeType.Fixed,
                        new OrderedDictionary<Guid, RosterTitleQuestionDescription?> { { fixedRosterGroup.PublicKey, null } });
            }

            return result;
        }

        private RosterScopeType GetRosterScopeTypeByQuestionType(QuestionType questionType)
        {
            switch (questionType)
            {
                case QuestionType.Numeric:
                    return RosterScopeType.Numeric;
                case QuestionType.MultyOption:
                    return RosterScopeType.MultyOption;
                case QuestionType.TextList:
                    return RosterScopeType.TextList;
            }
            return RosterScopeType.Numeric;
        }

        private Dictionary<Guid, Guid> GetAllRosterScopesGroupedByRosterId(ILookup<RosterSizeSourceType, Group> groupsByRosterSizeSourceLookup)
        {
            var result = 
                groupsByRosterSizeSourceLookup[RosterSizeSourceType.Question]
                    .Where(x=>x.RosterSizeQuestionId != null)
                    .ToDictionary(roster => roster.PublicKey, roster => roster.RosterSizeQuestionId!.Value);

            foreach (var roster in groupsByRosterSizeSourceLookup[RosterSizeSourceType.FixedTitles])
            {
                result.Add(roster.PublicKey, roster.PublicKey);
            }

            return result;
        }

        private ValueVector<Guid> GetScopeOfQuestionnaireItem(Dictionary<Guid, Guid> groupsMappedOnPropagatableQuestion, 
            IQuestionnaireEntity questionnaireItem)
        {
            var result = new List<Guid>();
            var questionParent = questionnaireItem;

            while (questionParent != null)
            {
                if (questionParent is Group @group && (group.IsRoster))
                {
                    result.Insert(0, groupsMappedOnPropagatableQuestion[group.PublicKey]);
                }
                questionParent = questionParent.GetParent();
            }

            return new ValueVector<Guid>(result);
        }

        private OrderedDictionary<Guid, RosterTitleQuestionDescription?> GetRosterIdToRosterTitleQuestionIdMapByRostersInScope(
            Dictionary<Guid, Question> questionsByPublicKeyDictionary,
            IEnumerable<Group> groupsFromRosterSizeQuestionScope)
        {
            return
                groupsFromRosterSizeQuestionScope
                    .ToOrderedDictionary(roster => roster.PublicKey,
                        roster => CreateRosterTitleQuestionDescription(questionsByPublicKeyDictionary, roster.RosterTitleQuestionId));
        }

        private RosterTitleQuestionDescription? CreateRosterTitleQuestionDescription(Dictionary<Guid, Question> questionsByPublicKeyDictionary, Guid? questionId)
        {
            if (questionId == null) return null;

            if (questionsByPublicKeyDictionary.TryGetValue(questionId.Value, out var question))
            {
                return new RosterTitleQuestionDescription(question.PublicKey,
                   questionsByPublicKeyDictionary[questionId.Value].Answers.ToDictionary(a => a.AnswerValue, a => a.AnswerText));
            }

            return null;
        }
    }
}
