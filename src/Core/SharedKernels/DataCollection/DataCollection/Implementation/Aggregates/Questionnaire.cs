using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class Questionnaire : AggregateRootMappedByConvention, ISnapshotable<QuestionnaireState>
    {
        #region State

        private bool isProxyToPlainQuestionnaireRepository;
        private Dictionary<long, IQuestionnaire> availableVersions = new Dictionary <long, IQuestionnaire>();
        private HashSet<long> disabledQuestionnaires = new HashSet<long>();

        protected internal void Apply(TemplateImported e)
        {
            var templateVersion = e.Version ?? (this.Version + 1);
            availableVersions[templateVersion] = new PlainQuestionnaire(e.Source, () => templateVersion, e.ResponsibleId);
        }

        protected internal void Apply(QuestionnaireDeleted e)
        {
            availableVersions[e.QuestionnaireVersion] = null;
            disabledQuestionnaires.Remove(e.QuestionnaireVersion);
        }

        protected internal void Apply(QuestionnaireDisabled e)
        {
            disabledQuestionnaires.Add(e.QuestionnaireVersion);
        }

        private void Apply(PlainQuestionnaireRegistered e)
        {
            this.isProxyToPlainQuestionnaireRepository = true;
            availableVersions[e.Version] = null;
        }

        protected internal void Apply(QuestionnaireAssemblyImported e)
        {
        }

        #endregion

        #region Dependencies
        
        public IPlainQuestionnaireRepository PlainQuestionnaireRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainQuestionnaireRepository>(); }
        }

        public IQuestionnaireAssemblyFileAccessor QuestionnareAssemblyFileAccessor
        {
            get { return ServiceLocator.Current.GetInstance<IQuestionnaireAssemblyFileAccessor>(); }
        }

        
        #endregion

        public Questionnaire() { }

        public Questionnaire(Guid createdBy, IQuestionnaireDocument source, bool allowCensusMode, string supportingAssembly)
            : base(source.PublicKey)
        {
            this.ImportFromDesigner(new ImportFromDesigner(createdBy, source, allowCensusMode, supportingAssembly));
        }

        public Questionnaire(IQuestionnaireDocument source)
            : base(source.PublicKey)
        {
            this.ImportFromQuestionnaireDocument(source);
        }

        public Questionnaire(Guid id, long version, bool allowCensusMode, string supportingAssembly)
            : base(id)
        {
            this.RegisterPlainQuestionnaire(new RegisterPlainQuestionnaire(id, version, allowCensusMode, supportingAssembly));
        }

        public IQuestionnaire GetQuestionnaire()
        {
            var presentVersions = availableVersions.Where(v => v.Value != null).ToList();
            if (!presentVersions.Any())
                return null;
            var maxVersion = presentVersions.Max(k => k.Key);
            return presentVersions.FirstOrDefault(v => v.Key == maxVersion).Value;
        }

        public IQuestionnaire GetHistoricalQuestionnaire(long version)
        {
            if (availableVersions.ContainsKey(version) && !disabledQuestionnaires.Contains(version))
                return availableVersions[version];

            return null;
        }

        public QuestionnaireState CreateSnapshot()
        {
            return this.isProxyToPlainQuestionnaireRepository
                ? new QuestionnaireState(
                    isProxyToPlainQuestionnaireRepository: true, availableVersions: this.availableVersions, disabledQuestionnaires: disabledQuestionnaires)
                : new QuestionnaireState(
                    isProxyToPlainQuestionnaireRepository: false, availableVersions: this.availableVersions, disabledQuestionnaires: disabledQuestionnaires);
        }

        public void RestoreFromSnapshot(QuestionnaireState snapshot)
        {
            this.isProxyToPlainQuestionnaireRepository = snapshot.IsProxyToPlainQuestionnaireRepository;

            this.availableVersions = snapshot.AvailableVersions;
            this.disabledQuestionnaires = snapshot.DisabledQuestionnaires;
        }

        public void ImportFromDesigner(ImportFromDesigner command)
        {
            QuestionnaireDocument document = CastToQuestionnaireDocumentOrThrow(command.Source);
            this.ThrowIfCurrentAggregateIsUsedOnlyAsProxyToPlainQuestionnaireRepository();

            if (string.IsNullOrWhiteSpace(command.SupportingAssembly))
            {
                throw new QuestionnaireException(string.Format("Cannot import questionnaire. Assembly file is empty. QuestionnaireId: {0}", this.EventSourceId));
            }

            var newVersion = GetNextVersion();

            QuestionnareAssemblyFileAccessor.StoreAssembly(EventSourceId, newVersion, command.SupportingAssembly);

            this.ApplyEvent(new TemplateImported
            {
                Source = document,
                AllowCensusMode = command.AllowCensusMode,
                Version = newVersion,
                ResponsibleId = command.CreatedBy
            });
            this.ApplyEvent(new QuestionnaireAssemblyImported { Version = newVersion });

        }

        public void ImportFromSupervisor(ImportFromSupervisor command)
        {
            ImportFromQuestionnaireDocument(command.Source);
        }

        public void ImportFromDesignerForTester(ImportFromDesignerForTester command)
        {
            ImportFromQuestionnaireDocument(command.Source);
        }

        public void DisableQuestionnaire(DisableQuestionnaire command)
        {
            if (!availableVersions.ContainsKey(command.QuestionnaireVersion))
                throw new QuestionnaireException(string.Format(
                    "Questionnaire {0} ver {1} cannot be deleted because it is absent in repository.",
                    this.EventSourceId.FormatGuid(), command.QuestionnaireVersion));

            var questionnaireTemplateVersion = availableVersions.ContainsKey(command.QuestionnaireVersion) ? availableVersions[command.QuestionnaireVersion] : null;

            if(disabledQuestionnaires.Contains(command.QuestionnaireVersion))
                throw new QuestionnaireException(string.Format(
                 "Questionnaire {0} ver {1} is already in delete process.",
                 this.EventSourceId.FormatGuid(), command.QuestionnaireVersion));

            var createdById = questionnaireTemplateVersion != null
                ? questionnaireTemplateVersion.ResponsibleId
                : null;

            if (createdById.HasValue && createdById != command.ResponsibleId)
            {
                throw new QuestionnaireException(
                    string.Format("You don't have permissions to delete this questionnaire. QuestionnaireId: {0}", this.EventSourceId));
            }

            this.ApplyEvent(new QuestionnaireDisabled()
            {
                QuestionnaireVersion = command.QuestionnaireVersion,
                ResponsibleId = command.ResponsibleId
            });
        }

        public void DeleteQuestionnaire(DeleteQuestionnaire command)
        {
            if (!availableVersions.ContainsKey(command.QuestionnaireVersion))
                throw new QuestionnaireException(string.Format(
                    "Questionnaire {0} ver {1} cannot be deleted because it is absent in repository.",
                    this.EventSourceId.FormatGuid(), command.QuestionnaireVersion));

            if (!disabledQuestionnaires.Contains(command.QuestionnaireVersion))
                throw new QuestionnaireException(string.Format(
                 "Questionnaire {0} ver {1} is not disabled.",
                 this.EventSourceId.FormatGuid(), command.QuestionnaireVersion));

            var questionnaireTemplateVersion = availableVersions.ContainsKey(command.QuestionnaireVersion) ? availableVersions[command.QuestionnaireVersion] : null;

            var createdById = questionnaireTemplateVersion != null
                ? questionnaireTemplateVersion.ResponsibleId
                : null;

            if (createdById.HasValue && createdById != command.ResponsibleId)
            {
                throw new QuestionnaireException(
                    string.Format("You don't have permissions to delete this questionnaire. QuestionnaireId: {0}", this.EventSourceId));
            }

            this.ApplyEvent(new QuestionnaireDeleted()
            {
                QuestionnaireVersion = command.QuestionnaireVersion,
                ResponsibleId = command.ResponsibleId
            });
        }

        public void RegisterPlainQuestionnaire(RegisterPlainQuestionnaire command)
        {
            QuestionnaireDocument questionnaireDocument = this.PlainQuestionnaireRepository.GetQuestionnaireDocument(command.Id, command.Version);
            
            if (questionnaireDocument == null || questionnaireDocument.IsDeleted)
                throw new QuestionnaireException(string.Format(
                    "Plain questionnaire {0} ver {1} cannot be registered because it is absent in plain repository.",
                    this.EventSourceId, command.Version));

            this.ApplyEvent(new PlainQuestionnaireRegistered(command.Version, command.AllowCensusMode));

            //ignoring on interviewer but saving on supervisor
            if (string.IsNullOrWhiteSpace(command.SupportingAssembly)) return;
            
            QuestionnareAssemblyFileAccessor.StoreAssembly(EventSourceId, command.Version, command.SupportingAssembly);
            this.ApplyEvent(new QuestionnaireAssemblyImported { Version = command.Version });
        }

        private static QuestionnaireDocument CastToQuestionnaireDocumentOrThrow(IQuestionnaireDocument source)
        {
            var document = source as QuestionnaireDocument;

            if (document == null)
                throw new QuestionnaireException(string.Format("Cannot import questionnaire with a document of a not supported type {0}. QuestionnaireId: {1}",
                    source.GetType(), source.PublicKey));

            return document;
        }

        private void ImportFromQuestionnaireDocument(IQuestionnaireDocument source)
        {
            QuestionnaireDocument document = CastToQuestionnaireDocumentOrThrow(source);
            this.ThrowIfCurrentAggregateIsUsedOnlyAsProxyToPlainQuestionnaireRepository();

            this.ApplyEvent(new TemplateImported { Source = document, Version = GetNextVersion() });
        }

        private long GetNextVersion()
        {
            if (!availableVersions.Any())
                return 1;
            return this.availableVersions.Keys.Max() + 1;
        }
        
        private void ThrowIfCurrentAggregateIsUsedOnlyAsProxyToPlainQuestionnaireRepository()
        {
            if (this.isProxyToPlainQuestionnaireRepository)
                throw new QuestionnaireException(string.Format("This aggregate instance only supports sending of plain questionnaire repository events and it is not intended to be used separately. QuestionnaireId: {0}", this.EventSourceId));
        }
    }
}