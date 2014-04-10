using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class SampleImportService : ISampleImportService
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDocumentVersioned> templateRepository;
        private readonly IViewFactory<QuestionnairePreloadingDataInputModel, QuestionnairePreloadingDataItem> questionnairePreloadingDataItemFactory;
        private readonly ITemporaryDataStorage<TempFileImportData> tempImportStorage;
        private readonly ITemporaryDataStorage<SampleCreationStatus> tempSampleCreationStorage;
        private readonly IQuestionnaireFactory questionnaireFactory;

        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public SampleImportService(IReadSideRepositoryWriter<QuestionnaireDocumentVersioned> templateRepository,
                                   IViewFactory<QuestionnairePreloadingDataInputModel, QuestionnairePreloadingDataItem> questionnairePreloadingDataItemFactory, 
            ITemporaryDataStorage<TempFileImportData> tempImportStorage,
            ITemporaryDataStorage<SampleCreationStatus> tempSampleCreationStorage,
            IQuestionnaireFactory questionnaireFactory)
        {
            this.templateRepository = templateRepository;
            this.questionnairePreloadingDataItemFactory = questionnairePreloadingDataItemFactory;
            this.tempImportStorage = tempImportStorage;
            this.tempSampleCreationStorage = tempSampleCreationStorage;
            this.questionnaireFactory = questionnaireFactory;
        }

        public Guid ImportSampleAsync(Guid templateId, long templateVersion, ISampleRecordsAccessor recordAccessor)
        {
            Guid importId = Guid.NewGuid();
            this.tempImportStorage.Store( this.CreateInitialTempRecord(templateId, importId), importId.ToString());
            new Task(() => this.Process(templateId,templateVersion, recordAccessor, importId)).Start();
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
            this.tempSampleCreationStorage.Store(new SampleCreationStatus(id), id.ToString());
            new Task(() => this.CreateSampleInternal(id,responsibleHeadquarterId,responsibleSupervisorId)).Start();
        }

        public SampleCreationStatus GetSampleCreationStatus(Guid id)
        {
            return this.tempSampleCreationStorage.GetByName(id.ToString());
        }

        private void CreateSampleInternal(Guid id, Guid responsibleHeadquarterId, Guid responsibleSupervisorId)
        {
            var result = this.GetSampleCreationStatus(id);
            var item = this.tempImportStorage.GetByName(id.ToString());
            if (item == null)
            {
                result.SetErrorMessage("Data is absent");
                this.tempSampleCreationStorage.Store(result, id.ToString());
                return;
            }
            if (!item.IsCompleted)
            {
                result.SetErrorMessage("Parsing is still in process");
                this.tempSampleCreationStorage.Store(result, id.ToString());
                return;
            }
            if (!string.IsNullOrEmpty(item.ErrorMassage))
            {
                result.SetErrorMessage("Parsing wasn't successed");
                this.tempSampleCreationStorage.Store(result, id.ToString());
                return;
            }

            var bigTemplateObject = this.templateRepository.GetById(item.TemplateId);

            var bigTemplate = bigTemplateObject == null ? null : bigTemplateObject.Questionnaire;
            if (bigTemplate == null)
            {
                result.SetErrorMessage("Template is absent");
                this.tempSampleCreationStorage.Store(result, id.ToString());
                return;
            }
            IQuestionnaire questionnarie = this.questionnaireFactory.CreateTemporaryInstance(bigTemplate);

            var i = 1;
            foreach (var value in item.Values)
            {
                try
                {
                    this.BuiltInterview(Guid.NewGuid(), value, item.Header, questionnarie, bigTemplate.PublicKey, responsibleHeadquarterId,
                        responsibleSupervisorId);
                    result.SetStatusMessage(string.Format("Created {0} interview(s) from {1}", i, item.Values.Count));
                    this.tempSampleCreationStorage.Store(result, id.ToString());
                    i++;
                }
                catch(Exception e)
                {
                    Logger.Error(e.Message, e);
                }
            }
            result.CompleteProcess();
            this.tempSampleCreationStorage.Store(result, id.ToString());
        }

        private void Process(Guid templateId, long templateVersion, ISampleRecordsAccessor recordAccessor, Guid id)
        {
            var smallTemplate = this.questionnairePreloadingDataItemFactory.Load(new QuestionnairePreloadingDataInputModel(templateId, templateVersion));
            if (smallTemplate == null)
            {
                this.tempImportStorage.Store(this.CreateErrorTempRecord(templateId, id, "Template Is Absent"), id.ToString());
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
                            this.ValidateHeader(header, smallTemplate.Questions, id, templateId);
                        }
                        catch (ArgumentException e)
                        {
                            this.tempImportStorage.Store(this.CreateErrorTempRecord(templateId, id, e.Message), id.ToString());
                            break;
                        }
                        continue;
                    }
                    tempBatch[i] = record;
                    i++;
                    if (i == BatchCount)
                    {
                        this.SaveBatch(id, tempBatch, false);
                        tempBatch = new string[BatchCount][];
                        i = 0;
                    }
                }
            }
            catch (Exception e)
            {
                this.tempImportStorage.Store(this.CreateErrorTempRecord(templateId, id, e.Message), id.ToString());
            }

            if (tempBatch.Length > 0)
                this.SaveBatch(id, tempBatch.Take(i).ToArray(), true);
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

        private void ValidateHeader(string[] header, QuestionDescription[] expectedHeader, Guid id, Guid templateId)
        {
            var newHeader = new List<QuestionDescription>();
            for (int i = 0; i < header.Length; i++)
            {
                var realHeader = expectedHeader.FirstOrDefault(h => h.Caption == header[i]);
                if(realHeader==null)
                    throw new ArgumentException("invalid header Caption");
                newHeader.Add(realHeader);
            }
            this.tempImportStorage.Store(
                this.CreateTempRecordWithHeader(templateId, id, newHeader.Select(q => q.Caption).ToArray()), id.ToString());
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
            this.CreateFeaturedAnswerList(values, header,
                getQuestionByStataCaption: template.GetQuestionByStataCaption, getAnswerOptionsAsValues: template.GetAnswerOptionsAsValues);
        }

        private void BuiltInterview(Guid interviewId, string[] values, string[] header, IQuestionnaire template, Guid templateId, Guid headqarterId, Guid supervisorId)
        {
            var answersToFeaturedQuestions = this.CreateFeaturedAnswerList(values, header,
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
                if(question.LinkedToQuestionId.HasValue)
                    continue;
                
                switch (question.QuestionType)
                {
                    case QuestionType.Text:
                        featuredAnswers.Add(question.PublicKey, values[i]);
                        break;

                    case QuestionType.AutoPropagate:
                        int intValue;
                        if (int.TryParse(values[i], out intValue))
                            featuredAnswers.Add(question.PublicKey, intValue);

                        break;

                    case QuestionType.Numeric:
                        var numericQuestion = question as INumericQuestion;
                        if (numericQuestion == null)
                            continue;    
                        // please don't trust R# warning below. if you simplify expression with '?' then answer would be saved as decimal even for integer question
                        if (numericQuestion.IsInteger)
                        {
                            int intNumericValue;
                            if (int.TryParse(values[i], out intNumericValue))
                                featuredAnswers.Add(question.PublicKey, intNumericValue);
                        }
                        else
                        {
                            decimal decimalNumericValue;
                            if (decimal.TryParse(values[i], out decimalNumericValue))
                                featuredAnswers.Add(question.PublicKey, decimalNumericValue);
                        }
                        break;

                    case QuestionType.DateTime:
                        DateTime date;
                        if (!DateTime.TryParse(values[i], out date))
                            continue;    
                            //throw new ArgumentException("date time value can't be parsed");
                        featuredAnswers.Add(question.PublicKey, date);
                        break;

                    case QuestionType.SingleOption:
                        var singleOption = question as SingleQuestion;
                        if (singleOption != null)
                        {
                            decimal answerValue;
                            if (!decimal.TryParse(values[i], out answerValue))
                                continue;    
                                //  throw new ArgumentException("date time value can't be parsed");
                            if (!getAnswerOptionsAsValues(question.PublicKey).Contains(answerValue))
                                continue;    
                                //throw new ArgumentException("passed option is missing");
                            featuredAnswers.Add(question.PublicKey, answerValue);
                        }
                        break;

                    case QuestionType.TextList:
                    case QuestionType.MultyOption:

                    case QuestionType.GpsCoordinates:
                    case QuestionType.QRBarcode:
                        continue;    
                    //throw new ArgumentException("Unsupported pre-filled question type in sample");
                }
            }
            return featuredAnswers;
        }
    }
}
