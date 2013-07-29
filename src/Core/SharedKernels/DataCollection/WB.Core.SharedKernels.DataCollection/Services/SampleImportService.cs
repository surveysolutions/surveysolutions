using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public class SampleImportService : ISampleImportService
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireDocument> templateRepository;
        private readonly IReadSideRepositoryWriter<QuestionnaireBrowseItem> templateSmallRepository;
        private readonly IReadSideRepositoryWriter<TempFileImportData> tempImportRepository;

        public SampleImportService(IReadSideRepositoryWriter<QuestionnaireDocument> templateRepository,
                                   IReadSideRepositoryWriter<TempFileImportData> tempImportRepository,
                                   IReadSideRepositoryWriter<QuestionnaireBrowseItem> templateSmallRepository)
        {
            this.templateRepository = templateRepository;
            this.tempImportRepository = tempImportRepository;
            this.templateSmallRepository = templateSmallRepository;
        }

        public Guid ImportSampleAsync(Guid templateId, ISampleRecordsAccessor recordAccessor)
        {
            Guid importId = Guid.NewGuid();
            tempImportRepository.Store( CreateInitialTempRecord(templateId, importId), importId);
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
            var item = this.tempImportRepository.GetById(id);
            if (item == null)
                return null;
            return new ImportResult(id, item.IsCompleted, item.ErrorMassage, item.Header, item.Values);
        }

        public void CreateSample(Guid id, Guid responsibleHeadquaterId, Guid responsibleSupervisorId)
        {
            var item = this.tempImportRepository.GetById(id);
            if (item == null)
                return;
            if (!item.IsCompleted)
                return;
            if (!string.IsNullOrEmpty(item.ErrorMassage))
                return;

            var bigTemplate = this.templateRepository.GetById(item.TemplateId);
            if (bigTemplate == null)
                return;
            Questionnaire questionnarie;

            using (new ObliviousEventContext())
            {
                questionnarie = new Questionnaire(Guid.NewGuid(), bigTemplate);
            }

            foreach (var value in item.Values)
            {
                using (new ObliviousEventContext())
                {
                    PreBuiltInterview(Guid.NewGuid(), value, item.Header, questionnarie);
                }
            }
            foreach (var value in item.Values)
            {
                try
                {
                    BuiltInterview(Guid.NewGuid(), value, item.Header, bigTemplate, responsibleHeadquaterId,
                                   responsibleSupervisorId);
                }
                catch
                {
                }
            }
        }

        private void Process(Guid templateId, ISampleRecordsAccessor recordAccessor, Guid id)
        {
            var smallTemplate = this.templateSmallRepository.GetById(templateId);
            if (smallTemplate == null)
            {
                tempImportRepository.Store(CreateErrorTempRecord(templateId, id, "Template Is Absent"), id);
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
                            tempImportRepository.Store(CreateErrorTempRecord(templateId, id, e.Message), id);
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
                tempImportRepository.Store(CreateErrorTempRecord(templateId, id, e.Message), id);
            }
            if (tempBatch.Length > 0)
                SaveBatch(id, tempBatch.Take(i).ToArray(), true);


        }

        private void SaveBatch(Guid id, IEnumerable<string[]> tempBatch, bool complete)
        {
            var item = this.tempImportRepository.GetById(id);
            if(tempBatch.Any())
                item.AddValueBatch(tempBatch.ToArray());
            if(complete)
                item.CompleteImport();
            this.tempImportRepository.Store(item, id);
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
                CreateTempRecordWithHeader(templateId, id, newHeader.Select(q => q.Caption).ToArray()), id);
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

        /*  public void ParseSource(Guid templateId, TextReader textReader)
        {
            var tempFile = new TempFileImportData() {PublicKey = Guid.NewGuid(), TemplateId = templateId};
            var template = templateRepository.GetById(templateId);
            var valueList = new List<string[]>();
            using (var reader = new CsvReader(textReader))
            {

                while (reader.Read())
                {
                    if (tempFile.Header == null)
                    {
                        tempFile.Header = reader.FieldHeaders;
                        continue;
                    }
                    valueList.Add(reader.CurrentRecord);
        //            yield return PreBuiltInterview(Guid.NewGuid(), reader.CurrentRecord, header, template);
                }
            }
            tempFile.Values = valueList.ToArray();
            tempFileRepository.Store(tempFile,tempFile.TemplateId);
        }*/
    }
}
