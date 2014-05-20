using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Factories
{
    internal class QuestionnaireRosterStructureFactory : IQuestionnaireRosterStructureFactory
    {
        public QuestionnaireRosterStructure CreateQuestionnaireRosterStructure(QuestionnaireDocument questionnaire, long version)
        {
            questionnaire.ConnectChildrenWithParent();
            var result = new QuestionnaireRosterStructure();
            result.QuestionnaireId = questionnaire.PublicKey;
            result.Version = version;

            var groupsMappedOnPropagatableQuestion = this.GetAllRosterScopesGroupedByRosterId(questionnaire);

            this.AddRosterScopesByAutoPropagatedQuestionsBackwardCompatibility(questionnaire, groupsMappedOnPropagatableQuestion, result.RosterScopes);
            this.AddRosterScopes(questionnaire, groupsMappedOnPropagatableQuestion, result.RosterScopes);
           
            return result;
        }

        private void AddRosterScopesByAutoPropagatedQuestionsBackwardCompatibility(QuestionnaireDocument questionnaire, IDictionary<Guid, Guid> groupsMappedOnPropagatableQuestion, Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes)
        {
            var autoPropagatebleQuestions =
                questionnaire.Find<IAutoPropagateQuestion>(
                    question =>
                        question.QuestionType == QuestionType.Numeric || question.QuestionType == QuestionType.AutoPropagate);

            foreach (var autoPropagatebleQuestion in autoPropagatebleQuestions)
            {
                var rosterIdMappedOfRosterTitleQuestionId = this.GetRosterIdToRosterTitleQuestionIdMapByAutopropagatedQuestion(questionnaire, autoPropagatebleQuestion);
                var scopeVectorsOfTriggers = autoPropagatebleQuestion.Triggers.Select(
                    trigger =>
                        this.GetScopeOfQuestionnaireItem(questionnaire.FirstOrDefault<IGroup>(g => g.PublicKey == trigger),
                            groupsMappedOnPropagatableQuestion)).GroupBy(k=>k);
                foreach (var scopeVectorsOfTrigger in scopeVectorsOfTriggers)
                {

                    var rosterDescription = new RosterScopeDescription(scopeVectorsOfTrigger.Key, string.Empty, RosterScopeType.Numeric,
                        rosterIdMappedOfRosterTitleQuestionId);

                    rosterScopes.Add(scopeVectorsOfTrigger.Key, rosterDescription);
                }
            }
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
            var result = new Dictionary<Guid, Guid>();

            foreach (var scope in template.Find<IAutoPropagateQuestion>(
                question =>
                    question.QuestionType == QuestionType.Numeric || question.QuestionType == QuestionType.AutoPropagate))
            {
                foreach (var triggarableGroup in scope.Triggers)
                {
                    result.Add(triggarableGroup, scope.PublicKey);
                }
            }

            foreach (var roster in template.Find<IGroup>(group => group.IsRoster && group.RosterSizeSource == RosterSizeSourceType.Question))
            {
                result.Add(roster.PublicKey, roster.RosterSizeQuestionId.Value);
            }

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
                if (group != null && (group.Propagated != Propagate.None || group.IsRoster))
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
            RosterTitleQuestionDescription capitalQuestion = this.GetRosterTitleQuestionDescriptionBasedOnCapitalQuestionsInsideGroups(groupsFromRosterSizeQuestionScope);
            return
                groupsFromRosterSizeQuestionScope
                    .ToDictionary(roster => roster.PublicKey,
                        roster => roster.RosterTitleQuestionId.HasValue
                            ? this.CreateRosterTitleQuestionDescription(
                                questionnaire.FirstOrDefault<IQuestion>(question => question.PublicKey == roster.RosterTitleQuestionId.Value))
                            : capitalQuestion);
        }

        private Dictionary<Guid, RosterTitleQuestionDescription> GetRosterIdToRosterTitleQuestionIdMapByAutopropagatedQuestion(QuestionnaireDocument questionnaire, IAutoPropagateQuestion autoPropagateQuestion)
        {
            IEnumerable<IGroup> groupsFromAutoPropagatedQuestionScope =
                questionnaire.Find<IGroup>(group => autoPropagateQuestion.Triggers.Contains(group.PublicKey));

            RosterTitleQuestionDescription capitalQuestion = this.GetRosterTitleQuestionDescriptionBasedOnCapitalQuestionsInsideGroups(groupsFromAutoPropagatedQuestionScope);

            var rosterGroupsWithTitleQuestionPairs = new Dictionary<Guid, RosterTitleQuestionDescription>();

            foreach (var rosterGroup in groupsFromAutoPropagatedQuestionScope)
            {
                rosterGroupsWithTitleQuestionPairs.Add(rosterGroup.PublicKey, capitalQuestion);
            }
            return rosterGroupsWithTitleQuestionPairs;
        }

        private RosterTitleQuestionDescription GetRosterTitleQuestionDescriptionBasedOnCapitalQuestionsInsideGroups(IEnumerable<IGroup> groups)
        {
            var capitalQuestions =
               groups.SelectMany(rosterGroup => rosterGroup.Find<IQuestion>(question => question.Capital));

            if (capitalQuestions.Any())
            {
                return this.CreateRosterTitleQuestionDescription(capitalQuestions.First());
            }
            return null;
        }

        private RosterTitleQuestionDescription CreateRosterTitleQuestionDescription(IQuestion question)
        {
            return new RosterTitleQuestionDescription(question.PublicKey,
                question.Answers.ToDictionary(a => decimal.Parse(a.AnswerValue), a => a.AnswerText));
        }
    }
}
