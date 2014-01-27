using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class QuestionnaireExportStructure : IVersionedView
    {
        public QuestionnaireExportStructure()
        {
            this.HeaderToLevelMap = new Dictionary<Guid, HeaderStructureForLevel>();
        }

        public QuestionnaireExportStructure(QuestionnaireDocument questionnaire, long version)
            : this()
        {
            QuestionnaireId = questionnaire.PublicKey;
            Version = version;

            questionnaire.ConnectChildrenWithParent();

            var maxValuesForRosterSizeQuestions = GetMaxValuesForRosterSizeQuestions(questionnaire);
            var questionnaireLevelStructure = new QuestionnaireRosterStructure(questionnaire, version);
            var referenceInfoForLinkedQuestions = new ReferenceInfoForLinkedQuestions(questionnaire, version);

            this.HeaderToLevelMap.Add(questionnaire.PublicKey,
                this.BuildHeaderByTemplate(questionnaire, questionnaire.PublicKey, questionnaireLevelStructure, referenceInfoForLinkedQuestions,
                    maxValuesForRosterSizeQuestions));

            foreach (var rosterScopeDescription in questionnaireLevelStructure.RosterScopes)
            {
                this.HeaderToLevelMap.Add(rosterScopeDescription.Key,
                    this.BuildHeaderByTemplate(questionnaire, rosterScopeDescription.Key, questionnaireLevelStructure,
                        referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions));
            }
        }

        public Guid QuestionnaireId { get; set; }
        public Dictionary<Guid, HeaderStructureForLevel> HeaderToLevelMap { get; set; }
        public long Version { get; set; }

        private static Dictionary<Guid, int> GetMaxValuesForRosterSizeQuestions(QuestionnaireDocument document)
        {
            IEnumerable<IAutoPropagateQuestion> autoPropagateQuestions = document.Find<IAutoPropagateQuestion>(question => true);

            var rosterGroups = document.Find<IGroup>(@group => @group.IsRoster && @group.RosterSizeQuestionId.HasValue);

            var fixedRosterGroups =
                document.Find<IGroup>(@group => @group.IsRoster && @group.RosterSizeSource == RosterSizeSourceType.FixedTitles);

            IEnumerable<INumericQuestion> rosterSizeNumericQuestions =
                rosterGroups.Select(@group => document.Find<INumericQuestion>(@group.RosterSizeQuestionId.Value))
                    .Where(question => question != null && question.MaxValue.HasValue).Distinct();

            IEnumerable<IMultyOptionsQuestion> rosterSizeMultyOptionQuestions =
                rosterGroups.Select(@group => document.Find<IMultyOptionsQuestion>(@group.RosterSizeQuestionId.Value))
                    .Where(question => question != null);

            IEnumerable<ITextListQuestion> rosterSizeTextListQuestions =
                rosterGroups.Select(@group => document.Find<ITextListQuestion>(@group.RosterSizeQuestionId.Value))
                    .Where(question => question != null);

            var collectedMaxValues = new Dictionary<Guid, int>();

            foreach (IAutoPropagateQuestion autoPropagateQuestion in autoPropagateQuestions)
            {
                collectedMaxValues.Add(autoPropagateQuestion.PublicKey, autoPropagateQuestion.MaxValue);
            }

            foreach (INumericQuestion rosterSizeNumericQuestion in rosterSizeNumericQuestions)
            {
                collectedMaxValues.Add(rosterSizeNumericQuestion.PublicKey, rosterSizeNumericQuestion.MaxValue.Value);
            }

            foreach (IMultyOptionsQuestion rosterSizeMultyOptionQuestion in rosterSizeMultyOptionQuestions)
            {
                collectedMaxValues.Add(rosterSizeMultyOptionQuestion.PublicKey, rosterSizeMultyOptionQuestion.Answers.Count);
            }

            foreach (ITextListQuestion rosterSizeTextListQuestion in rosterSizeTextListQuestions)
            {
                collectedMaxValues.Add(rosterSizeTextListQuestion.PublicKey, rosterSizeTextListQuestion.MaxAnswerCount ?? TextListQuestion.MaxAnswerCountLimit);
            }

            foreach (IGroup fixedRosterGroup in fixedRosterGroups)
            {
                collectedMaxValues.Add(fixedRosterGroup.PublicKey, fixedRosterGroup.RosterFixedTitles.Length);
            }

            return collectedMaxValues;
        }

        private HeaderStructureForLevel BuildHeaderByTemplate(QuestionnaireDocument questionnaire, Guid levelId,
            QuestionnaireRosterStructure questionnaireLevelStructure, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            var rootGroups = GetRootGroupsForLevel(questionnaire, questionnaireLevelStructure, levelId);
            return new HeaderStructureForLevel(rootGroups, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions, levelId);
        }

        private IEnumerable<IGroup> GetRootGroupsForLevel(QuestionnaireDocument questionnaire, QuestionnaireRosterStructure questionnaireLevelStructure, Guid levelId)
        {
            if (levelId == questionnaire.PublicKey)
            {
                yield return questionnaire;
                yield break;
            }

            var rootGroupsForLevel = GetRootGroupsByLevelIdOrThrow(questionnaireLevelStructure, levelId);

            foreach (var rootGroup in rootGroupsForLevel)
            {
                yield return questionnaire.FirstOrDefault<IGroup>(group => group.PublicKey == rootGroup);
            }
        }

        private HashSet<Guid> GetRootGroupsByLevelIdOrThrow(QuestionnaireRosterStructure questionnaireLevelStructure, Guid levelId)
        {
            if (!questionnaireLevelStructure.RosterScopes.ContainsKey(levelId))
                throw new InvalidOperationException("level is absent in template");

            return new HashSet<Guid>(questionnaireLevelStructure.RosterScopes[levelId].RosterIdToRosterTitleQuestionIdMap.Keys);
        }
    }
}
