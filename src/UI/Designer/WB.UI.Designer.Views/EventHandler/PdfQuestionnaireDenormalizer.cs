using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Views.Questionnaire.Pdf;

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
        IEventHandler<QuestionnaireDeleted>,
        IEventHandler<QuestionnaireItemMoved>,
        IEventHandler<QuestionnaireUpdated>,
        IEventHandler<TemplateImported>
    {
        private readonly IReadSideRepositoryWriter<PdfQuestionnaireView> repositoryWriter;
        private readonly ILogger logger;

        public PdfQuestionnaireDenormalizer(IReadSideRepositoryWriter<PdfQuestionnaireView> repositoryWriter, ILogger logger)
        {
            this.repositoryWriter = repositoryWriter;
            this.logger = logger;
        }

        private void HandleUpdateEvent<TEvent>(IPublishedEvent<TEvent> evnt, Func<TEvent, PdfQuestionnaireView, PdfQuestionnaireView> handle)
        {
            try
            {
                Guid questionnaireId = evnt.EventSourceId;
                PdfQuestionnaireView initialQuestionnaire = this.repositoryWriter.GetById(questionnaireId);

                PdfQuestionnaireView updatedQuestionnaire = handle(evnt.Payload, initialQuestionnaire);
                if (updatedQuestionnaire == null)
                {
                    this.repositoryWriter.Remove(questionnaireId);
                }
                else
                {
                    this.repositoryWriter.Store(updatedQuestionnaire, questionnaireId);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }

        public void Handle(IPublishedEvent<GroupCloned> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) => 
            {
                var newGroup = new PdfGroupView
                {
                    Title = @event.GroupText,
                    Id = @event.PublicKey,
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
                var newGroup = new PdfGroupView
                    {
                        Title = @event.GroupText,
                        Id = @event.PublicKey,
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
                        Id = @event.PublicKey,
                        Title = @event.QuestionText,
                        QuestionType = @event.QuestionType,
                        Answers = (@event.Answers ?? Enumerable.Empty<Answer>())
                                    .Select(x => new PdfAnswerView
                                        {
                                            Title = x.AnswerText,
                                            AnswerType = x.AnswerType,
                                            AnswerValue = x.AnswerValue
                                        }).ToList(),
                       HasCodition = !string.IsNullOrEmpty(@event.ConditionExpression)
                    };

                questionnaire.AddQuestion(newQuestion, @event.GroupPublicKey);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<QuestionChanged> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var existingQuestion = questionnaire.GetQuestion(@event.PublicKey);
                existingQuestion.HasCodition = !string.IsNullOrEmpty(@event.ConditionExpression);

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
                    Id = @event.PublicKey,
                    Title = @event.QuestionText,
                    QuestionType = @event.QuestionType,
                    Answers = (@event.Answers ?? Enumerable.Empty<Answer>()).Select(x => new PdfAnswerView
                    {
                        Title = x.AnswerText,
                        AnswerType = x.AnswerType,
                        AnswerValue = x.AnswerValue
                    }).ToList(),
                    HasCodition = !string.IsNullOrEmpty(@event.ConditionExpression)
                };

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

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) => null);
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
                var newQuestionnaire = new PdfQuestionnaireView {
                    Title = @event.Title,
                    CreationDate = @event.CreationDate
                };

                return newQuestionnaire;
            });
        }

        public void Handle(IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var itemToMove = questionnaire.Groups.TreeToEnumerable().FirstOrDefault(x => x.Id == @event.PublicKey);
                if (itemToMove != null && itemToMove.Parent != null)
                {

                    if (@event.TargetIndex < 0)
                    {
                        itemToMove.Parent.Children.Insert(0, itemToMove);
                    }
                    else if (@event.TargetIndex >= itemToMove.Parent.Children.Count)
                    {
                        itemToMove.Parent.Children.Add(itemToMove);
                    }
                    else
                    {
                        itemToMove.Parent.Children.Insert(@event.TargetIndex, itemToMove);
                    }
                }

                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var pdf = new PdfQuestionnaireView();
                pdf.Title = @event.Source.Title;
                pdf.FillFrom(@event.Source);

                return pdf;
            });
        }
    }
}