using System;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates
{
    public class Questionnaire : IPlainAggregateRoot
    {
        private const int MaxTitleLength = 500;
        private static readonly Regex InvalidTitleRegex = new Regex(@"^[\w \-\(\)\\/]*$", RegexOptions.Compiled);

        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;
        private readonly IPlainStorageAccessor<TranslationInstance> translations;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private Guid Id { get; set; }

        public Questionnaire(
            IQuestionnaireStorage questionnaireStorage, 
            IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage,
            IFileSystemAccessor fileSystemAccessor, 
            IPlainStorageAccessor<TranslationInstance> translations)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage;
            this.fileSystemAccessor = fileSystemAccessor;
            this.translations = translations;
        }

        public void SetId(Guid id) => this.Id = id;

        public void ImportFromDesigner(ImportFromDesigner command)
        {
            QuestionnaireDocument questionnaireDocument = CastToQuestionnaireDocumentOrThrow(command.Source);
            
            if (string.IsNullOrWhiteSpace(command.SupportingAssembly))
                throw new QuestionnaireException(
                    $"Cannot import questionnaire. Assembly file is empty. QuestionnaireId: {this.Id}");

            this.StoreQuestionnaireAndProjectionsAsNewVersion(
                questionnaireDocument, 
                command.SupportingAssembly,
                command.AllowCensusMode, 
                command.QuestionnaireContentVersion,
                command.QuestionnaireVersion,
                isSupportAssignments: true,
                isSupportExportVariables: true
                );
        }

        public void CloneQuestionnaire(CloneQuestionnaire command)
        {
            this.ThrowIfQuestionnaireIsAbsentOrDisabled(command.QuestionnaireId, command.SourceQuestionnaireVersion);

            QuestionnaireDocument sourceQuestionnaireClone = 
                this.questionnaireStorage.GetQuestionnaireDocument(command.QuestionnaireId, command.SourceQuestionnaireVersion)?.Clone();

            this.ThrowIfTitleIsInvalid(command.NewTitle, sourceQuestionnaireClone);

            string assemblyAsBase64 = this.questionnaireAssemblyFileAccessor.GetAssemblyAsBase64String(command.QuestionnaireId, command.SourceQuestionnaireVersion);
            QuestionnaireBrowseItem questionnaireBrowseItem = this.GetQuestionnaireBrowseItem(command.QuestionnaireId, command.SourceQuestionnaireVersion);

            sourceQuestionnaireClone.Title = command.NewTitle;

            CloneTranslations(sourceQuestionnaireClone.PublicKey, command.SourceQuestionnaireVersion, command.NewQuestionnaireVersion);

            this.StoreQuestionnaireAndProjectionsAsNewVersion(
                sourceQuestionnaireClone,
                assemblyAsBase64, 
                questionnaireBrowseItem.AllowCensusMode, 
                questionnaireBrowseItem.QuestionnaireContentVersion,
                command.NewQuestionnaireVersion,
                questionnaireBrowseItem.AllowAssignments,
                questionnaireBrowseItem.AllowExportVariables
                );
        }

        private void CloneTranslations(Guid sourceQuestionnaireId, long sourceQuestionnaireVersion, long newQuestionnaireVersion)
        {
            var oldTranslations = this.translations.Query(_ =>_.Where(x =>
                                x.QuestionnaireId.QuestionnaireId == sourceQuestionnaireId &&
                                x.QuestionnaireId.Version == newQuestionnaireVersion).ToList());

            this.translations.Remove(oldTranslations);

            var sourceTranslations = this.translations.Query(_ => _.Where(x =>
                             x.QuestionnaireId.QuestionnaireId == sourceQuestionnaireId &&
                             x.QuestionnaireId.Version == sourceQuestionnaireVersion).ToList());
            foreach (var translationInstance in sourceTranslations)
            {
                var targetTranslation = translationInstance.Clone();
                targetTranslation.QuestionnaireId = new QuestionnaireIdentity(targetTranslation.QuestionnaireId.QuestionnaireId, newQuestionnaireVersion);
                this.translations.Store(targetTranslation, null);
            }
        }

        private void StoreQuestionnaireAndProjectionsAsNewVersion(QuestionnaireDocument questionnaireDocument, 
            string assemblyAsBase64, 
            bool isCensus, 
            long questionnaireContentVersion, 
            long questionnaireVersion,
            bool isSupportAssignments,
            bool isSupportExportVariables)
        {
            var identity = new QuestionnaireIdentity(this.Id, questionnaireVersion);
            
            this.questionnaireAssemblyFileAccessor.StoreAssembly(identity.QuestionnaireId, identity.Version, assemblyAsBase64);

            this.questionnaireStorage.StoreQuestionnaire(identity.QuestionnaireId, identity.Version, questionnaireDocument);

            string projectionId = GetProjectionId(identity);

            this.questionnaireBrowseItemStorage.Store(
                new QuestionnaireBrowseItem(questionnaireDocument, identity.Version, isCensus, questionnaireContentVersion, isSupportAssignments, isSupportExportVariables),
                projectionId);
        }

        public void DisableQuestionnaire(DisableQuestionnaire command)
        {
            this.ThrowIfQuestionnaireIsAbsentOrDisabled(command.QuestionnaireId, command.QuestionnaireVersion);

            var browseItem = this.questionnaireBrowseItemStorage.GetById(new QuestionnaireIdentity(this.Id, command.QuestionnaireVersion).ToString());
            if (browseItem != null)
            {
                browseItem.Disabled = true;
                browseItem.DisabledBy = command.ResponsibleId;
                this.questionnaireBrowseItemStorage.Store(browseItem, browseItem.Id);
            }
        }

        public void DeleteQuestionnaire(DeleteQuestionnaire command)
        {
            this.ThrowIfQuestionnaireIsAbsentOrNotDisabled(command.QuestionnaireId, command.QuestionnaireVersion);

            var browseItem = this.questionnaireBrowseItemStorage.GetById(new QuestionnaireIdentity(this.Id, command.QuestionnaireVersion).ToString());
            if (browseItem != null)
            {
                browseItem.IsDeleted = true;
                this.questionnaireBrowseItemStorage.Store(browseItem, browseItem.Id);
            }

            QuestionnaireDocument questionnaireDocument = this.questionnaireStorage.GetQuestionnaireDocument(command.QuestionnaireId, command.QuestionnaireVersion);
            questionnaireDocument.IsDeleted = true;
            this.questionnaireStorage.StoreQuestionnaire(command.QuestionnaireId, command.QuestionnaireVersion, questionnaireDocument);
        }

        public void RegisterPlainQuestionnaire(RegisterPlainQuestionnaire command)
        {
            QuestionnaireDocument questionnaireDocument = this.questionnaireStorage.GetQuestionnaireDocument(command.Id, command.Version);
            
            if (questionnaireDocument == null || questionnaireDocument.IsDeleted)
                throw new QuestionnaireException(string.Format(
                    "Plain questionnaire {0} ver {1} cannot be registered because it is absent in plain repository.",
                    this.Id, command.Version));

            throw new NotSupportedException("This command is no longer supported and should be reimplemented if we decide to resurrect Supervisor");
        }

        private static QuestionnaireDocument CastToQuestionnaireDocumentOrThrow(IQuestionnaireDocument source)
        {
            var document = source as QuestionnaireDocument;

            if (document == null)
                throw new QuestionnaireException(
                    $"Cannot import questionnaire with a document of a not supported type {source.GetType()}. QuestionnaireId: {source.PublicKey}");

            return document;
        }

        private void ThrowIfTitleIsInvalid(string title, QuestionnaireDocument questionnaireDocument)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new QuestionnaireException("Questionnaire title should not be empty.");

            if (title.Length > MaxTitleLength)
                throw new QuestionnaireException($"Questionnaire title can't have more than {MaxTitleLength} symbols.");

            if (!InvalidTitleRegex.IsMatch(title))
                throw new QuestionnaireException("Questionnaire title contains characters that are not allowed. Only letters, numbers, space and _ are allowed.");

            IGroup rosterWithNameEqualToQuestionnaireTitle = questionnaireDocument.Find<IGroup>(
                group => this.IsRosterWithNameEqualToQuestionnaireTitle(@group, title)).FirstOrDefault();

            if (rosterWithNameEqualToQuestionnaireTitle != null)
                throw new QuestionnaireException($"Questionnaire title is similar to roster ID '{rosterWithNameEqualToQuestionnaireTitle.VariableName}'.");
        }

        private bool IsRosterWithNameEqualToQuestionnaireTitle(IGroup group, string title)
        {
            if (!group.IsRoster)
                return false;

            var questionnaireVariableName = this.fileSystemAccessor.MakeStataCompatibleFileName(title);

            return group.VariableName.Equals(questionnaireVariableName, StringComparison.InvariantCultureIgnoreCase);
        }

        private void ThrowIfQuestionnaireIsAbsentOrDisabled(Guid questionnaireId, long questionnaireVersion)
        {
            QuestionnaireBrowseItem questionnaireBrowseItem = this.GetQuestionnaireBrowseItemOrThrow(questionnaireId, questionnaireVersion);

            if (questionnaireBrowseItem.Disabled)
                throw new QuestionnaireException(
                    $"Questionnaire {questionnaireId.FormatGuid()} ver {questionnaireVersion} is disabled and probably is being deleted.");
        }

        private void ThrowIfQuestionnaireIsAbsentOrNotDisabled(Guid questionnaireId, long questionnaireVersion)
        {
            QuestionnaireBrowseItem questionnaireBrowseItem = this.GetQuestionnaireBrowseItemOrThrow(questionnaireId, questionnaireVersion);

            if (!questionnaireBrowseItem.Disabled)
                throw new QuestionnaireException(
                    $"Questionnaire {this.Id.FormatGuid()} ver {questionnaireVersion} is not disabled.");
        }

        private QuestionnaireBrowseItem GetQuestionnaireBrowseItemOrThrow(Guid questionnaireId, long questionnaireVersion)
        {
            QuestionnaireBrowseItem questionnaireBrowseItem = this.GetQuestionnaireBrowseItem(questionnaireId, questionnaireVersion);

            if (questionnaireBrowseItem == null)
                throw new QuestionnaireException(
                    $"Questionnaire {questionnaireId.FormatGuid()} ver {questionnaireVersion} is absent in repository.");

            return questionnaireBrowseItem;
        }

        private QuestionnaireBrowseItem GetQuestionnaireBrowseItem(Guid questionnaireId, long questionnaireVersion)
        {
            string projectionId = GetProjectionId(new QuestionnaireIdentity(questionnaireId, questionnaireVersion));

            var questionnaire = this.questionnaireBrowseItemStorage.GetById(projectionId);
            return questionnaire;
        }

        private static string GetProjectionId(QuestionnaireIdentity identity) => identity.ToString();
    }
}
