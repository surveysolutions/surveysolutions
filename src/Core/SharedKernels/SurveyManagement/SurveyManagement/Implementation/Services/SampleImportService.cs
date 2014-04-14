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
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class SampleImportService : ISampleImportService
    {
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;
        private readonly ITemporaryDataStorage<SampleCreationStatus> tempSampleCreationStorage;
        private readonly IQuestionDataParser questionDataParser;
        private readonly IQuestionnaireFactory questionnaireFactory;
        private readonly IRosterDataService rosterDataService;
        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public SampleImportService(IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage,
            ITemporaryDataStorage<SampleCreationStatus> tempSampleCreationStorage,
            IQuestionnaireFactory questionnaireFactory, IQuestionDataParser questionDataParser, IRosterDataService rosterDataService)
        {
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.tempSampleCreationStorage = tempSampleCreationStorage;
            this.questionnaireFactory = questionnaireFactory;
            this.questionDataParser = questionDataParser;
            this.rosterDataService = rosterDataService;
        }

        public void CreateSample(Guid questionnaireId, long version, Guid id, PreloadedDataByFile[] data, Guid responsibleHeadquarterId, Guid responsibleSupervisorId)
        {
            this.tempSampleCreationStorage.Store(new SampleCreationStatus(id), id.ToString());
            new Task(() => this.CreateSampleInternal(questionnaireId, version, id, data, responsibleHeadquarterId, responsibleSupervisorId))
                .Start();
        }

        public SampleCreationStatus GetSampleCreationStatus(Guid id)
        {
            return this.tempSampleCreationStorage.GetByName(id.ToString());
        }

        private void CreateSampleInternal(Guid questionnaireId, long version, Guid id, PreloadedDataByFile[] data, Guid responsibleHeadquarterId, Guid responsibleSupervisorId)
        {
            var result = this.GetSampleCreationStatus(id);
            if (data.Length == 0)
            {
                result.SetErrorMessage("Data is absent");
                this.tempSampleCreationStorage.Store(result, id.ToString());
                return;
            }

            var bigTemplateObject = this.questionnaireDocumentVersionedStorage.GetById(questionnaireId, version);

            var bigTemplate = bigTemplateObject == null ? null : bigTemplateObject.Questionnaire;
            if (bigTemplate == null)
            {
                result.SetErrorMessage("Template is absent");
                this.tempSampleCreationStorage.Store(result, id.ToString());
                return;
            }

            IQuestionnaire questionnarie = this.questionnaireFactory.CreateTemporaryInstance(bigTemplate);

            var i = 1;
            foreach (var preloadedDataByFile in data)
            {
                if(!preloadedDataByFile.FileName.Contains(bigTemplate.Title))
                    continue;

                foreach (var value in preloadedDataByFile.Content)
                {
                    try
                    {
                        this.BuiltInterview(Guid.NewGuid(), value, preloadedDataByFile.Header, questionnarie, bigTemplate.PublicKey,
                            responsibleHeadquarterId,
                            responsibleSupervisorId);
                        result.SetStatusMessage(string.Format("Created {0} interview(s) from {1}", i, preloadedDataByFile.Content.Length));
                        this.tempSampleCreationStorage.Store(result, id.ToString());
                        i++;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message, e);
                    }
                }
            }
            result.CompleteProcess();
            this.tempSampleCreationStorage.Store(result, id.ToString());
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
                var parsedAnswer = questionDataParser.Parse(values[i], header[i], getQuestionByStataCaption, getAnswerOptionsAsValues);
                if (parsedAnswer.HasValue)
                    featuredAnswers.Add(parsedAnswer.Value.Key, parsedAnswer.Value.Value);
            }
            return featuredAnswers;
        }
    }
}
