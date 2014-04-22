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
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class SampleImportService : ISampleImportService
    {
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;
        private readonly ITemporaryDataStorage<SampleCreationStatus> tempSampleCreationStorage;
        private readonly IQuestionDataParser questionDataParser;
        private readonly IQuestionnaireFactory questionnaireFactory;
        private readonly IPreloadedDataServiceFactory preloadedDataServiceFactory;
        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public SampleImportService(IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage,
            ITemporaryDataStorage<SampleCreationStatus> tempSampleCreationStorage,
            IQuestionnaireFactory questionnaireFactory, IQuestionDataParser questionDataParser,
            IPreloadedDataServiceFactory preloadedDataServiceFactory,
            IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireExportStructureStorage,
            IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireRosterStructureStorage)
        {
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.tempSampleCreationStorage = tempSampleCreationStorage;
            this.questionnaireFactory = questionnaireFactory;
            this.questionDataParser = questionDataParser;
            this.preloadedDataServiceFactory = preloadedDataServiceFactory;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
        }

        public void CreateSample(Guid questionnaireId, long version, string id, PreloadedDataByFile[] data, Guid responsibleHeadquarterId, Guid responsibleSupervisorId)
        {
            this.tempSampleCreationStorage.Store(new SampleCreationStatus(id), id);
            new Task(() => this.CreateSampleInternal(questionnaireId, version, id, data, responsibleHeadquarterId, responsibleSupervisorId))
                .Start();
        }

        public SampleCreationStatus GetSampleCreationStatus(string id)
        {
            return this.tempSampleCreationStorage.GetByName(id);
        }

        private void CreateSampleInternal(Guid questionnaireId, long version, string id, PreloadedDataByFile[] data,
            Guid responsibleHeadquarterId, Guid responsibleSupervisorId)
        {
            var result = this.GetSampleCreationStatus(id);
            if (data.Length == 0)
            {
                result.SetErrorMessage("Data is absent");
                this.tempSampleCreationStorage.Store(result, id);
                return;
            }

            var bigTemplateObject = this.questionnaireDocumentVersionedStorage.GetById(questionnaireId, version);
            var questionnaireExportStructure = this.questionnaireExportStructureStorage.GetById(questionnaireId, version);
            var questionnaireRosterStructure = this.questionnaireRosterStructureStorage.GetById(questionnaireId, version);
            var bigTemplate = bigTemplateObject == null ? null : bigTemplateObject.Questionnaire;

            if (bigTemplate == null || questionnaireExportStructure == null || questionnaireRosterStructure==null)
            {
                result.SetErrorMessage("Template is absent");
                this.tempSampleCreationStorage.Store(result, id);
                return;
            }

            IQuestionnaire questionnarie = this.questionnaireFactory.CreateTemporaryInstance(bigTemplate);

            var i = 1;
            var topLevelData = data.FirstOrDefault(d => d.FileName.Contains(bigTemplate.Title));

            if (topLevelData == null)
            {
                result.SetErrorMessage("Template is absent");
                this.tempSampleCreationStorage.Store(result, id);
                return;
            }
            var preloadedDataService = this.preloadedDataServiceFactory.CreatePreloadedDataService(questionnaireExportStructure,
                questionnaireRosterStructure);
            var idColumnIndex = preloadedDataService.GetIdColumnIndex(topLevelData);
            foreach (var value in topLevelData.Content)
            {
                try
                {
                    this.BuiltInterview(Guid.NewGuid(), topLevelData.FileName, value, topLevelData.Header,
                        value[idColumnIndex],
                        data.Except(new[] { topLevelData }).ToArray(),
                        questionnarie, preloadedDataService, bigTemplate.PublicKey,version,
                        responsibleHeadquarterId,
                        responsibleSupervisorId);

                    result.SetStatusMessage(string.Format("Created {0} interview(s) from {1}", i, topLevelData.Content.Length));
                    this.tempSampleCreationStorage.Store(result, id);
                    i++;
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e);
                }
            }

            result.CompleteProcess();
            this.tempSampleCreationStorage.Store(result, id);
        }

        private void BuiltInterview(Guid interviewId, string levelName, string[] values, string[] header,string id, PreloadedDataByFile[] rosterData,
            IQuestionnaire template, IPreloadedDataService preloadedDataService, Guid templateId, long version, Guid headqarterId, Guid supervisorId)
        {
            var answersToFeaturedQuestions = this.CreateAnswerList(values, header,
                getQuestionByStataCaption: template.GetQuestionByStataCaption, getAnswerOptionsAsValues: template.GetAnswerOptionsAsValues);

            var rosterAnswers = this.GetAnswers(levelName, id, new decimal[0], rosterData, preloadedDataService, template);
            var levels = new List<PreloadedLevelDto>() { new PreloadedLevelDto(new decimal[0], answersToFeaturedQuestions) };
            levels.AddRange(rosterAnswers);
            var preloadedData = new PreloadedDataDto(id, levels.ToArray());

            var commandInvoker = NcqrsEnvironment.Get<ICommandService>();
            commandInvoker.Execute(new CreateInterviewWithPreloadedData(interviewId, headqarterId, templateId, version, preloadedData,
                DateTime.UtcNow, supervisorId));
        }

        private PreloadedLevelDto[] GetAnswers(string levelName, string parentId, decimal[] rosterVector, PreloadedDataByFile[] rosterData, IPreloadedDataService preloadedDataService, IQuestionnaire template)
        {
            var result = new List<PreloadedLevelDto>();
            var childFiles = preloadedDataService.GetChildDataFiles(levelName, rosterData);

            foreach (var preloadedDataByFile in childFiles)
            {
                var parentIdColumnIndex = preloadedDataService.GetParentIdColumnIndex(preloadedDataByFile);
                var idColumnIndex = preloadedDataService.GetIdColumnIndex(preloadedDataByFile);
                var childRecrordsOfCurrentRow =
                    preloadedDataByFile.Content.Where(
                        record => record[parentIdColumnIndex] == parentId).ToArray();

                foreach (var rosterRow in childRecrordsOfCurrentRow)
                {
                    var newRosterVetor = new decimal[rosterVector.Length + 1];
                    rosterVector.CopyTo(newRosterVetor, 0);
                    newRosterVetor[newRosterVetor.Length - 1] = preloadedDataService.GetRecordIdValueAsDecimal(rosterRow, idColumnIndex);

                    var rosterAnswers = this.CreateAnswerList(rosterRow, preloadedDataByFile.Header,
                        getQuestionByStataCaption: template.GetQuestionByStataCaption,
                        getAnswerOptionsAsValues: template.GetAnswerOptionsAsValues);

                    result.Add(new PreloadedLevelDto(newRosterVetor, rosterAnswers));

                    result.AddRange(this.GetAnswers(preloadedDataByFile.FileName, rosterRow[idColumnIndex], newRosterVetor, rosterData, preloadedDataService, template));
                }
            }

            return result.ToArray();
        }

        private Dictionary<Guid, object> CreateAnswerList(string[] values, string[] header,
            Func<string, IQuestion> getQuestionByStataCaption, Func<Guid, IEnumerable<decimal>> getAnswerOptionsAsValues)
        {
            if (values.Length < header.Length)
            {
                throw new ArgumentOutOfRangeException("Values doesn't much header");
            }

            var featuredAnswers = new Dictionary<Guid, object>();
            for (int i = 0; i < header.Length; i++)
            {
                var parsedAnswer = questionDataParser.Parse(values[i], header[i], getQuestionByStataCaption, getAnswerOptionsAsValues);
                if (parsedAnswer.HasValue)
                    featuredAnswers.Add(parsedAnswer.Value.Key, parsedAnswer.Value.Value);
            }
            return featuredAnswers;
        }
    }
}
