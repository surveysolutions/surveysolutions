using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Main.Core.View;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Providers.CQRS.Accounts;
using WB.UI.Designer.Providers.CQRS.Accounts.View;
using WB.UI.Designer.Views.Questionnaire.Pdf;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Views.EventHandler
{
    public class PdfQuestionnaireDenormalizer :
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
        IEventHandler<QuestionnaireCloned>, IEventHandler
    {
        private readonly IReadSideRepositoryWriter<PdfQuestionnaireView> repositoryWriter;
        private readonly IReadSideRepositoryWriter<AccountDocument> accounts;
        private readonly ILogger logger;

        public PdfQuestionnaireDenormalizer(IReadSideRepositoryWriter<PdfQuestionnaireView> repositoryWriter,
            ILogger logger,
            IReadSideRepositoryWriter<AccountDocument> accounts)
        {
            this.repositoryWriter = repositoryWriter;
            this.logger = logger;
            this.accounts = accounts;
        }

        private void HandleUpdateEvent<TEvent>(IPublishedEvent<TEvent> evnt, Func<TEvent, PdfQuestionnaireView, PdfQuestionnaireView> handle)
        {
            try
            {
                Guid questionnaireId = evnt.EventSourceId;
                PdfQuestionnaireView initialQuestionnaire = this.repositoryWriter.GetById(questionnaireId);
                if(initialQuestionnaire!=null)
                    initialQuestionnaire.ReconnectWithParent();
                PdfQuestionnaireView updatedQuestionnaire = handle(evnt.Payload, initialQuestionnaire);
                if (updatedQuestionnaire != null)
                {
                    this.repositoryWriter.Store(updatedQuestionnaire, questionnaireId);
                }
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
                    Depth = questionnaire.GetEntityDepth(@event.ParentGroupPublicKey) + 1
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
                PdfGroupView @group = questionnaire.GetGroup(@event.GroupPublicKey);
                @group.Title = @event.GroupText;

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
                                            AnswerType = x.AnswerType,
                                            AnswerValue = x.AnswerValue
                                        }).ToList(),
                        Variable = @event.StataExportCaption
                    };

                newQuestion.ValidationExpression = @event.ValidationExpression;
                newQuestion.ConditionExpression = @event.ConditionExpression;
                questionnaire.AddQuestion(newQuestion, @event.GroupPublicKey);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<QuestionChanged> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var existingQuestion = questionnaire.GetQuestion(@event.PublicKey);
                existingQuestion.ConditionExpression = @event.ConditionExpression;
                existingQuestion.ValidationExpression = @event.ValidationExpression;

                existingQuestion.Title = @event.QuestionText;
                existingQuestion.QuestionType = @event.QuestionType;
                existingQuestion.Answers = (@event.Answers ?? Enumerable.Empty<Answer>()).Select(x => new PdfAnswerView
                    {
                        Title = x.AnswerText,
                        AnswerType = x.AnswerType,
                        AnswerValue = x.AnswerValue
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
                        AnswerType = x.AnswerType,
                        AnswerValue = x.AnswerValue
                    }).ToList(),
                    Variable = @event.StataExportCaption
                };

                newQuestion.ConditionExpression = @event.ConditionExpression;
                questionnaire.AddQuestion(newQuestion, @event.GroupPublicKey);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<QuestionDeleted> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                questionnaire.RemoveQuestion(@event.QuestionId);
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
                PdfEntityView itemToMove = questionnaire.Children.TreeToEnumerable()
                                                       .FirstOrDefault(x => x.PublicId == @event.PublicKey);
                var targetContainer = questionnaire.Children.TreeToEnumerable()
                                                            .FirstOrDefault(x => x.PublicId == @event.GroupKey) ?? questionnaire;


                itemToMove.GetParent().Children.Remove(itemToMove);
                itemToMove.Depth = targetContainer.Depth + 1;
                if (@event.TargetIndex < 0)
                {
                    targetContainer.InsertChild(itemToMove, 0);

                }
                else if (@event.TargetIndex >= targetContainer.Children.Count)
                {
                    targetContainer.AddChild(itemToMove);
                }
                else
                {
                    targetContainer.InsertChild(itemToMove, @event.TargetIndex);
                }

                return questionnaire;
            });
        }

        private static void AppendItemTo(List<PdfEntityView> appendTo, PdfEntityView itemToMove, int targetIndex)
        {
            if (appendTo == null) throw new ArgumentNullException("appendTo");
            if (itemToMove == null) throw new ArgumentNullException("itemToMove");
            if (targetIndex < 0)
            {
                appendTo.Insert(0, itemToMove);
            }
            else if (targetIndex >= appendTo.Count)
            {
                appendTo.Add(itemToMove);
            }
            else
            {
                appendTo.Insert(targetIndex, itemToMove);
            }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            this.HandleUpdateEvent(evnt, handle: (@event, questionnaire) => this.CreatePdfQuestionnaireViewFromQuestionnaireDocument(@event.Source));
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
                CreatedBy = accountView != null ? accountView.UserName : "n/a"
            };

            pdf.FillFrom(questionnaireDocument);
            return pdf;
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[] { typeof(AccountDocument) }; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[] { typeof(PdfQuestionnaireView) }; }
        }
    }
}