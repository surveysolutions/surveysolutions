using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Factories
{
    internal class ExportViewFactory : IExportViewFactory
    {
        private readonly IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory;
        
        public ExportViewFactory(IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory)
        {
            this.referenceInfoForLinkedQuestionsFactory = referenceInfoForLinkedQuestionsFactory;
        }

        public QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireDocument questionnaire, long version)
        {
            var result = new QuestionnaireExportStructure();
            result.QuestionnaireId = questionnaire.PublicKey;
            result.Version = version;

            questionnaire.ConnectChildrenWithParent();

            var maxValuesForRosterSizeQuestions = GetMaxValuesForRosterSizeQuestions(questionnaire);
            var questionnaireLevelStructure = new QuestionnaireRosterStructure(questionnaire, version);
            
            var referenceInfoForLinkedQuestions = referenceInfoForLinkedQuestionsFactory.CreateReferenceInfoForLinkedQuestions(questionnaire, version);

            result.HeaderToLevelMap.Add(questionnaire.PublicKey,
                this.BuildHeaderByTemplate(questionnaire, questionnaire.PublicKey, questionnaireLevelStructure, referenceInfoForLinkedQuestions,
                    maxValuesForRosterSizeQuestions));

            foreach (var rosterScopeDescription in questionnaireLevelStructure.RosterScopes)
            {
                result.HeaderToLevelMap.Add(rosterScopeDescription.Key,
                    this.BuildHeaderByTemplate(questionnaire, rosterScopeDescription.Key, questionnaireLevelStructure,
                        referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions));
            }

            return result;
        }

        protected HeaderStructureForLevel CreateHeaderStructureForLevel(
            IEnumerable<IGroup> groupsInLevel,
            ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions,
            Guid levelId)
        {
            var headerStructureForLevel = new HeaderStructureForLevel();
                //groupsInLevel, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions, levelId);
            headerStructureForLevel.LevelId = levelId;
            headerStructureForLevel.LevelIdColumnName = "Id";
            if (!groupsInLevel.Any())
                return headerStructureForLevel;

            headerStructureForLevel.LevelName = groupsInLevel.First().Title;

            foreach (var rootGroup in groupsInLevel)
            {
                FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, rootGroup, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions);
            }

            return headerStructureForLevel;
        }

        protected ExportedHeaderItem CreateExportedHeaderItem(IQuestion question)
        {
            var exportedHeaderItem = new ExportedHeaderItem();

            exportedHeaderItem.PublicKey = question.PublicKey;
            exportedHeaderItem.QuestionType = question.QuestionType;
            exportedHeaderItem.VariableName = question.StataExportCaption;
            exportedHeaderItem.Titles = new string[] { question.QuestionText };
            exportedHeaderItem.ColumnNames = new string[] { question.StataExportCaption };

            exportedHeaderItem.Labels = new Dictionary<Guid, LabelItem>();
            foreach (IAnswer answer in question.Answers)
            {
                exportedHeaderItem.Labels.Add(answer.PublicKey, new LabelItem(answer));
            }
            return exportedHeaderItem;
        }

        protected ExportedHeaderItem CreateExportedHeaderItem(IQuestion question, int columnCount)
        {
            var exportedHeaderItem = CreateExportedHeaderItem(question);
            this.ThrowIfQuestionIsNotMultiSelectOrTextList(question);

            exportedHeaderItem.ColumnNames = new string[columnCount];
            exportedHeaderItem.Titles = new string[columnCount];

            for (int i = 0; i < columnCount; i++)
            {
                exportedHeaderItem.ColumnNames[i] = string.Format("{0}_{1}", question.StataExportCaption, i);

                if (!IsQuestionLinked(question))
                {
                    if (question is IMultyOptionsQuestion)
                        exportedHeaderItem.Titles[i] += string.Format(":{0}", question.Answers[i].AnswerText);
                    if (question is ITextListQuestion)
                        exportedHeaderItem.Titles[i] += string.Format(":{0}", i);
                }
            }
            return exportedHeaderItem;
        }

        private static bool IsQuestionLinked(IQuestion question)
        {
            return question.LinkedToQuestionId.HasValue;
        }

        private void ThrowIfQuestionIsNotMultiSelectOrTextList(IQuestion question)
        {
            if (question.QuestionType != QuestionType.MultyOption && question.QuestionType != QuestionType.TextList)
                throw new InvalidOperationException(string.Format(
                    "question '{1}' with type '{0}' can't be exported as more then one column",
                    question.QuestionType, question.QuestionText));
        }

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
                    .Where(question => question != null).Distinct();

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
            return CreateHeaderStructureForLevel(rootGroups, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions, levelId);
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



        private void FillHeaderWithQuestionsInsideGroup(HeaderStructureForLevel headerStructureForLevel,IGroup @group, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            if (@group.RosterSizeSource == RosterSizeSourceType.FixedTitles && headerStructureForLevel.LevelLabels == null)
            {
                headerStructureForLevel.LevelLabels =
                    @group.RosterFixedTitles.Select((title, index) => new LabelItem() { Caption = index.ToString(), Title = title })
                        .ToArray();
            }

            foreach (var groupChild in @group.Children)
            {
                var question = groupChild as IQuestion;
                if (question != null)
                {
                    if (this.IsQuestionMultiOption(question))
                    {
                        if (question.LinkedToQuestionId.HasValue)
                            AddHeadersForLinkedMultiOptions(headerStructureForLevel.HeaderItems, question, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions);
                        else AddHeadersForMultiOptions(headerStructureForLevel.HeaderItems, question);
                    }
                    else if (this.IsQuestionTextList(question))
                    {
                        AddHeadersForTextList(headerStructureForLevel.HeaderItems, question);
                    }
                    else
                        AddHeaderForNotMultiOptions(headerStructureForLevel.HeaderItems, question);
                    continue;
                }

                var innerGroup = groupChild as IGroup;
                if (innerGroup != null)
                {
                    //### old questionnaires supporting        //### roster
                    if (innerGroup.Propagated != Propagate.None || innerGroup.IsRoster)
                        continue;
                    FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, innerGroup, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions);
                }
            }
        }

        private bool IsQuestionMultiOption(IQuestion question)
        {
            return question is IMultyOptionsQuestion;
        }

        private bool IsQuestionTextList(IQuestion question)
        {
            return question is ITextListQuestion;
        }

        private void AddHeadersForLinkedMultiOptions(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            headerItems.Add(question.PublicKey, CreateExportedHeaderItem(question, this.GetRosterSizeForLinkedQuestion(question, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions)));
        }

        protected void AddHeaderForNotMultiOptions(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question)
        {
            headerItems.Add(question.PublicKey, CreateExportedHeaderItem(question));
        }

        protected void AddHeadersForMultiOptions(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question)
        {
            var multiOptionQuestion = question as IMultyOptionsQuestion;
            var maxCount = (multiOptionQuestion == null ? null : multiOptionQuestion.MaxAllowedAnswers) ?? question.Answers.Count;
            headerItems.Add(question.PublicKey, CreateExportedHeaderItem(question, maxCount));
        }

        protected void AddHeadersForTextList(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question)
        {
            var textListQuestion = question as ITextListQuestion;
            var maxCount = (textListQuestion == null ? null : textListQuestion.MaxAnswerCount) ?? TextListQuestion.MaxAnswerCountLimit;
            headerItems.Add(question.PublicKey, CreateExportedHeaderItem(question, maxCount));
        }

        private int GetRosterSizeForLinkedQuestion(IQuestion question, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            var rosterSizeQuestionId =
                referenceInfoForLinkedQuestions.ReferencesOnLinkedQuestions[question.PublicKey].ScopeId;

            return maxValuesForRosterSizeQuestions[rosterSizeQuestionId];
        }
    }
}
