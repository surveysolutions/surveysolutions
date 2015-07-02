using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    internal class ExportViewFactory : IExportViewFactory
    {
        private readonly IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory;
        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ExportViewFactory(IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory,
            IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory, IFileSystemAccessor fileSystemAccessor)
        {
            this.referenceInfoForLinkedQuestionsFactory = referenceInfoForLinkedQuestionsFactory;
            this.questionnaireRosterStructureFactory = questionnaireRosterStructureFactory;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireDocument questionnaire, long version)
        {
            var result = new QuestionnaireExportStructure();
            result.QuestionnaireId = questionnaire.PublicKey;
            result.Version = version;

            questionnaire.ConnectChildrenWithParent();

            var maxValuesForRosterSizeQuestions = GetMaxValuesForRosterSizeQuestions(questionnaire);
            var questionnaireLevelStructure = this.questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnaire,
                version);

            var referenceInfoForLinkedQuestions =
                this.referenceInfoForLinkedQuestionsFactory.CreateReferenceInfoForLinkedQuestions(questionnaire, version);

            result.HeaderToLevelMap.Add(new ValueVector<Guid>(),
                this.BuildHeaderByTemplate(questionnaire, new ValueVector<Guid>(), questionnaireLevelStructure,
                    referenceInfoForLinkedQuestions,
                    maxValuesForRosterSizeQuestions));

            foreach (var rosterScopeDescription in questionnaireLevelStructure.RosterScopes)
            {
                result.HeaderToLevelMap.Add(rosterScopeDescription.Key,
                    this.BuildHeaderByTemplate(questionnaire, rosterScopeDescription.Key, questionnaireLevelStructure,
                        referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions));
            }

            return result;
        }

        public InterviewDataExportView CreateInterviewDataExportView(QuestionnaireExportStructure exportStructure, InterviewData interview)
        {
            return new InterviewDataExportView(interview.InterviewId, interview.QuestionnaireId,
                interview.QuestionnaireVersion,
                exportStructure.HeaderToLevelMap.Values.Select(
                    exportStructureForLevel =>
                        new InterviewDataExportLevelView(exportStructureForLevel.LevelScopeVector, exportStructureForLevel.LevelName,
                            this.BuildRecordsForHeader(interview, exportStructureForLevel), interview.InterviewId.FormatGuid())).ToArray());
        }

        private InterviewDataExportRecord[] BuildRecordsForHeader(InterviewData interview, HeaderStructureForLevel headerStructureForLevel)
        {
            var dataRecords = new List<InterviewDataExportRecord>();

            var interviewDataByLevels = this.GetLevelsFromInterview(interview, headerStructureForLevel.LevelScopeVector);

            foreach (var dataByLevel in interviewDataByLevels)
            {
                var vectorLength = dataByLevel.RosterVector.Length;

                string recordId = vectorLength == 0
                    ? interview.InterviewId.FormatGuid()
                    : dataByLevel.RosterVector.Last().ToString(CultureInfo.InvariantCulture);

                var parentRecordIds = new string[dataByLevel.RosterVector.Length];
                if (parentRecordIds.Length > 0)
                {
                    parentRecordIds[0] = interview.InterviewId.FormatGuid();
                    for (int i = 0; i < dataByLevel.RosterVector.Length - 1; i++)
                    {
                        parentRecordIds[i + 1] = dataByLevel.RosterVector[i].ToString(CultureInfo.InvariantCulture);
                    }

                    parentRecordIds = parentRecordIds.Reverse().ToArray();
                }

                string[] referenceValues = new string[0];

                if (headerStructureForLevel.IsTextListScope)
                {
                    referenceValues = new string[]
                    {
                        GetTextValueForTextListQuestion(interview, dataByLevel.RosterVector, headerStructureForLevel.LevelScopeVector.Last())
                    };
                }

                dataRecords.Add(new InterviewDataExportRecord(interview.InterviewId, recordId, referenceValues, parentRecordIds,
                    this.GetQuestionsForExport(dataByLevel, headerStructureForLevel)));
            }

            return dataRecords.ToArray();
        }

        private string CreateLevelIdFromPropagationVector(decimal[] vector)
        {
            return vector.Length == 0 ? "#" : EventHandlerUtils.CreateLeveKeyFromPropagationVector(vector);
        }

        private string GetTextValueForTextListQuestion(InterviewData interview, decimal[] rosterVector, Guid id)
        {
            decimal itemToSearch = rosterVector.Last();

            for (var i = 1; i <= rosterVector.Length; i++)
            {
                var levelForVector =
                    interview.Levels.SingleOrDefault(
                        l => l.Key == CreateLevelIdFromPropagationVector(rosterVector.Take(rosterVector.Length - i).ToArray()));

                var questionToCheck = levelForVector.Value.GetQuestion(id);

                if (questionToCheck == null)
                    continue;
                if (questionToCheck.Answer == null)
                    return string.Empty;
                var interviewTextListAnswer = questionToCheck.Answer as InterviewTextListAnswers;

                if (interviewTextListAnswer == null)
                    return string.Empty;
                var item = interviewTextListAnswer.Answers.SingleOrDefault(a => a.Value == itemToSearch);

                return item != null ? item.Answer : string.Empty;
            }

            return string.Empty;
        }

        private ExportedQuestion[] GetQuestionsForExport(InterviewLevel availableQuestions,
            HeaderStructureForLevel headerStructureForLevel)
        {
            var result = new List<ExportedQuestion>();
            foreach (var headerItem in headerStructureForLevel.HeaderItems.Values)
            {
                var question = availableQuestions.GetQuestion(headerItem.PublicKey);
                ExportedQuestion exportedQuestion = null;
                if (question == null)
                {
                    var ansnwers = new List<string>();
                    for (int i = 0; i < headerItem.ColumnNames.Count(); i++)
                    {
                        ansnwers.Add(string.Empty);
                    }
                    exportedQuestion = new ExportedQuestion(headerItem.PublicKey, headerItem.QuestionType, ansnwers.ToArray());
                }
                else
                {
                    exportedQuestion = new ExportedQuestion(question, headerItem);
                }

                result.Add(exportedQuestion);
            }
            return result.ToArray();
        }

        private IEnumerable<InterviewLevel> GetLevelsFromInterview(InterviewData interview, ValueVector<Guid> levelVector)
        {
            if (!levelVector.Any())
                return interview.Levels.Values.Where(level => level.ScopeVectors.ContainsKey(new ValueVector<Guid>()));
            return interview.Levels.Values.Where(level => level.ScopeVectors.ContainsKey(levelVector));
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
                this.FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, rootGroup, referenceInfoForLinkedQuestions,
                    maxValuesForRosterSizeQuestions);
            }

            return headerStructureForLevel;
        }

        protected ExportedHeaderItem CreateExportedHeaderItem(IQuestion question, int? lengthOfRosterVectorWhichNeedToBeExported)
        {
            var exportedHeaderItem = new ExportedHeaderItem();

            exportedHeaderItem.PublicKey = question.PublicKey;
            exportedHeaderItem.QuestionType = question.QuestionType;
            exportedHeaderItem.VariableName = question.StataExportCaption;
            exportedHeaderItem.Titles = new[]
            { string.IsNullOrEmpty(question.VariableLabel) ? question.QuestionText : question.VariableLabel };
            exportedHeaderItem.ColumnNames = new string[] { question.StataExportCaption };

            exportedHeaderItem.Labels = new Dictionary<Guid, LabelItem>();
            foreach (var answer in question.Answers)
            {
                exportedHeaderItem.Labels.Add(answer.PublicKey, new LabelItem(answer));
            }

            if (lengthOfRosterVectorWhichNeedToBeExported.HasValue)
            {
                exportedHeaderItem.LengthOfRosterVectorWhichNeedToBeExported = lengthOfRosterVectorWhichNeedToBeExported;
            }

            return exportedHeaderItem;
        }

        protected ExportedHeaderItem CreateExportedHeaderItem(IQuestion question, int columnCount,
            int? lengthOfRosterVectorWhichNeedToBeExported)
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
                collectedMaxValues.Add(rosterSizeTextListQuestion.PublicKey,
                    rosterSizeTextListQuestion.MaxAnswerCount ?? TextListQuestion.MaxAnswerCountLimit);
            }

            foreach (IGroup fixedRosterGroup in fixedRosterGroups)
            {
                collectedMaxValues.Add(fixedRosterGroup.PublicKey, fixedRosterGroup.FixedRosterTitles.Length);
            }

            return collectedMaxValues;
        }

        private HeaderStructureForLevel BuildHeaderByTemplate(QuestionnaireDocument questionnaire, ValueVector<Guid> levelVector,
            QuestionnaireRosterStructure questionnaireLevelStructure, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            var rootGroups = this.GetRootGroupsForLevel(questionnaire, questionnaireLevelStructure, levelVector);

            if (!rootGroups.Any())
                throw new InvalidOperationException("level is absent in template");

            var firstRootGroup = rootGroups.First();
            var levelTitle = firstRootGroup.VariableName ?? fileSystemAccessor.MakeValidFileName(firstRootGroup.Title);

            var structures = this.CreateHeaderStructureForLevel(levelTitle, rootGroups, referenceInfoForLinkedQuestions,
                maxValuesForRosterSizeQuestions, levelVector);

            if (questionnaireLevelStructure.RosterScopes.ContainsKey(levelVector) &&
                questionnaireLevelStructure.RosterScopes[levelVector].ScopeType == RosterScopeType.TextList)
            {
                structures.IsTextListScope = true;
                structures.ReferencedNames = new string[] { questionnaireLevelStructure.RosterScopes[levelVector].ScopeTriggerName };
            }

            return structures;
        }

        private IEnumerable<IGroup> GetRootGroupsForLevel(QuestionnaireDocument questionnaire,
            QuestionnaireRosterStructure questionnaireLevelStructure, ValueVector<Guid> levelVector)
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

        private HashSet<Guid> GetRootGroupsByLevelIdOrThrow(QuestionnaireRosterStructure questionnaireLevelStructure,
            ValueVector<Guid> levelVector)
        {
            if (!questionnaireLevelStructure.RosterScopes.ContainsKey(levelVector))
                throw new InvalidOperationException("level is absent in template");

            return new HashSet<Guid>(questionnaireLevelStructure.RosterScopes[levelVector].RosterIdToRosterTitleQuestionIdMap.Keys);
        }

        private void FillHeaderWithQuestionsInsideGroup(HeaderStructureForLevel headerStructureForLevel, IGroup @group,
            ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            if (@group.RosterSizeSource == RosterSizeSourceType.FixedTitles && headerStructureForLevel.LevelLabels == null)
            {
                headerStructureForLevel.LevelLabels =
                    @group.FixedRosterTitles.Select(title => new LabelItem() { Caption = title.Value.ToString(CultureInfo.InvariantCulture), Title = title.Title })
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
                            this.AddHeadersForLinkedMultiOptions(headerStructureForLevel.HeaderItems, question,
                                referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions);
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
                    if (innerGroup.IsRoster)
                        continue;
                    this.FillHeaderWithQuestionsInsideGroup(headerStructureForLevel, innerGroup, referenceInfoForLinkedQuestions,
                        maxValuesForRosterSizeQuestions);
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

        private void AddHeadersForLinkedMultiOptions(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question,
            ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            headerItems.Add(question.PublicKey,
                CreateExportedHeaderItem(question,
                    this.GetRosterSizeForLinkedQuestion(question, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions),
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, referenceInfoForLinkedQuestions)));
        }

        protected void AddHeaderForNotMultiOptions(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question,
            ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions)
        {
            headerItems.Add(question.PublicKey,
                CreateExportedHeaderItem(question,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, referenceInfoForLinkedQuestions)));
        }

        protected void AddHeadersForMultiOptions(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question,
            ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions)
        {
            var multiOptionQuestion = question as IMultyOptionsQuestion;
            var maxCount = (multiOptionQuestion == null ? null : multiOptionQuestion.MaxAllowedAnswers) ?? question.Answers.Count;
            headerItems.Add(question.PublicKey,
                CreateExportedHeaderItem(question, maxCount,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, referenceInfoForLinkedQuestions)));
        }

        protected void AddHeadersForTextList(IDictionary<Guid, ExportedHeaderItem> headerItems, IQuestion question,
            ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions)
        {
            var textListQuestion = question as ITextListQuestion;
            var maxCount = (textListQuestion == null ? null : textListQuestion.MaxAnswerCount) ?? TextListQuestion.MaxAnswerCountLimit;
            headerItems.Add(question.PublicKey,
                CreateExportedHeaderItem(question, maxCount,
                    this.GetLengthOfRosterVectorWhichNeedToBeExported(question, referenceInfoForLinkedQuestions)));
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

        private int GetLengthOfRosterVectorWhichNeedToBeExported(ValueVector<Guid> scopeOfLinkedQuestion,
            ValueVector<Guid> scopeOfReferenceQuestion)
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
