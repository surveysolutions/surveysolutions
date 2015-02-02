using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    internal class PdfQuestionnaireDenormalizer :BaseDenormalizer,
        IEventHandler<GroupCloned>,
        IEventHandler<GroupDeleted>,
        IEventHandler<GroupUpdated>,
        IEventHandler<NewGroupAdded>,
        IEventHandler<NewQuestionAdded>,
        IEventHandler<NewQuestionnaireCreated>,
        IEventHandler<QuestionChanged>,
        IEventHandler<QuestionCloned>,
        IEventHandler<QuestionDeleted>,
        IEventHandler<QuestionnaireItemMoved>,
        IEventHandler<QuestionnaireUpdated>,
        IEventHandler<TemplateImported>,
        IEventHandler<QuestionnaireCloned>,
        IEventHandler<NumericQuestionAdded>,
        IEventHandler<NumericQuestionCloned>,
        IEventHandler<NumericQuestionChanged>,
        IEventHandler<QRBarcodeQuestionAdded>,
        IEventHandler<QRBarcodeQuestionCloned>,
        IEventHandler<QRBarcodeQuestionUpdated>,
        IEventHandler<MultimediaQuestionUpdated>,
        IEventHandler<TextListQuestionAdded>,
        IEventHandler<TextListQuestionCloned>,
        IEventHandler<TextListQuestionChanged>,
        IEventHandler<StaticTextAdded>,
        IEventHandler<StaticTextUpdated>,
        IEventHandler<StaticTextCloned>,
        IEventHandler<StaticTextDeleted>
    {
        private readonly IReadSideKeyValueStorage<PdfQuestionnaireView> repositoryWriter;
        private readonly IReadSideRepositoryWriter<AccountDocument> accounts;
        private readonly ILogger logger;

        public PdfQuestionnaireDenormalizer(IReadSideKeyValueStorage<PdfQuestionnaireView> repositoryWriter,
            ILogger logger,
            IReadSideRepositoryWriter<AccountDocument> accounts)
        {
            this.repositoryWriter = repositoryWriter;
            this.logger = logger;
            this.accounts = accounts;
        }

        public override object[] Writers
        {
            get { return new object[] { repositoryWriter }; }
        }

        public override object[] Readers
        {
            get { return new object[] {accounts }; }
        }

        private void HandleUpdateEvent<TEvent>(IPublishedEvent<TEvent> evnt, Func<TEvent, PdfQuestionnaireView, PdfQuestionnaireView> handle)
        {
            try
            {
                Guid questionnaireId = evnt.EventSourceId;
                PdfQuestionnaireView initialQuestionnaire = this.repositoryWriter.GetById(questionnaireId);
                if (initialQuestionnaire != null)
                {
                    initialQuestionnaire.ReconnectWithParent();
                }

                PdfQuestionnaireView updatedQuestionnaire = handle(evnt.Payload, initialQuestionnaire);
                this.repositoryWriter.Store(updatedQuestionnaire, questionnaireId);
            }
            catch (ApplicationException e)
            {
                var errorMessage = string.Format("Error while apply event {0} with on questionnaire {1}", evnt.EventIdentifier, evnt.EventSourceId);
                logger.Error(errorMessage, e);
            }
        }

        public void Handle(IPublishedEvent<GroupCloned> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var newGroup = new PdfGroupView
                {
                    Title = @event.GroupText,
                    PublicId = @event.PublicKey,
                    Depth = questionnaire.GetEntityDepth(@event.ParentGroupPublicKey) + 1,
                    VariableName = @event.VariableName
                };

                questionnaire.AddGroup(newGroup, @event.ParentGroupPublicKey);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<GroupDeleted> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                questionnaire.RemoveGroup(@event.GroupPublicKey);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<GroupUpdated> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                PdfGroupView group = questionnaire.GetGroup(@event.GroupPublicKey);
                group.Title = @event.GroupText;
                group.VariableName = @event.VariableName;

                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<NewGroupAdded> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                if (questionnaire == null)
                {
                    var errorMessage = string.Format("There was an attempt to add a group to a questionnaire that was not created yet. GroupId {0}", @event.PublicKey);
                    throw new InvalidOperationException(errorMessage);
                }

                var newGroup = new PdfGroupView
                    {
                        Title = @event.GroupText,
                        PublicId = @event.PublicKey,
                        VariableName = @event.VariableName,
                        Depth = questionnaire.GetEntityDepth(@event.ParentGroupPublicKey) + 1
                    };

                questionnaire.AddGroup(newGroup, @event.ParentGroupPublicKey);

                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<NewQuestionAdded> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var newQuestion = new PdfQuestionView
                    {
                        PublicId = @event.PublicKey,
                        Title = @event.QuestionText,
                        QuestionType = @event.QuestionType,
                        Answers = (@event.Answers ?? Enumerable.Empty<Answer>())
                                    .Select(x => new PdfAnswerView
                                        {
                                            Title = x.AnswerText,
                                            AnswerValue = x.AnswerValue,
                                            ParentValue = x.ParentValue
                                        }).ToList(),
                        VariableName = @event.StataExportCaption
                    };

                newQuestion.ValidationExpression = @event.ValidationExpression;
                newQuestion.ConditionExpression = @event.ConditionExpression;
                questionnaire.AddEntity(newQuestion, @event.GroupPublicKey);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<QuestionChanged> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var existingQuestion = questionnaire.GetEntityById<PdfQuestionView>(@event.PublicKey);
                if (existingQuestion == null)
                {
                    return questionnaire;
                }
                existingQuestion.ConditionExpression = @event.ConditionExpression;
                existingQuestion.ValidationExpression = @event.ValidationExpression;

                existingQuestion.Title = @event.QuestionText;
                existingQuestion.QuestionType = @event.QuestionType;
                existingQuestion.VariableName = @event.StataExportCaption;

                existingQuestion.Answers = (@event.Answers ?? Enumerable.Empty<Answer>()).Select(x => new PdfAnswerView
                    {
                        Title = x.AnswerText,
                        AnswerValue = x.AnswerValue,
                        ParentValue = x.ParentValue
                    }).ToList();

                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<QuestionCloned> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var newQuestion = new PdfQuestionView
                {
                    PublicId = @event.PublicKey,
                    Title = @event.QuestionText,
                    QuestionType = @event.QuestionType,
                    Answers = (@event.Answers ?? Enumerable.Empty<Answer>()).Select(x => new PdfAnswerView
                    {
                        Title = x.AnswerText,
                        AnswerValue = x.AnswerValue,
                        ParentValue = x.ParentValue
                    }).ToList(),
                    VariableName = @event.StataExportCaption
                };

                newQuestion.ValidationExpression = @event.ValidationExpression;
                newQuestion.ConditionExpression = @event.ConditionExpression;
                questionnaire.AddEntity(newQuestion, @event.GroupPublicKey);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<NumericQuestionAdded> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {

                var newQuestion = new PdfQuestionView
                {
                    PublicId = @event.PublicKey,
                    Title = @event.QuestionText,
                    QuestionType = QuestionType.Numeric,
                    Answers = new List<PdfAnswerView>(),
                    VariableName = @event.StataExportCaption
                };

                newQuestion.ValidationExpression = @event.ValidationExpression;
                newQuestion.ConditionExpression = @event.ConditionExpression;
                questionnaire.AddEntity(newQuestion, @event.GroupPublicKey);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<NumericQuestionCloned> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var newQuestion = new PdfQuestionView
                {
                    PublicId = @event.PublicKey,
                    Title = @event.QuestionText,
                    QuestionType = QuestionType.Numeric,
                    Answers = new List<PdfAnswerView>(0),
                    VariableName = @event.StataExportCaption
                };

                newQuestion.ValidationExpression = @event.ValidationExpression;
                newQuestion.ConditionExpression = @event.ConditionExpression;
                questionnaire.AddEntity(newQuestion, @event.GroupPublicKey);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<NumericQuestionChanged> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var existingQuestion = questionnaire.GetEntityById<PdfQuestionView>(@event.PublicKey);
                if (existingQuestion == null)
                {
                    return questionnaire;
                }
                existingQuestion.ConditionExpression = @event.ConditionExpression;
                existingQuestion.ValidationExpression = @event.ValidationExpression;
                existingQuestion.VariableName = @event.StataExportCaption;

                existingQuestion.Title = @event.QuestionText;
                existingQuestion.QuestionType = QuestionType.Numeric;
                existingQuestion.Answers = new List<PdfAnswerView>(0);

                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<TextListQuestionAdded> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var newQuestion = new PdfQuestionView
                {
                    PublicId = @event.PublicKey,
                    Title = @event.QuestionText,
                    QuestionType = QuestionType.TextList,
                    Answers = new List<PdfAnswerView>(),
                    VariableName = @event.StataExportCaption
                };

                newQuestion.ConditionExpression = @event.ConditionExpression;
                questionnaire.AddEntity(newQuestion, @event.GroupId);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<TextListQuestionCloned> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var newQuestion = new PdfQuestionView
                {
                    PublicId = @event.PublicKey,
                    Title = @event.QuestionText,
                    QuestionType = QuestionType.TextList,
                    Answers = new List<PdfAnswerView>(0),
                    VariableName = @event.StataExportCaption
                };

                newQuestion.ConditionExpression = @event.ConditionExpression;
                questionnaire.AddEntity(newQuestion, @event.GroupId);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<TextListQuestionChanged> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var existingQuestion = questionnaire.GetEntityById<PdfQuestionView>(@event.PublicKey);
                if (existingQuestion == null)
                {
                    return questionnaire;
                }
                existingQuestion.ConditionExpression = @event.ConditionExpression;
                
                existingQuestion.Title = @event.QuestionText;
                existingQuestion.QuestionType = QuestionType.TextList;
                existingQuestion.Answers = new List<PdfAnswerView>(0);
                existingQuestion.VariableName = @event.StataExportCaption;

                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<QuestionDeleted> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                questionnaire.RemoveEntity(@event.QuestionId);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                questionnaire.Title = @event.Title;
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                AccountDocument accountView = @event.CreatedBy.HasValue ? accounts.GetById(@event.CreatedBy.Value) : null;
                var createdBy = accountView != null ? accountView.UserName : "n/a";
                var newQuestionnaire = new PdfQuestionnaireView
                {
                    Title = @event.Title,
                    CreationDate = @event.CreationDate,
                    CreatedBy = createdBy,
                    PublicId = @event.PublicKey
                };

                return newQuestionnaire;
            });
        }

        public void Handle(IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var itemToMove = questionnaire.Children.TreeToEnumerable().FirstOrDefault(x => x.PublicId == @event.PublicKey);
                var appendTo = questionnaire.Children.TreeToEnumerable().FirstOrDefault(x => x.PublicId == @event.GroupKey) ?? questionnaire;

                itemToMove.GetParent().Children.Remove(itemToMove);

                AppendItemTo(questionnaire: questionnaire, appendTo: appendTo, itemToMove: itemToMove,
                    targetIndex: @event.TargetIndex);

                return questionnaire;
            });
        }

        private static void AppendItemTo(PdfQuestionnaireView questionnaire, PdfEntityView appendTo, PdfEntityView itemToMove, int targetIndex)
        {
            if (appendTo == null) throw new ArgumentNullException("appendTo");
            if (itemToMove == null) throw new ArgumentNullException("itemToMove");

            itemToMove.Depth = appendTo.Depth + 1;
            if (targetIndex < 0)
            {
                appendTo.InsertChild(itemToMove, 0);
            }
            else if (targetIndex >= appendTo.Children.Count)
            {
                appendTo.AddChild(itemToMove);
            }
            else
            {
                appendTo.InsertChild(itemToMove, targetIndex);
            }

        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.HandleUpdateEvent(evnt,
                handle:
                    (@event, questionnaire) =>
                        this.CreatePdfQuestionnaireViewFromQuestionnaireDocument((@event.Source)));
        }

        public void Handle(IPublishedEvent<QuestionnaireCloned> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) => this.CreatePdfQuestionnaireViewFromQuestionnaireDocument(evnt.Payload.QuestionnaireDocument));
        }

        private PdfQuestionnaireView CreatePdfQuestionnaireViewFromQuestionnaireDocument(QuestionnaireDocument questionnaireDocument)
        {
            AccountDocument accountView = questionnaireDocument.CreatedBy.HasValue ? accounts.GetById(questionnaireDocument.CreatedBy.Value) : null;
            var pdf = new PdfQuestionnaireView
            {
                Title = questionnaireDocument.Title,
                CreationDate = questionnaireDocument.CreationDate,
                CreatedBy = accountView != null ? accountView.UserName : "n/a",
                PublicId = questionnaireDocument.PublicKey
            };

            pdf.FillFrom(questionnaireDocument);
            return pdf;
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAdded> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {

                var newQuestion = new PdfQuestionView
                {
                    PublicId = @event.QuestionId,
                    Title = @event.Title,
                    QuestionType = QuestionType.QRBarcode,
                    Answers = new List<PdfAnswerView>(),
                    VariableName = @event.VariableName,
                    ConditionExpression = @event.EnablementCondition
                };

                questionnaire.AddEntity(newQuestion, @event.ParentGroupId);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionCloned> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var newQuestion = new PdfQuestionView
                {
                    PublicId = @event.QuestionId,
                    Title = @event.Title,
                    QuestionType = QuestionType.QRBarcode,
                    Answers = new List<PdfAnswerView>(0),
                    VariableName = @event.VariableName,
                    ConditionExpression = @event.EnablementCondition
                };

                questionnaire.AddEntity(newQuestion, @event.ParentGroupId);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionUpdated> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var existingQuestion = questionnaire.GetEntityById<PdfQuestionView>(@event.QuestionId);
                if (existingQuestion == null)
                {
                    return questionnaire;
                }

                existingQuestion.VariableName = @event.VariableName;
                existingQuestion.ConditionExpression = @event.EnablementCondition;
                existingQuestion.Title = @event.Title;
                existingQuestion.QuestionType = QuestionType.QRBarcode;
                existingQuestion.Answers = new List<PdfAnswerView>(0);

                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<MultimediaQuestionUpdated> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var existingQuestion = questionnaire.GetEntityById<PdfQuestionView>(@event.QuestionId);
                if (existingQuestion == null)
                {
                    return questionnaire;
                }

                existingQuestion.VariableName = @event.VariableName;
                existingQuestion.ConditionExpression = @event.EnablementCondition;
                existingQuestion.Title = @event.Title;
                existingQuestion.QuestionType = QuestionType.Multimedia;
                existingQuestion.Answers = new List<PdfAnswerView>(0);

                return questionnaire;
            });
        }

        #region Static text handlers
        public void Handle(IPublishedEvent<StaticTextAdded> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var pdfStaticTextView = new PdfStaticTextView()
                {
                    PublicId = @event.EntityId,
                    Title = @event.Text
                };

                questionnaire.AddEntity(pdfStaticTextView, @event.ParentId);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<StaticTextUpdated> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var existingStaticTextView = questionnaire.GetEntityById<PdfStaticTextView>(@event.EntityId);
                if (existingStaticTextView == null)
                {
                    return questionnaire;
                }

                existingStaticTextView.Title = @event.Text;

                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<StaticTextCloned> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var parentEntity =
                    questionnaire.Children.TreeToEnumerable().FirstOrDefault(x => x.PublicId == @event.ParentId);

                if (parentEntity != null)
                {
                    var pdfStaticTextView = new PdfStaticTextView()
                    {
                        PublicId = @event.EntityId,
                        Title = @event.Text
                    };
                    AppendItemTo(questionnaire: questionnaire, appendTo: parentEntity, itemToMove: pdfStaticTextView,
                        targetIndex: @event.TargetIndex);

                }

                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<StaticTextDeleted> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                questionnaire.RemoveEntity(@event.EntityId);
                return questionnaire;
            });
        }
        #endregion
    }
}