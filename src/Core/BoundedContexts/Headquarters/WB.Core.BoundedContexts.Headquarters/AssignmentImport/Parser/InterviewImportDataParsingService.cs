using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    internal class InterviewImportDataParsingService : IInterviewImportDataParsingService
    {
        private readonly IPreloadedDataRepository preloadedDataRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IUserViewFactory userViewFactory;
        private readonly IQuestionDataParser dataParser;
        private IPlainTransactionManager plainTransactionManager => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        public InterviewImportDataParsingService(
            IPreloadedDataRepository preloadedDataRepository, 
            IQuestionnaireStorage questionnaireStorage,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage, 
            IUserViewFactory userViewFactory, 
            IQuestionDataParser dataParser)
        {
            this.preloadedDataRepository = preloadedDataRepository;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.userViewFactory = userViewFactory;
            this.dataParser = dataParser;
        }

        public AssignmentImportData[] GetAssignmentsData(QuestionnaireIdentity questionnaireIdentity, AssignmentImportType mode)
        {
            IQuestionnaire questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            if (questionnaire == null)
                throw new Exception("Questionnaire was not found");

            QuestionnaireExportStructure exportStructure = this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(questionnaireIdentity);

            IEnumerable<PreloadedInterviewBaseLevel> levels = mode == AssignmentImportType.Panel
                ? CreateParsedInterviewLevels(this.preloadedDataRepository.GetPreloadedDataOfPanel(), exportStructure, questionnaire)
                : new[]
                {
                    new PreloadedInterviewQuestionnaireLevel(
                        this.preloadedDataRepository.GetPreloadedDataOfSample(),
                        exportStructure)
                };

            PreloadedInterviewData[] interviewsToPreload = ParseInterviews(levels.ToList(), questionnaire);

            var result = new List<AssignmentImportData>();
            var supervisorsCache = new Dictionary<string, Guid>();
            var interviewersCache = new Dictionary<string, UserView>();

            foreach (var interview in interviewsToPreload)
            {
                Guid? interviewerId = null;
                Guid? supervisorId = null;
                var interviewer = this.GetInterviewerIdAndUpdateCache(interviewersCache, interview.ResponsibleName);
                if (interviewer != null)
                {
                    interviewerId = interviewer.PublicKey;
                    supervisorId = interviewer.Supervisor.Id;
                }
                else
                {
                    supervisorId = this.GetSupervisorIdAndUpdateCache(supervisorsCache, interview.ResponsibleName);
                }

                result.Add(new AssignmentImportData
                {
                    PreloadedData = new PreloadedDataDto(interview.Answers),
                    SupervisorId = supervisorId,
                    InterviewerId = interviewerId,
                    Quantity = interview.Quantity
                });
            }

            return result.ToArray();
        }

        private List<InterviewAnswer> BuildAnswerForInterview(List<PreloadedInterviewBaseLevel> levels, string interviewId, IQuestionnaire questionnaire)
        {
            var result = new List<InterviewAnswer>();

            foreach (var level in levels)
            {
                if (!level.HasDataForInterview(interviewId))
                    continue;

                foreach (PreloadedQuestionMeta questionMeta in level.Questions)
                {
                    var nullableQuestionId = questionnaire.GetQuestionIdByVariable(questionMeta.Variable); //slow

                    if (nullableQuestionId == null)
                        continue;

                    var questionId = nullableQuestionId.Value;

                    foreach (var interviewRow in level.GetInterviewRows(interviewId))
                    {
                        var answersWithColumnName = interviewRow.GetAnswersWithColumnNames(questionMeta);
                        if (answersWithColumnName == null) continue;

                        var parsedAnswer = this.dataParser.BuildAnswerFromStringArray(
                            answersWithColumnName.ToArray(),
                            questionnaire.GetQuestionByVariable(questionMeta.Variable));

                        if (parsedAnswer == null) continue;

                        if (parsedAnswer is TextListAnswer textListAnswer &&
                            questionnaire.IsRosterSizeQuestion(questionId))
                        {
                            var rostersTriggeredByListQuestion = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToHashSet();
                            var questionVariableName = questionnaire.GetQuestionVariableName(questionId);
                            UpdateListValuesWithRowcodes(textListAnswer, levels, interviewId, interviewRow.RosterVector, rostersTriggeredByListQuestion, questionVariableName);
                        }

                        result.Add(new InterviewAnswer
                        {
                            Identity = new Identity(questionId, interviewRow.RosterVector),
                            Answer = parsedAnswer
                        });
                    }
                }
            }
            return result;
        }

        private void UpdateListValuesWithRowcodes(TextListAnswer textListAnswer,
            List<PreloadedInterviewBaseLevel> levels,
            string interviewId,
            RosterVector rosterVector, 
            HashSet<Guid> rosterIds, 
            string questionVariableName)
        {
            if (!(levels.Where(x => x is PreloadedInterviewLevel).Cast<PreloadedInterviewLevel>().FirstOrDefault(x => rosterIds.Contains(x.RosterId)) is PreloadedInterviewLevel rosterLevel))
                return;

            var listRowsWithNewCodes = rosterLevel.GetRowCodeAndTitlesPairs(interviewId, rosterVector);

            var hasNoRostersInformationForListQuestion = listRowsWithNewCodes == null || listRowsWithNewCodes.Length == 0;
            if (hasNoRostersInformationForListQuestion) return;

            var hasDifferentAmountOfRecordsInListAndRosters = textListAnswer.Rows.Count != listRowsWithNewCodes.Length;
            if (hasDifferentAmountOfRecordsInListAndRosters)
            {
                throw new InterviewImportException("PL0045", string.Format(PreloadingVerificationMessages.PL0045_TextListAnswerHasDifferentAmountOfRecordsThanRosterFile, 
                    rosterLevel.Name, listRowsWithNewCodes.Length, questionVariableName, textListAnswer.Rows.Count, interviewId));
            }

            var listItemsAbsentInRosterFile = textListAnswer.Rows.Select(x => x.Text).Except(listRowsWithNewCodes.Select(x => x.Text)).ToArray();
            var rosterRowsAbsentInListQuestion = listRowsWithNewCodes.Select(x => x.Text).Except(textListAnswer.Rows.Select(x => x.Text)).ToArray();
            var hasDifferentTextsInListAndRosterTitles = listItemsAbsentInRosterFile.Any();
            if (hasDifferentTextsInListAndRosterTitles)
            {
                throw new InterviewImportException("PL0046", string.Format(
                    PreloadingVerificationMessages.PL0046_HasDifferentTextsInListAndRosterTitles,
                    rosterLevel.Name, questionVariableName, string.Join(", ", listItemsAbsentInRosterFile), string.Join(", ", rosterRowsAbsentInListQuestion), interviewId));
            }

            foreach (var listRow in listRowsWithNewCodes)
            {
                var rowTitle = listRow.Text;
                var textListAnswerRow = textListAnswer.Rows.FirstOrDefault(x => x.Text == rowTitle);

                if (textListAnswerRow == null)
                    continue;

                textListAnswerRow.Value = listRow.Value;
            }

            if (textListAnswer.Rows.Select(x => x.Text).Distinct().Count() != textListAnswer.Rows.Count)
            {
                var duplicatedRecords = textListAnswer.Rows.Select(x => x.Text).GroupBy(x => x)
                    .Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
                throw new InterviewImportException("PL0043", string.Format(
                    PreloadingVerificationMessages.PL0043_TextListAnswerHasDuplicatesInTexts, 
                    questionVariableName, string.Join(", ", duplicatedRecords), interviewId));
            }

            if (textListAnswer.Rows.Select(x => x.Value).Distinct().Count() != textListAnswer.Rows.Count)
            {
                var duplicatedRecords = textListAnswer.Rows.Select(x => x.Value).GroupBy(x => x)
                    .Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
                throw new InterviewImportException("PL0044", string.Format(
                    PreloadingVerificationMessages.PL0044_TextListAnswerHasDuplicatesInCodes, 
                    questionVariableName, string.Join(", ", duplicatedRecords), interviewId));
            }
        }
        
        public PreloadedInterviewData[] ParseInterviews(List<PreloadedInterviewBaseLevel> preloadedInterviewBaseLevels, IQuestionnaire questionnaire)
        {
            if (!(preloadedInterviewBaseLevels.FirstOrDefault(x => x is PreloadedInterviewQuestionnaireLevel) is PreloadedInterviewQuestionnaireLevel questionnaireLevel))
                return new PreloadedInterviewData[0];

            var result = new List<PreloadedInterviewData>();

            foreach (PreloadedInterviewQuestionnaireRow interviewRow in questionnaireLevel.Interviews)
            {
                List<InterviewAnswer> answers = this.BuildAnswerForInterview(preloadedInterviewBaseLevels, interviewRow.InterviewId, questionnaire);

                result.Add(new PreloadedInterviewData
                {
                    Answers = answers,
                    ResponsibleName = interviewRow.ResponsibleName,
                    Quantity = interviewRow.Quantity
                });
            }

            return result.ToArray();
        }

        private IEnumerable<PreloadedInterviewBaseLevel> CreateParsedInterviewLevels(
            PreloadedDataByFile[] preloadedDataByFiles,
            QuestionnaireExportStructure exportStructure,
            IQuestionnaire questionnaire)
        {
            var questionnaireLevelName = exportStructure.HeaderToLevelMap.Values.FirstOrDefault(x => x.LevelScopeVector.Length == 0)?.LevelName.ToLowerInvariant();

            foreach (var preloadedDataByFile in preloadedDataByFiles)
            {
                var rosterVariableLowercased = Path.GetFileNameWithoutExtension(preloadedDataByFile.FileName)?.ToLower();

                if (questionnaireLevelName == rosterVariableLowercased)
                {
                    yield return new PreloadedInterviewQuestionnaireLevel(preloadedDataByFile, exportStructure);
                }
                else if (!string.IsNullOrWhiteSpace(rosterVariableLowercased))
                {
                    var rosterId = questionnaire.GetRosterIdByVariableName(rosterVariableLowercased, ignoreCase: true);
                    if (!rosterId.HasValue)
                        continue;
                    var rosterScope = ValueVector.Create(questionnaire.GetRosterSizeSourcesForEntity(rosterId.Value));
                    yield return new PreloadedInterviewLevel(preloadedDataByFile, rosterId.Value, rosterScope, exportStructure);
                }
            }
        }

        private Guid? GetSupervisorIdAndUpdateCache(Dictionary<string, Guid> cache, string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var userNameLowerCase = name.ToLower();
            if (cache.ContainsKey(userNameLowerCase))
                return cache[userNameLowerCase];

            var user = this.GetUserByName(userNameLowerCase);//assuming that user exists
            if (user == null || !user.IsSupervisor()) throw new Exception($"Supervisor with name '{name}' does not exists");

            cache.Add(userNameLowerCase, user.PublicKey);
            return user.PublicKey;
        }

        private UserView GetInterviewerIdAndUpdateCache(Dictionary<string, UserView> interviewerCache, string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var userNameLowerCase = name.ToLower();
            if (interviewerCache.ContainsKey(userNameLowerCase))
                return interviewerCache[userNameLowerCase];

            var user = this.GetUserByName(userNameLowerCase);//assuming that user exists
            if (user == null || !user.Roles.Contains(UserRoles.Interviewer)) return null;

            interviewerCache.Add(userNameLowerCase, user);
            return user;
        }

        protected UserView GetUserByName(string userName)
        {
            return this.plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                var user = this.userViewFactory.GetUser(new UserViewInputModel(UserName: userName, UserEmail: null));
                if (user == null || user.IsArchived)
                    return null;
                return user;
            });
        }
    }
}
