using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.Interview;

namespace WB.Services.Export.Questionnaire
{
     public class RosterScopesFiller
    {
        private readonly QuestionnaireDocument document;
        private ILookup<RosterSizeSourceType, Group> groupsByRosterSizeSourceLookup;
        private Dictionary<Guid, Guid> groupsMappedOnPropagatableQuestion;
        private Dictionary<Guid, Question> questionsByPublicKeyDictionary;
        private List<Question> rosterSizeQuestions;
        private ILookup<Guid?, Group> rosterGroups;
        
        public RosterScopesFiller(QuestionnaireDocument document)
        {
            this.document = document;
        }

        public Dictionary<ValueVector<Guid>, RosterScopeDescription> Result { get; } = new Dictionary<ValueVector<Guid>, RosterScopeDescription>();

        public void FillRosterScopes()
        {
            this.groupsByRosterSizeSourceLookup = this.document.Find<Group>().Where(g => g.IsRoster).ToLookup(r => r.RosterSizeSource);
            this.groupsMappedOnPropagatableQuestion = this.GetAllRosterScopesGroupedByRosterId();
            this.questionsByPublicKeyDictionary = this.document.Find<Question>().ToDictionary(q => q.PublicKey);
            this.rosterGroups = this.groupsByRosterSizeSourceLookup[RosterSizeSourceType.Question].ToLookup(@group => @group.RosterSizeQuestionId);
            this.rosterSizeQuestions = this.document.Find<Question>(question => this.rosterGroups[question.PublicKey].Any()).ToList();
            
            foreach (var rosterSizeQuestion in this.rosterSizeQuestions)
            {
                var scopeVectorsOfTriggers = this.rosterGroups[rosterSizeQuestion.PublicKey].Select(this.GetScopeOfQuestionnaireItem).GroupBy(k => k).ToList();

                foreach (var scopeVectorsOfTrigger in scopeVectorsOfTriggers)
                {
                    var rosterIdWithTitleQuestionIds =
                        this.GetRosterIdToRosterTitleQuestionIdMapByRostersInScope(
                            this.rosterGroups[rosterSizeQuestion.PublicKey].Where(group => this.GetScopeOfQuestionnaireItem(group)
                                    .SequenceEqual(scopeVectorsOfTrigger.Key)));

                    var rosterDescription = new RosterScopeDescription(scopeVectorsOfTrigger.Key, rosterSizeQuestion.VariableName,
                        this.GetRosterScopeTypeByQuestionType(rosterSizeQuestion.QuestionType), rosterIdWithTitleQuestionIds);

                    this.Result.Add(new ValueVector<Guid>(scopeVectorsOfTrigger.Key), rosterDescription);
                }
            }

            foreach (var fixedRosterGroup in this.groupsByRosterSizeSourceLookup[RosterSizeSourceType.FixedTitles])
            {
                var scopeVector = this.GetScopeOfQuestionnaireItem(fixedRosterGroup);

                Result[scopeVector] =
                    new RosterScopeDescription(scopeVector,
                        string.Empty, RosterScopeType.Fixed,
                        new Dictionary<Guid, RosterTitleQuestionDescription> { { fixedRosterGroup.PublicKey, null } });
            }
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

        private Dictionary<Guid, Guid> GetAllRosterScopesGroupedByRosterId()
        {
            var result = this.groupsByRosterSizeSourceLookup[RosterSizeSourceType.Question].ToDictionary(roster => roster.PublicKey, roster => roster.RosterSizeQuestionId.Value);

            foreach (var roster in this.groupsByRosterSizeSourceLookup[RosterSizeSourceType.FixedTitles])
            {
                result.Add(roster.PublicKey, roster.PublicKey);
            }

            return result;
        }

        private ValueVector<Guid> GetScopeOfQuestionnaireItem(IQuestionnaireEntity questionnaireItem)
        {
            var result = new List<Guid>();
            var questionParent = questionnaireItem;

            while (questionParent != null)
            {
                var group = questionParent as Group;
                if (group != null && (group.IsRoster))
                {
                    result.Insert(0, groupsMappedOnPropagatableQuestion[group.PublicKey]);
                }
                questionParent = questionParent.GetParent();
            }

            return new ValueVector<Guid>(result);
        }

        private Dictionary<Guid, RosterTitleQuestionDescription> GetRosterIdToRosterTitleQuestionIdMapByRostersInScope(IEnumerable<Group> groupsFromRosterSizeQuestionScope)
        {
            return
                groupsFromRosterSizeQuestionScope
                    .ToDictionary(roster => roster.PublicKey,
                        roster => CreateRosterTitleQuestionDescription(roster.RosterTitleQuestionId));
        }

        private RosterTitleQuestionDescription CreateRosterTitleQuestionDescription(Guid? questionId)
        {
            if (questionId == null) return null;

            Question question;
            if (this.questionsByPublicKeyDictionary.TryGetValue(questionId.Value, out question))
            {
                return new RosterTitleQuestionDescription(question.PublicKey,
                   this.questionsByPublicKeyDictionary[questionId.Value].Answers.ToDictionary(a => a.AnswerValue, a => a.AnswerText));
            }

            return null;
        }
    }
}
