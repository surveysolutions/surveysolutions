using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    internal class ExportViewFactory : IExportViewFactory
    {
        private readonly IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory;
        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;

        public ExportViewFactory(IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory,
            IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory)
        {
            this.referenceInfoForLinkedQuestionsFactory = referenceInfoForLinkedQuestionsFactory;
            this.questionnaireRosterStructureFactory = questionnaireRosterStructureFactory;
        }

        public QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireDocument questionnaire, long version)
        {
            var result = new QuestionnaireExportStructure();
            result.QuestionnaireId = questionnaire.PublicKey;
            result.Version = version;

            questionnaire.ConnectChildrenWithParent();

            var maxValuesForRosterSizeQuestions = GetMaxValuesForRosterSizeQuestions(questionnaire);
            var questionnaireLevelStructure = this.questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnaire, version);
            
            var referenceInfoForLinkedQuestions = this.referenceInfoForLinkedQuestionsFactory.CreateReferenceInfoForLinkedQuestions(questionnaire, version);

            result.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                this.BuildHeaderByTemplate(questionnaire, new ValueVector<Guid>(), questionnaireLevelStructure, referenceInfoForLinkedQuestions,
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
            string levelTitle,
            IEnumerable<IGroup> groupsInLevel,
            ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions,
            ValueVector<Guid> levelVector)
        {
            var headerStructureForLevel = new HeaderStructureForLevel();
            headerStructureForLevel.LevelScopeVector = levelVector;
            headerStructureForLevel.LevelIdColumnName = "Id";

            headerStructureForLevel.LevelName = levelTitle;

            foreach (var rootGroup in groupsInLevel)
            {
                this.FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, rootGroup, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions);
            }

            return headerStructureForLevel;
        }

        protected ExportedHeaderItem CreateExportedHeaderItem(IQuestion question, int? lengthOfRosterVectorWhichNeedToBeExported)
        {
            var exportedHeaderItem = new ExportedHeaderItem();

            exportedHeaderItem.PublicKey = question.PublicKey;
            exportedHeaderItem.QuestionType = question.QuestionType;
            exportedHeaderItem.VariableName = question.StataExportCaption;
            exportedHeaderItem.Titles = new []{ string.IsNullOrEmpty(question.VariableLabel) ? question.QuestionText : question.VariableLabel };
            exportedHeaderItem.ColumnNames = new string[] { question.StataExportCaption };

            exportedHeaderItem.Labels = new Dictionary<Guid, LabelItem>();
            foreach (IAnswer answer in question.Answers)
            {
                exportedHeaderItem.Labels.Add(answer.PublicKey, new LabelItem(answer));
            }

            if (lengthOfRosterVectorWhichNeedToBeExported.HasValue)
            {
                exportedHeaderItem.LengthOfRosterVectorWhichNeedToBeExported = lengthOfRosterVectorWhichNeedToBeExported;
            }

            return exportedHeaderItem;
        }

        protected ExportedHeaderItem CreateExportedHeaderItem(IQuestion question, int columnCount, int? lengthOfRosterVectorWhichNeedToBeExported)
        {
            var exportedHeaderItem = this.CreateExportedHeaderItem(question, lengthOfRosterVectorWhichNeedToBeExported);
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
                    .Where(question => question != null).Distinct();

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

        private HeaderStructureForLevel BuildHeaderByTemplate(QuestionnaireDocument questionnaire, ValueVector<Guid>  levelVector,
            QuestionnaireRosterStructure questionnaireLevelStructure, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            var rootGroups = this.GetRootGroupsForLevel(questionnaire, questionnaireLevelStructure, levelVector);

            if(!rootGroups.Any())
                throw new InvalidOperationException("level is absent in template");

            var firstRootGroup = rootGroups.First();
            var levelTitle = firstRootGroup.VariableName ?? firstRootGroup.Title;
            
            var structures = this.CreateHeaderStructureForLevel(levelTitle, rootGroups, referenceInfoForLinkedQuestions,
                maxValuesForRosterSizeQuestions, levelVector);

            if (questionnaireLevelStructure.RosterScopes.ContainsKey(levelVector) && questionnaireLevelStructure.RosterScopes[levelVector].ScopeType==RosterScopeType.TextList)
            {
                structures.IsTextListScope = true;
                structures.ReferencedNames = new string[]{questionnaireLevelStructure.RosterScopes[levelVector].ScopeTriggerName};
            }

            return structures;
        }

        private IEnumerable<IGroup> GetRootGroupsForLevel(QuestionnaireDocument questionnaire, QuestionnaireRosterStructure questionnaireLevelStructure, ValueVector<Guid> levelVector)
        {
            if (!levelVector.Any())
            {
                yield return questionnaire;
                yield break;
            }

            var rootGroupsForLevel = this.GetRootGroupsByLevelIdOrThrow(questionnaireLevelStructure, levelVector);

            foreach (var rootGroup in rootGroupsForLevel)
            {
                yield return questionnaire.FirstOrDefault<IGroup>(group => group.PublicKey == rootGroup);
            }
        }

        private HashSet<Guid> GetRootGroupsByLevelIdOrThrow(QuestionnaireRosterStructure questionnaireLevelStructure, ValueVector<Guid> levelVector)
        {
            if (!questionnaireLevelStructure.RosterScopes.ContainsKey(levelVector))
                throw new InvalidOperationException("level is absent in template");

            return new HashSet<Guid>(questionnaireLevelStructure.RosterScopes[levelVector].RosterIdToRosterTitleQuestionIdMap.Keys);
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
                            this.AddHeadersForLinkedMultiOptions(headerStructureForLevel.HeaderItems, question, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions);
                        else this.AddHeadersForMultiOptions(headerStructureForLevel.HeaderItems, question, referenceInfoForLinkedQuestions);
                    }
                    else if (this.IsQuestionTextList(question))
                    {
                        this.AddHeadersForTextList(headerStructureForLevel.HeaderItems, question, referenceInfoForLinkedQuestions);
                    }
                    else
                        this.AddHeaderForNotMultiOptions(headerStructureForLevel.HeaderItems, question, referenceInfoForLinkedQuestions);
                    continue;
                }

                var innerGroup = groupChild as IGroup;
                if (innerGroup != null)
                {
                    //### old questionnaires supporting        //### roster
                    if (innerGroup.Propagated != Propagate.None || innerGroup.IsRoster)
                        continue;
                    this.FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, innerGroup, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions);
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
            headerItems.Add(question.PublicKey, CreateExportedHeaderItem(question, this.GetRosterSizeForLinkedQuestion(question, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions), this.GetLengthOfRosterVectorWhichNeedToBeExported(question, referenceInfoForLinkedQuestions)));
        }

        protected void AddHeaderForNotMultiOptions(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions)
        {
            headerItems.Add(question.PublicKey, CreateExportedHeaderItem(question, this.GetLengthOfRosterVectorWhichNeedToBeExported(question, referenceInfoForLinkedQuestions)));
        }

        protected void AddHeadersForMultiOptions(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions)
        {
            var multiOptionQuestion = question as IMultyOptionsQuestion;
            var maxCount = (multiOptionQuestion == null ? null : multiOptionQuestion.MaxAllowedAnswers) ?? question.Answers.Count;
            headerItems.Add(question.PublicKey, CreateExportedHeaderItem(question, maxCount, this.GetLengthOfRosterVectorWhichNeedToBeExported(question, referenceInfoForLinkedQuestions)));
        }

        protected void AddHeadersForTextList(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions)
        {
            var textListQuestion = question as ITextListQuestion;
            var maxCount = (textListQuestion == null ? null : textListQuestion.MaxAnswerCount) ?? TextListQuestion.MaxAnswerCountLimit;
            headerItems.Add(question.PublicKey, CreateExportedHeaderItem(question, maxCount, this.GetLengthOfRosterVectorWhichNeedToBeExported(question, referenceInfoForLinkedQuestions)));
        }

        private int GetRosterSizeForLinkedQuestion(IQuestion question, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            var rosterSizeQuestionId =
                referenceInfoForLinkedQuestions.ReferencesOnLinkedQuestions[question.PublicKey].ReferencedQuestionRosterScope.Last();

            return maxValuesForRosterSizeQuestions[rosterSizeQuestionId];
        }

        private int? GetLengthOfRosterVectorWhichNeedToBeExported(IQuestion question,
            ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions)
        {
            if (!referenceInfoForLinkedQuestions.ReferencesOnLinkedQuestions.ContainsKey(question.PublicKey))
                return null;
            return
                GetLengthOfRosterVectorWhichNeedToBeExported(
                    referenceInfoForLinkedQuestions.ReferencesOnLinkedQuestions[question.PublicKey].LinkedQuestionRosterScope,
                    referenceInfoForLinkedQuestions.ReferencesOnLinkedQuestions[question.PublicKey].ReferencedQuestionRosterScope);
        }

        private int GetLengthOfRosterVectorWhichNeedToBeExported(ValueVector<Guid> scopeOfLinkedQuestion, ValueVector<Guid> scopeOfReferenceQuestion)
        {
            if (scopeOfLinkedQuestion.Length > scopeOfReferenceQuestion.Length)
            {
                return 1;
            }

            for (int i = 0; i < Math.Min(scopeOfLinkedQuestion.Length, scopeOfReferenceQuestion.Length); i++)
            {
                if (scopeOfReferenceQuestion[i] != scopeOfLinkedQuestion[i])
                {
                    return scopeOfReferenceQuestion.Length - i;
                }
            }

            if (scopeOfLinkedQuestion.Length == scopeOfReferenceQuestion.Length)
            {
                return 1;
            }

            return scopeOfReferenceQuestion.Length - scopeOfLinkedQuestion.Length;
        }
    }
}
