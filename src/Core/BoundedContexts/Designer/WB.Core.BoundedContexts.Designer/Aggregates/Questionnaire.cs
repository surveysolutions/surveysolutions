using Main.Core.Entities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Raven.Abstractions.Extensions;
using WB.Core.BoundedContexts.Designer.Aggregates.Snapshots;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;

namespace WB.Core.BoundedContexts.Designer.Aggregates
{
    public class Questionnaire : AggregateRootMappedByConvention, ISnapshotable<QuestionnaireState>
    {
        private const int MaxCountOfDecimalPlaces = 15;
        private const int MaxChapterItemsCount = 400;
        private const int MaxTitleLength = 250;
        private const int maxFilteredComboboxOptionsCount = 5000;

        private static readonly HashSet<QuestionType> RosterSizeQuestionTypes = new HashSet<QuestionType>
        {
            QuestionType.Numeric,
            QuestionType.MultyOption,
            QuestionType.TextList,
        };

        #region State

        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();
        private bool wasExpressionsMigrationPerformed = false;

        private void Apply(SharedPersonToQuestionnaireAdded e)
        {
            this.innerDocument.SharedPersons.Add(e.PersonId);
        }

        private void Apply(SharedPersonFromQuestionnaireRemoved e)
        {
            this.innerDocument.SharedPersons.Remove(e.PersonId);
        }

        private void Apply(QuestionnaireUpdated e)
        {
            this.innerDocument.Title = e.Title;
            this.innerDocument.IsPublic = e.IsPublic;
        }

        private void Apply(QuestionnaireDeleted e)
        {
            this.innerDocument.IsDeleted = true;
        }

        private void Apply(ExpressionsMigratedToCSharp e)
        {
            this.wasExpressionsMigrationPerformed = true;
        }

        private void Apply(GroupDeleted e)
        {
            this.innerDocument.RemoveGroup(e.GroupPublicKey);
        }

        private void Apply(GroupUpdated e)
        {
            this.innerDocument.UpdateGroup(e.GroupPublicKey, e.GroupText,e.VariableName, e.Description, e.ConditionExpression);
        }

        internal void Apply(NewGroupAdded e)
        {
            var group = new Group();
            group.Title = e.GroupText;
            group.VariableName = e.VariableName;
            group.Propagated = Propagate.None;
            group.PublicKey = e.PublicKey;
            group.Description = e.Description;
            group.ConditionExpression = e.ConditionExpression;
            this.innerDocument.Add(group, e.ParentGroupPublicKey, null);
        }

        internal void Apply(TemplateImported e)
        {
            var upgradedDocument = e.Source;
            this.innerDocument = upgradedDocument;
        }

        private void Apply(QuestionnaireCloned e)
        {
            this.innerDocument = e.QuestionnaireDocument;
        }

        private void Apply(GroupCloned e)
        {
            var group = new Group();
            group.Title = e.GroupText;
            group.VariableName = e.VariableName;
            group.Propagated = Propagate.None;
            group.PublicKey = e.PublicKey;
            group.Description = e.Description;
            group.ConditionExpression = e.ConditionExpression;
            this.innerDocument.Insert(e.TargetIndex, group, e.ParentGroupPublicKey);
        }

        internal void Apply(GroupBecameARoster e)
        {
            this.innerDocument.UpdateGroup(e.GroupId, group => group.IsRoster = true);
        }

        internal void Apply(RosterChanged e)
        {
            this.innerDocument.UpdateGroup(e.GroupId, group =>
            {
                group.RosterSizeQuestionId = e.RosterSizeQuestionId;
                group.RosterSizeSource = e.RosterSizeSource;
                group.RosterFixedTitles = e.RosterFixedTitles;
                group.RosterTitleQuestionId = e.RosterTitleQuestionId;
            });
        }

        private void Apply(GroupStoppedBeingARoster e)
        {
            this.innerDocument.UpdateGroup(e.GroupId, group => group.IsRoster = false);
        }

        internal void Apply(NewQuestionAdded e)
        {
            QuestionType questionType = e.QuestionType;
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        e.QuestionType,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        e.AnswerOrder,
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        e.Mask,
                        e.Triggers,
                        null,
                        e.Answers,
                        e.LinkedToQuestionId,
                        e.IsInteger,
                        null,
                        e.AreAnswersOrdered,
                        e.MaxAllowedAnswers,
                        null,
                        e.IsFilteredCombobox,
                        e.CascadeFromQuestionId
                        ));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Add(question, e.GroupPublicKey, null);

            this.innerDocument.UpdateRosterGroupsIfNeeded(e.Triggers, e.PublicKey);

            if (e.Capital)
                this.innerDocument.MoveHeadQuestionPropertiesToRoster(e.PublicKey, e.GroupPublicKey);
        }



        internal void Apply(NumericQuestionAdded e)
        {
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        QuestionType.Numeric,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        Order.AZ,
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        null,
                        e.Triggers,
                        e.MaxAllowedValue,
                        null,
                        null,
                        e.IsInteger,
                        e.CountOfDecimalPlaces,
                        null,
                        null,
                        null,
                        null,
                        null));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Add(question, e.GroupPublicKey, null);

            this.innerDocument.UpdateRosterGroupsIfNeeded(e.Triggers, e.PublicKey);

            if (e.Capital)
                this.innerDocument.MoveHeadQuestionPropertiesToRoster(e.PublicKey, e.GroupPublicKey);
        }

        internal void Apply(TextListQuestionAdded e)
        {
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        QuestionType.TextList,
                        QuestionScope.Interviewer,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        null,
                        null,
                        Order.AZ,
                        false,
                        e.Mandatory,
                        false,
                        e.Instructions,
                        null,
                        new List<Guid>(),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        e.MaxAnswerCount,
                        null,
                        null));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Add(question, e.GroupId, null);

        }

        internal void Apply(QuestionCloned e)
        {
            var maxValue = e.QuestionType == QuestionType.Numeric ? e.MaxValue as int? : null;
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        e.QuestionType,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        e.AnswerOrder,
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        e.Mask,
                        e.Triggers,
                        maxValue,
                        e.Answers,
                        e.LinkedToQuestionId,
                        e.IsInteger,
                        e.CountOfDecimalPlaces,
                        e.AreAnswersOrdered,
                        e.MaxAllowedAnswers,
                        e.MaxAnswerCount,
                        e.IsFilteredCombobox,
                        e.CascadeFromQuestionId));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Insert(e.TargetIndex, question, e.GroupPublicKey);

            this.innerDocument.UpdateRosterGroupsIfNeeded(e.Triggers, e.PublicKey);

            if (e.Capital)
                this.innerDocument.MoveHeadQuestionPropertiesToRoster(e.PublicKey, e.GroupPublicKey);
        }

        private void Apply(NumericQuestionCloned e)
        {
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        QuestionType.Numeric,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        Order.AZ,
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        null,
                        e.Triggers,
                        e.MaxAllowedValue,
                        null,
                        null,
                        e.IsInteger,
                        e.CountOfDecimalPlaces,
                        null,
                        null,
                        null,
                        null,
                        null));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Insert(e.TargetIndex, question, e.GroupPublicKey);

            this.innerDocument.UpdateRosterGroupsIfNeeded(e.Triggers, e.PublicKey);

            if (e.Capital)
                this.innerDocument.MoveHeadQuestionPropertiesToRoster(e.PublicKey, e.GroupPublicKey);
        }


        internal void Apply(TextListQuestionCloned e)
        {
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        QuestionType.TextList,
                        QuestionScope.Interviewer,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        null,
                        null,
                        Order.AZ,
                        false,
                        e.Mandatory,
                        false,
                        e.Instructions,
                        null,
                        new List<Guid>(),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        e.MaxAnswerCount,
                        null,
                        null));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Insert(e.TargetIndex, question, e.GroupId);

        }

        private void Apply(NewQuestionnaireCreated e)
        {
            this.innerDocument.IsPublic = e.IsPublic;
            this.innerDocument.Title = e.Title;
            this.innerDocument.PublicKey = e.PublicKey;
            this.innerDocument.CreationDate = e.CreationDate;
            this.innerDocument.LastEntryDate = e.CreationDate;
            this.innerDocument.CreatedBy = e.CreatedBy;
        }

        internal void Apply(QuestionChanged e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.PublicKey);
            QuestionType questionType = e.QuestionType;
            IQuestion newQuestion =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        question.PublicKey,
                        e.QuestionType,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        e.AnswerOrder,
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        e.Mask,
                        e.Triggers,
                        questionType == QuestionType.AutoPropagate || questionType == QuestionType.Numeric ? e.MaxValue as int? : null,
                        e.Answers,
                        e.LinkedToQuestionId,
                        e.IsInteger,
                        null,
                        e.AreAnswersOrdered,
                        e.MaxAllowedAnswers,
                        null,
                        e.IsFilteredCombobox,
                        e.CascadeFromQuestionId));

            this.innerDocument.ReplaceEntity(question, newQuestion);

            this.innerDocument.UpdateRosterGroupsIfNeeded(e.Triggers, e.PublicKey);

            if (e.Capital)
                this.innerDocument.MoveHeadQuestionPropertiesToRoster(e.PublicKey, null);
        }

        internal void Apply(NumericQuestionChanged e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.PublicKey);
            IQuestion newQuestion =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        question.PublicKey,
                        QuestionType.Numeric,
                        e.QuestionScope,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        e.ValidationExpression,
                        e.ValidationMessage,
                        Order.AZ,
                        e.Featured,
                        e.Mandatory,
                        e.Capital,
                        e.Instructions,
                        null,
                        e.Triggers,
                        e.MaxAllowedValue,
                        null,
                        null,
                        e.IsInteger,
                        e.CountOfDecimalPlaces,
                        null,
                        null,
                        null,
                        null,
                        null));

            this.innerDocument.ReplaceEntity(question, newQuestion);

            this.innerDocument.UpdateRosterGroupsIfNeeded(e.Triggers, e.PublicKey);

            if (e.Capital)
                this.innerDocument.MoveHeadQuestionPropertiesToRoster(e.PublicKey, null);
        }

        internal void Apply(TextListQuestionChanged e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.PublicKey);
            IQuestion newQuestion =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.PublicKey,
                        QuestionType.TextList,
                        QuestionScope.Interviewer,
                        e.QuestionText,
                        e.StataExportCaption,
                        e.VariableLabel,
                        e.ConditionExpression,
                        null,
                        null,
                        Order.AZ,
                        false,
                        e.Mandatory,
                        false,
                        e.Instructions,
                        null,
                        new List<Guid>(),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        e.MaxAnswerCount,
                        null,
                        null));

            if (question == null)
            {
                return;
            }

            this.innerDocument.ReplaceEntity(question, newQuestion);

        }

        private void Apply(QuestionDeleted e)
        {
            this.innerDocument.RemoveEntity(e.QuestionId);

            this.innerDocument.RemoveHeadPropertiesFromRosters(e.QuestionId);
        }

        private void Apply(QuestionnaireItemMoved e)
        {
            bool isLegacyEvent = e.AfterItemKey != null;

            if (isLegacyEvent)
            {
                Logger.Warn(string.Format("Ignored legacy MoveItem event in questionnaire {0}", this.EventSourceId));
                return;
            }

            this.innerDocument.MoveItem(e.PublicKey, e.GroupKey, e.TargetIndex);

            this.innerDocument.CheckIsQuestionHeadAndUpdateRosterProperties(e.PublicKey, e.GroupKey);
        }

        internal void Apply(QRBarcodeQuestionAdded e)
        {
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.QuestionId,
                        QuestionType.QRBarcode,
                        QuestionScope.Interviewer,
                        e.Title,
                        e.VariableName,
                        e.VariableLabel,
                        e.EnablementCondition,
                        null,
                        null,
                        Order.AZ,
                        false,
                        e.IsMandatory,
                        false,
                        e.Instructions,
                        null,
                        new List<Guid>(),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Add(question, e.ParentGroupId, null);
        }

        internal void Apply(QRBarcodeQuestionUpdated e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.QuestionId);
            IQuestion newQuestion =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.QuestionId,
                        QuestionType.QRBarcode,
                        QuestionScope.Interviewer,
                        e.Title,
                        e.VariableName,
                        e.VariableLabel,
                        e.EnablementCondition,
                        null,
                        null,
                        Order.AZ,
                        false,
                        e.IsMandatory,
                        false,
                        e.Instructions,
                        null,
                        new List<Guid>(),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null));

            if (question == null)
            {
                return;
            }

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        internal void Apply(MultimediaQuestionUpdated e)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(e.QuestionId);
            IQuestion newQuestion =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.QuestionId,
                        QuestionType.Multimedia,
                        QuestionScope.Interviewer,
                        e.Title,
                        e.VariableName,
                        e.VariableLabel,
                        e.EnablementCondition,
                        null,
                        null,
                        Order.AZ,
                        false,
                        e.IsMandatory,
                        false,
                        e.Instructions,
                        null,
                        new List<Guid>(),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null));

            if (question == null)
            {
                return;
            }

            this.innerDocument.ReplaceEntity(question, newQuestion);
        }

        internal void Apply(QRBarcodeQuestionCloned e)
        {
            IQuestion question =
                this.questionnaireEntityFactory.CreateQuestion(
                    new QuestionData(
                        e.QuestionId,
                        QuestionType.QRBarcode,
                        QuestionScope.Interviewer,
                        e.Title,
                        e.VariableName,
                        e.VariableLabel,
                        e.EnablementCondition,
                        null,
                        null,
                        Order.AZ,
                        false,
                        e.IsMandatory,
                        false,
                        e.Instructions,
                        null,
                        new List<Guid>(),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null));

            if (question == null)
            {
                return;
            }

            this.innerDocument.Insert(e.TargetIndex, question, e.ParentGroupId);
        }

        internal void Apply(StaticTextAdded e)
        {
            var staticText = this.questionnaireEntityFactory.CreateStaticText(entityId: e.EntityId, text: e.Text);

            this.innerDocument.Add(c: staticText, parent: e.ParentId, parentPropagationKey: null);
        }

        internal void Apply(StaticTextUpdated e)
        {
            var oldStaticText = this.innerDocument.Find<IStaticText>(e.EntityId);
            var newStaticText = this.questionnaireEntityFactory.CreateStaticText(entityId: e.EntityId, text: e.Text);

            this.innerDocument.ReplaceEntity(oldStaticText, newStaticText);
        }

        internal void Apply(StaticTextCloned e)
        {
            var staticText = this.questionnaireEntityFactory.CreateStaticText(entityId: e.EntityId, text: e.Text);

            this.innerDocument.Insert(e.TargetIndex, staticText, e.ParentId);
        }

        internal void Apply(StaticTextDeleted e)
        {
            this.innerDocument.RemoveEntity(e.EntityId);   
        }

        public QuestionnaireState CreateSnapshot()
        {
            return new QuestionnaireState
            {
                QuestionnaireDocument = this.innerDocument,
                Version = this.Version,
                WasExpressionsMigrationPerformed = wasExpressionsMigrationPerformed,
            };
        }

        public void RestoreFromSnapshot(QuestionnaireState snapshot)
        {
            this.innerDocument = snapshot.QuestionnaireDocument.Clone() as QuestionnaireDocument;
            this.wasExpressionsMigrationPerformed = snapshot.WasExpressionsMigrationPerformed;
        }

        private static int? DetermineActualMaxValueForNumericQuestion(bool isAutopropagating, int? legacyMaxValue, int? actualMaxValue)
        {
            return isAutopropagating ? legacyMaxValue : actualMaxValue;
        }

        #endregion

        #region Dependencies

        private readonly IQuestionnaireEntityFactory questionnaireEntityFactory;

        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        private static IClock Clock
        {
            get { return NcqrsEnvironment.Get<IClock>(); /*ServiceLocator.Current.GetInstance<IClock>(); */}
        }

        private static IExpressionProcessor ExpressionProcessor
        {
            get { return ServiceLocator.Current.GetInstance<IExpressionProcessor>(); }
        }

        protected ISubstitutionService SubstitutionService
        {
            get { return ServiceLocator.Current.GetInstance<ISubstitutionService>(); }
        }

        protected IKeywordsProvider VariableNameValidator
        {
            get { return ServiceLocator.Current.GetInstance<IKeywordsProvider>(); }
        }

        #endregion

        #region Questionnaire command handlers

        public Questionnaire()
            : base()
        {
            this.questionnaireEntityFactory = new QuestionnaireEntityFactory();
        }

        public Questionnaire(Guid publicKey)
            : base(publicKey)
        {
            this.questionnaireEntityFactory = new QuestionnaireEntityFactory();
        }

        public Questionnaire(Guid publicKey, string title, Guid? createdBy = null, bool isPublic = false)
            : base(publicKey)
        {
            this.questionnaireEntityFactory = new QuestionnaireEntityFactory();

            this.CreateQuestionnaire(publicKey, title, createdBy, isPublic);
        }

        public void CreateQuestionnaire(Guid publicKey, string title, Guid? createdBy, bool isPublic)
        {
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespacesOrTooLong(title);

            this.ApplyEvent(
                new NewQuestionnaireCreated
                {
                    IsPublic = isPublic,
                    PublicKey = publicKey,
                    Title = title,
                    CreationDate = Clock.UtcNow(),
                    CreatedBy = createdBy
                });

            this.ApplyEvent(
                new NewGroupAdded
                {
                    GroupText = "New Chapter",
                    PublicKey = Guid.NewGuid()
                }
            );

            this.ApplyEvent(new ExpressionsMigratedToCSharp());
        }

        public Questionnaire(Guid createdBy, IQuestionnaireDocument source)
            : base(source.PublicKey)
        {
            this.questionnaireEntityFactory = new QuestionnaireEntityFactory();

            ImportQuestionnaire(createdBy, source);
        }

        public Questionnaire(Guid publicKey, string title, Guid createdBy, IQuestionnaireDocument source)
            : this(publicKey, title, createdBy, false, source) { }

        public Questionnaire(Guid publicKey, string title, Guid createdBy, bool isPublic, IQuestionnaireDocument source)
            : base(publicKey)
        {
            this.CloneQuestionnaire(title, isPublic, createdBy, publicKey, source);
        }

        public void CloneQuestionnaire(string title, bool isPublic, Guid createdBy, Guid publicKey, IQuestionnaireDocument source)
        {
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespacesOrTooLong(title);

            var clock = NcqrsEnvironment.Get<IClock>();

            var document = source as QuestionnaireDocument;
            if (document == null)
                throw new QuestionnaireException(DomainExceptionType.TemplateIsInvalid, "only QuestionnaireDocuments are supported for now");

            var clonedDocument = (QuestionnaireDocument)document.Clone();
            clonedDocument.PublicKey = this.EventSourceId;
            clonedDocument.CreatedBy = createdBy;
            clonedDocument.CreationDate = clock.UtcNow();
            clonedDocument.Title = title;
            clonedDocument.IsPublic = isPublic;
            if (clonedDocument.SharedPersons != null)
            {
                clonedDocument.SharedPersons.Clear();
            }

            ApplyEvent(new QuestionnaireCloned
            {
                QuestionnaireDocument = clonedDocument,
                ClonedFromQuestionnaireId = clonedDocument.PublicKey,
                ClonedFromQuestionnaireVersion = clonedDocument.LastEventSequence
            });

            if (source.UsesCSharp)
            {
                this.ApplyEvent(new ExpressionsMigratedToCSharp());
            }
        }

        public void ImportQuestionnaire(Guid createdBy, IQuestionnaireDocument source)
        {

            var document = source as QuestionnaireDocument;
            if (document == null)
                throw new QuestionnaireException(DomainExceptionType.TemplateIsInvalid, "Only QuestionnaireDocuments are supported for now");
            document.CreatedBy = createdBy;
            ApplyEvent(new TemplateImported() { Source = document });

        }

        public void UpdateQuestionnaire(string title, bool isPublic, Guid responsibleId)
#warning CRUD
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespacesOrTooLong(title);

            this.ApplyEvent(new QuestionnaireUpdated() { Title = title, IsPublic = isPublic, ResponsibleId = responsibleId });
        }

        public void DeleteQuestionnaire()
        {
            this.ApplyEvent(new QuestionnaireDeleted());
        }

        #endregion

        #region Group command handlers

        public void AddGroupAndMoveIfNeeded(Guid groupId, Guid responsibleId, string title, string variableName, Guid? rosterSizeQuestionId, string description, string condition, Guid? parentGroupId, bool isRoster, RosterSizeSourceType rosterSizeSource, string[] rosterFixedTitles, Guid? rosterTitleQuestionId, int? index = null)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupAlreadyExists(groupId);

            this.ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespacesOrTooLong(title);

            this.ThrowDomainExceptionIfVariableNameIsInvalid(groupId, variableName);

            this.ThrowIfExpressionContainsNotExistingQuestionReference(condition, variableName);

            this.ThrowIfRosterInformationIsIncorrect(groupId: groupId, isRoster: isRoster, rosterSizeSource: rosterSizeSource,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: rosterFixedTitles,
                rosterTitleQuestionId: rosterTitleQuestionId, rosterDepthFunc: () => GetQuestionnaireItemDepthAsVector(parentGroupId));

            if (parentGroupId.HasValue)
            {
                this.innerDocument.ConnectChildrenWithParent();
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroupId.Value);
            }

            this.ApplyEvent(new NewGroupAdded
            {
                PublicKey = groupId,
                GroupText = title,
                VariableName = variableName,
                ParentGroupPublicKey = parentGroupId,
                Description = description,
                ConditionExpression = condition,
                ResponsibleId = responsibleId
            });

            if (isRoster)
            {
                this.ApplyEvent(new GroupBecameARoster(responsibleId, groupId));
                this.ApplyEvent(new RosterChanged(responsibleId, groupId, rosterSizeQuestionId, rosterSizeSource, rosterFixedTitles,
                    rosterTitleQuestionId));
            }
            else
            {
                this.ApplyEvent(new GroupStoppedBeingARoster(responsibleId, groupId));
            }

            if (index.HasValue)
            {
                this.ApplyEvent(new QuestionnaireItemMoved
                {
                    PublicKey = groupId,
                    GroupKey = parentGroupId,
                    TargetIndex = index.Value,
                    ResponsibleId = responsibleId
                });
            }
        }

        public void CloneGroupWithoutChildren(Guid groupId, Guid responsibleId,
            string title, string variableName, Guid? rosterSizeQuestionId, string description, string condition,
            Guid? parentGroupId, Guid sourceGroupId, int targetIndex, bool isRoster, RosterSizeSourceType rosterSizeSource,
            string[] rosterFixedTitles, Guid? rosterTitleQuestionId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupAlreadyExists(groupId);

            this.ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespacesOrTooLong(title);

            this.ThrowIfExpressionContainsNotExistingQuestionReference(condition, variableName);

            this.ThrowIfRosterInformationIsIncorrect(groupId: groupId, 
                isRoster: isRoster,
                rosterSizeSource: rosterSizeSource,
                rosterSizeQuestionId: rosterSizeQuestionId, 
                rosterFixedTitles: rosterFixedTitles,
                rosterTitleQuestionId: 
                rosterTitleQuestionId, 
                rosterDepthFunc: () => this.GetQuestionnaireItemDepthAsVector(parentGroupId));

            var events = this.CreateCloneGroupWithoutChildrenEvents(groupId, responsibleId, title, variableName, rosterSizeQuestionId, description, condition, parentGroupId, sourceGroupId, targetIndex, isRoster, rosterSizeSource, rosterFixedTitles, rosterTitleQuestionId);

            events.ForEach(this.ApplyEvent);
        }

        public void CloneGroup(Guid groupId, Guid responsibleId, Guid sourceGroupId, int targetIndex)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupAlreadyExists(groupId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(sourceGroupId);

            this.innerDocument.ConnectChildrenWithParent();

            var sourceGroup = this.innerDocument.FirstOrDefault<IGroup>(group => group.PublicKey == sourceGroupId);

            var numberOfCopiedItems = sourceGroup.Children.TreeToEnumerable(x => x.Children).Count();
            var numberOfItemsInChapter = this.innerDocument.GetChapterOfItemById(sourceGroupId)
                                                           .Children
                                                           .TreeToEnumerable(x => x.Children)
                                                           .Count();
            
            if ((numberOfCopiedItems + numberOfItemsInChapter) >= MaxChapterItemsCount)
            {
                throw new QuestionnaireException(string.Format("Chapter cannot have more than {0} elements", MaxChapterItemsCount));
            }

            var parentGroupId = sourceGroup.GetParent() == null ? (Guid?)null : sourceGroup.GetParent().PublicKey;

            var events = new List<object>();

            this.FillGroup(groupId: groupId,
                parentGroupId: parentGroupId, 
                responsibleId: responsibleId,
                sourceGroup: sourceGroup, 
                targetIndex: targetIndex,
                events: events);

            events.ForEach(this.ApplyEvent);
        }

        private void FillGroup(Guid groupId, Guid? parentGroupId, Guid responsibleId, IGroup sourceGroup, int targetIndex, List<object> events)
        {
            events.AddRange(this.CreateCloneGroupWithoutChildrenEvents(groupId: groupId, 
                responsibleId: responsibleId, 
                parentGroupId: parentGroupId,
                sourceGroupId: sourceGroup.PublicKey, 
                title: sourceGroup.Title, 
                targetIndex: targetIndex,
                description: sourceGroup.Description, 
                condition: sourceGroup.ConditionExpression,
                isRoster: sourceGroup.IsRoster, 
                rosterSizeSource: sourceGroup.RosterSizeSource,
                rosterSizeQuestionId: sourceGroup.RosterSizeQuestionId,
                rosterTitleQuestionId: null,
                rosterFixedTitles: sourceGroup.RosterFixedTitles,
                variableName: sourceGroup.VariableName));

            foreach (var questionnaireItem in sourceGroup.Children)
            {
                var itemId = Guid.NewGuid();
                var sourceItemId = questionnaireItem.PublicKey;
                var itemTargetIndex = sourceGroup.Children.IndexOf(questionnaireItem);

                var @group = questionnaireItem as IGroup;
                if (@group != null)
                {
                    this.FillGroup(groupId: itemId, 
                        parentGroupId: groupId, 
                        responsibleId: responsibleId,
                        sourceGroup: @group, 
                        targetIndex: itemTargetIndex,
                        events: events);
                    continue;
                }

                var question = questionnaireItem as IQuestion;
                if (question != null)
                {
                    var variableName = string.Empty;
                    var variableLabel = question.VariableLabel;
                    var title = question.QuestionText;
                    var isMandatory = question.Mandatory;
                    var enablementCondition = question.ConditionExpression;
                    var instructions = question.Instructions;

                    var numericQuestion = question as INumericQuestion;
                    if (numericQuestion != null)
                    {
                        events.AddRange(this.CreateNumericQuestionCloneEvents(questionId: itemId, targetIndex: itemTargetIndex,
                            variableName: variableName, 
                            variableLabel: variableLabel, 
                            parentGroupId: groupId,
                            title: title,
                            isMandatory: isMandatory, 
                            isPreFilled: numericQuestion.Featured,
                            scope: numericQuestion.QuestionScope, 
                            enablementCondition: enablementCondition,
                            validationExpression: numericQuestion.ValidationExpression,
                            validationMessage: numericQuestion.ValidationMessage, 
                            instructions: instructions,
                            sourceQuestionId: sourceItemId,
                            responsibleId: responsibleId,
                            maxValue: numericQuestion.MaxValue, 
                            isInteger: numericQuestion.IsInteger,
                            countOfDecimalPlaces: numericQuestion.CountOfDecimalPlaces));
                        continue;
                    }

                    var textListQuestion = question as ITextListQuestion;
                    if (textListQuestion != null)
                    {
                        events.AddRange(this.CreateTextListQuestionClonedEvents(questionId: itemId, targetIndex: itemTargetIndex,
                            variableName: variableName, variableLabel: variableLabel, parentGroupId: groupId,
                            title: title,
                            isMandatory: isMandatory, enablementCondition: enablementCondition,
                            instructions: instructions,
                            sourceQuestionId: sourceItemId, responsibleId: responsibleId,
                            maxAnswerCount: textListQuestion.MaxAnswerCount));
                        continue;
                    }

                    var qrBarcodeQuestion = question as IQRBarcodeQuestion;
                    if (qrBarcodeQuestion != null)
                    {
                        events.AddRange(this.CreateQrBarcodeQuestionClonedEvents(questionId: itemId, targetIndex: itemTargetIndex,
                            variableName: variableName, variableLabel: variableLabel, parentGroupId: groupId,
                            title: title,
                            isMandatory: isMandatory, enablementCondition: enablementCondition,
                            instructions: instructions,
                            sourceQuestionId: sourceItemId, responsibleId: responsibleId));
                        continue;
                    }

                    var textQuestion = question as TextQuestion;
                    if (textQuestion != null)
                    {
                        events.AddRange(this.CreateTextQuestionClonedEvents(questionId: itemId, targetIndex: itemTargetIndex,
                            variableName: variableName, variableLabel: variableLabel, parentGroupId: groupId,
                            title: title, isMandatory: isMandatory,
                            enablementCondition: enablementCondition, responsibleId: responsibleId,
                            sourceQuestionId: sourceItemId, instructions: instructions, mask: textQuestion.Mask,
                            isPreFilled: textQuestion.Featured,
                            scope: textQuestion.QuestionScope, validationExpression: textQuestion.ValidationExpression,
                            validationMessage: textQuestion.ValidationMessage));
                        continue;
                    }

                    var geoLocationQuestion = question as GpsCoordinateQuestion;
                    if (geoLocationQuestion != null)
                    {
                        events.AddRange(this.CreateGeoLocationQuestionClonedEvents(questionId: itemId, targetIndex: itemTargetIndex,
                            variableName: variableName, variableLabel: variableLabel, title: title,
                            isMandatory: isMandatory,
                            enablementCondition: enablementCondition, instructions: instructions,
                            parentGroupId: groupId, sourceQuestionId: sourceItemId,
                            responsibleId: responsibleId));
                        continue;
                    }

                    var dateTitmeQuestion = question as DateTimeQuestion;
                    if (dateTitmeQuestion != null)
                    {
                        events.AddRange(this.CreateDateTimeQuestionClonedEvents(questionId: itemId, targetIndex: itemTargetIndex,
                            variableName: variableName, variableLabel: variableLabel, title: title,
                            isMandatory: isMandatory,
                            enablementCondition: enablementCondition, instructions: instructions,
                            parentGroupId: groupId, sourceQuestionId: sourceItemId,
                            responsibleId: responsibleId, scope: dateTitmeQuestion.QuestionScope,
                            isPreFilled: dateTitmeQuestion.Featured,
                            validationExpression: dateTitmeQuestion.ValidationExpression,
                            validationMessage: dateTitmeQuestion.ValidationMessage));
                        continue;
                    }

                    var categoricalMultiQuestion = question as MultyOptionsQuestion;
                    if (categoricalMultiQuestion != null)
                    {
                        events.AddRange(this.CreateCategoricalMultiAnswersQuestionClonedEvents(questionId: itemId,
                            targetIndex: itemTargetIndex, variableName: variableName, variableLabel: variableLabel,
                            title: title, isMandatory: isMandatory,
                            enablementCondition: enablementCondition, parentGroupId: groupId,
                            sourceQuestionId: sourceItemId, instructions: instructions, responsibleId: responsibleId,
                            scope: categoricalMultiQuestion.QuestionScope,
                            validationExpression: categoricalMultiQuestion.ValidationExpression,
                            validationMessage: categoricalMultiQuestion.ValidationMessage,
                            linkedToQuestionId: categoricalMultiQuestion.LinkedToQuestionId,
                            areAnswersOrdered: categoricalMultiQuestion.AreAnswersOrdered,
                            maxAllowedAnswers: categoricalMultiQuestion.MaxAllowedAnswers,
                            options:
                                categoricalMultiQuestion.Answers.Select(
                                    answer => new Option(answer.PublicKey, answer.AnswerValue, answer.AnswerText))
                                    .ToArray()));
                        continue;
                    }

                    var categoricalSingleQuestion = question as SingleQuestion;
                    if (categoricalSingleQuestion != null)
                    {
                        events.AddRange(this.CreateCategoricalSingleAnswerQuestionEvents(questionId: itemId,
                            targetIndex: itemTargetIndex, variableName: variableName, variableLabel: variableLabel,
                            title: title, isMandatory: isMandatory,
                            enablementCondition: enablementCondition, parentGroupId: groupId,
                            sourceQuestionId: sourceItemId, instructions: instructions, responsibleId: responsibleId,
                            scope: categoricalSingleQuestion.QuestionScope,
                            validationExpression: categoricalSingleQuestion.ValidationExpression,
                            validationMessage: categoricalSingleQuestion.ValidationMessage,
                            linkedToQuestionId: categoricalSingleQuestion.LinkedToQuestionId,
                            isPreFilled: categoricalSingleQuestion.Featured,
                            isFilteredCombobox: categoricalSingleQuestion.IsFilteredCombobox,
                            cascadeFromQuestionId: categoricalSingleQuestion.CascadeFromQuestionId,
                            options:
                                categoricalSingleQuestion.Answers.Select(
                                    answer => new Option(answer.PublicKey, answer.AnswerValue, answer.AnswerText, answer.ParentValue))
                                    .ToArray()));
                        continue;
                    }

                    var multimediaQuestion = question as IMultimediaQuestion;
                    if (multimediaQuestion != null)
                    {
                        events.AddRange(this.CreateMultimediaQuestionClonedEvents(questionId: itemId, targetIndex: itemTargetIndex,
                            variableName: variableName, variableLabel: variableLabel, parentGroupId: groupId,
                            title: title,
                            isMandatory: isMandatory, enablementCondition: enablementCondition,
                            instructions: instructions,
                            sourceQuestionId: sourceItemId, responsibleId: responsibleId));
                        continue;
                    }
                }

                var staticText = questionnaireItem as IStaticText;
                if (staticText != null)
                {
                    events.Add(new StaticTextCloned
                    {
                        EntityId = itemId,
                        ParentId = groupId,
                        SourceEntityId = sourceItemId,
                        TargetIndex = itemTargetIndex,
                        Text = staticText.Text,
                        ResponsibleId = responsibleId
                    });
                    continue;
                }
            }
        }

        public void UpdateGroup(Guid groupId, Guid responsibleId,
            string title,string variableName, Guid? rosterSizeQuestionId, string description, string condition, bool isRoster,
            RosterSizeSourceType rosterSizeSource, string[] rosterFixedTitles, Guid? rosterTitleQuestionId)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);

            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);

            this.ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespacesOrTooLong(title);

            this.ThrowDomainExceptionIfVariableNameIsInvalid(groupId, variableName);

            this.ThrowIfExpressionContainsNotExistingQuestionReference(condition, variableName);

            this.ThrowIfRosterInformationIsIncorrect(groupId: groupId, isRoster: isRoster, rosterSizeSource: rosterSizeSource,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterFixedTitles: rosterFixedTitles,
                rosterTitleQuestionId: rosterTitleQuestionId, rosterDepthFunc: () => GetQuestionnaireItemDepthAsVector(groupId));

            var group = this.GetGroupById(groupId);

            var wasGroupAndBecomeARoster = !@group.IsRoster && isRoster;
            var wasRosterAndBecomeAGroup = @group.IsRoster && !isRoster;

            if (wasGroupAndBecomeARoster)
            {
                this.ThrowIfGroupCantBecomeARosterBecauseOfPrefilledQuestions(group);
            }
            if (wasRosterAndBecomeAGroup)
            {
                this.ThrowIfRosterHaveAQuestionThatUsedAsRosterTitleQuestionOfOtherGroups(group);
                this.ThrowIfRosterCantBecomeAGroupBecauseContainsLinkedSourceQuestions(group);
                this.ThrowIfRosterCantBecomeAGroupBecauseOfReferencesOnRosterTitleInSubstitutions(group, wasRosterAndBecomeAGroup: true);
            }

            this.ApplyEvent(new GroupUpdated
            {
                GroupPublicKey = groupId,
                GroupText = title,
                VariableName = variableName,
                Description = description,
                ConditionExpression = condition,
                ResponsibleId = responsibleId
            });

            if (isRoster)
            {
                this.ApplyEvent(new GroupBecameARoster(responsibleId, groupId));
                this.ApplyEvent(new RosterChanged(responsibleId, groupId, rosterSizeQuestionId, rosterSizeSource, rosterFixedTitles,
                    rosterTitleQuestionId));
            }
            else
            {
                this.ApplyEvent(new GroupStoppedBeingARoster(responsibleId, groupId));
            }
        }

        public void DeleteGroup(Guid groupId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);
            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);
            this.ThrowDomainExceptionIfGroupQuestionsUsedAsRosterTitleQuestionOfOtherGroups(groupId);

            var group = this.GetGroupById(groupId);

            this.ThrowDomainExceptionIfRosterQuestionsUsedAsLinkedSourceQuestions(group);

            this.ApplyEvent(new GroupDeleted() { GroupPublicKey = groupId, ResponsibleId = responsibleId });
        }

        public void MoveGroup(Guid groupId, Guid? targetGroupId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(groupId);

            this.ThrowDomainExceptionIfMoreThanOneGroupExists(groupId);

            if (targetGroupId.HasValue)
            {
                this.ThrowDomainExceptionIfGroupDoesNotExist(targetGroupId.Value);
            }
            this.innerDocument.ConnectChildrenWithParent();
            var sourceGroup = this.GetGroupById(groupId);

            if (targetGroupId.HasValue)
            {
                var sourceChapter = this.innerDocument.GetChapterOfItemById(groupId);
                var targetChapter = this.innerDocument.GetChapterOfItemById(targetGroupId.Value);

                if (sourceChapter.PublicKey != targetChapter.PublicKey)
                {
                    var numberOfMovedItems = sourceGroup.Children
                        .TreeToEnumerable(x => x.Children)
                        .Count();

                    var numberOfItemsInChapter = targetChapter.Children
                        .TreeToEnumerable(x => x.Children)
                        .Count();

                    if ((numberOfMovedItems + numberOfItemsInChapter) >= MaxChapterItemsCount)
                    {
                        throw new QuestionnaireException(string.Format("Chapter cannot have more than {0} elements", MaxChapterItemsCount));
                    }
                }
            }
            
            // if we don't have a target group we would like to move source group into root of questionnaire
            var targetGroup = targetGroupId.HasValue ? this.GetGroupById(targetGroupId.Value) : this.innerDocument;

            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, sourceGroup.GetParent() as IGroup);

            this.ThrowIfGroupFromRosterThatContainsRosterTitleQuestionMovedToAnotherGroup(sourceGroup, targetGroup);
            this.ThrowIfSourceGroupContainsInvalidRosterSizeQuestions(sourceGroup, targetGroup);
            this.ThrowIfGroupFromRosterThatContainsLinkedSourceQuestionsMovedToGroup(sourceGroup, targetGroup);
            this.ThrowIfGroupMovedFromRosterToGroupAndContainsRosterTitleInSubstitution(sourceGroup, targetGroup);

            this.ThrowIfRosterInformationIsIncorrect(groupId: groupId, isRoster: sourceGroup.IsRoster,
                rosterSizeSource: sourceGroup.RosterSizeSource,
                rosterSizeQuestionId: sourceGroup.RosterSizeQuestionId, rosterFixedTitles: sourceGroup.RosterFixedTitles,
                rosterTitleQuestionId: sourceGroup.RosterTitleQuestionId,
                rosterDepthFunc: () => GetQuestionnaireItemDepthAsVector(targetGroup.PublicKey));
            this.ApplyEvent(new QuestionnaireItemMoved
            {
                PublicKey = groupId,
                GroupKey = targetGroupId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId
            });
        }

        #endregion

        public void CloneQuestionById(Guid questionId, Guid responsibleId, Guid targetId)
        {
            this.ThrowDomainExceptionIfQuestionAlreadyExists(targetId);

            IQuestion question = this.GetQuestion(questionId);
            
            this.innerDocument.ConnectChildrenWithParent();
            IComposite parentGroup = question.GetParent();
            this.ThrowIfChapterHasMoreThanAllowedLimit(question.PublicKey);

            var asTextQuestion = question as TextQuestion;
            var asMultioptions = question as IMultyOptionsQuestion;
            var asNumeric = question as NumericQuestion;
            var asListQuestion = question as TextListQuestion;

            var questionCloned = new QuestionCloned
            {
                PublicKey = targetId,

                GroupPublicKey = parentGroup.PublicKey,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                VariableLabel = question.VariableLabel,

                Mandatory = question.Mandatory,
                Featured = question.Featured,
                Capital = question.Capital,

                QuestionScope = question.QuestionScope,
                ConditionExpression = question.ConditionExpression,
                ValidationExpression = question.ValidationExpression,
                ValidationMessage = question.ValidationMessage,
                Instructions = question.Instructions,

                Answers = question.Answers.ToArray(),
                SourceQuestionId = questionId,
                TargetIndex =  parentGroup.Children.IndexOf(question) + 1,
                ResponsibleId = responsibleId,
                LinkedToQuestionId = question.LinkedToQuestionId,

                AreAnswersOrdered = asMultioptions != null ? (bool?)asMultioptions.AreAnswersOrdered : null,
              
                Mask = asTextQuestion != null ? asTextQuestion.Mask : null,

                CascadeFromQuestionId = question.CascadeFromQuestionId,
                IsFilteredCombobox = question.IsFilteredCombobox
            };
            if (asNumeric != null)
            {
                questionCloned.IsInteger = asNumeric.IsInteger;
                questionCloned.MaxValue = asNumeric.MaxValue;
                questionCloned.CountOfDecimalPlaces = asNumeric.CountOfDecimalPlaces;
            }
            if (asListQuestion != null)
            {
                questionCloned.MaxAnswerCount = asListQuestion.MaxAnswerCount;
            }
            if (asMultioptions != null)
            {
                questionCloned.MaxAllowedAnswers = asMultioptions.MaxAllowedAnswers;
            }

            this.ApplyEvent(questionCloned);
        }

        public void CloneStaticText(Guid entityId, Guid sourceEntityId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            this.ThrowDomainExceptionIfEntityDoesNotExists(sourceEntityId);
            this.ThrowDomainExceptionIfEntityAlreadyExists(entityId);

            this.innerDocument.ConnectChildrenWithParent();

            var staticText = this.innerDocument.Find<IStaticText>(sourceEntityId);
            var parentOfStaticText = staticText.GetParent();

            this.ApplyEvent(new StaticTextCloned()
            {
                EntityId = entityId,
                ParentId = parentOfStaticText.PublicKey,
                SourceEntityId = sourceEntityId,
                TargetIndex = parentOfStaticText.Children.IndexOf(staticText) + 1,
                Text = staticText.Text,
                ResponsibleId = responsibleId
            });
        }

        public void AddDefaultTypeQuestionAdnMoveIfNeeded(AddDefaultTypeQuestionCommand command)
        {
            this.ThrowDomainExceptionIfQuestionAlreadyExists(command.QuestionId);
            var parentGroup = this.GetGroupById(command.ParentGroupId);
            this.ThrowIfChapterHasMoreThanAllowedLimit(command.ParentGroupId);

            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(command.ResponsibleId);

            this.innerDocument.ConnectChildrenWithParent();
            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }
           
            this.ApplyEvent(new NewQuestionAdded
            {
                PublicKey = command.QuestionId,
                GroupPublicKey = command.ParentGroupId,
                QuestionText = command.Title,
                QuestionType = QuestionType.Text,
                StataExportCaption = null,
                VariableLabel = null,
                Mandatory = false,
                Featured = false,
                QuestionScope = QuestionScope.Interviewer,
                ConditionExpression = null,
                ValidationExpression = null,
                ValidationMessage = null,
                Instructions = null,
                ResponsibleId = command.ResponsibleId,
                LinkedToQuestionId = null,
                AreAnswersOrdered = null,
                MaxAllowedAnswers = null,
                Mask = null,
                IsFilteredCombobox = false,
                CascadeFromQuestionId = null
            });

            if (command.Index.HasValue)
            {
                this.ApplyEvent(new QuestionnaireItemMoved
                {
                    PublicKey = command.QuestionId,
                    GroupKey = command.ParentGroupId,
                    TargetIndex = command.Index.Value,
                    ResponsibleId = command.ResponsibleId
                });
            }
        }

        public void DeleteQuestion(Guid questionId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfQuestionUsedInConditionOrValidationOfOtherQuestionsAndGroups(questionId);
            this.ThrowIfQuestionIsUsedAsRosterSize(questionId);
            this.ThrowIfQuestionIsUsedAsRosterTitle(questionId);
            this.ThrowIfQuestionIsUsedAsCascadingParent(questionId);

            this.ApplyEvent(new QuestionDeleted() { QuestionId = questionId, ResponsibleId = responsibleId });
        }

        public void MoveQuestion(Guid questionId, Guid targetGroupId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(targetGroupId);

            this.ThrowIfChapterHasMoreThanAllowedLimit(targetGroupId);

            var question = this.innerDocument.Find<AbstractQuestion>(questionId);
            var targetGroup = this.innerDocument.Find<IGroup>(targetGroupId);

            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, question.GetParent() as IGroup);

            this.ThrowDomainExceptionIfQuestionTitleContainsIncorrectSubstitution(question.QuestionText, question.StataExportCaption,
                questionId, question.Featured, targetGroup);

            this.innerDocument.ConnectChildrenWithParent();
            this.ThrowDomainExceptionIfQuestionIsPrefilledAndParentGroupIsRoster(question.Featured, targetGroup);
            this.ThrowDomainExceptionIfQuestionIsRosterTitleAndItsMovedToIncorrectGroup(question, targetGroup);

            this.ThrowDomainExceptionIfQuestionIsRosterSizeAndItsMovedToIncorrectGroup(question, targetGroup);

            this.ApplyEvent(new QuestionnaireItemMoved
            {
                PublicKey = questionId,
                GroupKey = targetGroupId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId
            });
        }

        public void UpdateTextQuestion(
            Guid questionId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            bool isPreFilled,
            QuestionScope scope,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            string mask,
            Guid responsibleId)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, isPreFilled,
                responsibleId);
            this.ThrowIfConditionOrValidationExpressionContainsNotExistingQuestionReference(enablementCondition, validationExpression, variableName);

            this.ApplyEvent(new QuestionChanged
            {
                PublicKey = questionId,
                QuestionText = title,
                QuestionType = QuestionType.Text,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                Featured = isPreFilled,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                ResponsibleId = responsibleId,
                Mask = mask
            });
        }

        public void UpdateGpsCoordinatesQuestion(
            Guid questionId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            QuestionScope scope,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            Guid responsibleId)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, false, responsibleId);
            this.ThrowIfConditionOrValidationExpressionContainsNotExistingQuestionReference(enablementCondition, validationExpression, variableName);

            this.ApplyEvent(new QuestionChanged
            {
                PublicKey = questionId,
                QuestionText = title,
                QuestionType = QuestionType.GpsCoordinates,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                ResponsibleId = responsibleId
            });
        }

        public void UpdateDateTimeQuestion(
            Guid questionId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            bool isPreFilled,
            QuestionScope scope,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            Guid responsibleId)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, isPreFilled,
                responsibleId);
            this.ThrowIfConditionOrValidationExpressionContainsNotExistingQuestionReference(enablementCondition, validationExpression, variableName);

            this.ApplyEvent(new QuestionChanged
            {
                PublicKey = questionId,
                QuestionText = title,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                Featured = isPreFilled,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                ResponsibleId = responsibleId
            });
        }

        public void UpdateMultiOptionQuestion(
            Guid questionId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            QuestionScope scope,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            Guid responsibleId,
            Option[] options,
            Guid? linkedToQuestionId,
            bool areAnswersOrdered,
            int? maxAllowedAnswers)
        {
            PrepareGeneralProperties(ref title, ref variableName);
            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, false, responsibleId);
            this.ThrowIfQuestionIsRosterTitleLinkedCategoricalQuestion(questionId, linkedToQuestionId);
            this.ThrowIfCategoricalQuestionIsInvalid(questionId, options, linkedToQuestionId, false, null, scope, null);
            this.ThrowIfMaxAllowedAnswersInvalid(QuestionType.MultyOption, linkedToQuestionId, maxAllowedAnswers, options);
            this.ThrowIfConditionOrValidationExpressionContainsNotExistingQuestionReference(enablementCondition, validationExpression, variableName);

            this.ApplyEvent(new QuestionChanged
            {
                PublicKey = questionId,
                QuestionText = title,
                QuestionType = QuestionType.MultyOption,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                Answers = ConvertOptionsToAnswers(options),
                ResponsibleId = responsibleId,
                LinkedToQuestionId = linkedToQuestionId,
                AreAnswersOrdered = areAnswersOrdered,
                MaxAllowedAnswers = maxAllowedAnswers
            });
        }

        #region Question: SingleOption command handlers

        public void UpdateSingleOptionQuestion(
            Guid questionId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            bool isPreFilled,
            QuestionScope scope,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            Guid responsibleId,
            Option[] options,
            Guid? linkedToQuestionId,
            bool isFilteredCombobox,
            Guid? cascadeFromQuestionId)
        {
            Answer[] answers;

            if (options == null && (isFilteredCombobox || cascadeFromQuestionId.HasValue))
            {
                IQuestion question = this.GetQuestion(questionId);
                answers = question.Answers.ToArray();                
            }
            else
            {
                answers = ConvertOptionsToAnswers(options);                
            }

            PrepareGeneralProperties(ref title, ref variableName);
            IGroup parentGroup = this.innerDocument.GetParentById(questionId);
            
            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);
            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, isPreFilled, responsibleId);

            if (isFilteredCombobox || cascadeFromQuestionId.HasValue)
            {
                var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);
                answers = categoricalOneAnswerQuestion != null ? categoricalOneAnswerQuestion.Answers.ToArray() : null;
            }

            this.ThrowIfQuestionIsRosterTitleLinkedCategoricalQuestion(questionId, linkedToQuestionId);
            this.ThrowIfCategoricalQuestionIsInvalid(questionId, options, linkedToQuestionId, isPreFilled, isFilteredCombobox, scope, cascadeFromQuestionId);
            this.ThrowIfCascadingQuestionHasConditionOrValidation(questionId, cascadeFromQuestionId, validationExpression, enablementCondition);
            this.ThrowIfCategoricalSingleOptionsQuestionHasMoreThan200Options(options, isFilteredCombobox, cascadeFromQuestionId, linkedToQuestionId.HasValue);
            this.ThrowIfConditionOrValidationExpressionContainsNotExistingQuestionReference(enablementCondition, validationExpression, variableName);


            this.ApplyEvent(new QuestionChanged
            {
                PublicKey = questionId,
                QuestionText = title,
                QuestionType = QuestionType.SingleOption,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                Featured = isPreFilled,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                Answers = answers,
                ResponsibleId = responsibleId,
                LinkedToQuestionId = linkedToQuestionId,
                IsFilteredCombobox = isFilteredCombobox,
                CascadeFromQuestionId = cascadeFromQuestionId
            });
        }

        public void UpdateFilteredComboboxOptions(Guid questionId, Guid responsibleId, Option[] options)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfFilteredComboboxIsInvalid(questionId, options);
            ThrowIfNotLinkedCategoricalQuestionIsInvalid(options);

            var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);

            this.ApplyEvent(new QuestionChanged
            {
                PublicKey = questionId,
                QuestionText = categoricalOneAnswerQuestion.QuestionText,
                QuestionType = categoricalOneAnswerQuestion.QuestionType,
                StataExportCaption = categoricalOneAnswerQuestion.StataExportCaption,
                VariableLabel = categoricalOneAnswerQuestion.VariableLabel,
                Mandatory = categoricalOneAnswerQuestion.Mandatory,
                Featured = categoricalOneAnswerQuestion.Featured,
                QuestionScope = categoricalOneAnswerQuestion.QuestionScope,
                ConditionExpression = categoricalOneAnswerQuestion.ConditionExpression,
                ValidationExpression = categoricalOneAnswerQuestion.ValidationExpression,
                ValidationMessage = categoricalOneAnswerQuestion.ValidationMessage,
                Instructions = categoricalOneAnswerQuestion.Instructions,
                Answers = ConvertOptionsToAnswers(options),
                ResponsibleId = responsibleId,
                LinkedToQuestionId = categoricalOneAnswerQuestion.LinkedToQuestionId,
                CascadeFromQuestionId = categoricalOneAnswerQuestion.CascadeFromQuestionId,
                IsFilteredCombobox = categoricalOneAnswerQuestion.IsFilteredCombobox
            });
        }

        public void UpdateCascadingComboboxOptions(Guid questionId, Guid responsibleId, Option[] options)
        {
            ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            ThrowIfNotLinkedCategoricalQuestionIsInvalid(options, isCascade: true);

            ThrowDomainExceptionIfOptionsHasEmptyParentValue(options);

            ThrowDomainExceptionIfOptionsHasNotDecimalParentValue(options);

            ThrowDomainExceptionIfOptionsHasNotUniqueTitleAndParentValuePair(options);

            var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);

            this.ApplyEvent(new QuestionChanged
            {
                PublicKey = questionId,
                QuestionText = categoricalOneAnswerQuestion.QuestionText,
                QuestionType = categoricalOneAnswerQuestion.QuestionType,
                StataExportCaption = categoricalOneAnswerQuestion.StataExportCaption,
                VariableLabel = categoricalOneAnswerQuestion.VariableLabel,
                Mandatory = categoricalOneAnswerQuestion.Mandatory,
                Featured = categoricalOneAnswerQuestion.Featured,
                QuestionScope = categoricalOneAnswerQuestion.QuestionScope,
                ConditionExpression = categoricalOneAnswerQuestion.ConditionExpression,
                ValidationExpression = categoricalOneAnswerQuestion.ValidationExpression,
                ValidationMessage = categoricalOneAnswerQuestion.ValidationMessage,
                Instructions = categoricalOneAnswerQuestion.Instructions,
                Answers = ConvertOptionsToAnswers(options),
                ResponsibleId = responsibleId,
                LinkedToQuestionId = categoricalOneAnswerQuestion.LinkedToQuestionId,
                CascadeFromQuestionId = categoricalOneAnswerQuestion.CascadeFromQuestionId,
                IsFilteredCombobox = categoricalOneAnswerQuestion.IsFilteredCombobox
            });
        }
        #endregion

        public void UpdateNumericQuestion(
            Guid questionId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory, 
            bool isPreFilled,
            QuestionScope scope, 
            string enablementCondition, 
            string validationExpression, 
            string validationMessage,
            string instructions,
            int? maxValue, 
            Guid responsibleId,
            bool isInteger,
            int? countOfDecimalPlaces)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, isPreFilled, responsibleId);

            this.ThrowIfPrecisionSettingsAreInConflictWithDecimalPlaces(isInteger, countOfDecimalPlaces);
            this.ThrowIfDecimalPlacesValueIsIncorrect(countOfDecimalPlaces);
            this.ThrowIfConditionOrValidationExpressionContainsNotExistingQuestionReference(enablementCondition, validationExpression, variableName);

            this.ApplyEvent(new NumericQuestionChanged
            {
                PublicKey = questionId,
                QuestionText = title,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                Featured = isPreFilled,
                Capital = false,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                ResponsibleId = responsibleId,
                MaxAllowedValue = maxValue,
                IsInteger = isInteger,
                CountOfDecimalPlaces = countOfDecimalPlaces
            });
        }

        public void UpdateTextListQuestion(Guid questionId, string title, string variableName, string variableLabel,
            bool isMandatory, string enablementCondition, string instructions, Guid responsibleId, int? maxAnswerCount)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            var isPrefilled = false;
            var validationExpression = string.Empty;
            var validationMessage = string.Empty;

            IGroup parentGroup = this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(questionId, parentGroup, title, variableName, isPrefilled, responsibleId);

            this.ThrowIfConditionOrValidationExpressionContainsNotExistingQuestionReference(enablementCondition, validationExpression, variableName);

            ThrowIfMaxAnswerCountNotInRange1to40(maxAnswerCount);

            this.ApplyEvent(new TextListQuestionChanged
            {
                PublicKey = questionId,

                QuestionText = title,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                ConditionExpression = enablementCondition,
                Instructions = instructions,
                ResponsibleId = responsibleId,

                MaxAnswerCount = maxAnswerCount
            });
        }

        public void UpdateMultimediaQuestion(Guid questionId, string title, string variableName, string variableLabel,
         bool isMandatory, string enablementCondition, string instructions, Guid responsibleId)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            this.ThrowIfGeneralQuestionSettingsAreInvalid(questionId: questionId, parentGroupId: null, title: title,
                variableName: variableName, condition: enablementCondition, responsibleId: responsibleId);

            this.ApplyEvent(new MultimediaQuestionUpdated()
            {
                QuestionId = questionId,
                Title = title,
                VariableName = variableName,
                VariableLabel = variableLabel,
                IsMandatory = isMandatory,
                EnablementCondition = enablementCondition,
                Instructions = instructions,
                ResponsibleId = responsibleId
            });
        }

        public void UpdateQRBarcodeQuestion(Guid questionId, string title, string variableName, string variableLabel,
            bool isMandatory, string enablementCondition,
            string validationExpression, 
            string validationMessage, string instructions, Guid responsibleId)
        {
            PrepareGeneralProperties(ref title, ref variableName);

            this.ThrowDomainExceptionIfQuestionDoesNotExist(questionId);
            this.ThrowDomainExceptionIfMoreThanOneQuestionExists(questionId);

            this.ThrowIfGeneralQuestionSettingsAreInvalid(questionId: questionId, parentGroupId: null, title: title,
                variableName: variableName, condition: enablementCondition, responsibleId: responsibleId);

            this.ThrowIfConditionOrValidationExpressionContainsNotExistingQuestionReference(enablementCondition, validationExpression, variableName);
            this.ApplyEvent(new QRBarcodeQuestionUpdated()
            {
                QuestionId = questionId,
                Title = title,
                VariableName = variableName,
                VariableLabel = variableLabel,
                IsMandatory = isMandatory,
                EnablementCondition = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                ResponsibleId = responsibleId
            });
        }

        #region Static text command handlers
        public void AddStaticTextAndMoveIfNeeded(Guid entityId, Guid parentId, string text, Guid responsibleId, int? index = null)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            this.ThrowDomainExceptionIfEntityAlreadyExists(entityId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(parentId);
            this.ThrowDomainExceptionIfStaticTextIsEmpty(text);
            this.innerDocument.ConnectChildrenWithParent();
            this.ThrowIfChapterHasMoreThanAllowedLimit(parentId);

            this.ApplyEvent(new StaticTextAdded
            {
                EntityId = entityId,
                ParentId = parentId,
                ResponsibleId = responsibleId,
                Text = text
            });

            if (index.HasValue)
            {
                this.ApplyEvent(new QuestionnaireItemMoved
                {
                    PublicKey = entityId,
                    GroupKey = parentId,
                    TargetIndex = index.Value,
                    ResponsibleId = responsibleId
                });
            }
        }

        public void UpdateStaticText(Guid entityId, string text, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            
            this.ThrowDomainExceptionIfEntityDoesNotExists(entityId);
            this.ThrowDomainExceptionIfStaticTextIsEmpty(text);

            this.ApplyEvent(new StaticTextUpdated()
            {
                EntityId = entityId,
                Text = text,
                ResponsibleId = responsibleId
            });
        }

        public void DeleteStaticText(Guid entityId, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfEntityDoesNotExists(entityId);

            this.ApplyEvent(new StaticTextDeleted() { EntityId = entityId, ResponsibleId = responsibleId });
        }

        public void MoveStaticText(Guid entityId, Guid targetEntityId, int targetIndex, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfEntityDoesNotExists(entityId);
            this.ThrowDomainExceptionIfGroupDoesNotExist(targetEntityId);
            this.ThrowIfChapterHasMoreThanAllowedLimit(targetEntityId);

            // if we don't have a target group we would like to move source group into root of questionnaire
            var targetGroup = this.GetGroupById(targetEntityId);
            var sourceStaticText = this.innerDocument.Find<IStaticText>(entityId);
            this.ThrowIfTargetIndexIsNotAcceptable(targetIndex, targetGroup, sourceStaticText.GetParent() as IGroup);

            this.ApplyEvent(new QuestionnaireItemMoved
            {
                PublicKey = entityId,
                GroupKey = targetEntityId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId
            });
        }
        #endregion

        #region Shared Person command handlers

        public void AddSharedPerson(Guid personId, string email, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            if (responsibleId == personId)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.OwnerCannotBeInShareList,
                    "You are the owner of this questionnaire. Please, input another email");
            }

            if (this.innerDocument.SharedPersons.Contains(personId))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.UserExistInShareList,
                    string.Format("User {0} already exist in share list", email));
            }

            this.ApplyEvent(new SharedPersonToQuestionnaireAdded()
            {
                PersonId = personId,
                Email = email,
                ResponsibleId = responsibleId
            });
        }

        public void RemoveSharedPerson(Guid personId, string email, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);

            if (!this.innerDocument.SharedPersons.Contains(personId))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.UserDoesNotExistInShareList,
                    "Couldn't remove user, because it doesn't exist in share list");
            }

            this.ApplyEvent(new SharedPersonFromQuestionnaireRemoved()
            {
                PersonId = personId,
                ResponsibleId = responsibleId
            });
        }

        #endregion

        #region Questionnaire Invariants

        private void ThrowIfQuestionIsUsedAsCascadingParent(Guid questionId)
        {
            var usedInCascades = this.innerDocument.Find<SingleQuestion>(x => x.CascadeFromQuestionId == questionId).Any();
            if (usedInCascades)
            {
                throw new QuestionnaireException(ExceptionMessages.CantRemoveParentQuestionInCascading);
            }
        }

        private void ThrowIfGeneralQuestionSettingsAreInvalid(Guid questionId, Guid? parentGroupId, string title, string variableName,
            string condition, Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfTitleIsEmptyOrTooLong(title);
            this.ThrowDomainExceptionIfVariableNameIsInvalid(questionId, variableName);
            this.ThrowIfExpressionContainsNotExistingQuestionReference(condition, variableName);

            var parentGroup = parentGroupId.HasValue
                ? this.GetGroupById(parentGroupId.Value)
                : this.innerDocument.GetParentById(questionId);

            this.ThrowDomainExceptionIfQuestionTitleContainsIncorrectSubstitution(title, variableName, questionId, false, parentGroup);

            if (parentGroupId.HasValue)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroupId.Value);
            }
        }

        private void ThrowIfConditionOrValidationExpressionContainsNotExistingQuestionReference(string condition,
            string validationExpression, string variableName)
        {
            this.ThrowIfExpressionContainsNotExistingQuestionReference(validationExpression, variableName);
            this.ThrowIfExpressionContainsNotExistingQuestionReference(condition, string.Empty);
        }

        private void ThrowDomainExceptionIfGeneralQuestionSettingsAreInvalid(Guid questionId, 
            IGroup parentGroup, 
            string title,
            string alias,
            bool isPrefilled,
            Guid responsibleId)
        {
            this.ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(responsibleId);
            this.ThrowDomainExceptionIfTitleIsEmptyOrTooLong(title);
            this.ThrowDomainExceptionIfVariableNameIsInvalid(questionId, alias);

            this.ThrowDomainExceptionIfQuestionTitleContainsIncorrectSubstitution(title, alias, questionId, isPrefilled, parentGroup);

            this.innerDocument.ConnectChildrenWithParent();
            this.ThrowDomainExceptionIfQuestionIsPrefilledAndParentGroupIsRoster(isPrefilled, parentGroup);

            if (parentGroup != null)
            {
                this.ThrowIfChapterHasMoreThanAllowedLimit(parentGroup.PublicKey);
            }
        }

        private void ThrowIfChapterHasMoreThanAllowedLimit(Guid itemId)
        {
            var chapter = this.innerDocument.GetChapterOfItemById(itemId);
            if (chapter.Children.TreeToEnumerable(x => x.Children).Count() >= MaxChapterItemsCount)
            {
                throw new QuestionnaireException(string.Format("Chapter cannot have more than {0} child items", MaxChapterItemsCount));
            }
        }

        private void ThrowIfGroupMovedFromRosterToGroupAndContainsRosterTitleInSubstitution(IGroup sourceGroup, IGroup targetGroup)
        {
            if (this.IsRosterOrInsideRoster(targetGroup) || sourceGroup.IsRoster)
                return;

            this.ThrowIfRosterCantBecomeAGroupBecauseOfReferencesOnRosterTitleInSubstitutions(sourceGroup);
        }

        private void ThrowIfTargetIndexIsNotAcceptable(int targetIndex, IGroup targetGroup, IGroup parentGroup)
        {
            var maxAcceptableIndex = targetGroup.Children.Count;
            if (parentGroup != null && targetGroup.PublicKey == parentGroup.PublicKey)
                maxAcceptableIndex--;

            if (targetIndex < 0 || maxAcceptableIndex < targetIndex)
                throw new QuestionnaireException(
                   string.Format(
                       "You can't move to group {0}  because it position {1} in not acceptable",
                       FormatGroupForException(targetGroup.PublicKey, this.innerDocument), targetIndex));
        }

        private void ThrowIfGroupFromRosterThatContainsLinkedSourceQuestionsMovedToGroup(IGroup sourceGroup, IGroup targetGroup)
        {
            if (this.IsRosterOrInsideRoster(targetGroup)) return;

            var allQuestionsIdsFromGroup = this.GetAllQuestionsInGroup(sourceGroup).Select(question => question.PublicKey);

            var linkedQuestionSourcesInGroup = this.GetAllLinkedSourceQuestions().Intersect(allQuestionsIdsFromGroup);

            if (linkedQuestionSourcesInGroup.Any())
            {
                throw new QuestionnaireException(
                    string.Format(
                        "You can't move {0} group to another group because it contains linked source question(s): {1}",
                        FormatGroupForException(sourceGroup.PublicKey, this.innerDocument),
                        string.Join(Environment.NewLine,
                            linkedQuestionSourcesInGroup.Select(
                                questionId => this.FormatQuestionForException(questionId, this.innerDocument)))));
            }
        }

        private void ThrowIfSourceGroupContainsInvalidRosterSizeQuestions(IGroup sourceGroup, IGroup targetGroup)
        {
            var allQuestionsIdsFromGroup = this.GetAllQuestionsInGroup(sourceGroup).Select(question => question.PublicKey);

            var rosterSizeQuestionsInGroup = this.GetAllRosterSizeQuestionIds().Intersect(allQuestionsIdsFromGroup);
            var rosterSizeQuestionsOfTargetGroupAndUpperRosters = GetRosterSizeQuestionsOfGroupAndUpperRosters(targetGroup);

            if (rosterSizeQuestionsOfTargetGroupAndUpperRosters.Intersect(rosterSizeQuestionsInGroup).Any())
            {
                throw new QuestionnaireException(
                    string.Format(
                        "You can't move group {0} to roster {1} because it contains roster source question(s): {2}",
                        FormatGroupForException(sourceGroup.PublicKey, this.innerDocument),
                        FormatGroupForException(targetGroup.PublicKey, this.innerDocument),
                        string.Join(Environment.NewLine,
                            rosterSizeQuestionsInGroup.Select(questionId => this.FormatQuestionForException(questionId, this.innerDocument)))));
            }
        }

        private void ThrowIfGroupFromRosterThatContainsRosterTitleQuestionMovedToAnotherGroup(IGroup sourceGroup, IGroup targetGroup)
        {
            if (!IsRosterOrInsideRoster(sourceGroup) && !ContainsRoster(sourceGroup)) return;

            IEnumerable<IQuestion> groupQuestions = GetAllQuestionsInGroup(sourceGroup);

            var rosterTitleQuestionsWithDependentGroups =
                from question in groupQuestions
                let groupsWhereQuestionIsRosterTitle = this.GetGroupsByRosterTitleId(question.PublicKey, sourceGroup.PublicKey)
                where groupsWhereQuestionIsRosterTitle.Any()
                select new { RostertTitleQuestion = question, DependentGroups = groupsWhereQuestionIsRosterTitle };

            if (rosterTitleQuestionsWithDependentGroups.All(question => !question.DependentGroups.Any())) return;

            Func<Guid, IEnumerable<IGroup>, string> getWarningMessage = (rosterTitleQuestionId, invalidGroups) =>
            {
                return string.Format("Question {0} used as roster title question in group(s):{1}{2}",
                    this.FormatQuestionForException(rosterTitleQuestionId, this.innerDocument), Environment.NewLine,
                    string.Join(Environment.NewLine,
                        invalidGroups.Select(group => FormatGroupForException(@group.PublicKey, this.innerDocument))));
            };

            if (IsRosterOrInsideRoster(targetGroup))
            {
                var rosterForTargetGroup = GetFirstRosterParentGroupOrNull(targetGroup);

                var rosterTitleQuestionsWithDependentGroupsByTargetRosterSizeQuestion =
                    rosterTitleQuestionsWithDependentGroups.Select(
                        question =>
                            new
                            {
                                RostertTitleQuestion = question.RostertTitleQuestion,
                                DependentGroups =
                                    question.DependentGroups.Where(
                                        group => group.RosterSizeQuestionId != rosterForTargetGroup.RosterSizeQuestionId)
                            }).Where(question => question.DependentGroups.Any());

                if (!rosterTitleQuestionsWithDependentGroupsByTargetRosterSizeQuestion.Any()) return;

                var warningsForRosterTitlesNotInRostersByRosterSize =
                    rosterTitleQuestionsWithDependentGroupsByTargetRosterSizeQuestion.Select(
                        x => getWarningMessage(x.RostertTitleQuestion.PublicKey, x.DependentGroups));

                throw new QuestionnaireException(
                    string.Join(
                        string.Format(
                            "Group {0} could not be moved to group {1} because contains some questions that used as roster title questions in groups which have roster size question not the same as have target {1} group: ",
                            FormatGroupForException(sourceGroup.PublicKey, this.innerDocument),
                            FormatGroupForException(targetGroup.PublicKey, this.innerDocument)), Environment.NewLine,
                        string.Join(Environment.NewLine, warningsForRosterTitlesNotInRostersByRosterSize)));
            }
            else
            {
                if (sourceGroup.IsRoster || ContainsRoster(sourceGroup)) return;

                var warningsForRosterTitlesNotInRostersByRosterSize =
                    rosterTitleQuestionsWithDependentGroups.Select(
                        x => getWarningMessage(x.RostertTitleQuestion.PublicKey, x.DependentGroups));

                throw new QuestionnaireException(
                    string.Join(
                        string.Format("Group {0} could not be moved to group {1} because: ",
                            FormatGroupForException(sourceGroup.PublicKey, this.innerDocument),
                            FormatGroupForException(targetGroup.PublicKey, this.innerDocument)),
                        Environment.NewLine,
                        string.Join(Environment.NewLine, warningsForRosterTitlesNotInRostersByRosterSize)));
            }
        }

        private void ThrowDomainExceptionIfQuestionIsPrefilledAndParentGroupIsRoster(bool isPrefilled, IGroup parentGroup)
        {
            if (isPrefilled && IsRosterOrInsideRoster(parentGroup))
                throw new QuestionnaireException("Question inside roster group can not be pre-filled.");
        }

        private void ThrowDomainExceptionIfGroupTitleIsEmptyOrWhitespacesOrTooLong(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.GroupTitleRequired,
                    "The titles of groups and chapters can not be empty or contains whitespace only");
            }

            if (title.Length > MaxTitleLength)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.TitleIsTooLarge,
                    string.Format("The titles of groups and chapters can't have more than {0} symbols", MaxTitleLength));
            }
        }

        private void ThrowDomainExceptionIfQuestionnaireTitleIsEmptyOrWhitespacesOrTooLong(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionnaireTitleRequired,
                    "Questionnaire's title can not be empty or contains whitespace only");
            }
            if (title.Length > MaxTitleLength)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.TitleIsTooLarge, 
                    string.Format("Questionnaire's title can't have more than {0} symbols", MaxTitleLength));
            }
        }

        private void ThrowDomainExceptionIfEntityDoesNotExists(Guid entityId)
        {
            var staticText = this.innerDocument.Find<IComposite>(entityId);
            if (staticText == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.EntityNotFound,
                    string.Format("Questionnaire item with id {0} can't be found", entityId));
            }
        }

        private void ThrowDomainExceptionIfQuestionDoesNotExist(Guid publicKey)
        {
            var question = this.innerDocument.Find<AbstractQuestion>(publicKey);
            if (question == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionNotFound,
                    string.Format("Question with public key {0} can't be found", publicKey));
            }
        }

        private void ThrowDomainExceptionIfGroupDoesNotExist(Guid groupPublicKey)
        {
            var group = this.innerDocument.Find<Group>(groupPublicKey);
            if (group == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.GroupNotFound,
                    string.Format("group with public key {0} can't be found", groupPublicKey));
            }
        }

        private void ThrowDomainExceptionIfTitleIsEmptyOrTooLong(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new QuestionnaireException(DomainExceptionType.QuestionTitleRequired, "Question title can't be empty");

            if (title.Length > MaxTitleLength)
            {
                throw new QuestionnaireException(DomainExceptionType.TitleIsTooLarge, 
                    string.Format("Question's title can't have more than {0} symbols", MaxTitleLength));
            }
        }

        private void ThrowDomainExceptionIfStaticTextIsEmpty(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new QuestionnaireException(DomainExceptionType.StaticTextIsEmpty, "Static text is empty");
        }

        private void ThrowDomainExceptionIfVariableNameIsInvalid(Guid questionPublicKey, string stataCaption)
        {
            if (string.IsNullOrEmpty(stataCaption))
            {
                return;
            }

            bool isTooLong = stataCaption.Length > 32;
            if (isTooLong)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.VariableNameMaxLength, "Variable name shouldn't be longer than 32 characters");
            }

            bool containsInvalidCharacters = stataCaption.Any(c => !(c == '_' || Char.IsLetterOrDigit(c)));
            if (containsInvalidCharacters)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.VariableNameSpecialCharacters,
                    "Valid variable name should contain only letters, digits and underscore character");
            }

            bool startsWithDigitOrUnderscore = Char.IsDigit(stataCaption[0]) || stataCaption[0] == '_';
            if (startsWithDigitOrUnderscore)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.VariableNameStartWithDigit, "Variable name shouldn't starts with digit or underscore");
            }

            var captions = this.innerDocument.GetEntitiesByType<AbstractQuestion>()
                .Where(q => q.PublicKey != questionPublicKey)
                .Select(q => q.StataExportCaption);

            bool isNotUnique = captions.Contains(stataCaption);
            if (isNotUnique)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.VarialbeNameNotUnique, "Variable name should be unique in questionnaire's scope");
            }
            
            var keywords = VariableNameValidator.GetAllReservedKeywords();

            foreach (var keyword in keywords.Where(keyword => stataCaption.ToLower() == keyword)) {
                throw new QuestionnaireException(
                    DomainExceptionType.VariableNameShouldNotMatchWithKeywords,
                    keyword + " is a keyword. Variable name shouldn't match with keywords");
            }
        }

        private void ThrowIfExpressionContainsNotExistingQuestionReference(string expression, string variableName)
        {
            if (!IsExpressionDefined(expression))
                return;

            IEnumerable<string> identifiersUsedInExpression = ExpressionProcessor.GetIdentifiersUsedInExpression(expression)
                .Except(new [] { variableName });

            foreach (var identifier in identifiersUsedInExpression)
            {
                this.ParseExpressionIdentifierToExistingQuestionIdIgnoringThisIdentifierOrThrow(identifier, expression);
            }
        }

        private void ThrowIfCascadingQuestionHasConditionOrValidation(Guid questionId, Guid? cascadeFromQuestionId, string validationExpression, string enablementCondition)
        {
            if (!cascadeFromQuestionId.HasValue )
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(validationExpression))
            {
                throw new QuestionnaireException(ExceptionMessages.CascadingCantHaveValidationExpression);
            }

            if (!string.IsNullOrWhiteSpace(enablementCondition))
            {
                throw new QuestionnaireException(ExceptionMessages.CascadingCantHaveConditionExpression);
            }
        }
        private void ThrowIfCategoricalQuestionIsInvalid(Guid questionId, Option[] options, Guid? linkedToQuestionId, bool isFeatured, bool? isFilteredCombobox, QuestionScope scope, Guid? cascadeFromQuestionId)
        {
            bool questionIsLinked = linkedToQuestionId.HasValue;
            bool questionHasOptions = options != null && options.Any();

            if (questionIsLinked && questionHasOptions)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.ConflictBetweenLinkedQuestionAndOptions,
                    "Categorical question cannot be with answers and linked to another question in the same time");
            }

            if (cascadeFromQuestionId.HasValue && questionIsLinked)
            {
                throw new QuestionnaireException(ExceptionMessages.CantBeLinkedAndCascadingAtSameTime);
            }

            if (cascadeFromQuestionId.HasValue)
            {
                var cascadefrom = this.innerDocument.Find<IQuestion>(x => x.PublicKey == cascadeFromQuestionId).FirstOrDefault();
                if (cascadefrom == null)
                {
                    throw new QuestionnaireException(ExceptionMessages.ShouldCascadeFromExistingQuestion);
                }
            }

            if (questionIsLinked)
            {
                //this.ThrowIfQuestionIsRosterTitleLinkedCategoricalQuestion(questionId);
                this.ThrowIfLinkedCategoricalQuestionIsInvalid(linkedToQuestionId, isFeatured);
                this.ThrowIfLinkedCategoricalQuestionIsNotFilledByInterviewer(scope);
            }
            else if (isFilteredCombobox != true && !(cascadeFromQuestionId.HasValue))
            {
                ThrowIfNotLinkedCategoricalQuestionIsInvalid(options);
            }
        }

        private void ThrowIfCategoricalSingleOptionsQuestionHasMoreThan200Options(Option[] options, bool isFilteredCombobox, Guid? cascadeFromQuestionId, bool isLinkedQuestion)
        {
            if (!isLinkedQuestion && !isFilteredCombobox && !cascadeFromQuestionId.HasValue && options.Count() > 200)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CategoricalSingleOptionHasMoreThan200Options,
                    "Categorical one answer question contains more than 200 options");
            }
        }

        private void ThrowIfQuestionIsRosterTitleLinkedCategoricalQuestion(Guid questionId, Guid? linkedToQuestionId)
        {
            if (!linkedToQuestionId.HasValue) return;

            var rosterTitleQuestionGroups =
                this.innerDocument.Find<IGroup>(
                    group => group.RosterTitleQuestionId.HasValue && group.RosterTitleQuestionId.Value == questionId)
                    .Select(group => group.PublicKey);

            if (rosterTitleQuestionGroups.Any())
            {
                throw new QuestionnaireException(
                    string.Format("Linked categorical multy answers question could not be used as a roster title question in group(s): {0}",
                        string.Join(Environment.NewLine,
                            rosterTitleQuestionGroups.Select(groupId => FormatGroupForException(groupId, this.innerDocument)))));
            }
        }

        private void ThrowIfMaxAllowedAnswersInvalid(QuestionType questionType, Guid? linkedToQuestionId, int? maxAllowedAnswers,
            Option[] options)
        {
            if (!maxAllowedAnswers.HasValue) return;

            if (maxAllowedAnswers.Value < 2)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.MaxAllowedAnswersLessThan2,
                    "Maximum Allowed Answers for question should be more than one");
            }

            if (!linkedToQuestionId.HasValue && maxAllowedAnswers.Value > options.Length)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.MaxAllowedAnswersMoreThanOptions,
                    "Maximum Allowed Answers more than question's options");
            }
        }

        private void ThrowIfLinkedCategoricalQuestionIsInvalid(Guid? linkedToQuestionId, bool isPrefilled)
        {
            var linkedToQuestion =
                this.innerDocument.Find<IQuestion>(x => x.PublicKey == linkedToQuestionId).FirstOrDefault();

            if (linkedToQuestion == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.LinkedQuestionDoesNotExist,
                    "Question that you are linked to does not exist");
            }

            bool typeOfLinkedQuestionIsNotSupported = !(
                linkedToQuestion.QuestionType == QuestionType.DateTime ||
                    linkedToQuestion.QuestionType == QuestionType.Numeric ||
                    linkedToQuestion.QuestionType == QuestionType.Text);

            if (typeOfLinkedQuestionIsNotSupported)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.NotSupportedQuestionForLinkedQuestion,
                    "Linked question can be only type of number, text or date");
            }

            if (isPrefilled)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionWithLinkedQuestionCanNotBeFeatured,
                    "Question that linked to another question can not be pre-filled");
            }

            if (!this.IsUnderPropagatableGroup(linkedToQuestion))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.LinkedQuestionIsNotInPropagateGroup,
                    "Question that you are linked to is not in the roster group");
            }
        }

        private void ThrowIfLinkedCategoricalQuestionIsNotFilledByInterviewer(QuestionScope scope)
        {
            if (scope != QuestionScope.Interviewer)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.LinkedCategoricalQuestionCanNotBeFilledBySupervisor,
                    "Linked categorical questions cannot be filled by supervisor");
            }
        }

        private static void ThrowIfNotLinkedCategoricalQuestionIsInvalid(Option[] options, bool isCascade = false)
        {
            if ((options == null || !options.Any() || options.Count() < 2))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.SelectorEmpty, "Question with options should have two options at least");
            }

            if (options.Any(x => string.IsNullOrEmpty(x.Value)))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.SelectorValueRequired, "Answer option value is required");
            }

            var tooLongValues = options.Where(option => option.Value.Length > 16).Select(option => option.Value).ToList();
            if (tooLongValues.Any())
            {
                throw new QuestionnaireException(string.Format("Following option values are too long: {0}", string.Join(", ", tooLongValues)));
            }

            if (options.Any(x => !x.Value.IsDecimal()))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.SelectorValueSpecialCharacters,
                    "Option value should have only number characters");
            }

            if (!AreElementsUnique(options.Select(x => x.Value)))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.SelectorValueNotUnique,
                    "Option values must be unique for categorical question");
            }

            if (options.Any(x => string.IsNullOrEmpty(x.Title)))
            {
                throw new QuestionnaireException(DomainExceptionType.SelectorTextRequired, "Answer title can't be empty");
            }

            if (!isCascade && !AreElementsUnique(options.Select(x => x.Title)))
            {
                throw new QuestionnaireException(DomainExceptionType.SelectorTextNotUnique, "Answer title is not unique");
            }
        }

        private static void ThrowIfMaxAnswerCountNotInRange1to40(int? maxAnswerCount)
        {
            if (maxAnswerCount.HasValue && !Enumerable.Range(1, 40).Contains(maxAnswerCount.Value))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.MaxAnswerCountNotInRange,
                    "Maximum number of answers should be in range from 1 to 40");
            }
        }

        private void ThrowDomainExceptionIfEntityAlreadyExists(Guid entityId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IComposite>(
                elementId: entityId,
                expectedCount: 0,
                exceptionType: DomainExceptionType.EntityWithSuchIdAlreadyExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more questionnaire item(s) with same ID {0} already exists",
                        entityId));
        }

        private void ThrowDomainExceptionIfQuestionAlreadyExists(Guid questionId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IQuestion>(
                elementId: questionId,
                expectedCount: 0,
                exceptionType: DomainExceptionType.QuestionWithSuchIdAlreadyExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more question(s) with same ID {0} already exist:{1}{2}",
                        questionId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(question => question.QuestionText ?? "<untitled>"))));
        }

        private void ThrowDomainExceptionIfGroupAlreadyExists(Guid groupId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IGroup>(
                elementId: groupId,
                expectedCount: 0,
                exceptionType: DomainExceptionType.GroupWithSuchIdAlreadyExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more group(s) with same ID {0} already exist:{1}{2}",
                        groupId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(group => group.Title ?? "<untitled>"))));
        }

        private void ThrowDomainExceptionIfMoreThanOneEntityExists(Guid entityId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IComposite>(
                elementId: entityId,
                expectedCount: 1,
                exceptionType: DomainExceptionType.MoreThanOneEntityWithSuchIdExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more questionnaire item(s) with same ID {0} already exists",
                        entityId));
        }

        private void ThrowDomainExceptionIfMoreThanOneQuestionExists(Guid questionId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IQuestion>(
                elementId: questionId,
                expectedCount: 1,
                exceptionType: DomainExceptionType.MoreThanOneQuestionsWithSuchIdExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more question(s) with same ID {0} already exist:{1}{2}",
                        questionId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(question => question.QuestionText ?? "<untitled>"))));
        }

        private void ThrowIfPrecisionSettingsAreInConflictWithDecimalPlaces(bool isInteger, int? countOfDecimalPlaces)
        {
            if (isInteger && countOfDecimalPlaces.HasValue)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.IntegerQuestionCantHaveDecimalPlacesSettings,
                    "Roster size question can't have decimal places settings");
            }
        }

        private void ThrowIfDecimalPlacesValueIsIncorrect(int? countOfDecimalPlaces)
        {
            if (!countOfDecimalPlaces.HasValue)
                return;

            if (countOfDecimalPlaces.Value > MaxCountOfDecimalPlaces)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CountOfDecimalPlacesValueIsIncorrect,
                    string.Format("Count of decimal places '{0}' exceeded maximum '{1}'.", countOfDecimalPlaces, MaxCountOfDecimalPlaces));
            }

            if (countOfDecimalPlaces.Value < 0)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CountOfDecimalPlacesValueIsIncorrect,
                    string.Format("Count of decimal places cant be negative."));
            }

            if (countOfDecimalPlaces.Value == 0)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CountOfDecimalPlacesValueIsIncorrect,
                    string.Format("If you want Count of decimal places equals to 0 please select integer."));
            }
        }

        private void ThrowDomainExceptionIfMoreThanOneGroupExists(Guid groupId)
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IGroup>(
                elementId: groupId,
                expectedCount: 1,
                exceptionType: DomainExceptionType.MoreThanOneGroupsWithSuchIdExists,
                getExceptionDescription:
                    elementsWithSameId => string.Format("One or more group(s) with same ID {0} already exist:{1}{2}",
                        groupId,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, elementsWithSameId.Select(group => group.Title ?? "<untitled>"))));
        }

        private void ThrowDomainExceptionIfElementCountIsMoreThanExpected<T>(Guid elementId, int expectedCount,
            DomainExceptionType exceptionType, Func<IEnumerable<T>, string> getExceptionDescription)
            where T : class, IComposite
        {
            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<T>(x => x.PublicKey == elementId, expectedCount,
                exceptionType, getExceptionDescription);
        }

        private void ThrowDomainExceptionIfElementCountIsMoreThanExpected<T>(Func<T, bool> condition, int expectedCount,
            DomainExceptionType exceptionType, Func<IEnumerable<T>, string> getExceptionDescription)
            where T : class, IComposite
        {
            List<T> elementsWithSameId = this.innerDocument.Find(condition).ToList();

            if (elementsWithSameId.Count > expectedCount)
            {
                throw new QuestionnaireException(exceptionType, getExceptionDescription(elementsWithSameId));
            }
        }

        private void ThrowDomainExceptionIfViewerDoesNotHavePermissionsForEditQuestionnaire(Guid viewerId)
        {
            if (this.innerDocument.CreatedBy != viewerId && !this.innerDocument.SharedPersons.Contains(viewerId))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.DoesNotHavePermissionsForEdit,
                    "You don't have permissions for changing this questionnaire");
            }
        }

        private void ThrowDomainExceptionIfOptionsHasEmptyParentValue(Option[] options)
        {
            if (options.Select(x => x.ParentValue).Any(string.IsNullOrWhiteSpace))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CategoricalCascadingOptionsCantContainsEmptyParentValueField,
                    ExceptionMessages.CategoricalCascadingOptionsCantContainsEmptyParentValueField);
            }
        }

        private void ThrowDomainExceptionIfOptionsHasNotDecimalParentValue(Option[] options)
        {
            decimal d;
            if (options.Select(x => x.ParentValue).Any(number => !Decimal.TryParse(number, out d)))
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CategoricalCascadingOptionsCantContainsNotDecimalParentValueField,
                    ExceptionMessages.CategoricalCascadingOptionsCantContainsNotDecimalParentValueField);
            }
        }

        private void ThrowDomainExceptionIfOptionsHasNotUniqueTitleAndParentValuePair(Option[] options)
        {

            if (options.Select(x => x.ParentValue + "$" + x.Title).Distinct().Count() != options.Length)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.CategoricalCascadingOptionsContainsNotUniqueTitleAndParentValuePair,
                    ExceptionMessages.CategoricalCascadingOptionsContainsNotUniqueTitleAndParentValuePair);
            }
        }

        private void ThrowDomainExceptionIfQuestionTitleContainsIncorrectSubstitution(string questionTitle, string alias,
            Guid questionPublicKey, bool isFeatured, IGroup group)
        {
            string[] substitutionReferences = SubstitutionService.GetAllSubstitutionVariableNames(questionTitle);
            if (substitutionReferences.Length == 0)
                return;

            if (isFeatured)
                throw new QuestionnaireException(
                    DomainExceptionType.FeaturedQuestionTitleContainsSubstitutionReference,
                    "Pre-filled question title contains substitution references. It's illegal");

            if (substitutionReferences.Contains(alias))
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionTitleContainsSubstitutionReferenceToSelf,
                    "Question title contains illegal substitution references to self");

            List<string> unknownReferences, questionsIncorrectTypeOfReferenced, questionsIllegalPropagationScope;

            this.innerDocument.ConnectChildrenWithParent(); //find all references and do it only once

            ValidateSubstitutionReferences(questionPublicKey, @group, substitutionReferences,
                out unknownReferences, out questionsIncorrectTypeOfReferenced, out questionsIllegalPropagationScope);

            if (unknownReferences.Count > 0)
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionTitleContainsUnknownSubstitutionReference,
                    "Question title contains unknown substitution references: " + String.Join(", ", unknownReferences.ToArray()));

            if (questionsIncorrectTypeOfReferenced.Count > 0)
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionTitleContainsSubstitutionReferenceQuestionOfInvalidType,
                    "Question title contains substitution references to questions of illegal type: " +
                        String.Join(", ", questionsIncorrectTypeOfReferenced.ToArray()));

            if (questionsIllegalPropagationScope.Count > 0)
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionTitleContainsInvalidSubstitutionReference,
                    "Question title contains illegal substitution references to questions: " +
                        String.Join(", ", questionsIllegalPropagationScope.ToArray()));
        }

        private void ThrowDomainExceptionIfQuestionUsedInConditionOrValidationOfOtherQuestionsAndGroups(Guid questionId)
        {
            var question = this.innerDocument.FirstOrDefault<IQuestion>(x => x.PublicKey == questionId);

            this.ThrowDomainExceptionIfElementCountIsMoreThanExpected<IComposite>(
                condition:
                    x => IsGroupAndHaveQuestionIdInCondition(x, question) || IsQuestionAndHaveQuestionIdInConditionOrValidation(x, question),
                expectedCount: 0,
                exceptionType: DomainExceptionType.QuestionOrGroupDependOnAnotherQuestion,
                getExceptionDescription:
                    groupsAndQuestions => string.Format("One or more questions/groups depend on {0}:{1}{2}",
                        question.QuestionText,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, GetTitleList(groupsAndQuestions))));
        }

        private void ThrowDomainExceptionIfGroupQuestionsUsedAsRosterTitleQuestionOfOtherGroups(Guid groupId)
        {
            var groupQuestions = this.innerDocument.Find<IQuestion>(x => IsQuestionParent(groupId, x));

            var referencedQuestions = groupQuestions.ToDictionary(question => question.PublicKey,
                question => this.GetGroupsByRosterTitleId(question.PublicKey, groupId).Select(GetTitle));

            if (referencedQuestions.Values.Count(x => x.Any()) > 0)
            {
                throw new QuestionnaireException(DomainExceptionType.QuestionUsedAsRosterTitleOfOtherGroup, string.Join(Environment.NewLine,
                    referencedQuestions.Select(x => string.Format("Question {0} used as roster title question in group(s):{1}{2}",
                        FormatQuestionForException(x.Key, this.innerDocument),
                        Environment.NewLine,
                        string.Join(Environment.NewLine, x.Value)))));
            }
        }

        private void ThrowDomainExceptionIfQuestionIsRosterTitleAndItsMovedToIncorrectGroup(IQuestion question, IGroup targetGroup)
        {

            if (!GetGroupsByRosterTitleId(question.PublicKey).Any())
                return;

            this.innerDocument.ConnectChildrenWithParent();

            IGroup sourceRoster = GetFirstRosterParentGroupOrNull(question);
            IGroup targetRoster = GetFirstRosterParentGroupOrNull(targetGroup);

            if (targetRoster == null || (sourceRoster != null && targetRoster != null && sourceRoster.RosterSizeQuestionId.HasValue &&
                targetRoster.RosterSizeQuestionId.HasValue && sourceRoster.RosterSizeQuestionId != targetRoster.RosterSizeQuestionId))
                throw new QuestionnaireException(
                    string.Format(
                        "You can move a roster title question {0} only to a roster group that has a roster source question {1}",
                        this.FormatQuestionForException(question.PublicKey, this.innerDocument),
                        this.FormatQuestionForException(sourceRoster.RosterSizeQuestionId.Value, this.innerDocument)));
        }

        private void ThrowDomainExceptionIfQuestionIsRosterSizeAndItsMovedToIncorrectGroup(AbstractQuestion question, IGroup targetGroup)
        {
            var groupsByRosterSizeQuestion =
                this.GetGroupsByRosterSizeQuestion(question.PublicKey).Select(x => x.PublicKey);

            if (!groupsByRosterSizeQuestion.Any())
                return;

            foreach (var groupByRosterSizeQuestion in groupsByRosterSizeQuestion)
            {
                if (
                    !this.IsReferencedItemInTheSameScopeWithReferencesItem(GetQuestionnaireItemDepthAsVector(targetGroup.PublicKey),
                        GetQuestionnaireItemDepthAsVector(groupByRosterSizeQuestion)))
                    //   if (GetQuestionnaireItemDepth(targetGroup.PublicKey) > GetQuestionnaireItemDepth(groupByRosterSizeQuestion) - 1)
                    throw new QuestionnaireException(string.Format(
                        "Roster source question {0} cannot be placed deeper then roster.",
                        FormatQuestionForException(question.PublicKey, this.innerDocument)));
            }
        }

        private void ThrowIfRosterInformationIsIncorrect(Guid groupId, bool isRoster, RosterSizeSourceType rosterSizeSource,
            Guid? rosterSizeQuestionId,
            string[] rosterFixedTitles, Guid? rosterTitleQuestionId, Func<Guid[]> rosterDepthFunc)
        {
            if (!isRoster) return;

            switch (rosterSizeSource)
            {
                case RosterSizeSourceType.Question:
                    this.ThrowIfRosterSizeQuestionIsIncorrect(groupId, rosterSizeQuestionId, rosterTitleQuestionId, rosterFixedTitles,
                        rosterDepthFunc);
                    break;
                case RosterSizeSourceType.FixedTitles:
                    this.ThrowIfRosterByFixedTitlesIsIncorrect(rosterSizeQuestionId, rosterTitleQuestionId, rosterFixedTitles);
                    break;
            }
        }

        private void ThrowIfRosterByFixedTitlesIsIncorrect(Guid? rosterSizeQuestionId, Guid? rosterTitleQuestionId,
            string[] rosterFixedTitles)
        {
            if (rosterFixedTitles == null || rosterFixedTitles.Length == 0)
            {
                throw new QuestionnaireException("List of fixed roster titles could not be empty");
            }

            if (rosterFixedTitles.Length > 250)
            {
                throw new QuestionnaireException("Number of fixed roster titles could not be more than 250");
            }

            if (rosterFixedTitles.Any(string.IsNullOrWhiteSpace))
            {
                throw new QuestionnaireException("Fixed roster titles could not have empty titles");
            }

            if (rosterSizeQuestionId.HasValue)
            {
                throw new QuestionnaireException("Roster by fixed titles could not have roster source question");
            }

            if (rosterTitleQuestionId.HasValue)
            {
                throw new QuestionnaireException("Roster by fixed titles could not have roster title question");
            }
        }

        private void ThrowIfRosterSizeQuestionIsIncorrect(Guid groupId, Guid? rosterSizeQuestionId, Guid? rosterTitleQuestionId,
            string[] rosterFixedTitles, Func<Guid[]> rosterDepthFunc)
        {
            if (!rosterSizeQuestionId.HasValue)
                throw new QuestionnaireException("Roster source question is empty");

            var localRosterSizeQuestionId = rosterSizeQuestionId.Value;
            var rosterSizeQuestion = this.innerDocument.Find<IQuestion>(localRosterSizeQuestionId);
            if (rosterSizeQuestion == null)
                // TODO: Guid should be replaced, but question is missing, so title or variable name cannot be found 
                throw new QuestionnaireException(string.Format(
                    "Roster source question {0} is missing in questionnaire.",
                    rosterSizeQuestionId));

            if (!RosterSizeQuestionTypes.Contains(rosterSizeQuestion.QuestionType))
                throw new QuestionnaireException(string.Format(
                    "Roster source question {0} should have Numeric or Categorical Multy Answers or List type.",
                    FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));

            if (
                !this.IsReferencedItemInTheSameScopeWithReferencesItem(this.GetQuestionnaireItemDepthAsVector(rosterSizeQuestionId),
                    rosterDepthFunc()))
                throw new QuestionnaireException(string.Format(
                    "Roster source question {0} cannot be placed deeper then roster.",
                    FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));


            if (rosterSizeQuestion.QuestionType == QuestionType.MultyOption && rosterSizeQuestion.LinkedToQuestionId.HasValue)
                throw new QuestionnaireException(string.Format(
                    "Roster source question {0} should not be linked.",
                    FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));

            if (rosterSizeQuestion.QuestionType == QuestionType.MultyOption && rosterTitleQuestionId.HasValue)
                throw new QuestionnaireException(string.Format(
                    "Roster having categorical multiple answers question {0} as roster size source cannot have roster title question.",
                    this.FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));

            if (rosterSizeQuestion.QuestionType == QuestionType.TextList && rosterTitleQuestionId.HasValue)
                throw new QuestionnaireException(string.Format(
                    "Roster having list question {0} as roster size source cannot have roster title question.",
                    this.FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));

            if (rosterSizeQuestion.QuestionType == QuestionType.Numeric)
            {
                var numericQuestion = (INumericQuestion)rosterSizeQuestion;

                if (!numericQuestion.IsInteger)
                    throw new QuestionnaireException(string.Format(
                        "Roster source question {0} should be Integer.",
                        FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));

                if (rosterTitleQuestionId.HasValue)
                {
                    var rosterTitleQuestion = this.innerDocument.Find<IQuestion>(rosterTitleQuestionId.Value);
                    if (rosterTitleQuestion == null)
                        // TODO: Guid should be replaced, but question is missing, so title or variable name cannot be found 
                        throw new QuestionnaireException(string.Format(
                            "Roster title question {0} is missing in questionnaire.", rosterTitleQuestionId));

                    if (
                        !IsRosterTitleInRosterByRosterSize(rosterTitleQuestion: rosterTitleQuestion,
                            rosterSizeQuestionId: localRosterSizeQuestionId, currentRosterId: groupId))
                        throw new QuestionnaireException(string.Format(
                            "Question for roster titles {0} should be placed only inside groups where roster source question is {1}",
                            FormatQuestionForException(rosterTitleQuestionId.Value, this.innerDocument),
                            FormatQuestionForException(localRosterSizeQuestionId, this.innerDocument)));
                }
            }

            if (rosterFixedTitles != null && rosterFixedTitles.Any())
            {
                throw new QuestionnaireException(string.Format("Roster fixed titles should be empty for roster group by question: {0}.",
                    FormatGroupForException(groupId, this.innerDocument)));
            }
        }

        private void ThrowIfQuestionIsUsedAsRosterSize(Guid questionId)
        {
            var referencingRoster = this.innerDocument.Find<IGroup>(group => @group.RosterSizeQuestionId == questionId).FirstOrDefault();

            if (referencingRoster != null)
                throw new QuestionnaireException(
                    string.Format("Question {0} is referenced as roster source question by group {1}.",
                        FormatQuestionForException(questionId, this.innerDocument),
                        FormatGroupForException(referencingRoster.PublicKey, this.innerDocument)));
        }

        private void ThrowIfQuestionIsUsedAsRosterTitle(Guid questionId)
        {
            var referencingRosterTitle =
                this.innerDocument.Find<IGroup>(
                    group =>
                        @group.RosterTitleQuestionId == questionId && group.RosterSizeQuestionId.HasValue &&
                            this.innerDocument.FirstOrDefault<IQuestion>(
                                question =>
                                    question.PublicKey == @group.RosterSizeQuestionId.Value && question.QuestionType == QuestionType.Numeric) != null)
                    .FirstOrDefault();

            if (referencingRosterTitle != null)
                throw new QuestionnaireException(
                    string.Format("Question {0} is referenced as roster title question by group {1}.",
                        FormatQuestionForException(questionId, this.innerDocument),
                        FormatGroupForException(referencingRosterTitle.PublicKey, this.innerDocument)));
        }

        private void ThrowIfRosterHaveAQuestionThatUsedAsRosterTitleQuestionOfOtherGroups(IGroup group)
        {
            var allRosterTitleQuestions =
                this.innerDocument.Find<IGroup>(g => g.PublicKey != group.PublicKey && g.RosterTitleQuestionId.HasValue)
                    .Select(g => g.RosterTitleQuestionId.Value);


            if (!allRosterTitleQuestions.Any()) return;

            var allQuestionsInGroup = GetAllQuestionsInGroup(group).Select(q => q.PublicKey);

            var rosterTitleQuestionsOfOtherGroups =
                allQuestionsInGroup.Intersect(allRosterTitleQuestions);

            if (!rosterTitleQuestionsOfOtherGroups.Any()) return;

            throw new QuestionnaireException(
                string.Format(
                    "This roster can't become a group because contains a roster title questions of other group(s): {0}",
                    string.Join(Environment.NewLine,
                        rosterTitleQuestionsOfOtherGroups.Select(
                            questionId => FormatQuestionForException(questionId, this.innerDocument)))));
        }

        private void ThrowIfGroupCantBecomeARosterBecauseOfPrefilledQuestions(IGroup group)
        {
            var hasAnyPrefilledQuestion = this.innerDocument.GetEntitiesByType<AbstractQuestion>(@group).Any(question => question.Featured);

            if (!hasAnyPrefilledQuestion) return;

            var questionVariables = GetFilteredQuestionForException(@group, question => question.Featured);

            throw new QuestionnaireException(
                string.Format(
                    "This group can't become a roster because contains pre-filled questions: {0}. Toggle pre-filled property for that questions to complete this operation",
                    string.Join(Environment.NewLine, questionVariables)));
        }

        private void ThrowIfRosterCantBecomeAGroupBecauseOfReferencesOnRosterTitleInSubstitutions(IGroup group, bool wasRosterAndBecomeAGroup = false)
        {
            var hasAnyQuestionsWithRosterTitleInSubstitutions =
                this.innerDocument.GetEntitiesByType<AbstractQuestion>(@group)
                    .Where(x => wasRosterAndBecomeAGroup || GetFirstRosterParentGroupOrNull(x, group) == null)
                    .Any(
                        question =>
                            SubstitutionService.GetAllSubstitutionVariableNames(question.QuestionText)
                                .Contains(SubstitutionService.RosterTitleSubstitutionReference));

            if (!hasAnyQuestionsWithRosterTitleInSubstitutions) return;

            var questionVariables = GetFilteredQuestionForException(@group, question => SubstitutionService.GetAllSubstitutionVariableNames(question.QuestionText)
                                .Contains(SubstitutionService.RosterTitleSubstitutionReference));

            throw new QuestionnaireException(
                string.Format(
                    "This group can't become a roster because contains questions with reference on roster title in substitution: {0}.",
                    string.Join(Environment.NewLine, questionVariables)));
        }

        private void ThrowIfRosterCantBecomeAGroupBecauseContainsLinkedSourceQuestions(IGroup group)
        {
            if (GetFirstRosterParentGroupOrNull(group.GetParent()) != null)
                return;

            var allQuestionsIdsFromGroup = this.GetAllQuestionsInGroup(@group).Select(question => question.PublicKey);

            var linkedQuestionSourcesInGroup = this.GetAllLinkedSourceQuestions().Intersect(allQuestionsIdsFromGroup);

            if (linkedQuestionSourcesInGroup.Any())
            {
                throw new QuestionnaireException(
                    string.Format(
                        "This {0} roster can't become a group because contains linked source question(s): {1}",
                        FormatGroupForException(@group.PublicKey, this.innerDocument),
                        string.Join(Environment.NewLine,
                            linkedQuestionSourcesInGroup.Select(
                                questionId => this.FormatQuestionForException(questionId, this.innerDocument)))));
            }
        }

        private void ThrowDomainExceptionIfRosterQuestionsUsedAsLinkedSourceQuestions(IGroup group)
        {
            if (!this.IsRosterOrInsideRoster(@group)) return;

            var allQuestionsIdsFromGroup = this.GetAllQuestionsInGroup(@group).Select(question => question.PublicKey);

            var linkedQuestionSourcesInGroup = this.GetAllLinkedSourceQuestions().Intersect(allQuestionsIdsFromGroup);

            if (linkedQuestionSourcesInGroup.Any())
            {
                throw new QuestionnaireException(
                    string.Format(
                        "You can't delete {0} group because it contains linked source question(s): {1}",
                        FormatGroupForException(@group.PublicKey, this.innerDocument),
                        string.Join(Environment.NewLine,
                            linkedQuestionSourcesInGroup.Select(
                                questionId => this.FormatQuestionForException(questionId, this.innerDocument)))));
            }
        }

        private void ThrowDomainExceptionIfFilteredComboboxIsInvalid(Guid questionId, Option[] options)
        {
            var categoricalOneAnswerQuestion = this.innerDocument.Find<SingleQuestion>(questionId);
            if (categoricalOneAnswerQuestion == null)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.FilteredComboboxQuestionNotFound,
                    string.Format("Filtered combobox with public key {0} can't be found", questionId));
            }

            if (!categoricalOneAnswerQuestion.IsFilteredCombobox.HasValue || !categoricalOneAnswerQuestion.IsFilteredCombobox.Value)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.QuestionIsNotAFilteredCombobox,
                    string.Format("Question {0} is not a filtered combobox", FormatQuestionForException(questionId, this.innerDocument)));
            }

            if (options != null && options.Length > maxFilteredComboboxOptionsCount)
            {
                throw new QuestionnaireException(
                    DomainExceptionType.FilteredComboboxQuestionOptionsMaxLength,
                    string.Format("Filtered combobox question {0} contains more than {1} options",
                        FormatQuestionForException(questionId, this.innerDocument), maxFilteredComboboxOptionsCount));
            }
        }

        private void ThrowIfExpressionsAreAlreadyMigrated()
        {
            if (this.wasExpressionsMigrationPerformed)
                throw new QuestionnaireException("Expressions are already migrated to C#.");
        }

        #endregion

        #region Utilities

        private static void PrepareGeneralProperties(ref string title, ref string variableName)
        {
            if (variableName != null)
                variableName = variableName.Trim();

            if (title != null)
                title = title.Trim();
        }

        private void ValidateSubstitutionReferences(Guid questionPublicKey, IGroup @group, string[] substitutionReferences,
            out List<string> unknownReferences, out List<string> questionsIncorrectTypeOfReferenced,
            out List<string> questionsIllegalPropagationScope)
        {
            unknownReferences = new List<string>();
            questionsIncorrectTypeOfReferenced = new List<string>();
            questionsIllegalPropagationScope = new List<string>();

            var questions = this.innerDocument.GetEntitiesByType<AbstractQuestion>()
                .Where(q => q.PublicKey != questionPublicKey)
                .Where(q => !string.IsNullOrEmpty(q.StataExportCaption))
                .ToDictionary(q => q.StataExportCaption, q => q);

            var propagationQuestionsVector = GetQuestionnaireItemDepthAsVector(@group.PublicKey);

            foreach (var substitutionReference in substitutionReferences)
            {
                if (substitutionReference == SubstitutionService.RosterTitleSubstitutionReference)
                {
                    if (propagationQuestionsVector.Length > 0)
                        continue;
                }
                //extract validity of variable name to separate method and make check validity of substitutionReference  
                if (substitutionReference.Length > 32)
                {
                    unknownReferences.Add(substitutionReference);
                    continue;
                }

                if (!questions.ContainsKey(substitutionReference))
                    unknownReferences.Add(substitutionReference);
                else
                {
                    var currentQuestion = questions[substitutionReference];
                    bool typeOfRefQuestionIsNotSupported = !(currentQuestion.QuestionType == QuestionType.DateTime ||
                        currentQuestion.QuestionType == QuestionType.Numeric ||
                        currentQuestion.QuestionType == QuestionType.SingleOption ||
                        currentQuestion.QuestionType == QuestionType.Text ||
                        currentQuestion.QuestionType == QuestionType.AutoPropagate ||
                        currentQuestion.QuestionType == QuestionType.QRBarcode);

                    if (typeOfRefQuestionIsNotSupported)
                        questionsIncorrectTypeOfReferenced.Add(substitutionReference);

                    if (
                        !this.IsReferencedItemInTheSameScopeWithReferencesItem(
                            this.GetQuestionnaireItemDepthAsVector(currentQuestion.PublicKey), propagationQuestionsVector))
                        questionsIllegalPropagationScope.Add(substitutionReference);
                }
            }
        }

        private static Answer[] ConvertOptionsToAnswers(Option[] options)
        {
            if (options == null)
                return null;

            return options.Select(ConvertOptionToAnswer).ToArray();
        }

        private static Answer ConvertOptionToAnswer(Option option)
        {
            return new Answer
            {
                PublicKey = option.Id,
                AnswerValue = option.Value,
                AnswerText = option.Title,
                ParentValue = option.ParentValue
            };
        }

        private IEnumerable<Guid> GetRosterSizeQuestionsOfGroupAndUpperRosters(IGroup group)
        {
            var targerGroup = @group;
            while (targerGroup != null)
            {
                if (targerGroup.IsRoster && targerGroup.RosterSizeQuestionId.HasValue)
                    yield return targerGroup.RosterSizeQuestionId.Value;

                targerGroup = targerGroup.GetParent() as IGroup;
            }
        }

        private IEnumerable<IQuestion> GetAllCategoricalLinkedQuestions()
        {
            return this.innerDocument.Find<IQuestion>(question => question.LinkedToQuestionId.HasValue);
        }

        private IEnumerable<Guid> GetAllLinkedSourceQuestions()
        {
            return this.GetAllCategoricalLinkedQuestions().Select(question => question.LinkedToQuestionId.Value);
        }

        private IEnumerable<Guid> GetAllRosterSizeQuestionIds()
        {
            return
                this.innerDocument.Find<IGroup>(group => group.RosterSizeQuestionId.HasValue)
                    .Select(group => group.RosterSizeQuestionId.Value);
        }

        private bool IsUnderPropagatableGroup(IComposite item)
        {
            this.innerDocument.ConnectChildrenWithParent();

            return this.GetFirstRosterParentGroupOrNull(item) != null;
        }

        private IGroup GetFirstRosterParentGroupOrNull(IComposite item, IGroup stopGroup = null)
        {
            while (item != null)
            {
                var parentGroup = item as IGroup;
                if (stopGroup != null && parentGroup != null && parentGroup == stopGroup)
                {
                    return parentGroup.IsRoster ? parentGroup : (IGroup) null;
                }
                if (parentGroup != null && parentGroup.IsRoster)
                    return parentGroup;

                item = item.GetParent();
            }
            return null;
        }

        private Guid GetScopeOrRoster(IGroup entityAsGroup)
        {
            if (entityAsGroup.RosterSizeSource == RosterSizeSourceType.FixedTitles)
                return entityAsGroup.PublicKey;

            if (!entityAsGroup.RosterSizeQuestionId.HasValue)
                return entityAsGroup.PublicKey;

            var rosterSizeQuestion = innerDocument.Find<IQuestion>(entityAsGroup.RosterSizeQuestionId.Value);
            if (rosterSizeQuestion == null)
                return entityAsGroup.PublicKey;

            return rosterSizeQuestion.PublicKey;
        }

        private Guid[] GetQuestionnaireItemDepthAsVector(Guid? itemId)
        {
            if (!itemId.HasValue)
                return new Guid[0];

            var entity = innerDocument.Find<IComposite>(itemId.Value);
            if (entity == null)
                return new Guid[0];

            var scopeIds = new List<Guid>();

            var entityAsGroup = entity as IGroup;
            if (entityAsGroup != null && entityAsGroup.IsRoster)
            {
                scopeIds.Add(GetScopeOrRoster(entityAsGroup));
            }

            this.innerDocument.ConnectChildrenWithParent();
            var currentParent = (IGroup)entity.GetParent();
            while (currentParent != null)
            {
                if (currentParent.IsRoster)
                    scopeIds.Add(GetScopeOrRoster(currentParent));

                currentParent = (IGroup)currentParent.GetParent();
            }
            return scopeIds.ToArray();
        }

        private void ParseExpressionIdentifierToExistingQuestionIdIgnoringThisIdentifierOrThrow(string identifier,
            string expression)
        {
            IQuestion question = GetQuestionByStringIdOrVariableName(identifier);

            IGroup roster = this.GetRosterByrVariableName(identifier);

            if (question == null && roster == null)
                throw new QuestionnaireException(
                    DomainExceptionType.ExpressionContainsNotExistingQuestionOrRosterReference, 
                    string.Format(ExceptionMessages.QuestionOrRosterIdentifierIsMissing, identifier, expression));
        }

        private IQuestion GetQuestionByStringIdOrVariableName(string identifier)
        {
            Guid parsedId;
            return !Guid.TryParse(identifier, out parsedId) ? this.GetQuestionByStataCaption(identifier) : this.GetQuestion(parsedId);
        }

        public IQuestion GetQuestionByStataCaption(string stataCaption)
        {
            return this.innerDocument.FirstOrDefault<IQuestion>(q => q.StataExportCaption == stataCaption);
        }

        private IQuestion GetQuestion(Guid questionId)
        {
            return this.innerDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == questionId);
        }

        private static bool IsExpressionDefined(string expression)
        {
            return !string.IsNullOrWhiteSpace(expression);
        }

        private IEnumerable<IGroup> GetGroupsByRosterTitleId(Guid questionId, params Guid[] exceptGroups)
        {
            return this.innerDocument.Find<IGroup>(
                group => !exceptGroups.Contains(group.PublicKey) && (group.RosterTitleQuestionId == questionId));
        }

        private bool IsRosterOrInsideRoster(IGroup group)
        {
            while (group != null)
            {
                if (group.IsRoster)
                    return true;

                group = (IGroup)group.GetParent();
            }

            return false;
        }

        private bool ContainsRoster(IGroup group)
        {
            if (group != null)
            {
                foreach (var subGroup in group.Children.OfType<IGroup>())
                {
                    if (subGroup.IsRoster || ContainsRoster(subGroup))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool AreElementsUnique(IEnumerable<string> elements)
        {
            return elements.Distinct().Count() == elements.Count();
        }

        private bool IsReferencedItemInTheSameScopeWithReferencesItem(Guid[] referencedItemRosterVector, Guid[] referencesItemRosterVector)
        {
            if (referencedItemRosterVector.Length > referencesItemRosterVector.Length)
                return false;

            return referencedItemRosterVector.All(referencesItemRosterVector.Contains);
        }

        private bool IsRosterTitleInRosterByRosterSize(IQuestion rosterTitleQuestion, Guid rosterSizeQuestionId, Guid currentRosterId)
        {
            var groupsByRosterSizeQuestion =
                this.GetGroupsByRosterSizeQuestion(rosterSizeQuestionId).Select(x => x.PublicKey).ToHashSet();
            groupsByRosterSizeQuestion.Add(currentRosterId);

            var parentForRosterTitleQuestion = rosterTitleQuestion.GetParent();
            while (parentForRosterTitleQuestion != null)
            {
                if (groupsByRosterSizeQuestion.Contains(parentForRosterTitleQuestion.PublicKey))
                    return true;
                var parentGroup = parentForRosterTitleQuestion as IGroup;
                if (parentGroup != null && parentGroup.IsRoster)
                    break;

                parentForRosterTitleQuestion = parentForRosterTitleQuestion.GetParent();
            }

            return false;
        }

        private IGroup GetRosterByrVariableName(string rosterName)
        {
            return this.innerDocument.Find<IGroup>(group => group.VariableName == rosterName && group.IsRoster).FirstOrDefault();
        }

        private IEnumerable<IGroup> GetGroupsByRosterSizeQuestion(Guid rosterSizeQuestionId)
        {
            return this.innerDocument.Find<IGroup>(
                group => group.RosterSizeQuestionId == rosterSizeQuestionId);
        }

        private IEnumerable<IQuestion> GetAllQuestionsInGroup(IGroup group)
        {
            var questionsInGroup = new List<IQuestion>();

            foreach (var groupItem in group.Children)
            {
                var itemAsGroup = groupItem as IGroup;
                if (itemAsGroup != null)
                {
                    questionsInGroup.AddRange(GetAllQuestionsInGroup(itemAsGroup));
                }

                var itemAsQuestion = groupItem as IQuestion;
                if (itemAsQuestion != null)
                {
                    questionsInGroup.Add(itemAsQuestion);
                }
            }
            return questionsInGroup;
        }

        private string[] GetFilteredQuestionForException(IGroup @group, Func<AbstractQuestion, bool> filter)
        {
            return this.innerDocument.GetEntitiesByType<AbstractQuestion>(@group)
                .Where(filter)
                .Select(question => string.Format("'{0}', [{1}]", question.QuestionText, question.StataExportCaption))
                .ToArray();
        }

        private string FormatQuestionForException(Guid questionId, QuestionnaireDocument document)
        {
            var question = document.Find<IQuestion>(questionId);

            return string.Format("'{0}', {1}", question.QuestionText, question.StataExportCaption);
        }

        private IEnumerable<IGroup> GetAllParentGroups(IComposite entity)
        {
            this.innerDocument.ConnectChildrenWithParent();

            var currentParent = (IGroup)entity.GetParent();

            while (currentParent != null)
            {
                yield return currentParent;

                currentParent = (IGroup)currentParent.GetParent();
            }
        }

        private bool IsQuestionParent(Guid groupId, IQuestion question)
        {
            return GetAllParentGroups(question).Any(x => x.PublicKey == groupId);
        }

        private IGroup GetGroupById(Guid groupId)
        {
            return this.innerDocument.Find<IGroup>(groupId);
        }

        private static string[] GetTitleList(IEnumerable<IComposite> groupsAndQuestions)
        {
            return groupsAndQuestions.Select(GetTitle).ToArray();
        }

        private static string GetTitle(IComposite composite)
        {
            var question = composite as IQuestion;
            var group = composite as IGroup;
            if (group != null)
            {
                return group.Title;
            }
            if (question != null)
            {
                return question.QuestionText;
            }
            return "<untitled>";
        }

        private static bool IsQuestionAndHaveQuestionIdInConditionOrValidation(IComposite composite, IQuestion sourceQuestion)
        {
            var question = composite as IQuestion;

            if (question != null)
            {
                string questionId = sourceQuestion.PublicKey.ToString();
                string alias = sourceQuestion.StataExportCaption;

                IEnumerable<string> conditionIds = new List<string>();
                if (IsExpressionDefined(question.ConditionExpression))
                {
                    conditionIds = ExpressionProcessor.GetIdentifiersUsedInExpression(question.ConditionExpression);
                }

                IEnumerable<string> validationIds = new List<string>();
                if (IsExpressionDefined(question.ValidationExpression))
                {
                    validationIds = ExpressionProcessor.GetIdentifiersUsedInExpression(question.ValidationExpression);
                }

                return validationIds.Contains(questionId) || validationIds.Contains(alias) ||
                    conditionIds.Contains(questionId) || conditionIds.Contains(alias);
            }
            return false;
        }

        private static bool IsGroupAndHaveQuestionIdInCondition(IComposite composite, IQuestion question)
        {
            var group = composite as IGroup;
            if (group != null && IsExpressionDefined(group.ConditionExpression))
            {
                string alias = question.StataExportCaption;

                IEnumerable<string> conditionIds = ExpressionProcessor.GetIdentifiersUsedInExpression(group.ConditionExpression).ToList();
               
                return conditionIds.Contains(alias);
            }
            return false;
        }

        private static string FormatGroupForException(Guid groupId, QuestionnaireDocument questionnaireDocument)
        {
            return string.Format("'{0}'", GetGroupTitleForException(groupId, questionnaireDocument));
        }

        private static string GetGroupTitleForException(Guid groupId, QuestionnaireDocument questionnaireDocument)
        {
            var @group = questionnaireDocument.Find<IGroup>(groupId);

            return @group != null
                ? @group.Title ?? "<<NO GROUP TITLE>>"
                : "<<MISSING GROUP>>";
        }

        private static void UpdateCustomMappingsWithContextQuestion(Dictionary<string, string> customMappings, IQuestion contextQuestion)
        {
            customMappings["this"] = contextQuestion.StataExportCaption;
        }

        private Dictionary<string, string> BuildCustomMappingsFromIdsToIdentifiers()
        {
            return this.innerDocument
                .Find<IQuestion>(question => !string.IsNullOrWhiteSpace(question.StataExportCaption))
                .SelectMany(question => new []
                {
                    new KeyValuePair<string, string>(question.PublicKey.ToString(), question.StataExportCaption),
                    new KeyValuePair<string, string>(question.PublicKey.FormatGuid(), question.StataExportCaption),
                })
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value);
        }

        private static bool HasEnablementCondition(IGroup group)
        {
            return !string.IsNullOrWhiteSpace(@group.ConditionExpression);
        }

        private static bool HasEnablementConditionOrValidationExpression(IQuestion question)
        {
            return !string.IsNullOrWhiteSpace(question.ConditionExpression)
                || !string.IsNullOrWhiteSpace(question.ValidationExpression);
        }


        #endregion

        #region Create clone events

        private IEnumerable<object> CreateTextQuestionClonedEvents(Guid questionId, string title, string variableName, string variableLabel, bool isMandatory, bool isPreFilled, QuestionScope scope, string enablementCondition, string validationExpression, string validationMessage, string instructions, string mask, Guid parentGroupId, Guid sourceQuestionId, int targetIndex, Guid responsibleId)
        {
            yield return new QuestionCloned
            {
                PublicKey = questionId,
                GroupPublicKey = parentGroupId,
                QuestionText = title,
                QuestionType = QuestionType.Text,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                Featured = isPreFilled,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                SourceQuestionId = sourceQuestionId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId,
                Mask = mask
            };
        }

        private IEnumerable<object> CreateGeoLocationQuestionClonedEvents(Guid questionId, string title, string variableName, string variableLabel, bool isMandatory, string enablementCondition, string instructions, Guid parentGroupId, Guid sourceQuestionId, int targetIndex, Guid responsibleId)
        {
            yield return new QuestionCloned
            {
                PublicKey = questionId,
                GroupPublicKey = parentGroupId,
                QuestionText = title,
                QuestionType = QuestionType.GpsCoordinates,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                QuestionScope = QuestionScope.Interviewer,
                ConditionExpression = enablementCondition,
                Instructions = instructions,
                SourceQuestionId = sourceQuestionId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId,
            };
        }

        private IEnumerable<object> CreateDateTimeQuestionClonedEvents(Guid questionId, string title, string variableName, string variableLabel, bool isMandatory, bool isPreFilled, QuestionScope scope, string enablementCondition, string validationExpression, string validationMessage, string instructions, Guid parentGroupId, Guid sourceQuestionId, int targetIndex, Guid responsibleId)
        {
            yield return new QuestionCloned
            {
                PublicKey = questionId,
                GroupPublicKey = parentGroupId,
                QuestionText = title,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                Featured = isPreFilled,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                SourceQuestionId = sourceQuestionId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId,
            };
        }

        private IEnumerable<object> CreateCategoricalMultiAnswersQuestionClonedEvents(Guid questionId, string title, string variableName, string variableLabel, bool isMandatory, QuestionScope scope, string enablementCondition, string validationExpression, string validationMessage, string instructions, Guid parentGroupId, Guid sourceQuestionId, int targetIndex, Guid responsibleId, Option[] options, Guid? linkedToQuestionId, bool areAnswersOrdered, int? maxAllowedAnswers)
        {
            yield return new QuestionCloned
            {
                PublicKey = questionId,
                GroupPublicKey = parentGroupId,
                QuestionText = title,
                QuestionType = QuestionType.MultyOption,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                Answers = ConvertOptionsToAnswers(options),
                SourceQuestionId = sourceQuestionId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId,
                LinkedToQuestionId = linkedToQuestionId,
                AreAnswersOrdered = areAnswersOrdered,
                MaxAllowedAnswers = maxAllowedAnswers
            };
        }

        private IEnumerable<object> CreateCategoricalSingleAnswerQuestionEvents(Guid questionId, string title, string variableName, string variableLabel, bool isMandatory, bool isPreFilled, QuestionScope scope, string enablementCondition, string validationExpression, string validationMessage, string instructions, Guid parentGroupId, Guid sourceQuestionId, int targetIndex, Guid responsibleId, Option[] options, Guid? linkedToQuestionId, bool? isFilteredCombobox, Guid? cascadeFromQuestionId)
        {
            yield return new QuestionCloned
            {
                PublicKey = questionId,
                GroupPublicKey = parentGroupId,
                QuestionText = title,
                QuestionType = QuestionType.SingleOption,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                Featured = isPreFilled,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                Answers = ConvertOptionsToAnswers(options),
                SourceQuestionId = sourceQuestionId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId,
                LinkedToQuestionId = linkedToQuestionId,
                IsFilteredCombobox = isFilteredCombobox,
                CascadeFromQuestionId = cascadeFromQuestionId
            };
        }

        private IEnumerable<object> CreateNumericQuestionCloneEvents(Guid questionId, Guid parentGroupId, string title, string variableName, string variableLabel, bool isMandatory, bool isPreFilled, QuestionScope scope, string enablementCondition, string validationExpression, string validationMessage, string instructions, Guid sourceQuestionId, int targetIndex, Guid responsibleId, int? maxValue, bool isInteger, int? countOfDecimalPlaces)
        {
            yield return new NumericQuestionCloned
            {
                PublicKey = questionId,
                GroupPublicKey = parentGroupId,
                QuestionText = title,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                Featured = isPreFilled,
                Capital = false,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Instructions = instructions,
                SourceQuestionId = sourceQuestionId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId,
                MaxAllowedValue = maxValue,
                IsInteger = isInteger,
                CountOfDecimalPlaces = countOfDecimalPlaces
            };
        }

        private IEnumerable<object> CreateTextListQuestionClonedEvents(Guid questionId, Guid parentGroupId, string title, string variableName, string variableLabel, bool isMandatory, string enablementCondition, string instructions, Guid sourceQuestionId, int targetIndex, Guid responsibleId, int? maxAnswerCount)
        {
            yield return new TextListQuestionCloned
            {
                PublicKey = questionId,
                GroupId = parentGroupId,
                QuestionText = title,
                StataExportCaption = variableName,
                VariableLabel = variableLabel,
                Mandatory = isMandatory,
                ConditionExpression = enablementCondition,
                Instructions = instructions,
                SourceQuestionId = sourceQuestionId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId,
                MaxAnswerCount = maxAnswerCount
            };
        }

        private IEnumerable<object> CreateQrBarcodeQuestionClonedEvents(Guid questionId, Guid parentGroupId, string title, string variableName, string variableLabel, bool isMandatory, string enablementCondition, string instructions, Guid sourceQuestionId, int targetIndex, Guid responsibleId)
        {
            yield return new QRBarcodeQuestionCloned
            {
                QuestionId = questionId,
                ParentGroupId = parentGroupId,
                Title = title,
                VariableName = variableName,
                VariableLabel = variableLabel,
                IsMandatory = isMandatory,
                EnablementCondition = enablementCondition,
                Instructions = instructions,
                SourceQuestionId = sourceQuestionId,
                TargetIndex = targetIndex,
                ResponsibleId = responsibleId
            };
        }

        private IEnumerable<object> CreateMultimediaQuestionClonedEvents(Guid questionId, Guid parentGroupId, string title, string variableName, string variableLabel,
            bool isMandatory, string enablementCondition, string instructions, Guid sourceQuestionId, int targetIndex,
            Guid responsibleId)
        {
            yield return
                new QuestionCloned
                {
                    PublicKey = questionId,
                    GroupPublicKey = parentGroupId,
                    QuestionText = title,
                    QuestionType = QuestionType.Multimedia,
                    StataExportCaption = variableName,
                    VariableLabel = variableLabel,
                    Mandatory = isMandatory,
                    Featured = false,
                    Capital = false,
                    QuestionScope = QuestionScope.Interviewer,
                    ConditionExpression = enablementCondition,
                    Instructions = instructions,
                    SourceQuestionId = sourceQuestionId,
                    TargetIndex = targetIndex,
                    ResponsibleId = responsibleId
                };
            yield return
                new MultimediaQuestionUpdated
                {
                    QuestionId = questionId,
                    Title = title,
                    VariableName = variableName,
                    VariableLabel = variableLabel,
                    IsMandatory = isMandatory,
                    EnablementCondition = enablementCondition,
                    Instructions = instructions,
                    ResponsibleId = responsibleId
                };
        }

        public IEnumerable<object> CreateCloneGroupWithoutChildrenEvents(Guid groupId, Guid responsibleId, string title, string variableName, Guid? rosterSizeQuestionId, string description, string condition, Guid? parentGroupId, Guid sourceGroupId, int targetIndex, bool isRoster, RosterSizeSourceType rosterSizeSource, string[] rosterFixedTitles, Guid? rosterTitleQuestionId)
        {
            yield return
                new GroupCloned
                {
                    PublicKey = groupId,
                    GroupText = title,
                    VariableName = null,
                    ParentGroupPublicKey = parentGroupId,
                    Description = description,
                    ConditionExpression = condition,
                    SourceGroupId = sourceGroupId,
                    TargetIndex = targetIndex,
                    ResponsibleId = responsibleId
                };

            if (isRoster)
            {
                yield return new GroupBecameARoster(responsibleId, groupId);
                yield return new RosterChanged(responsibleId, groupId, rosterSizeQuestionId, rosterSizeSource, rosterFixedTitles, rosterTitleQuestionId);
            }
            else
            {
                yield return new GroupStoppedBeingARoster(responsibleId, groupId);
            }
        }
        #endregion
    }
}