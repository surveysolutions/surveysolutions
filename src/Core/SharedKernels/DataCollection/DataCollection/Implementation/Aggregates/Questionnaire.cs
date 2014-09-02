using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class Questionnaire : AggregateRootMappedByConvention, IQuestionnaire, ISnapshotable<QuestionnaireState>
    {
        #region State

        private PlainQuestionnaire plainQuestionnaire;
        private bool isProxyToPlainQuestionnaireRepository;
        private HashSet<long> availableVersions = new HashSet<long>() { 0 };


        protected internal void Apply(TemplateImported e)
        {
            var templateVersion = e.Version ?? (this.Version+1);
            this.plainQuestionnaire = new PlainQuestionnaire(e.Source, () => templateVersion);
            availableVersions.Add(templateVersion);
        }

        protected internal void Apply(QuestionnaireDeleted e)
        {
        }

        private void Apply(PlainQuestionnaireRegistered e)
        {
            this.isProxyToPlainQuestionnaireRepository = true;
            this.plainQuestionnaire = null;
            availableVersions.Add(e.Version);
        }

        public QuestionnaireState CreateSnapshot()
        {
            return this.isProxyToPlainQuestionnaireRepository
                ? new QuestionnaireState(
                    null,
                    null,
                    null,
                    isProxyToPlainQuestionnaireRepository: true, availableVersions: this.availableVersions)
                : new QuestionnaireState(
                    this.plainQuestionnaire.QuestionnaireDocument,
                    this.plainQuestionnaire.QuestionCache,
                    this.plainQuestionnaire.GroupCache,
                    isProxyToPlainQuestionnaireRepository: false, availableVersions: this.availableVersions);
        }

        public void RestoreFromSnapshot(QuestionnaireState snapshot)
        {
            this.isProxyToPlainQuestionnaireRepository = snapshot.IsProxyToPlainQuestionnaireRepository;

            this.plainQuestionnaire = snapshot.IsProxyToPlainQuestionnaireRepository
                ? null
                : new PlainQuestionnaire(snapshot.Document, () => this.Version, snapshot.GroupCache, snapshot.QuestionCache);
            this.availableVersions = snapshot.AvailableVersions;
        }

        #endregion

        #region Dependencies

        private static IQuestionnaireVerifier QuestionnaireVerifier
        {
            get { return ServiceLocator.Current.GetInstance<IQuestionnaireVerifier>(); }
        }

        public IPlainQuestionnaireRepository PlainQuestionnaireRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainQuestionnaireRepository>(); }
        }

        #endregion


        public Questionnaire() {}

        public Questionnaire(Guid createdBy, IQuestionnaireDocument source, bool allowCensusMode)
            : base(source.PublicKey)
        {
            this.ImportFromDesigner(createdBy, source, allowCensusMode);
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


        private void ImportFromQuestionnaireDocument(IQuestionnaireDocument source)
        {
            QuestionnaireDocument document = CastToQuestionnaireDocumentOrThrow(source);
            this.ThrowIfCurrentAggregateIsUsedOnlyAsProxyToPlainQuestionnaireRepository();


            this.ApplyEvent(new TemplateImported { Source = document, Version = this.availableVersions.Max() + 1 });
        }

        public void ImportFromDesigner(Guid createdBy, IQuestionnaireDocument source, bool allowCensusMode)
        {
            QuestionnaireDocument document = CastToQuestionnaireDocumentOrThrow(source);
            ThrowIfVerifierFindsErrors(document);
            this.ThrowIfCurrentAggregateIsUsedOnlyAsProxyToPlainQuestionnaireRepository();


            this.ApplyEvent(new TemplateImported { Source = document, AllowCensusMode = allowCensusMode, Version = this.availableVersions.Max() + 1 });
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
            if(!availableVersions.Contains(questionnaireVersion))
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

        private static void ThrowIfVerifierFindsErrors(QuestionnaireDocument document)
        {
            var errors = QuestionnaireVerifier.Verify(document);
            if (errors.Any())
                throw new QuestionnaireVerificationException(string.Format("Questionnaire '{0}' can't be imported", document.Title),
                    errors.ToArray());
        }

        private void ThrowIfCurrentAggregateIsUsedOnlyAsProxyToPlainQuestionnaireRepository()
        {
            if (this.isProxyToPlainQuestionnaireRepository)
                throw new QuestionnaireException("This aggregate instance only supports sending of plain questionnaire repository events and it is not intended to be used separately.");
        }

        public void InitializeQuestionnaireDocument()
        {
            ((IQuestionnaire) this.plainQuestionnaire).InitializeQuestionnaireDocument();
        }

        public IQuestion GetQuestionByStataCaption(string stataCaption)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetQuestionByStataCaption(stataCaption);
        }

        public bool HasQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).HasQuestion(questionId);
        }

        public bool HasGroup(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).HasGroup(groupId);
        }

        public QuestionType GetQuestionType(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetQuestionType(questionId);
        }

        public bool IsQuestionLinked(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).IsQuestionLinked(questionId);
        }

        public string GetQuestionTitle(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetQuestionTitle(questionId);
        }

        public string GetQuestionVariableName(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetQuestionVariableName(questionId);
        }

        public string GetGroupTitle(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetGroupTitle(groupId);
        }

        public IEnumerable<decimal> GetAnswerOptionsAsValues(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetAnswerOptionsAsValues(questionId);
        }

        public string GetAnswerOptionTitle(Guid questionId, decimal answerOptionValue)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetAnswerOptionTitle(questionId, answerOptionValue);
        }

        public int? GetMaxSelectedAnswerOptions(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetMaxSelectedAnswerOptions(questionId);
        }

        public bool IsCustomValidationDefined(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).IsCustomValidationDefined(questionId);
        }

        public IEnumerable<Guid> GetQuestionsInvolvedInCustomValidation(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetQuestionsInvolvedInCustomValidation(questionId);
        }

        public string GetCustomValidationExpression(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetCustomValidationExpression(questionId);
        }

        public IEnumerable<Guid> GetAllQuestionsWithNotEmptyValidationExpressions()
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetAllQuestionsWithNotEmptyValidationExpressions();
        }

        public IEnumerable<Guid> GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(questionId);
        }

        public IEnumerable<Guid> GetAllParentGroupsForQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetAllParentGroupsForQuestion(questionId);
        }

        public string GetCustomEnablementConditionForQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetCustomEnablementConditionForQuestion(questionId);
        }

        public string GetCustomEnablementConditionForGroup(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetCustomEnablementConditionForGroup(groupId);
        }

        public IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionOfGroup(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetQuestionsInvolvedInCustomEnablementConditionOfGroup(groupId);
        }

        public IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(questionId);
        }

        public IEnumerable<Guid> GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionId);
        }

        public IEnumerable<Guid> GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionId);
        }

        public bool ShouldQuestionSpecifyRosterSize(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).ShouldQuestionSpecifyRosterSize(questionId);
        }

        public IEnumerable<Guid> GetRosterGroupsByRosterSizeQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetRosterGroupsByRosterSizeQuestion(questionId);
        }

        public int? GetMaxValueForNumericQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetMaxValueForNumericQuestion(questionId);
        }

        public int? GetListSizeForListQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetListSizeForListQuestion(questionId);
        }

        public IEnumerable<Guid> GetRostersFromTopToSpecifiedQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetRostersFromTopToSpecifiedQuestion(questionId);
        }

        public IEnumerable<Guid> GetRostersFromTopToSpecifiedGroup(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetRostersFromTopToSpecifiedGroup(groupId);
        }

        public IEnumerable<Guid> GetFixedRosterGroups(Guid? parentRosterId = null)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetFixedRosterGroups(parentRosterId);
        }

        public int GetRosterLevelForQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetRosterLevelForQuestion(questionId);
        }

        public int GetRosterLevelForGroup(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetRosterLevelForGroup(groupId);
        }

        public IEnumerable<Guid> GetAllMandatoryQuestions()
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetAllMandatoryQuestions();
        }

        public IEnumerable<Guid> GetAllQuestionsWithNotEmptyCustomEnablementConditions()
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetAllQuestionsWithNotEmptyCustomEnablementConditions();
        }

        public IEnumerable<Guid> GetAllGroupsWithNotEmptyCustomEnablementConditions()
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetAllGroupsWithNotEmptyCustomEnablementConditions();
        }

        public bool IsRosterGroup(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).IsRosterGroup(groupId);
        }

        public IEnumerable<Guid> GetAllUnderlyingQuestions(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetAllUnderlyingQuestions(groupId);
        }

        public IEnumerable<Guid> GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(groupId);
        }

        public IEnumerable<Guid> GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditions(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditions(groupId);
        }

        public IEnumerable<Guid> GetUnderlyingQuestionsWithNotEmptyCustomValidationExpressions(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetUnderlyingQuestionsWithNotEmptyCustomValidationExpressions(groupId);
        }

        public IEnumerable<Guid> GetUnderlyingMandatoryQuestions(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetUnderlyingMandatoryQuestions(groupId);
        }

        public Guid GetQuestionReferencedByLinkedQuestion(Guid linkedQuestionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetQuestionReferencedByLinkedQuestion(linkedQuestionId);
        }

        public bool IsQuestionMandatory(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).IsQuestionMandatory(questionId);
        }

        public bool IsQuestionInteger(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).IsQuestionInteger(questionId);
        }

        public int? GetCountOfDecimalPlacesAllowedByQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetCountOfDecimalPlacesAllowedByQuestion(questionId);
        }

        public IEnumerable<string> GetFixedRosterTitles(Guid groupId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetFixedRosterTitles(groupId);
        }

        public bool DoesQuestionSpecifyRosterTitle(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).DoesQuestionSpecifyRosterTitle(questionId);
        }

        public IEnumerable<Guid> GetRostersAffectedByRosterTitleQuestion(Guid questionId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetRostersAffectedByRosterTitleQuestion(questionId);
        }

        public IEnumerable<Guid> GetNestedRostersOfGroupById(Guid rosterId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetNestedRostersOfGroupById(rosterId);
        }

        public Guid? GetRosterSizeQuestion(Guid rosterId)
        {
            return ((IQuestionnaire) this.plainQuestionnaire).GetRosterSizeQuestion(rosterId);
        }
    }
}