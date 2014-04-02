﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
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

            var interviewDataExportView= new InterviewDataExportView(interview.Document.QuestionnaireId, interview.Document.QuestionnaireVersion,
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
                string recordId = vectorLength == 0 ? interview.InterviewId.FormatGuid() : dataByLevel.RosterVector.Last().ToString(CultureInfo.InvariantCulture);

                string parentRecordId = vectorLength == 0
                    ? null
                    : (vectorLength == 1 ? interview.InterviewId.FormatGuid() : dataByLevel.RosterVector[vectorLength - 2].ToString(CultureInfo.InvariantCulture));

                dataRecords.Add(new InterviewDataExportRecord(interview.InterviewId, recordId, parentRecordId,
                    this.GetQuestionsFroExport(dataByLevel.GetAllQuestions(), headerStructureForLevel)));
            }

            return dataRecords.ToArray();
        }

        private ExportedQuestion[] GetQuestionsFroExport(IEnumerable<InterviewQuestion> availableQuestions, HeaderStructureForLevel headerStructureForLevel)
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
