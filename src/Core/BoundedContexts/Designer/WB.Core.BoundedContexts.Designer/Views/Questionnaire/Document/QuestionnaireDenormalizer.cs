using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Translation;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document
{
    using Main.Core.Entities;

    internal class QuestionnaireDenormalizer :BaseDenormalizer,
        IEventHandler<NewQuestionnaireCreated>,
        IEventHandler<ExpressionsMigratedToCSharp>,

        IEventHandler<NewGroupAdded>,
        IEventHandler<GroupCloned>,
        IEventHandler<QuestionnaireItemMoved>,
        IEventHandler<QuestionDeleted>,
        IEventHandler<NewQuestionAdded>,
        IEventHandler<QuestionCloned>,
        IEventHandler<QuestionChanged>,
        IEventHandler<NumericQuestionCloned>,
        IEventHandler<NumericQuestionChanged>,
        IEventHandler<GroupDeleted>,
        IEventHandler<GroupUpdated>,
        IEventHandler<GroupBecameARoster>,
        IEventHandler<RosterChanged>,
        IEventHandler<GroupStoppedBeingARoster>,
        IEventHandler<QuestionnaireUpdated>,
        IEventHandler<QuestionnaireDeleted>,
        IEventHandler<TemplateImported>,
        IEventHandler<QuestionnaireCloned>,

        IEventHandler<TextListQuestionCloned>,
        IEventHandler<TextListQuestionChanged>,

        IEventHandler<QRBarcodeQuestionUpdated>,
        IEventHandler<QRBarcodeQuestionCloned>,

        IEventHandler<MultimediaQuestionUpdated>,

        IEventHandler<StaticTextAdded>,
        IEventHandler<StaticTextUpdated>,
        IEventHandler<StaticTextCloned>,
        IEventHandler<StaticTextDeleted>,

        IEventHandler<VariableAdded>,
        IEventHandler<VariableUpdated>,
        IEventHandler<VariableCloned>,
        IEventHandler<VariableDeleted>,

        IEventHandler<MacroAdded>,
        IEventHandler<MacroUpdated>,
        IEventHandler<MacroDeleted>,

        IEventHandler<LookupTableAdded>,
        IEventHandler<LookupTableUpdated>,
        IEventHandler<LookupTableDeleted>,
        
        IEventHandler<AttachmentUpdated>,
        IEventHandler<AttachmentDeleted>,

        IEventHandler<TranslationUpdated>,
        IEventHandler<TranslationDeleted>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage;
        private readonly IQuestionnaireEntityFactory questionnaireEntityFactory;
        private readonly ILogger logger;

        public QuestionnaireDenormalizer(
            IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage,
            IQuestionnaireEntityFactory questionnaireEntityFactory, 
            ILogger logger)
        {
            this.documentStorage = documentStorage;
            this.questionnaireEntityFactory = questionnaireEntityFactory;
            this.logger = logger;
        }

        public override object[] Writers
        {
            get { return new object[] { documentStorage}; }
        }

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            var item = new QuestionnaireDocument();

            item.Title = System.Web.HttpUtility.HtmlDecode(evnt.Payload.Title);
            item.IsPublic = evnt.Payload.IsPublic;
            item.PublicKey = evnt.Payload.PublicKey;
            item.CreationDate = evnt.Payload.CreationDate;
            item.CreatedBy = evnt.Payload.CreatedBy;

            this.documentStorage.Store(item, item.PublicKey);
        }

        public void Handle(IPublishedEvent<ExpressionsMigratedToCSharp> evnt)
        {
            QuestionnaireDocument questionnaire = this.documentStorage.GetById(evnt.EventSourceId);

            questionnaire.UsesCSharp = true;

            this.UpdateQuestionnaire(evnt, questionnaire);
        }

        public void Handle(IPublishedEvent<NewGroupAdded> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            if (item == null)
            {
                return;
            }
            
            var group = new Group();
            group.Title = System.Web.HttpUtility.HtmlDecode(evnt.Payload.GroupText);
            group.VariableName = evnt.Payload.VariableName;
            group.PublicKey = evnt.Payload.PublicKey;
            group.ConditionExpression = evnt.Payload.ConditionExpression;
            group.Description = evnt.Payload.Description;

            Guid? parentGroupPublicKey = evnt.Payload.ParentGroupPublicKey;
            if (parentGroupPublicKey.HasValue)
            {
                var parentGroup = item.Find<Group>(parentGroupPublicKey.Value);
                if (parentGroup != null)
                {
                    group.SetParent(parentGroup);
                }
                else
                {
                    string errorMessage = string.Format("Event {0} attempted to add group {1} into group {2}. But group {2} doesnt exist in document {3}",
                        evnt.EventIdentifier, 
                        evnt.Payload.PublicKey, 
                        evnt.Payload.ParentGroupPublicKey, 
                        item.PublicKey);

                    logger.Error(errorMessage);
                }
            }


            item.Add(group, parentGroupPublicKey, null);
            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<GroupCloned> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            var group = new Group();
            group.Title = System.Web.HttpUtility.HtmlDecode(evnt.Payload.GroupText);
            group.VariableName = evnt.Payload.VariableName;
            group.PublicKey = evnt.Payload.PublicKey;
            group.ConditionExpression = evnt.Payload.ConditionExpression;
            group.Description = evnt.Payload.Description;

            #warning Slava: uncomment this line and get rig of read and write side objects
            item.Insert(evnt.Payload.TargetIndex, group, evnt.Payload.ParentGroupPublicKey);
            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            QuestionnaireDocument questionnaire = this.documentStorage.GetById(evnt.EventSourceId);

            questionnaire.MoveItem(evnt.Payload.PublicKey, evnt.Payload.GroupKey, evnt.Payload.TargetIndex);

            this.UpdateQuestionnaire(evnt, questionnaire);
        }

        public void Handle(IPublishedEvent<QuestionDeleted> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            item.RemoveEntity(evnt.Payload.QuestionId);

            item.RemoveHeadPropertiesFromRosters(evnt.Payload.QuestionId);

            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<QuestionCloned> evnt)
        {
            QuestionCloned e = evnt.Payload;
            CloneQuestion(evnt, e.GroupPublicKey.Value, e.TargetIndex,EventConverter.QuestionClonedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<NewQuestionAdded> evnt)
        {
            AddQuestion(evnt, evnt.Payload.GroupPublicKey.Value, EventConverter.NewQuestionAddedToQuestionData(evnt));
        }

        //// move it out of there
        public void Handle(IPublishedEvent<QuestionChanged> evnt)
        {
            UpdateQuestion(evnt, EventConverter.QuestionChangedToQuestionData(evnt));
        }

        protected void AddQuestion(IPublishableEvent evnt, Guid groupId, QuestionData data)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            if (item == null)
            {
                return;
            }
            IQuestion result = questionnaireEntityFactory.CreateQuestion(data);
            IGroup group = item.Find<IGroup>(groupId);

            if (result == null || group == null)
            {
                return;
            }

            item.Add(result, groupId, null);

            if (result.Capital)
                item.MoveHeadQuestionPropertiesToRoster(result.PublicKey, groupId);

            this.UpdateQuestionnaire(evnt, item);
        }

        protected void UpdateQuestion(IPublishableEvent evnt, QuestionData data)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);

            if (document == null)
            {
                return;
            }

            var question = document.Find<AbstractQuestion>(data.PublicKey);

            if (question == null)
            {
                return;
            }

            IQuestion newQuestion = this.questionnaireEntityFactory.CreateQuestion(data);

            document.ReplaceEntity(question, newQuestion);

            if (newQuestion.Capital)
                document.MoveHeadQuestionPropertiesToRoster(question.PublicKey, null);

            this.UpdateQuestionnaire(evnt, document);
        }

        protected void CloneQuestion(IPublishableEvent evnt, Guid groupId,int index, QuestionData data)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            IQuestion result = questionnaireEntityFactory.CreateQuestion(data);
            IGroup group = document.Find<IGroup>(groupId);

            if (result == null || group == null)
            {
                return;
            }

            document.Insert(index, result, groupId);

            if (result.Capital)
                document.MoveHeadQuestionPropertiesToRoster(result.PublicKey, groupId);

            this.UpdateQuestionnaire(evnt, document);
        }

        public void Handle(IPublishedEvent<NumericQuestionCloned> evnt)
        {
            NumericQuestionCloned e = evnt.Payload;
            CloneQuestion(evnt, e.GroupPublicKey, e.TargetIndex, EventConverter.NumericQuestionClonedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<NumericQuestionChanged> evnt)
        {
            UpdateQuestion(evnt, EventConverter.NumericQuestionChangedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<TextListQuestionCloned> evnt)
        {
            TextListQuestionCloned e = evnt.Payload;
            CloneQuestion(evnt, e.GroupId, e.TargetIndex, EventConverter.TextListQuestionClonedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<TextListQuestionChanged> evnt)
        {
            UpdateQuestion(evnt, EventConverter.TextListQuestionChangedToQuestionData(evnt));
        }
        
        public void Handle(IPublishedEvent<GroupDeleted> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            item.RemoveGroup(evnt.Payload.GroupPublicKey);

            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<GroupUpdated> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            item.UpdateGroup(
                evnt.Payload.GroupPublicKey,
                evnt.Payload.GroupText, evnt.Payload.VariableName,
                evnt.Payload.Description,
                evnt.Payload.ConditionExpression,
                evnt.Payload.HideIfDisabled);

            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<GroupBecameARoster> @event)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(@event.EventSourceId);

            item.UpdateGroup(@event.Payload.GroupId,  group => group.IsRoster = true);

            this.UpdateQuestionnaire(@event, item);
        }

        public void Handle(IPublishedEvent<RosterChanged> @event)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(@event.EventSourceId);

            item.UpdateGroup(@event.Payload.GroupId, group =>
            {
                group.RosterSizeQuestionId = @event.Payload.RosterSizeQuestionId;
                group.RosterSizeSource = @event.Payload.RosterSizeSource;
                group.FixedRosterTitles = @event.Payload.FixedRosterTitles;
                group.RosterTitleQuestionId = @event.Payload.RosterTitleQuestionId;
            });

            this.UpdateQuestionnaire(@event, item);
        }

        public void Handle(IPublishedEvent<GroupStoppedBeingARoster> @event)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(@event.EventSourceId);
            if (document == null)
            {
                return;
            }
            document.UpdateGroup(@event.Payload.GroupId, group =>
            {
                group.IsRoster = false;
                group.RosterSizeSource = RosterSizeSourceType.Question;
                group.RosterSizeQuestionId = null;
                group.RosterTitleQuestionId = null;
                group.FixedRosterTitles = new FixedRosterTitle[0];
            });

            this.UpdateQuestionnaire(@event, document);
        }

        public void Handle(IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            if (document == null) return;
            document.Title = System.Web.HttpUtility.HtmlDecode(evnt.Payload.Title);
            document.IsPublic = evnt.Payload.IsPublic;
            this.UpdateQuestionnaire(evnt, document);
        }

        private void UpdateQuestionnaire(IPublishableEvent evnt, QuestionnaireDocument document)
        {
            document.LastEntryDate = evnt.EventTimeStamp;
            document.LastEventSequence = evnt.EventSequence;
            this.documentStorage.Store(document, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            if (document == null) return;
            document.IsDeleted = true;
            this.documentStorage.Store(document, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var upgradedDocument = evnt.Payload.Source;

            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            if (document != null)
                upgradedDocument.ReplaceSharedPersons(document.SharedPersons);

            this.AddNewQuestionnaire(upgradedDocument);
        }

        public void Handle(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            this.AddNewQuestionnaire(evnt.Payload.QuestionnaireDocument);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionUpdated> evnt)
        {
            UpdateQuestion(evnt, EventConverter.QRBarcodeQuestionUpdatedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            QRBarcodeQuestionCloned e = evnt.Payload;
            CloneQuestion(evnt, e.ParentGroupId, e.TargetIndex,EventConverter.QRBarcodeQuestionClonedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<MultimediaQuestionUpdated> evnt)
        {
            UpdateQuestion(evnt, EventConverter.MultimediaQuestionUpdatedToQuestionData(evnt));
        }

        public void Handle(IPublishedEvent<StaticTextAdded> evnt)
        {
            QuestionnaireDocument questionnaireDocument = this.documentStorage.GetById(evnt.EventSourceId);

            var parentGroup = questionnaireDocument?.Find<IGroup>(evnt.Payload.ParentId);
            if (parentGroup == null)
            {
                return;
            }

            var staticText = questionnaireEntityFactory.CreateStaticText(entityId: evnt.Payload.EntityId, text: evnt.Payload.Text, 
                attachmentName: null, hideIfDisabled: evnt.Payload.HideIfDisabled, enablementCondition: evnt.Payload.EnablementCondition, 
                validationConditions: evnt.Payload.ValidationConditions);

            questionnaireDocument.Add(staticText, evnt.Payload.ParentId, null);

            this.UpdateQuestionnaire(evnt, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<StaticTextUpdated> evnt)
        {
            QuestionnaireDocument questionnaireDocument = this.documentStorage.GetById(evnt.EventSourceId);

            var oldStaticText = questionnaireDocument?.Find<IStaticText>(evnt.Payload.EntityId);
            if (oldStaticText == null)
            {
                return;
            }

            var newStaticText = this.questionnaireEntityFactory.CreateStaticText(entityId: evnt.Payload.EntityId, text: evnt.Payload.Text, 
                attachmentName: evnt.Payload.AttachmentName, enablementCondition: evnt.Payload.EnablementCondition, hideIfDisabled: evnt.Payload.HideIfDisabled, 
                validationConditions: evnt.Payload.ValidationConditions);

            questionnaireDocument.ReplaceEntity(oldStaticText, newStaticText);

            this.UpdateQuestionnaire(evnt, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<StaticTextCloned> evnt)
        {
            QuestionnaireDocument questionnaireDocument = this.documentStorage.GetById(evnt.EventSourceId);

            var parentGroup = questionnaireDocument?.Find<IGroup>(evnt.Payload.ParentId);
            if (parentGroup == null)
            {
                return;
            }

            var staticText = questionnaireEntityFactory.CreateStaticText(entityId: evnt.Payload.EntityId, text: evnt.Payload.Text, attachmentName: 
                evnt.Payload.AttachmentName, enablementCondition: evnt.Payload.EnablementCondition, hideIfDisabled: evnt.Payload.HideIfDisabled, 
                validationConditions: evnt.Payload.ValidationConditions);

            questionnaireDocument.Insert(index: evnt.Payload.TargetIndex, c: staticText, parent: evnt.Payload.ParentId);

            this.UpdateQuestionnaire(evnt, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<StaticTextDeleted> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            item.RemoveEntity(evnt.Payload.EntityId);
            
            this.UpdateQuestionnaire(evnt, item);
        }

        public void Handle(IPublishedEvent<VariableAdded> evnt)
        {
            QuestionnaireDocument questionnaireDocument = this.documentStorage.GetById(evnt.EventSourceId);

            var parentGroup = questionnaireDocument?.Find<IGroup>(evnt.Payload.ParentId);
            if (parentGroup == null)
            {
                return;
            }

            var variable = questionnaireEntityFactory.CreateVariable(evnt.Payload);
            questionnaireDocument.Add(variable, evnt.Payload.ParentId, null);
            this.UpdateQuestionnaire(evnt, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<VariableUpdated> evnt)
        {
            QuestionnaireDocument questionnaireDocument = this.documentStorage.GetById(evnt.EventSourceId);

            var oldVariable = questionnaireDocument?.Find<IVariable>(evnt.Payload.EntityId);
            if (oldVariable == null)
            {
                return;
            }

            var newVariable = this.questionnaireEntityFactory.CreateVariable(evnt.Payload);
            questionnaireDocument.ReplaceEntity(oldVariable, newVariable);
            this.UpdateQuestionnaire(evnt, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<VariableCloned> evnt)
        {
            QuestionnaireDocument questionnaireDocument = this.documentStorage.GetById(evnt.EventSourceId);

            var parentGroup = questionnaireDocument?.Find<IGroup>(evnt.Payload.ParentId);
            if (parentGroup == null)
            {
                return;
            }

            var variable = questionnaireEntityFactory.CreateVariable(evnt.Payload);

            questionnaireDocument.Insert(index: evnt.Payload.TargetIndex, c: variable, parent: evnt.Payload.ParentId);

            this.UpdateQuestionnaire(evnt, questionnaireDocument);
        }

        public void Handle(IPublishedEvent<VariableDeleted> evnt)
        {
            QuestionnaireDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            item.RemoveEntity(evnt.Payload.EntityId);

            this.UpdateQuestionnaire(evnt, item);
        }

        private void AddNewQuestionnaire(QuestionnaireDocument questionnaireDocument)
        {
            this.documentStorage.Store(questionnaireDocument, questionnaireDocument.PublicKey);
        }

        public void Handle(IPublishedEvent<MacroAdded> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);

            document.Macros[evnt.Payload.MacroId] = new Macro();

            this.UpdateQuestionnaire(evnt, document);
        }

        public void Handle(IPublishedEvent<MacroUpdated> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            var macroId = evnt.Payload.MacroId;
            if (!document.Macros.ContainsKey(macroId))
                return;

            document.Macros[macroId].Name = evnt.Payload.Name;
            document.Macros[macroId].Content = evnt.Payload.Content;
            document.Macros[macroId].Description = evnt.Payload.Description;

            this.UpdateQuestionnaire(evnt, document);
        }

        public void Handle(IPublishedEvent<MacroDeleted> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            document.Macros.Remove(evnt.Payload.MacroId);
            this.UpdateQuestionnaire(evnt, document);
        }

        public void Handle(IPublishedEvent<LookupTableAdded> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);

            document.LookupTables[evnt.Payload.LookupTableId] = new LookupTable()
            {
                TableName = evnt.Payload.LookupTableName,
                FileName = evnt.Payload.LookupTableFileName
            };

            this.UpdateQuestionnaire(evnt, document);
        }

        public void Handle(IPublishedEvent<LookupTableUpdated> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);

            document.LookupTables[evnt.Payload.LookupTableId] = new LookupTable
            {
                TableName = evnt.Payload.LookupTableName,
                FileName = evnt.Payload.LookupTableFileName
            };

            this.UpdateQuestionnaire(evnt, document);
        }

        public void Handle(IPublishedEvent<LookupTableDeleted> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);

            document.LookupTables.Remove(evnt.Payload.LookupTableId);

            this.UpdateQuestionnaire(evnt, document);
        }

        public void Handle(IPublishedEvent<AttachmentUpdated> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            AddOrUpdateAttachment(document, evnt.Payload.AttachmentId, new Attachment
            {
                AttachmentId = evnt.Payload.AttachmentId,
                Name = evnt.Payload.AttachmentName,
                ContentId = evnt.Payload.AttachmentContentId
            });
            this.UpdateQuestionnaire(evnt, document);
        }

        public void Handle(IPublishedEvent<AttachmentDeleted> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            document.Attachments.RemoveAll(x => x.AttachmentId == evnt.Payload.AttachmentId);
            this.UpdateQuestionnaire(evnt, document);
        }

        public void Handle(IPublishedEvent<TranslationUpdated> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            var translation = new Translation
            {
                Id = evnt.Payload.TranslationId,
                Name = evnt.Payload.Name,
            };
            document.Translations.RemoveAll(x => x.Id == evnt.Payload.TranslationId);
            document.Translations.Add(translation);

            this.UpdateQuestionnaire(evnt, document);
        }

        public void Handle(IPublishedEvent<TranslationDeleted> evnt)
        {
            QuestionnaireDocument document = this.documentStorage.GetById(evnt.EventSourceId);
            document.Translations.RemoveAll(x => x.Id == evnt.Payload.TranslationId);
            this.UpdateQuestionnaire(evnt, document);
        }

        private void AddOrUpdateAttachment(QuestionnaireDocument document, Guid attachmentId, Attachment attachment)
        {
            document.Attachments.RemoveAll(x => x.AttachmentId == attachmentId);
            document.Attachments.Add(attachment);
        }
    }
}