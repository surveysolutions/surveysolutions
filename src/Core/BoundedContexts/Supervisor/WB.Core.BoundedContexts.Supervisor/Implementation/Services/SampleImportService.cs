﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.SampleImport;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services
{
    internal class SampleImportService : ISampleImportService
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDocumentVersioned> templateRepository;
        private readonly IReadSideRepositoryWriter<QuestionnaireBrowseItem> templateSmallRepository;
        private readonly ITemporaryDataStorage<TempFileImportData> tempImportStorage;
        private readonly ITemporaryDataStorage<SampleCreationStatus> tempSampleCreationStorage;
        private readonly IQuestionnaireFactory questionnaireFactory;


        public SampleImportService(IReadSideRepositoryWriter<QuestionnaireDocumentVersioned> templateRepository,
                                   IReadSideRepositoryWriter<QuestionnaireBrowseItem> templateSmallRepository, 
            ITemporaryDataStorage<TempFileImportData> tempImportStorage,
            ITemporaryDataStorage<SampleCreationStatus> tempSampleCreationStorage,
            IQuestionnaireFactory questionnaireFactory)
        {
            this.templateRepository = templateRepository;
            this.templateSmallRepository = templateSmallRepository;
            this.tempImportStorage = tempImportStorage;
            this.tempSampleCreationStorage = tempSampleCreationStorage;
            this.questionnaireFactory = questionnaireFactory;
        }

        public Guid ImportSampleAsync(Guid templateId, ISampleRecordsAccessor recordAccessor)
        {
            Guid importId = Guid.NewGuid();
            tempImportStorage.Store( CreateInitialTempRecord(templateId, importId), importId.ToString());
            new Task(() => Process(templateId, recordAccessor, importId)).Start();
            return importId;
        }

        public ImportResult GetImportStatus(Guid id)
        {
            var item = this.tempImportStorage.GetByName(id.ToString());
            if (item == null)
                return null;
            return new ImportResult(id, item.IsCompleted, item.ErrorMassage, item.Header, item.Values);
        }

        public void CreateSample(Guid id, Guid responsibleHeadquarterId, Guid responsibleSupervisorId)
        {
            tempSampleCreationStorage.Store(new SampleCreationStatus(id), id.ToString());
            new Task(() => this.CreateSampleInternal(id,responsibleHeadquarterId,responsibleSupervisorId)).Start();
        }

        public SampleCreationStatus GetSampleCreationStatus(Guid id)
        {
            return tempSampleCreationStorage.GetByName(id.ToString());
        }

        private void CreateSampleInternal(Guid id, Guid responsibleHeadquarterId, Guid responsibleSupervisorId)
        {
            var result = GetSampleCreationStatus(id);
            var item = this.tempImportStorage.GetByName(id.ToString());
            if (item == null)
            {
                result.SetErrorMessage("Data is absent");
                tempSampleCreationStorage.Store(result, id.ToString());
                return;
            }
            if (!item.IsCompleted)
            {
                result.SetErrorMessage("Parsing is still in process");
                tempSampleCreationStorage.Store(result, id.ToString());
                return;
            }
            if (!string.IsNullOrEmpty(item.ErrorMassage))
                {
                    result.SetErrorMessage("Parsing wasn't successed");
                    tempSampleCreationStorage.Store(result, id.ToString());
                    return;
                }

            var bigTemplateObject = this.templateRepository.GetById(item.TemplateId);

            var bigTemplate = bigTemplateObject == null ? null : bigTemplateObject.Questionnaire;
            if (bigTemplate == null)
            {
                result.SetErrorMessage("Template is absent");
                tempSampleCreationStorage.Store(result, id.ToString());
                return;
            }
            IQuestionnaire questionnarie;

            using (new ObliviousEventContext())
            {
                questionnarie = questionnaireFactory.CreateTemporaryInstance(bigTemplate);
            }
            int i = 0;
            foreach (var value in item.Values)
            {
                try
                {
                    using (new ObliviousEventContext())
                    {
                        PreBuiltInterview(value, item.Header, questionnarie);

                        i++;

                        result.SetStatusMessage(string.Format("Validated {0} interview(s) from {1}", i, item.Values.Count));
                        tempSampleCreationStorage.Store(result, id.ToString());
                    }
                }
                catch
                {
                    result.SetErrorMessage(string.Format("Invalid data in row {0}", i + 1));
                    tempSampleCreationStorage.Store(result, id.ToString());
                    return;
                }
            }

            i = 1;
            foreach (var value in item.Values)
            {
                try
                {
                    BuiltInterview(Guid.NewGuid(), value, item.Header, questionnarie, bigTemplate.PublicKey, responsibleHeadquarterId,
                                   responsibleSupervisorId);
                    result.SetStatusMessage(string.Format("Created {0} interview(s) from {1}", i, item.Values.Count));
                    tempSampleCreationStorage.Store(result, id.ToString());
                    i++;
                }
                catch
                {
                   // result.SetErrorMessage(string.Format("Invalid data in row {0}", i + 1));
                }
            }
            result.CompleteProcess();
            tempSampleCreationStorage.Store(result, id.ToString());
        }

        private void Process(Guid templateId, ISampleRecordsAccessor recordAccessor, Guid id)
        {
            var smallTemplate = this.templateSmallRepository.GetById(templateId);
            if (smallTemplate == null)
            {
                tempImportStorage.Store(CreateErrorTempRecord(templateId, id, "Template Is Absent"), id.ToString());
                return;
            }

            string[] header = null;
            int i = 0;
            const int BatchCount = 10;
            var tempBatch = new string[BatchCount][];
            try
            {
                foreach (var record in recordAccessor.Records)
                {
                    if (header == null)
                    {
                        header = record;
                        try
                        {
                            ValidateHeader(header, smallTemplate.FeaturedQuestions, id, templateId);
                        }
                        catch (ArgumentException e)
                        {
                            tempImportStorage.Store(CreateErrorTempRecord(templateId, id, e.Message), id.ToString());
                            break;
                        }
                        continue;
                    }
                    tempBatch[i] = record;
                    i++;
                    if (i == BatchCount)
                    {
                        SaveBatch(id, tempBatch, false);
                        tempBatch = new string[BatchCount][];
                        i = 0;
                    }
                }
            }
            catch (Exception e)
            {
                tempImportStorage.Store(CreateErrorTempRecord(templateId, id, e.Message), id.ToString());
            }

            if (tempBatch.Length > 0)
                SaveBatch(id, tempBatch.Take(i).ToArray(), true);
        }

        private void SaveBatch(Guid id, IEnumerable<string[]> tempBatch, bool complete)
        {
            var item = this.tempImportStorage.GetByName(id.ToString());
            if(tempBatch.Any())
                item.AddValueBatch(tempBatch.ToArray());
            if(complete)
                item.CompleteImport();
            this.tempImportStorage.Store(item, id.ToString());
        }

        private void ValidateHeader(string[] header, FeaturedQuestionItem[] expectedHeader, Guid id,Guid templateId)
        {
            var newHeader = new List<FeaturedQuestionItem>();
            for (int i = 0; i < header.Length; i++)
            {
                var realHeader = expectedHeader.FirstOrDefault(h => h.Caption == header[i]);
                if(realHeader==null)
                    throw new ArgumentException("invalid header Caption");
                newHeader.Add(realHeader);
            }
            tempImportStorage.Store(
                CreateTempRecordWithHeader(templateId, id, newHeader.Select(q => q.Caption).ToArray()), id.ToString());
        }

        private TempFileImportData CreateInitialTempRecord(Guid templateId, Guid importId)
        {
            return new TempFileImportData(){ PublicKey = importId, TemplateId = templateId};
        }

        private TempFileImportData CreateTempRecordWithHeader(Guid templateId, Guid importId, string[] header)
        {
            return new TempFileImportData() {PublicKey = importId, TemplateId = templateId, Header = header};
        }

        private TempFileImportData CreateErrorTempRecord(Guid templateId, Guid importId, string errorMessage)
        {
            return new TempFileImportData() { PublicKey = importId, TemplateId = templateId, IsCompleted = true, ErrorMassage = errorMessage };
        }

        private void PreBuiltInterview(string[] values, string[] header, IQuestionnaire template)
        {
            CreateFeaturedAnswerList(values, header,
                getQuestionByStataCaption: template.GetQuestionByStataCaption, getAnswerOptionsAsValues: template.GetAnswerOptionsAsValues);
        }

        private void BuiltInterview(Guid interviewId, string[] values, string[] header, IQuestionnaire template, Guid templateId, Guid headqarterId, Guid supervisorId)
        {
            var answersToFeaturedQuestions = CreateFeaturedAnswerList(values, header,
                getQuestionByStataCaption: template.GetQuestionByStataCaption, getAnswerOptionsAsValues: template.GetAnswerOptionsAsValues);

            var commandInvoker = NcqrsEnvironment.Get<ICommandService>();
            commandInvoker.Execute(new CreateInterviewCommand(interviewId, headqarterId, templateId, answersToFeaturedQuestions, DateTime.UtcNow, supervisorId));
        }

        private Dictionary<Guid, object> CreateFeaturedAnswerList(string[] values, string[] header,
            Func<string, IQuestion> getQuestionByStataCaption, Func<Guid, IEnumerable<decimal>> getAnswerOptionsAsValues)
        {
            if (values.Length < header.Length)
            {
                throw new ArgumentOutOfRangeException("Values doesn't much header");
            }

            var featuredAnswers = new Dictionary<Guid, object>();
            for (int i = 0; i < header.Length; i++)
            {
                var question = getQuestionByStataCaption(header[i]);
                if (question == null)
                    continue;

                switch (question.QuestionType)
                {
                    case QuestionType.Text:
                        featuredAnswers.Add(question.PublicKey, values[i]);
                        break;

                    case QuestionType.AutoPropagate:
                        featuredAnswers.Add(question.PublicKey, int.Parse(values[i]));
                        break;

                    case QuestionType.Numeric:
                        var numericQuestion = question as INumericQuestion;
                        if (numericQuestion == null)
                            break;
                        // please don't trust R# warning below. if you simplify expression with '?' then answer would be saved as decimal even for integer question
                        if (numericQuestion.IsInteger)
                            featuredAnswers.Add(question.PublicKey, int.Parse(values[i]));
                        else
                            featuredAnswers.Add(question.PublicKey, decimal.Parse(values[i]));
                        break;

                    case QuestionType.DateTime:
                        DateTime date;
                        if (!DateTime.TryParse(values[i], out date))
                            throw new ArgumentException("date time value can't be parsed");
                        featuredAnswers.Add(question.PublicKey, date);
                        break;

                    case QuestionType.SingleOption:
                        var singleOption = question as SingleQuestion;
                        if (singleOption != null)
                        {
                            decimal answerValue;
                            if (!decimal.TryParse(values[i], out answerValue))
                                throw new ArgumentException("date time value can't be parsed");
                            if (!getAnswerOptionsAsValues(question.PublicKey).Contains(answerValue))
                                throw new ArgumentException("passed option is missing");
                            featuredAnswers.Add(question.PublicKey, answerValue);
                        }
                        break;

                    case QuestionType.GpsCoordinates:
                    case QuestionType.MultyOption:
                        //throw new Exception("Unsupported pre-filled question type in sample");
                        break;
                }
            }
            return featuredAnswers;
        }
    }
}
