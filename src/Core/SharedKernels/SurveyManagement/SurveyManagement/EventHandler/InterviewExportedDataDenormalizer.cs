﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class InterviewExportedDataDenormalizer : IEventHandler<InterviewApprovedByHQ>, IEventHandler
    {
        private readonly IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> questionnaireExportStructureWriter;
        private readonly IDataExportService dataExportService;

        public InterviewExportedDataDenormalizer(IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter, IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> questionnaireExportStructureWriter, IDataExportService dataExportService)
        {
            this.interviewDataWriter = interviewDataWriter;
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.dataExportService = dataExportService;
        }

        public string Name { get { return this.GetType().Name; } }

        public Type[] UsesViews
        {
            get { return new Type[] { typeof(InterviewData), typeof(QuestionnaireExportStructure) }; }
        }

        public Type[] BuildsViews { get { return new Type[] { typeof(InterviewDataExportView) }; } }

        public void Handle(IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            var interview = this.interviewDataWriter.GetById(evnt.EventSourceId);

            var exportStructure = this.questionnaireExportStructureWriter.GetById(interview.Document.QuestionnaireId,
                interview.Document.QuestionnaireVersion);

            var interviewDataExportView = new InterviewDataExportView(interview.Document.QuestionnaireId, interview.Document.QuestionnaireVersion,
                exportStructure.HeaderToLevelMap.Values.Select(
                    exportStructureForLevel => 
                        new InterviewDataExportLevelView(exportStructureForLevel.LevelId, exportStructureForLevel.LevelName, 
                            this.BuildRecordsForHeader(interview.Document, exportStructureForLevel))).ToArray());
            
            this.dataExportService.AddExportedDataByInterview(interviewDataExportView);
        }

        private InterviewDataExportRecord[] BuildRecordsForHeader(InterviewData interview, HeaderStructureForLevel headerStructureForLevel)
        {
            var dataRecords = new List<InterviewDataExportRecord>();

            var interviewDataByLevels = this.GetLevelsFromInterview(interview, headerStructureForLevel.LevelId);
          
            foreach (var dataByLevel in interviewDataByLevels)
            {
                var vectorLength = dataByLevel.RosterVector.Length;
                
                string recordId = vectorLength == 0 ?
                    interview.InterviewId.FormatGuid() :
                    dataByLevel.RosterVector.Last().ToString(CultureInfo.InvariantCulture);

                string parentRecordId = vectorLength == 0 ?
                    null:
                    (vectorLength == 1 ? interview.InterviewId.FormatGuid() : dataByLevel.RosterVector[vectorLength - 2].ToString(CultureInfo.InvariantCulture));

                string[] referenceValues = new string[0];

                if (headerStructureForLevel.IsTextListScope)
                {
                    referenceValues = new string[]{ GetTextValueForTextListQuestion(interview, dataByLevel.RosterVector, headerStructureForLevel.LevelId)};
                }

                dataRecords.Add(new InterviewDataExportRecord(interview.InterviewId, recordId, referenceValues, parentRecordId,
                    this.GetQuestionsForExport(dataByLevel.GetAllQuestions(), headerStructureForLevel)));
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

            for(var i = 1; i <= rosterVector.Length; i++)
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

        private ExportedQuestion[] GetQuestionsForExport(IEnumerable<InterviewQuestion> availableQuestions, HeaderStructureForLevel headerStructureForLevel)
        {
            var result = new List<ExportedQuestion>();
            foreach (var headerItem in headerStructureForLevel.HeaderItems.Values)
            {
                var question = availableQuestions.FirstOrDefault(q => q.Id == headerItem.PublicKey);
                ExportedQuestion exportedQuestion = null;
                if (question == null)
                {
                    var ansnwers = new List<string>();
                    for (int i = 0; i < headerItem.ColumnNames.Count(); i++)
                    {
                        ansnwers.Add(string.Empty);
                    }
                    exportedQuestion = new ExportedQuestion(headerItem.PublicKey, ansnwers.ToArray());
                }
                else
                {
                    exportedQuestion = new ExportedQuestion(question, headerItem);
                }

                result.Add(exportedQuestion);
            }
            return result.ToArray();
        }

        private IEnumerable<InterviewLevel> GetLevelsFromInterview(InterviewData interview, Guid levelId)
        {
            if (levelId == interview.QuestionnaireId)
                return interview.Levels.Values.Where(level => level.ScopeIds.ContainsKey(interview.InterviewId));
            return interview.Levels.Values.Where(level => level.ScopeIds.ContainsKey(levelId));
        }
    }
}
