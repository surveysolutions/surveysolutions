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
using Main.Core.View.Export;
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

        private readonly IVersionedReadSideRepositoryReader<QuestionnairePropagationStructure> questionnaireLevelStorage;

        public InterviewDataExportFactory(IReadSideRepositoryReader<InterviewData> interviewStorage,IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage,
                                          IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireStorage,
                                          IVersionedReadSideRepositoryReader<QuestionnairePropagationStructure>
                                              questionnaireLevelStorage)
        {
            this.interviewStorage = interviewStorage;
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireLevelStorage = questionnaireLevelStorage;
        }

        public InterviewDataExportView Load(InterviewDataExportInputModel input)
        {
            var questionnaire = GetQuestionnaireOrThrow(input.TemplateId, input.TemplateVersion).Questionnaire;
            var questionnaireLevelStructure = GetQuestionnaireLevelStructureOrThrow(input.TemplateId,
                                                                                    input.TemplateVersion);

            var header = BuildHeaderByTemplate(questionnaire, questionnaireLevelStructure, input.LevelId);

            var records = BuildRecordsForHeader(input.TemplateId, input.TemplateVersion,
                                                input.LevelId);

            var levelName = BuildLevelName(questionnaire, questionnaireLevelStructure, input.LevelId);

            var exportedData = new InterviewDataExportView(input.TemplateId, input.TemplateVersion, input.LevelId,
                                                           levelName,
                                                           header, records);
            return exportedData;
        }

        private string BuildLevelName(QuestionnaireDocument questionnaire, QuestionnairePropagationStructure questionnaireLevelStructure, Guid? levelId)
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
                            interview.Status == InterviewStatus.ApprovedBySupervisor)).Skip(returnedEventCount)
                        .Take(bulkSize).ToArray();
                bool allEventsWereAlreadyReturned = interviews.Length == 0;

                if (allEventsWereAlreadyReturned)
                     break;

                result.AddRange(interviews);

                returnedEventCount += bulkSize;
            }
            return result;
        }

        private InterviewDataExportRerord[] BuildRecordsForHeader(Guid templateId,
                                                                  long templateVersion, Guid? levelId)
        {
            var dataRecords = new List<InterviewDataExportRerord>();

            var interviews =
                GetApprovedInterviews(templateId, templateVersion)
                    .Select(interview => this.interviewStorage.GetById(interview.InterviewId));

            foreach (var interview in interviews)
            {
                FillDataRecoordsWithDataFromInterviewByLevel(dataRecords, interview, levelId);
            }
            
            return dataRecords.ToArray();
        }

        private void FillDataRecoordsWithDataFromInterviewByLevel(List<InterviewDataExportRerord> dataRecords,
                                                                  InterviewData interview, Guid? levelId)
        {
            var interviewDataByLevels = GetLevelsFromInterview(interview, levelId);
            int i = 1;
            foreach (var dataByLevel in interviewDataByLevels)
            {
                AddDataRecordFromInterviewLevel(dataRecords, dataByLevel,i, interview.InterviewId);
                i++;
            }
        }

        private IEnumerable<InterviewLevel> GetLevelsFromInterview(InterviewData interview, Guid? levelId)
        {
            if (!levelId.HasValue)
            {
                return interview.Levels.Values.Where(level => level.ScopeId == interview.InterviewId);
            }
            return interview.Levels.Values.Where(level => level.ScopeId == levelId.Value);
        }

        private void AddDataRecordFromInterviewLevel(List<InterviewDataExportRerord> dataRecords, InterviewLevel level,
                                                     int recordId,
                                                     Guid interviewId)
        {
#warning parentid is always null
            var record = new InterviewDataExportRerord(interviewId, recordId, null, BuildExportedQuestionsByLevel(level));
            dataRecords.Add(record);
        }

        private Dictionary<Guid, ExportedQuestion> BuildExportedQuestionsByLevel(InterviewLevel level)
        {
            var answeredQuestions = new Dictionary<Guid,ExportedQuestion>();

            foreach (var question in level.Questions)
            {
                if (!question.Enabled)
                    continue;

                var exportedQuestion = new ExportedQuestion(question.Id, GetAnswers(question));
                answeredQuestions.Add(question.Id, exportedQuestion);
            }

            return answeredQuestions;
        }

        private string[] GetAnswers(InterviewQuestion question)
        {
            if (question.Answer == null)
                return new string[0];

            var listOfAnswers = question.Answer as IEnumerable<object>;
            if (listOfAnswers == null)
                return new string[] {question.Answer.ToString()};
            return listOfAnswers.Select(a => a.ToString()).ToArray();
        }

        private QuestionnaireDocumentVersioned GetQuestionnaireOrThrow(Guid questionnaireId, long version)
        {
            var questionnaire = questionnaireStorage.GetById(questionnaireId,version);
            if (questionnaire == null)
                throw new InvalidOperationException("template is absent");
            return questionnaire;
        }

        private QuestionnairePropagationStructure GetQuestionnaireLevelStructureOrThrow(Guid questionnaireId, long version)
        {
            var questionnaireLevelStructure = questionnaireLevelStorage.GetById(questionnaireId, version);
            if (questionnaireLevelStructure == null)
                throw new InvalidOperationException("template is present but propagation structure is abseent is absent");
            return questionnaireLevelStructure;
        }

        private HeaderCollection BuildHeaderByTemplate(QuestionnaireDocument questionnaire, QuestionnairePropagationStructure questionnaireLevelStructure, Guid? levelId)
        {

            var result = new HeaderCollection();

            var rootGroups = GetRootGroupsForLevel(questionnaire,questionnaireLevelStructure, levelId);

            foreach (var rootGroup in rootGroups)
            {
                FillHeaderWithQuestionsInsideGroup(result, rootGroup);    
            }
            
            return result;
        }

        private IEnumerable<IGroup> GetRootGroupsForLevel(QuestionnaireDocument questionnaire, QuestionnairePropagationStructure questionnaireLevelStructure, Guid? levelId)
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

        private HashSet<Guid> GetRootGroupsByLevelIdOrThrow(QuestionnairePropagationStructure questionnaireLevelStructure, Guid levelId)
        {
            if (!questionnaireLevelStructure.PropagationScopes.ContainsKey(levelId))
                throw new InvalidOperationException("level is absent in template");
         
            return questionnaireLevelStructure.PropagationScopes[levelId];
        }

        private void FillHeaderWithQuestionsInsideGroup(HeaderCollection headerItems, IGroup @group)
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
                    if (innerGroup.Propagated != Propagate.None)
                        continue;
                    FillHeaderWithQuestionsInsideGroup(headerItems, innerGroup);
                }
            }
        }
    }
}
