using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class InterviewDataExportFactory : IViewFactory<InterviewDataExportInputModel, InterviewDataExportView>
    {
        private readonly IReadSideRepositoryReader<InterviewData> interviewStorage;

        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;

        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireStorage;

        private readonly IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireLevelStorage;

        private readonly IVersionedReadSideRepositoryReader<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions;

        public InterviewDataExportFactory(IReadSideRepositoryReader<InterviewData> interviewStorage,IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage,
                                          IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireStorage,
                                          IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure>
                                              questionnaireLevelStorage, IVersionedReadSideRepositoryReader<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions)
        {
            this.interviewStorage = interviewStorage;
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireLevelStorage = questionnaireLevelStorage;
            this.questionnaireReferenceInfoForLinkedQuestions = questionnaireReferenceInfoForLinkedQuestions;
        }

        public InterviewDataExportView Load(InterviewDataExportInputModel input)
        {
            var questionnaire = GetQuestionnaireOrThrow(input.TemplateId, input.TemplateVersion).Questionnaire;
            var questionnaireLevelStructure = GetQuestionnaireLevelStructureOrThrow(input.TemplateId,
                                                                                    input.TemplateVersion);

            var questionnaireReferenceInfo = GetQuestionnaireReferenceInfoForLinkedQuestions(input.TemplateId,
                                                                                    input.TemplateVersion);

            var header = BuildHeaderByTemplate(questionnaire, questionnaireLevelStructure, questionnaireReferenceInfo, input.LevelId);

            var records = BuildRecordsForHeader(input.TemplateId, input.TemplateVersion,
                                                input.LevelId, header);

            var levelName = BuildLevelName(questionnaire, questionnaireLevelStructure, input.LevelId);

            var exportedData = new InterviewDataExportView(input.TemplateId, input.TemplateVersion, input.LevelId,
                                                           levelName,
                                                           header, records);
            return exportedData;
        }

        private string BuildLevelName(QuestionnaireDocument questionnaire, QuestionnaireRosterStructure questionnaireLevelStructure, Guid? levelId)
        {
            var rootGroups = GetRootGroupsForLevel(questionnaire, questionnaireLevelStructure, levelId);
            return rootGroups.First().Title;
        }

        public IList<InterviewSummary> GetApprovedInterviews(Guid templateId,
                                                                  long templateVersion, int bulkSize=256)
        {
            var result = new List<InterviewSummary>();
            int returnedEventCount = 0;
            while (true)
            {
                var interviews =
                    this.interviewSummaryStorage.Query(
                        _ =>
                        _.Where(
                            interview =>
                            interview.QuestionnaireId == templateId && interview.QuestionnaireVersion == templateVersion &&
                            interview.Status == InterviewStatus.ApprovedBySupervisor).Skip(returnedEventCount)
                        .Take(bulkSize).ToArray());

                result.AddRange(interviews);

                bool allEventsWereAlreadyReturned = interviews.Length < bulkSize;

                if (allEventsWereAlreadyReturned)
                     break;

                returnedEventCount += bulkSize;
            }
            return result;
        }

        private InterviewDataExportRerord[] BuildRecordsForHeader(Guid templateId,
                                                                  long templateVersion, Guid? levelId, ExportedHeaderCollection header)
        {
            var dataRecords = new List<InterviewDataExportRerord>();

            var interviews =
                GetApprovedInterviews(templateId, templateVersion)
                    .Select(interview => this.interviewStorage.GetById(interview.InterviewId));

            int recordId = 0;

            foreach (var interview in interviews)
            {
                recordId = this.FillDataRecordsWithDataFromInterviewByLevelAndReturnIndexOfNextRecord(dataRecords, interview, levelId, header, recordId);
            }
            
            return dataRecords.ToArray();
        }

        private int FillDataRecordsWithDataFromInterviewByLevelAndReturnIndexOfNextRecord(List<InterviewDataExportRerord> dataRecords,
            InterviewData interview, Guid? levelId, ExportedHeaderCollection header, int recordIndex)
        {
            var interviewDataByLevels = GetLevelsFromInterview(interview, levelId);

            foreach (var dataByLevel in interviewDataByLevels)
            {
                AddDataRecordFromInterviewLevel(dataRecords, dataByLevel, recordIndex, interview.InterviewId, header);
                if (levelId.HasValue) recordIndex++;
            }

            //increase only in case of top level, if I increase record index inside roaster, linked questions data would be broken
            recordIndex = !levelId.HasValue ? recordIndex + 1 : 0;
            return recordIndex;
        }

        private IEnumerable<InterviewLevel> GetLevelsFromInterview(InterviewData interview, Guid? levelId)
        {
            if (!levelId.HasValue)
            {
                return interview.Levels.Values.Where(level => level.ScopeIds.ContainsKey(interview.InterviewId));
            }
            return interview.Levels.Values.Where(level => level.ScopeIds.ContainsKey(levelId.Value));
        }

        private void AddDataRecordFromInterviewLevel(List<InterviewDataExportRerord> dataRecords, InterviewLevel level,
                                                     int recordId,
                                                     Guid interviewId, ExportedHeaderCollection header)
        {
#warning parentid is always null
            var record = new InterviewDataExportRerord(interviewId, recordId, GetParentRecordIndex(level), BuildExportedQuestionsByLevel(level, header));
            dataRecords.Add(record);
        }

        private decimal? GetParentRecordIndex(InterviewLevel level)
        {
            if (level.RosterVector.Length < 2)
                return null;
            return level.RosterVector[level.RosterVector.Length - 2];
        }

        private Dictionary<Guid, ExportedQuestion> BuildExportedQuestionsByLevel(InterviewLevel level,  ExportedHeaderCollection header)
        {
            var answeredQuestions = new Dictionary<Guid,ExportedQuestion>();

            foreach (var question in level.GetAllQuestions())
            {
                var headerItem = header[question.Id];

                if (headerItem == null)
                    continue;

                var exportedQuestion = new ExportedQuestion(question, headerItem);
                answeredQuestions.Add(question.Id, exportedQuestion);
            }

            return answeredQuestions;
        }

        private QuestionnaireDocumentVersioned GetQuestionnaireOrThrow(Guid questionnaireId, long version)
        {
            var questionnaire = questionnaireStorage.GetById(questionnaireId,version);
            if (questionnaire == null)
                throw new InvalidOperationException("template is absent");
            return questionnaire;
        }

        private QuestionnaireRosterStructure GetQuestionnaireLevelStructureOrThrow(Guid questionnaireId, long version)
        {
            var questionnaireLevelStructure = questionnaireLevelStorage.GetById(questionnaireId, version);
            if (questionnaireLevelStructure == null)
                throw new InvalidOperationException("template is present but propagation structure is absent is absent");
            return questionnaireLevelStructure;
        }

        private ReferenceInfoForLinkedQuestions GetQuestionnaireReferenceInfoForLinkedQuestions(Guid templateId, long templateVersion)
        {
            var questionnaireLevelStructure = questionnaireReferenceInfoForLinkedQuestions.GetById(templateId, templateVersion);
            if (questionnaireLevelStructure == null)
                throw new InvalidOperationException("template is present but reference structure structure is absent is absent");
            return questionnaireLevelStructure;
        }

        private ExportedHeaderCollection BuildHeaderByTemplate(QuestionnaireDocument questionnaire, QuestionnaireRosterStructure questionnaireLevelStructure, ReferenceInfoForLinkedQuestions questionnaireReferenceInfoForLinkedQuestions, Guid? levelId)
        {

            var result = new ExportedHeaderCollection(questionnaireReferenceInfoForLinkedQuestions, questionnaire);

            var rootGroups = GetRootGroupsForLevel(questionnaire,questionnaireLevelStructure, levelId);

            foreach (var rootGroup in rootGroups)
            {
                FillHeaderWithQuestionsInsideGroup(result, rootGroup);    
            }
            
            return result;
        }

        private IEnumerable<IGroup> GetRootGroupsForLevel(QuestionnaireDocument questionnaire, QuestionnaireRosterStructure questionnaireLevelStructure, Guid? levelId)
        {
            if (!levelId.HasValue)
            {
                yield return questionnaire;
                yield break;
            }

            var rootGroupsForLevel = GetRootGroupsByLevelIdOrThrow(questionnaireLevelStructure, levelId.Value);

            foreach (var rootGroup in rootGroupsForLevel)
            {
                yield return questionnaire.FirstOrDefault<IGroup>(group => group.PublicKey == rootGroup);
            }
        }

        private HashSet<Guid> GetRootGroupsByLevelIdOrThrow(QuestionnaireRosterStructure questionnaireLevelStructure, Guid levelId)
        {
            if (!questionnaireLevelStructure.RosterScopes.ContainsKey(levelId))
                throw new InvalidOperationException("level is absent in template");
         
            return questionnaireLevelStructure.RosterScopes[levelId];
        }

        private void FillHeaderWithQuestionsInsideGroup(ExportedHeaderCollection headerItems, IGroup @group)
        {
            foreach (var groupChild in @group.Children)
            {
                var question = groupChild as IQuestion;
                if (question != null)
                {
                    headerItems.Add(question);
                    continue;
                }
                var innerGroup = groupChild as IGroup;
                if (innerGroup != null)
                {
                        //### old questionnaires supporting        //### roster
                    if (innerGroup.Propagated != Propagate.None || innerGroup.IsRoster)
                        continue;
                    FillHeaderWithQuestionsInsideGroup(headerItems, innerGroup);
                }
            }
        }
    }
}
