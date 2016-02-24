using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Commands;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Aggregates
{
    public class Questionnaire : AggregateRootMappedByConvention
    {
        #region Dependencies

        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;

        private readonly IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory;
        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;
        private readonly IExportViewFactory exportViewFactory;

        private IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<QuestionnaireBrowseItem>>();

        private IPlainKeyValueStorage<ReferenceInfoForLinkedQuestions> referenceInfoForLinkedQuestionsStorage => ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<ReferenceInfoForLinkedQuestions>>();
        private IPlainKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage => ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireRosterStructure>>();
        private IPlainKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage => ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireExportStructure>>();
        private IPlainKeyValueStorage<QuestionnaireQuestionsInfo> questionnaireQuestionsInfoStorage => ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireQuestionsInfo>>();
        #endregion

        public Questionnaire(
            IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor, 
            IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory, 
            IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory, 
            IExportViewFactory exportViewFactory)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.referenceInfoForLinkedQuestionsFactory = referenceInfoForLinkedQuestionsFactory;
            this.questionnaireRosterStructureFactory = questionnaireRosterStructureFactory;
            this.exportViewFactory = exportViewFactory;
        }

        public void ImportFromDesigner(ImportFromDesigner command)
        {
            QuestionnaireDocument document = CastToQuestionnaireDocumentOrThrow(command.Source);

            document.ConnectChildrenWithParent();

            if (string.IsNullOrWhiteSpace(command.SupportingAssembly))
            {
                throw new QuestionnaireException($"Cannot import questionnaire. Assembly file is empty. QuestionnaireId: {this.EventSourceId}");
            }

            var newVersion = this.GetNextVersion();

            this.plainQuestionnaireRepository.StoreQuestionnaire(this.EventSourceId, newVersion, document);
            this.questionnaireAssemblyFileAccessor.StoreAssembly(this.EventSourceId, newVersion, command.SupportingAssembly);
            this.questionnaireBrowseItemStorage.AsVersioned().Store(new QuestionnaireBrowseItem((QuestionnaireDocument) command.Source, newVersion,command.AllowCensusMode, command.QuestionnaireContentVersion), this.EventSourceId.FormatGuid(), newVersion);
            
            var questionnaireEntityId = new QuestionnaireIdentity(command.QuestionnaireId, newVersion).ToString();

            this.referenceInfoForLinkedQuestionsStorage.Store(this.referenceInfoForLinkedQuestionsFactory.CreateReferenceInfoForLinkedQuestions(document, newVersion), questionnaireEntityId);
            this.questionnaireExportStructureStorage.Store(this.exportViewFactory.CreateQuestionnaireExportStructure(document,newVersion), questionnaireEntityId);
            this.questionnaireRosterStructureStorage.Store(this.questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(document,newVersion), questionnaireEntityId);
            this.questionnaireQuestionsInfoStorage.Store(new QuestionnaireQuestionsInfo{
                QuestionIdToVariableMap =
                    document.Find<IQuestion>(question => true).ToDictionary(x => x.PublicKey, x => x.StataExportCaption)
            }, questionnaireEntityId);
        }

        public void DisableQuestionnaire(DisableQuestionnaire command)
        {
            var questionnaire = GetQuestionnaireBrowseItem(command.QuestionnaireId, command.QuestionnaireVersion);

            if (questionnaire==null)
                throw new QuestionnaireException(string.Format(
                    "Questionnaire {0} ver {1} cannot be deleted because it is absent in repository.",
                    this.EventSourceId.FormatGuid(), command.QuestionnaireVersion));

            if (questionnaire.Disabled)
                throw new QuestionnaireException(string.Format(
                    "Questionnaire {0} ver {1} is already in delete process.",
                    this.EventSourceId.FormatGuid(), command.QuestionnaireVersion));


            this.plainQuestionnaireRepository.DeleteQuestionnaireDocument(command.QuestionnaireId, command.QuestionnaireVersion);
            
            var browseItem = questionnaireBrowseItemStorage.AsVersioned().Get(EventSourceId.FormatGuid(), command.QuestionnaireVersion);
            if (browseItem != null)
            {
                browseItem.Disabled = true;
                questionnaireBrowseItemStorage.Store(browseItem, browseItem.Id);
            }
        }

        public void DeleteQuestionnaire(DeleteQuestionnaire command)
        {
            var questionnaire = GetQuestionnaireBrowseItem(command.QuestionnaireId, command.QuestionnaireVersion);
            if (questionnaire == null)
                throw new QuestionnaireException(string.Format(
                    "Questionnaire {0} ver {1} cannot be deleted because it is absent in repository.",
                    this.EventSourceId.FormatGuid(), command.QuestionnaireVersion));

            if (!questionnaire.Disabled)
                throw new QuestionnaireException(string.Format(
                 "Questionnaire {0} ver {1} is not disabled.",
                 this.EventSourceId.FormatGuid(), command.QuestionnaireVersion));

            var browseItem = questionnaireBrowseItemStorage.AsVersioned().Get(EventSourceId.FormatGuid(), command.QuestionnaireVersion);
            if (browseItem != null)
            {
                browseItem.IsDeleted = true;
                questionnaireBrowseItemStorage.Store(browseItem, browseItem.Id);
            }

            var questionnaireEntityId = new QuestionnaireIdentity(command.QuestionnaireId, command.QuestionnaireVersion).ToString();
            this.referenceInfoForLinkedQuestionsStorage.Remove(questionnaireEntityId);
            this.questionnaireExportStructureStorage.Remove(questionnaireEntityId);
            this.questionnaireRosterStructureStorage.Remove(questionnaireEntityId);
            this.questionnaireQuestionsInfoStorage.Remove(questionnaireEntityId);
        }

        public void RegisterPlainQuestionnaire(RegisterPlainQuestionnaire command)
        {
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(command.Id, command.Version);
            
            if (questionnaireDocument == null || questionnaireDocument.IsDeleted)
                throw new QuestionnaireException(string.Format(
                    "Plain questionnaire {0} ver {1} cannot be registered because it is absent in plain repository.",
                    this.EventSourceId, command.Version));

            //this.ApplyEvent(new PlainQuestionnaireRegistered(command.Version, command.AllowCensusMode));
        }

        private static QuestionnaireDocument CastToQuestionnaireDocumentOrThrow(IQuestionnaireDocument source)
        {
            var document = source as QuestionnaireDocument;

            if (document == null)
                throw new QuestionnaireException(string.Format("Cannot import questionnaire with a document of a not supported type {0}. QuestionnaireId: {1}",
                    source.GetType(), source.PublicKey));

            return document;
        }

        private long GetNextVersion()
        {

            var availableVersions =
                this.questionnaireBrowseItemStorage.Query(
                    _ => _.Where(q => q.QuestionnaireId == this.EventSourceId).Select(q => q.Version));
            if (!availableVersions.Any())
                return 1;
            return availableVersions.Max() + 1;
        }

        private QuestionnaireBrowseItem GetQuestionnaireBrowseItem(Guid id, long version)
        {
            return
                this.questionnaireBrowseItemStorage.AsVersioned()
                    .Get(id.FormatGuid(), version);
        }
    }
}