using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events;
using Main.Core.Events.User;
using Main.Core.Utility;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ncqrs.Spec;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Services.SampleImport.DTO;
using WB.Core.SharedKernels.DataCollection.Services.SampleImport.SampleDataReaders;
using WB.Core.SharedKernels.DataCollection.Services.SampleImport.TemporaryDataAccessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public class SampleImportService : ISampleImportService
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDocument> templateRepository;
        private readonly IReadSideRepositoryWriter<QuestionnaireBrowseItem> templateSmallRepository;
        private readonly ITemporaryDataRepositoryAccessor tempImportRepository;

        public SampleImportService(IReadSideRepositoryWriter<QuestionnaireDocument> templateRepository,
                                   IReadSideRepositoryWriter<QuestionnaireBrowseItem> templateSmallRepository, ITemporaryDataRepositoryAccessor tempImportRepository)
        {
            this.templateRepository = templateRepository;
            this.templateSmallRepository = templateSmallRepository;
            this.tempImportRepository = tempImportRepository;
        }

        public Guid ImportSampleAsync(Guid templateId, ISampleRecordsAccessor recordAccessor)
        {
            Guid importId = Guid.NewGuid();
            tempImportRepository.Store( CreateInitialTempRecord(templateId, importId), importId.ToString());
            new Task(() => Process(templateId, recordAccessor, importId)).Start();
            return importId;
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

        public ImportResult GetImportStatus(Guid id)
        {
            var item = this.tempImportRepository.GetByName<TempFileImportData>(id.ToString());
            if (item == null)
                return null;
            return new ImportResult(id, item.IsCompleted, item.ErrorMassage, item.Header, item.Values);
        }

        public void CreateSample(Guid id, Guid responsibleHeadquaterId, Guid responsibleSupervisorId)
        {
            tempImportRepository.Store(new SampleCreationStatus(id), id.ToString());
            new Task(() => CreateSamplInternal(id,responsibleHeadquaterId,responsibleSupervisorId)).Start();
        }

        public SampleCreationStatus GetSampleCreationStatus(Guid id)
        {
            return tempImportRepository.GetByName<SampleCreationStatus>(id.ToString());
        }
        private void CreateSamplInternal(Guid id, Guid responsibleHeadquaterId, Guid responsibleSupervisorId)
        {
            var result = GetSampleCreationStatus(id);
            var item = this.tempImportRepository.GetByName<TempFileImportData>(id.ToString());
            if (item == null)
            {
                result.SetErrorMessage("Data is absent");
                tempImportRepository.Store(result,id.ToString());
                return;
            }
            if (!item.IsCompleted)
            {
                result.SetErrorMessage("Parsing is still in process");
                tempImportRepository.Store(result, id.ToString());
                return;
            }
            if (!string.IsNullOrEmpty(item.ErrorMassage))
                {
                    result.SetErrorMessage("Parsing wasn't successed");
                    tempImportRepository.Store(result, id.ToString());
                    return;
                }

            var bigTemplate = this.templateRepository.GetById(item.TemplateId);
            if (bigTemplate == null)
            {
                result.SetErrorMessage("Template is absent");
                tempImportRepository.Store(result, id.ToString());
                return;
            }
            Questionnaire questionnarie;
            //return;
            
            using (new ObliviousEventContext())
            {
                questionnarie = new Questionnaire(Guid.NewGuid(), bigTemplate);
            }
            int i = 0;
            foreach (var value in item.Values)
            {
                try
                {
                    using (new ObliviousEventContext())
                    {
                        PreBuiltInterview(Guid.NewGuid(), value, item.Header, questionnarie);

                        i++;

                        result.SetStatusMessage(string.Format("Validated {0} interview(s) from {1}", i, item.Values.Count));
                        tempImportRepository.Store(result, id.ToString());
                    }
                }
                catch
                {
                    result.SetErrorMessage(string.Format("Invalid data in row {0}", i + 1));
                    tempImportRepository.Store(result, id.ToString());
                    return;
                }
            }
            i = 1;
            foreach (var value in item.Values)
            {
                try
                {
                    BuiltInterview(Guid.NewGuid(), value, item.Header, bigTemplate, responsibleHeadquaterId,
                                   responsibleSupervisorId);

                    result.SetStatusMessage(string.Format("Created {0} interview(s) from {1}", i, item.Values.Count));
                    tempImportRepository.Store(result, id.ToString());
                    i++;
                }
                catch
                {
                }
            }
            result.CompleteProcess();
            tempImportRepository.Store(result, id.ToString());
        }
        private void Process(Guid templateId, ISampleRecordsAccessor recordAccessor, Guid id)
        {
            var smallTemplate = this.templateSmallRepository.GetById(templateId);
            if (smallTemplate == null)
            {
                tempImportRepository.Store(CreateErrorTempRecord(templateId, id, "Template Is Absent"), id.ToString());
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
                            tempImportRepository.Store(CreateErrorTempRecord(templateId, id, e.Message), id.ToString());
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
                tempImportRepository.Store(CreateErrorTempRecord(templateId, id, e.Message), id.ToString());
            }
            if (tempBatch.Length > 0)
                SaveBatch(id, tempBatch.Take(i).ToArray(), true);


        }

        private void SaveBatch(Guid id, IEnumerable<string[]> tempBatch, bool complete)
        {
            var item = this.tempImportRepository.GetByName<TempFileImportData>(id.ToString());
            if(tempBatch.Any())
                item.AddValueBatch(tempBatch.ToArray());
            if(complete)
                item.CompleteImport();
            this.tempImportRepository.Store(item, id.ToString());
        }


        private void ValidateHeader(string[] header, FeaturedQuestionItem[] expectedHeader, Guid id,Guid templateId)
        {
            var newHeader = new List<FeaturedQuestionItem>();
            for (int i = 0; i < header.Length; i++)
            {
                var realHeader = expectedHeader.FirstOrDefault(h => h.Caption != header[i]);
                if(realHeader==null)
                    throw new ArgumentException("invalid header Capiton");
                newHeader.Add(realHeader);
            }
            tempImportRepository.Store(
                CreateTempRecordWithHeader(templateId, id, newHeader.Select(q => q.Caption).ToArray()), id.ToString());
        }

        private void PreBuiltInterview(Guid publicKey, string[] values, string[] header, Questionnaire template)
        {
            var featuredAnswers = CreateFeaturedAnswerList(values, header, template.CreateSnapshot());
            template.CreateInterviewWithFeaturedQuestions(publicKey, new UserLight(Guid.NewGuid(), "test"),
                                                    new UserLight(Guid.NewGuid(), "test"), featuredAnswers);
        }
        private void BuiltInterview(Guid publicKey, string[] values, string[] header, QuestionnaireDocument template, Guid headqarter, Guid supervisor)
        {
            var featuredAnswers = CreateFeaturedAnswerList(values, header, template);
            var commandInvoker = NcqrsEnvironment.Get<ICommandService>();
            commandInvoker.Execute(new CreateInterviewWithFeaturedQuestionsCommand(publicKey, template.PublicKey,
                                                                                   new UserLight(headqarter, ""),
                                                                                   new UserLight(supervisor, ""),
                                                                                   featuredAnswers));
        }
        private List<QuestionAnswer> CreateFeaturedAnswerList(string[] values, string[] header, QuestionnaireDocument template)
        {
            var featuredAnswers = new List<QuestionAnswer>();
            for (int i = 0; i < header.Length; i++)
            {
                var question =
                    template.FirstOrDefault<IQuestion>(q => q.StataExportCaption == header[i]);
                if (question == null)
                    continue;

                var singleOption = question as SingleQuestion;
                if (singleOption != null)
                {
                    var answer = singleOption.Answers.FirstOrDefault(a => a.AnswerValue == values[i]);
                    featuredAnswers.Add(new QuestionAnswer() {Answers = new Guid[] {answer.PublicKey}, Id = question.PublicKey});
                }
                else
                {
                    featuredAnswers.Add(new QuestionAnswer()
                        {
                            Answer = values[i],
                            Answers = new Guid[] {},
                            Id = question.PublicKey
                        });
                }
            }
            return featuredAnswers;
        }
    }
}
