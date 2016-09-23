using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class RosterStructureService  : IRostrerStructureService
    {
        public Dictionary<ValueVector<Guid>, RosterScopeDescription> GetRosterScopes(QuestionnaireDocument document)
        {
            var result = new Dictionary<ValueVector<Guid>, RosterScopeDescription>();
            var groupsMappedOnPropagatableQuestion = this.GetAllRosterScopesGroupedByRosterId(document);
            this.AddRosterScopes(document, groupsMappedOnPropagatableQuestion, result);

            return result;
        }

        private void AddRosterScopes(QuestionnaireDocument questionnaire, IDictionary<Guid, Guid> groupsMappedOnPropagatableQuestion,
           Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes)
        {
            var rosterGroups = questionnaire.Find<IGroup>(@group => @group.IsRoster && @group.RosterSizeSource == RosterSizeSourceType.Question);
            var fixedRosterGroups = questionnaire.Find<IGroup>(@group => @group.IsRoster && @group.RosterSizeSource == RosterSizeSourceType.FixedTitles).ToList();

            var rosterSizeQuestions = questionnaire.Find<IQuestion>(question => rosterGroups.Any(group => group.RosterSizeQuestionId.Value == question.PublicKey));

            foreach (var rosterSizeQuestion in rosterSizeQuestions)
            {
                var groupsFromRosterSizeQuestionScope =
                    rosterGroups.Where(group => group.RosterSizeQuestionId == rosterSizeQuestion.PublicKey).ToList();

                var scopeVectorsOfTriggers = groupsFromRosterSizeQuestionScope.Select(
                    roster =>
                        this.GetScopeOfQuestionnaireItem(roster,
                            groupsMappedOnPropagatableQuestion)).GroupBy(k => k);

                foreach (var scopeVectorsOfTrigger in scopeVectorsOfTriggers)
                {
                    var rosterIdWithTitleQuestionIds =
                        this.GetRosterIdToRosterTitleQuestionIdMapByRostersInScope(questionnaire,
                            groupsFromRosterSizeQuestionScope.Where(group =>
                                GetScopeOfQuestionnaireItem(group, groupsMappedOnPropagatableQuestion)
                                    .SequenceEqual(scopeVectorsOfTrigger.Key)));

                    var rosterDescription = new RosterScopeDescription(scopeVectorsOfTrigger.Key, rosterSizeQuestion.StataExportCaption,
                        GetRosterScopeTypeByQuestionType(rosterSizeQuestion.QuestionType), rosterIdWithTitleQuestionIds);

                    rosterScopes.Add(scopeVectorsOfTrigger.Key, rosterDescription);
                }
            }

            foreach (var fixedRosterGroup in fixedRosterGroups)
            {
                var scopeVector = this.GetScopeOfQuestionnaireItem(fixedRosterGroup, groupsMappedOnPropagatableQuestion);
                rosterScopes[scopeVector] =
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

        private IDictionary<Guid, Guid> GetAllRosterScopesGroupedByRosterId(QuestionnaireDocument template)
        {
            var result = template.Find<IGroup>(group => @group.IsRoster && @group.RosterSizeSource == RosterSizeSourceType.Question).ToDictionary(roster => roster.PublicKey, roster => roster.RosterSizeQuestionId.Value);

            foreach (var roster in template.Find<IGroup>(group => group.IsRoster && group.RosterSizeSource == RosterSizeSourceType.FixedTitles))
            {
                result.Add(roster.PublicKey, roster.PublicKey);
            }

            return result;
        }

        private ValueVector<Guid> GetScopeOfQuestionnaireItem(IComposite questionnaireItem,
            IDictionary<Guid, Guid> groupsMappedOnPropagatableQuestion)
        {
            var result = new List<Guid>();
            var questionParent = questionnaireItem;

            while (questionParent != null)
            {
                var group = questionParent as IGroup;
                if (group != null && (group.IsRoster))
                {
                    result.Add(groupsMappedOnPropagatableQuestion[group.PublicKey]);
                }
                questionParent = questionParent.GetParent();
            }
            result.Reverse();

            return new ValueVector<Guid>(result);
        }

        private Dictionary<Guid, RosterTitleQuestionDescription> GetRosterIdToRosterTitleQuestionIdMapByRostersInScope(QuestionnaireDocument questionnaire, IEnumerable<IGroup> groupsFromRosterSizeQuestionScope)
        {
            return
                groupsFromRosterSizeQuestionScope
                    .ToDictionary(roster => roster.PublicKey,
                        roster => roster.RosterTitleQuestionId.HasValue
                            ? this.CreateRosterTitleQuestionDescription(
                                questionnaire.FirstOrDefault<IQuestion>(question => question.PublicKey == roster.RosterTitleQuestionId.Value))
                            : null);
        }

        private RosterTitleQuestionDescription CreateRosterTitleQuestionDescription(IQuestion question)
        {
            return new RosterTitleQuestionDescription(question.PublicKey,
                question.Answers.ToDictionary(a => a.AnswerCode.Value, a => a.AnswerText));
        }
    }
   
}
