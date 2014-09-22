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
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
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

        protected internal void Apply(TemplateImported e)
        {
            var templateVersion = e.Version ?? (this.Version + 1);
            availableVersions[templateVersion] = new PlainQuestionnaire(e.Source, () => templateVersion);
        }

        protected internal void Apply(QuestionnaireDeleted e)
        {
            availableVersions[e.QuestionnaireVersion] = null;
        }

        private void Apply(PlainQuestionnaireRegistered e)
        {
            this.isProxyToPlainQuestionnaireRepository = true;
            availableVersions[e.Version] = null;
        }

        protected internal void Apply(TemplateAssemblyImported e)
        {
        }

        #endregion

        #region Dependencies
        
        public IPlainQuestionnaireRepository PlainQuestionnaireRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainQuestionnaireRepository>(); }
        }

        #endregion

        public Questionnaire() { }

        public Questionnaire(Guid createdBy, IQuestionnaireDocument source, bool allowCensusMode)
            : base(source.PublicKey)
        {
            this.ImportFromDesigner(createdBy, source, allowCensusMode, null);
        }

        public Questionnaire(IQuestionnaireDocument source)
            : base(source.PublicKey)
        {
            this.ImportFromQuestionnaireDocument(source);
        }

        public Questionnaire(Guid id, long version, bool allowCensusMode)
            : base(id)
        {
            this.RegisterPlainQuestionnaire(id, version, allowCensusMode);
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
            if (availableVersions.ContainsKey(version))
                return availableVersions[version];
            return null;
        }

        public QuestionnaireState CreateSnapshot()
        {
            return this.isProxyToPlainQuestionnaireRepository
                ? new QuestionnaireState(
                    isProxyToPlainQuestionnaireRepository: true, availableVersions: this.availableVersions)
                : new QuestionnaireState(
                    isProxyToPlainQuestionnaireRepository: false, availableVersions: this.availableVersions);
        }

        public void RestoreFromSnapshot(QuestionnaireState snapshot)
        {
            this.isProxyToPlainQuestionnaireRepository = snapshot.IsProxyToPlainQuestionnaireRepository;

            this.availableVersions = snapshot.AvailableVersions;
        }

        public void ImportFromDesigner(Guid createdBy, IQuestionnaireDocument source, bool allowCensusMode, string supportingAssembly)
        {
            QuestionnaireDocument document = CastToQuestionnaireDocumentOrThrow(source);
            this.ThrowIfCurrentAggregateIsUsedOnlyAsProxyToPlainQuestionnaireRepository();

            var newVersion = GetNextVersion();
            this.ApplyEvent(new TemplateImported { Source = document, AllowCensusMode = allowCensusMode, Version = newVersion });

            if (supportingAssembly != null && !string.IsNullOrWhiteSpace(supportingAssembly))
            {
                this.ApplyEvent(new TemplateAssemblyImported { AssemblySource = supportingAssembly, Version = newVersion });
            }
        }

        public void ImportFromSupervisor(IQuestionnaireDocument source)
        {
            ImportFromQuestionnaireDocument(source);
        }

        public void ImportFromDesignerForTester(IQuestionnaireDocument source)
        {
            ImportFromQuestionnaireDocument(source);
        }

        public void DeleteQuestionnaire(long questionnaireVersion)
        {
            if(!availableVersions.ContainsKey(questionnaireVersion))
                throw new QuestionnaireException(string.Format(
                    "Questionnaire {0} ver {1} cannot be deleted because it is absent in repository.",
                    this.EventSourceId.FormatGuid(), questionnaireVersion));
            this.ApplyEvent(new QuestionnaireDeleted() { QuestionnaireVersion = questionnaireVersion });
        }

        public void RegisterPlainQuestionnaire(Guid id, long version, bool allowCensusMode)
        {
            QuestionnaireDocument questionnaireDocument = this.PlainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            if (questionnaireDocument == null || questionnaireDocument.IsDeleted)
                throw new QuestionnaireException(string.Format(
                    "Plain questionnaire {0} ver {1} cannot be registered because it is absent in plain repository.",
                    this.EventSourceId.FormatGuid(), version));

            this.ApplyEvent(new PlainQuestionnaireRegistered(version, allowCensusMode));
        }

        private static QuestionnaireDocument CastToQuestionnaireDocumentOrThrow(IQuestionnaireDocument source)
        {
            var document = source as QuestionnaireDocument;

            if (document == null)
                throw new QuestionnaireException(string.Format("Cannot import questionnaire with a document of a not supported type {0}.",
                    source.GetType()));

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
                throw new QuestionnaireException("This aggregate instance only supports sending of plain questionnaire repository events and it is not intended to be used separately.");
        }
    }
}