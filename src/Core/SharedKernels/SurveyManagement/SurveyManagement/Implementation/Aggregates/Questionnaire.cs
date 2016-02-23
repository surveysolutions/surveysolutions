using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Commands;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Aggregates
{
    public class Questionnaire : AggregateRootMappedByConvention
    {
        #region Dependencies

        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;

        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;
        #endregion

        public Questionnaire(
            IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage;
        }

        public void ImportFromDesigner(ImportFromDesigner command)
        {
            QuestionnaireDocument document = CastToQuestionnaireDocumentOrThrow(command.Source);

            if (string.IsNullOrWhiteSpace(command.SupportingAssembly))
            {
                throw new QuestionnaireException($"Cannot import questionnaire. Assembly file is empty. QuestionnaireId: {this.EventSourceId}");
            }

            var newVersion = this.GetNextVersion();

            this.plainQuestionnaireRepository.StoreQuestionnaire(this.EventSourceId, newVersion, document);
            this.questionnaireAssemblyFileAccessor.StoreAssembly(this.EventSourceId, newVersion, command.SupportingAssembly);
            this.questionnaireBrowseItemStorage.AsVersioned().Store(new QuestionnaireBrowseItem((QuestionnaireDocument) command.Source, newVersion,command.AllowCensusMode), this.EventSourceId.FormatGuid(), newVersion);
            //this.ApplyEvent(new TemplateImported
            //{
            //    AllowCensusMode = command.AllowCensusMode,
            //    Version = newVersion,
            //    ResponsibleId = command.CreatedBy,
            //    ContentVersion = command.QuestionnaireContentVersion
            //});
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

            //this.ApplyEvent(new QuestionnaireDisabled()
            //{
            //    QuestionnaireVersion = command.QuestionnaireVersion,
            //    ResponsibleId = command.ResponsibleId
            //});
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
            //this.ApplyEvent(new QuestionnaireDeleted()
            //{
            //    QuestionnaireVersion = command.QuestionnaireVersion,
            //    ResponsibleId = command.ResponsibleId
            //});
        }

        public void RegisterPlainQuestionnaire(RegisterPlainQuestionnaire command)
        {
            QuestionnaireDocument questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(command.Id, command.Version);
            
            if (questionnaireDocument == null || questionnaireDocument.IsDeleted)
                throw new QuestionnaireException(string.Format(
                    "Plain questionnaire {0} ver {1} cannot be registered because it is absent in plain repository.",
                    this.EventSourceId, command.Version));

            this.ApplyEvent(new PlainQuestionnaireRegistered(command.Version, command.AllowCensusMode));
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
            IPlainTransactionManager plainTransactionManager = ServiceLocator.Current.GetInstance<IPlainTransactionManager>();

            var availableVersions =
                plainTransactionManager.ExecuteInPlainTransaction(
                    () =>
                        this.questionnaireBrowseItemStorage.Query(
                            _ => _.Where(q => q.QuestionnaireId == this.EventSourceId).Select(q => q.Version)));
            if (!availableVersions.Any())
                return 1;
            return availableVersions.Max() + 1;
        }

        private QuestionnaireBrowseItem GetQuestionnaireBrowseItem(Guid id, long version)
        {
            IPlainTransactionManager plainTransactionManager = ServiceLocator.Current.GetInstance<IPlainTransactionManager>();
            return plainTransactionManager.ExecuteInPlainTransaction(() =>
                this.questionnaireBrowseItemStorage.AsVersioned()
                    .Get(id.FormatGuid(), version));
        }
    }
}