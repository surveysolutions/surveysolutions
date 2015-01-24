﻿extern alias designer;

using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using System.Collections.Generic;
using Ncqrs.Eventing.Storage;
using Ncqrs.Spec;
using NSubstitute;
using Quartz;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers;
using WB.Core.BoundedContexts.Supervisor;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom.Implementation;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.BoundedContexts.Supervisor.Users.Implementation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandTransformation;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.SharedKernels.SurveySolutions.Services;
using WB.UI.Designer.Api;
using WB.UI.Supervisor.Controllers;
using Identity = WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity;
using Questionnaire = WB.Core.BoundedContexts.Designer.Aggregates.Questionnaire;
using QuestionnaireDeleted = WB.Core.SharedKernels.DataCollection.Events.Questionnaire.QuestionnaireDeleted;

namespace WB.Tests.Unit
{
    internal static class Create
    {
        public static IPublishedEvent<T> ToPublishedEvent<T>(this T @event, Guid? eventSourceId = null)
            where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event
                && publishedEvent.EventSourceId == (eventSourceId ?? Guid.NewGuid()));
        }

        public static class Event
        {
            public static class Designer
            {
                public static designer::Main.Core.Events.Questionnaire.TemplateImported TemplateImported(QuestionnaireDocument questionnaireDocument)
                {
                    return new designer::Main.Core.Events.Questionnaire.TemplateImported { Source = questionnaireDocument };
                }
            }

            public static IPublishedEvent<QuestionnaireDeleted> QuestionnaireDeleted(Guid? questionnaireId = null, long? version = null)
            {
                var questionnaireDeleted = new QuestionnaireDeleted
                {

                    QuestionnaireVersion = version ?? 1
                }.ToPublishedEvent(questionnaireId ?? Guid.NewGuid());
                return questionnaireDeleted;
            }

            public static ExpressionsMigratedToCSharp ExpressionsMigratedToCSharpEvent()
            {
                return new ExpressionsMigratedToCSharp();
            }

            public static NewGroupAdded AddGroup(Guid groupId, Guid? parentId = null, string variableName = null)
            {
                return new NewGroupAdded
                {
                    PublicKey = groupId,
                    ParentGroupPublicKey = parentId,
                    VariableName = variableName
                };
            }

            public static GroupBecameARoster GroupBecameRoster(Guid rosterId)
            {
                return new GroupBecameARoster(Guid.NewGuid(), rosterId);
            }

            public static RosterChanged RosterChanged(Guid rosterId, RosterSizeSourceType rosterType, string[] titles)
            {
                return new RosterChanged(Guid.NewGuid(), rosterId, null, rosterType, titles, null);
            }

            public static NewQuestionAdded AddTextQuestion(Guid questionId, Guid parentId)
            {
                return new NewQuestionAdded
                {
                    PublicKey = questionId,
                    GroupPublicKey = parentId,
                    QuestionType = QuestionType.Text
                };
            }

            public static NumericQuestionChanged UpdateNumericIntegerQuestion(Guid questionId, string variableName)
            {
                return new NumericQuestionChanged
                {
                    PublicKey = questionId,
                    StataExportCaption = variableName,
                    IsInteger = true
                };
            }

            public static QuestionChanged QuestionChanged(Guid questionId, string variableName, QuestionType questionType)
            {
                return new QuestionChanged
                {
                    PublicKey = questionId,
                    StataExportCaption = variableName,
                    QuestionType = questionType
                };
            }

            public static GroupsDisabled GroupsDisabled(Guid? id = null, decimal[] rosterVector = null)
            {
                var identities = new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
                };
                return new GroupsDisabled(identities);
            }

            public static QuestionsDisabled QuestionsDisabled(Guid? id = null, decimal[] rosterVector = null)
            {
                var identities = new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
                };
                return new QuestionsDisabled(identities);
            }

            public static RosterInstancesAdded RosterInstancesAdded(Guid? rosterGroupId = null,
                decimal[] rosterVector = null,
                decimal? rosterInstanceId = null,
                int? sortIndex = null)
            {
                return new RosterInstancesAdded(new[]
                {
                    new AddedRosterInstance(rosterGroupId ?? Guid.NewGuid(), rosterVector ?? new decimal[0], rosterInstanceId ?? 0.0m, sortIndex)
                });
            }

            public static RosterInstancesRemoved RosterInstancesRemoved(Guid? rosterGroupId = null)
            {
                return new RosterInstancesRemoved(new[]
                {
                    new RosterInstance(rosterGroupId ?? Guid.NewGuid(), new decimal[0], 0.0m)
                });
            }

            public static RosterInstancesTitleChanged RosterInstancesTitleChanged(Guid? rosterId = null)
            {
                return new RosterInstancesTitleChanged(
                    new[]
                {
                    new ChangedRosterInstanceTitleDto(new RosterInstance(rosterId ?? Guid.NewGuid(), new decimal[0], 0.0m), "title")
                });
            }

            public static StaticTextAdded StaticTextAdded(Guid? parentId = null, string text = null, Guid? responsibleId = null, Guid? publicKey = null)
            {
                return new StaticTextAdded
                {
                    EntityId = publicKey.GetValueOrDefault(Guid.NewGuid()),
                    ResponsibleId = responsibleId ?? Guid.NewGuid(),
                    ParentId =  parentId ?? Guid.NewGuid(),
                    Text = text
                };
            }
        }

        public static QuestionnaireDocument QuestionnaireDocument(Guid? id = null, params IComposite[] children)
        {
            return new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children != null ? children.ToList() : new List<IComposite>(),
            };
        }

        public static Group Chapter(string title = "Chapter X", IEnumerable<IComposite> children = null)
        {
            return Create.Group(
                title: title,
                children: children);
        }

        public static Group Group(
            Guid? groupId = null,
            string title = "Group X",
            string variable = null,
            string enablementCondition = null,
            IEnumerable<IComposite> children = null)
        {
            return new Group(title)
            {
                PublicKey = groupId ?? Guid.NewGuid(),
                VariableName = variable,
                ConditionExpression = enablementCondition,
                Children = children != null ? children.ToList() : new List<IComposite>(),
            };
        }

        public static IQuestion Question(
            Guid? questionId = null,
            string variable = null,
            string enablementCondition = null,
            string validationExpression = null,
            bool isMandatory = false,
            string validationMessage = null)
        {
            return new TextQuestion("Question X")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionType = QuestionType.Text,
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Mandatory = isMandatory
            };
        }

        public static MultyOptionsQuestion MultyOptionsQuestion(Guid? id = null, bool isMandatory = false,
            IEnumerable<Answer> answers = null, Guid? linkedToQuestionId = null, string variable = null)
        {
            return new MultyOptionsQuestion
            {
                QuestionType = QuestionType.MultyOption,
                PublicKey = id ?? Guid.NewGuid(),
                Mandatory = isMandatory,
                Answers = linkedToQuestionId.HasValue ? null : new List<Answer>(answers ?? new Answer[] { }),
                LinkedToQuestionId = linkedToQuestionId,
                StataExportCaption = variable
            };
        }

        public static Group Roster(Guid? rosterId = null, string title = "Roster X", string variable = null, string enablementCondition = null,
            string[] fixedTitles = null, IEnumerable<IComposite> children = null,
            RosterSizeSourceType rosterSizeSourceType = RosterSizeSourceType.FixedTitles,
            Guid? rosterSizeQuestionId = null, Guid? rosterTitleQuestionId = null)
        {
            Group group = Create.Group(
                groupId: rosterId,
                title: title,
                variable: variable,
                enablementCondition: enablementCondition,
                children: children);

            group.IsRoster = true;
            group.RosterSizeSource = rosterSizeSourceType;

            if (rosterSizeSourceType == RosterSizeSourceType.FixedTitles)
                group.RosterFixedTitles = fixedTitles ?? new[] { "Roster X-1", "Roster X-2", "Roster X-3" };

            group.RosterSizeQuestionId = rosterSizeQuestionId;
            group.RosterTitleQuestionId = rosterTitleQuestionId;

            return group;
        }

        public static NumericQuestion NumericIntegerQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null, bool isMandatory = false)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = true,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Mandatory = isMandatory
            };
        }

        public static SingleQuestion SingleQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null, bool isMandatory = false,
            Guid? cascadeFromQuestionId = null, List<Answer> options = null)
        {
            return new SingleQuestion
            {
                QuestionType = QuestionType.SingleOption,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Mandatory = isMandatory,
                Answers = options ?? new List<Answer>(),
                CascadeFromQuestionId = cascadeFromQuestionId
            };
        }

        public static Answer Option(Guid? id = null, string text = null, string value = null, string parentValue = null)
        {
            return new Answer
            {
                PublicKey = id ?? Guid.NewGuid(),
                AnswerText = text ?? "text",
                AnswerValue = value ?? "1",
                ParentValue = parentValue
            };
        }

        private class SyncAsyncExecutorStub : IAsyncExecutor
        {
            public void ExecuteAsync(Action action)
            {
                action.Invoke();
            }
        }

        public static PdfQuestionnaireView PdfQuestionnaireView(Guid? publicId = null)
        {
            return new PdfQuestionnaireView
            {
                PublicId = publicId ?? Guid.Parse("FEDCBA98765432100123456789ABCDEF"),
            };
        }

        public static PdfQuestionView PdfQuestionView()
        {
            return new PdfQuestionView();
        }

        public static PdfGroupView PdfGroupView()
        {
            return new PdfGroupView();
        }

        public static RoslynExpressionProcessor RoslynExpressionProcessor()
        {
            return new RoslynExpressionProcessor();
        }

        public static CreateInterviewControllerCommand CreateInterviewControllerCommand()
        {
            return new CreateInterviewControllerCommand()
            {
                AnswersToFeaturedQuestions = new List<UntypedQuestionAnswer>()
            };
        }

        public static NCalcToCSharpConverter NCalcToCSharpConverter()
        {
            return new NCalcToCSharpConverter();
        }

        public static IAsyncExecutor SyncAsyncExecutor()
        {
            return new SyncAsyncExecutorStub();
        }

        public static AtomFeedReader AtomFeedReader(Func<HttpMessageHandler> messageHandler = null, IHeadquartersSettings settings = null)
        {
            return new AtomFeedReader(
                messageHandler ?? Mock.Of<Func<HttpMessageHandler>>(),
                settings ?? Mock.Of<IHeadquartersSettings>());
        }

        public static InterviewSummary InterviewSummary(Guid? questionnaireId = null, long? questionnaireVersion = null)
        {
            return new InterviewSummary()
            {
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                QuestionnaireVersion = questionnaireVersion ?? 1
            };
        }

        public static InterviewItemId InterviewItemId(Guid id, decimal[] rosterVector = null)
        {
            return new InterviewItemId(id, rosterVector);
        }

        public static IPublishedEvent<QuestionDeleted> QuestionDeletedEvent(string questionId = null)
        {
            return ToPublishedEvent(new QuestionDeleted(GetQuestionnaireItemId(questionId)));
        }

        public static IPublishedEvent<QuestionCloned> QuestionClonedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            QuestionType questionType = QuestionType.Text, string questionConditionExpression = null,
            string sourceQuestionId = null)
        {
            return ToPublishedEvent(new QuestionCloned()
            {
                PublicKey = GetQuestionnaireItemId(questionId),
                GroupPublicKey = GetQuestionnaireItemId(parentGroupId),
                StataExportCaption = questionVariable,
                QuestionText = questionTitle,
                QuestionType = questionType,
                ConditionExpression = questionConditionExpression,
                SourceQuestionId = GetQuestionnaireItemId(sourceQuestionId),
                TargetIndex = 0
            });
        }

        public static IPublishedEvent<QuestionChanged> QuestionChangedEvent(string questionId, string parentGroupId,
            string questionVariable, string questionTitle, QuestionType questionType, string questionConditionExpression)
        {
            return ToPublishedEvent(new QuestionChanged()
            {
                PublicKey = Guid.Parse(questionId),
                GroupPublicKey = Guid.Parse(parentGroupId),
                StataExportCaption = questionVariable,
                QuestionText = questionTitle,
                QuestionType = questionType,
                ConditionExpression = questionConditionExpression
            });
        }

        public static IPublishedEvent<QuestionnaireCloned> QuestionnaireClonedEvent(string questionnaireId,
            string chapter1Id, string chapter1Title, string chapter2Id, string chapter2Title,
            string questionnaireTitle = null, string chapter1GroupId = null, string chapter1GroupTitle = null,
            string chapter2QuestionId = null, string chapter2QuestionTitle = null,
            string chapter2QuestionVariable = null,
            string chapter2QuestionConditionExpression = null,
            string chapter1StaticTextId = null, string chapter1StaticText = null,
            bool? isPublic = null)
        {
            var result = ToPublishedEvent(new QuestionnaireCloned()
            {
                QuestionnaireDocument =
                    CreateQuestionnaireDocument(questionnaireId: questionnaireId, questionnaireTitle: questionnaireTitle,
                        chapter1Id: chapter1Id, chapter1Title: chapter1Title, chapter2Id: chapter2Id,
                        chapter2Title: chapter2Title, chapter1GroupId: chapter1GroupId,
                        chapter1GroupTitle: chapter1GroupTitle, chapter2QuestionId: chapter2QuestionId,
                        chapter2QuestionTitle: chapter2QuestionTitle, chapter2QuestionVariable: chapter2QuestionVariable,
                        chapter2QuestionConditionExpression: chapter2QuestionConditionExpression,
                        chapter1StaticTextId: chapter1StaticTextId, chapter1StaticText: chapter1StaticText,
                        isPublic: isPublic ?? false)
            }, new Guid(questionnaireId));
            return result;
        }

        public static IPublishedEvent<designer::Main.Core.Events.Questionnaire.TemplateImported> TemplateImportedEvent(string questionnaireId,
            string chapter1Id,
            string chapter1Title,
            string chapter2Id,
            string chapter2Title,
            string questionnaireTitle = null,
            string chapter1GroupId = null, string chapter1GroupTitle = null,
            string chapter2QuestionId = null,
            string chapter2QuestionTitle = null,
            string chapter2QuestionVariable = null,
            string chapter2QuestionConditionExpression = null,
            string chapter1StaticTextId = null, string chapter1StaticText = null,
            bool? isPublic = null)
        {
            return ToPublishedEvent(new designer::Main.Core.Events.Questionnaire.TemplateImported()
            {
                Source =
                    CreateQuestionnaireDocument(questionnaireId: questionnaireId, questionnaireTitle: questionnaireTitle,
                        chapter1Id: chapter1Id, chapter1Title: chapter1Title, chapter2Id: chapter2Id,
                        chapter2Title: chapter2Title, chapter1GroupId: chapter1GroupId,
                        chapter1GroupTitle: chapter1GroupTitle, chapter2QuestionId: chapter2QuestionId,
                        chapter2QuestionTitle: chapter2QuestionTitle, chapter2QuestionVariable: chapter2QuestionVariable,
                        chapter2QuestionConditionExpression: chapter2QuestionConditionExpression,
                        chapter1StaticTextId: chapter1StaticTextId, chapter1StaticText: chapter1StaticText,
                        isPublic: isPublic ?? false)
            }, new Guid(questionnaireId));
        }

        public static IPublishedEvent<QuestionnaireItemMoved> QuestionnaireItemMovedEvent(string itemId,
            string targetGroupId = null)
        {
            return ToPublishedEvent(new QuestionnaireItemMoved()
            {
                PublicKey = Guid.Parse(itemId),
                GroupKey = GetQuestionnaireItemParentId(targetGroupId),
                TargetIndex = 0
            });
        }

        public static IPublishedEvent<QuestionnaireUpdated> QuestionnaireUpdatedEvent(string questionnaireId,
            string questionnaireTitle,
            bool isPublic = false)
        {
            return ToPublishedEvent(new QuestionnaireUpdated() { Title = questionnaireTitle, IsPublic = isPublic }, new Guid(questionnaireId));
        }

        public static IPublishedEvent<TextListQuestionAdded> TextListQuestionAddedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            string questionConditionExpression = null)
        {
            return ToPublishedEvent(new TextListQuestionAdded()
            {
                PublicKey = GetQuestionnaireItemId(questionId),
                GroupId = GetQuestionnaireItemId(parentGroupId),
                StataExportCaption = questionVariable,
                QuestionText = questionTitle,
                ConditionExpression = questionConditionExpression
            });
        }

        public static IPublishedEvent<TextListQuestionChanged> TextListQuestionChangedEvent(string questionId,
            string questionVariable, string questionTitle, string questionConditionExpression)
        {
            return ToPublishedEvent(new TextListQuestionChanged()
            {
                PublicKey = Guid.Parse(questionId),
                StataExportCaption = questionVariable,
                QuestionText = questionTitle,
                ConditionExpression = questionConditionExpression
            });
        }

        public static IPublishedEvent<TextListQuestionCloned> TextListQuestionClonedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            string questionConditionExpression = null, string sourceQuestionId = null)
        {
            return ToPublishedEvent(new TextListQuestionCloned()
            {
                PublicKey = GetQuestionnaireItemId(questionId),
                GroupId = GetQuestionnaireItemId(parentGroupId),
                StataExportCaption = questionVariable,
                QuestionText = questionTitle,
                ConditionExpression = questionConditionExpression,
                SourceQuestionId = GetQuestionnaireItemId(sourceQuestionId),
                TargetIndex = 0
            });
        }

        public static IPublishedEvent<QRBarcodeQuestionUpdated> QRBarcodeQuestionUpdatedEvent(string questionId,
            string questionVariable, string questionTitle, string questionConditionExpression)
        {
            return ToPublishedEvent(new QRBarcodeQuestionUpdated()
            {
                QuestionId = Guid.Parse(questionId),
                VariableName = questionVariable,
                Title = questionTitle,
                EnablementCondition = questionConditionExpression
            });
        }

        public static IPublishedEvent<QRBarcodeQuestionCloned> QRBarcodeQuestionClonedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            string questionConditionExpression = null, string sourceQuestionId = null)
        {
            return ToPublishedEvent(new QRBarcodeQuestionCloned()
            {
                QuestionId = GetQuestionnaireItemId(questionId),
                ParentGroupId = GetQuestionnaireItemId(parentGroupId),
                VariableName = questionVariable,
                Title = questionTitle,
                EnablementCondition = questionConditionExpression,
                SourceQuestionId = GetQuestionnaireItemId(sourceQuestionId),
                TargetIndex = 0
            });
        }

        public static IPublishedEvent<QRBarcodeQuestionAdded> QRBarcodeQuestionAddedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            string questionConditionExpression = null)
        {
            return ToPublishedEvent(new QRBarcodeQuestionAdded()
            {
                QuestionId = GetQuestionnaireItemId(questionId),
                ParentGroupId = GetQuestionnaireItemId(parentGroupId),
                VariableName = questionVariable,
                Title = questionTitle,
                EnablementCondition = questionConditionExpression
            });
        }

        public static IPublishedEvent<StaticTextAdded> StaticTextAddedEvent(string entityId = null, string parentId = null, string text = null)
        {
            return ToPublishedEvent(new StaticTextAdded()
            {
                EntityId = GetQuestionnaireItemId(entityId),
                ParentId = GetQuestionnaireItemId(parentId),
                Text = text
            });
        }

        public static IPublishedEvent<StaticTextUpdated> StaticTextUpdatedEvent(string entityId = null, string text = null)
        {
            return ToPublishedEvent(new StaticTextUpdated()
            {
                EntityId = GetQuestionnaireItemId(entityId),
                Text = text
            });
        }

        public static IPublishedEvent<StaticTextCloned> StaticTextClonedEvent(string entityId = null,
            string parentId = null, string sourceEntityId = null, string text = null, int targetIndex = 0)
        {
            return ToPublishedEvent(new StaticTextCloned()
            {
                EntityId = GetQuestionnaireItemId(entityId),
                ParentId = GetQuestionnaireItemId(parentId),
                SourceEntityId = GetQuestionnaireItemId(sourceEntityId),
                Text = text,
                TargetIndex = targetIndex
            });
        }

        public static IPublishedEvent<StaticTextDeleted> StaticTextDeletedEvent(string entityId = null)
        {
            return ToPublishedEvent(new StaticTextDeleted()
            {
                EntityId = GetQuestionnaireItemId(entityId)
            });
        }

        public static IPublishedEvent<NumericQuestionCloned> NumericQuestionClonedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            string questionConditionExpression = null, string sourceQuestionId = null)
        {
            return ToPublishedEvent(new NumericQuestionCloned()
            {
                PublicKey = GetQuestionnaireItemId(questionId),
                GroupPublicKey = GetQuestionnaireItemId(parentGroupId),
                StataExportCaption = questionVariable,
                QuestionText = questionTitle,
                ConditionExpression = questionConditionExpression,
                SourceQuestionId = GetQuestionnaireItemId(sourceQuestionId),
                TargetIndex = 0
            });
        }

        public static IPublishedEvent<NumericQuestionChanged> NumericQuestionChangedEvent(string questionId,
            string questionVariable, string questionTitle, string questionConditionExpression)
        {
            return ToPublishedEvent(new NumericQuestionChanged()
            {
                PublicKey = Guid.Parse(questionId),
                StataExportCaption = questionVariable,
                QuestionText = questionTitle,
                ConditionExpression = questionConditionExpression
            });
        }

        public static IPublishedEvent<NumericQuestionAdded> NumericQuestionAddedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            string questionConditionExpression = null)
        {
            return ToPublishedEvent(new NumericQuestionAdded()
            {
                PublicKey = GetQuestionnaireItemId(questionId),
                GroupPublicKey = GetQuestionnaireItemId(parentGroupId),
                StataExportCaption = questionVariable,
                QuestionText = questionTitle,
                ConditionExpression = questionConditionExpression
            });
        }

        public static IPublishedEvent<NewQuestionnaireCreated> NewQuestionnaireCreatedEvent(string questionnaireId,
            string questionnaireTitle = null,
            bool? isPublic = null)
        {
            return ToPublishedEvent(new NewQuestionnaireCreated()
            {
                PublicKey = new Guid(questionnaireId),
                Title = questionnaireTitle,
                IsPublic = isPublic ?? false
            }, new Guid(questionnaireId));
        }

        public static IPublishedEvent<NewQuestionAdded> NewQuestionAddedEvent(string questionId = null,
            string parentGroupId = null, QuestionType questionType = QuestionType.Text, string questionVariable = null,
            string questionTitle = null, string questionConditionExpression = null)
        {
            return ToPublishedEvent(new NewQuestionAdded()
            {
                PublicKey = GetQuestionnaireItemId(questionId),
                GroupPublicKey = GetQuestionnaireItemId(parentGroupId),
                QuestionType = questionType,
                StataExportCaption = questionVariable,
                QuestionText = questionTitle,
                ConditionExpression = questionConditionExpression
            });
        }

        public static IPublishedEvent<NewGroupAdded> NewGroupAddedEvent(string groupId, string parentGroupId = null,
            string groupTitle = null)
        {
            return ToPublishedEvent(new NewGroupAdded()
            {
                PublicKey = Guid.Parse(groupId),
                ParentGroupPublicKey = GetQuestionnaireItemParentId(parentGroupId),
                GroupText = groupTitle
            });
        }

        public static IPublishedEvent<GroupUpdated> GroupUpdatedEvent(string groupId, string groupTitle)
        {
            return ToPublishedEvent(new GroupUpdated()
            {
                GroupPublicKey = Guid.Parse(groupId),
                GroupText = groupTitle
            });
        }

        public static IPublishedEvent<GroupBecameARoster> GroupBecameARosterEvent(string groupId)
        {
            return ToPublishedEvent(new GroupBecameARoster(responsibleId: new Guid(), groupId: Guid.Parse(groupId)));
        }

        public static IPublishedEvent<GroupStoppedBeingARoster> GroupStoppedBeingARosterEvent(string groupId)
        {
            return ToPublishedEvent(new GroupStoppedBeingARoster(responsibleId: new Guid(), groupId: Guid.Parse(groupId)));
        }

        public static IPublishedEvent<GroupDeleted> GroupDeletedEvent(string groupId)
        {
            return ToPublishedEvent(new GroupDeleted()
            {
                GroupPublicKey = Guid.Parse(groupId)
            });
        }

        public static IPublishedEvent<GroupCloned> GroupClonedEvent(string groupId, string groupTitle = null,
            string parentGroupId = null)
        {
            return ToPublishedEvent(new GroupCloned()
            {
                PublicKey = Guid.Parse(groupId),
                ParentGroupPublicKey = GetQuestionnaireItemParentId(parentGroupId),
                GroupText = groupTitle,
                TargetIndex = 0
            });
        }

        public static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] children)
        {
            return CreateQuestionnaireDocumentWithOneChapter(null, children);
        }

        public static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(Guid? chapterId = null, params IComposite[] children)
        {
            var result = new QuestionnaireDocument();
            var chapter = new Group("Chapter") { PublicKey = chapterId.GetValueOrDefault() };

            result.Children.Add(chapter);

            foreach (var child in children)
            {
                chapter.Children.Add(child);
            }

            return result;
        }

        private static Guid GetQuestionnaireItemId(string questionnaireItemId)
        {
            return string.IsNullOrEmpty(questionnaireItemId) ? Guid.NewGuid() : Guid.Parse(questionnaireItemId);
        }

        private static Guid? GetQuestionnaireItemParentId(string questionnaireItemParentId)
        {
            return string.IsNullOrEmpty(questionnaireItemParentId)
                ? (Guid?)null
                : Guid.Parse(questionnaireItemParentId);
        }

        private static QuestionnaireDocument CreateQuestionnaireDocument(string questionnaireId,
            string questionnaireTitle,
            string chapter1Id,
            string chapter1Title,
            string chapter2Id,
            string chapter2Title,
            string chapter1GroupId,
            string chapter1GroupTitle,
            string chapter2QuestionId,
            string chapter2QuestionTitle,
            string chapter2QuestionVariable,
            string chapter2QuestionConditionExpression,
            string chapter1StaticTextId,
            string chapter1StaticText,
            bool isPublic)
        {
            return new QuestionnaireDocument()
            {
                PublicKey = Guid.Parse(questionnaireId),
                Title = questionnaireTitle,
                IsPublic = isPublic,
                Children = new List<IComposite>()
                {
                    new Group()
                    {
                        PublicKey = Guid.Parse(chapter1Id),
                        Title = chapter1Title,
                        Children = new List<IComposite>()
                        {
                            new StaticText(publicKey: GetQuestionnaireItemId(chapter1StaticTextId), text: chapter1StaticText),
                            new Group()
                            {
                                PublicKey = GetQuestionnaireItemId(chapter1GroupId),
                                Title = chapter1GroupTitle,
                                Children = new List<IComposite>()
                                {
                                    new Group()
                                    {
                                        IsRoster = true
                                    }
                                }
                            }
                        }
                    },
                    new Group()
                    {
                        PublicKey = Guid.Parse(chapter2Id),
                        Title = chapter2Title,
                        Children = new List<IComposite>()
                        {
                            new TextQuestion()
                            {
                                PublicKey = GetQuestionnaireItemId(chapter2QuestionId),
                                QuestionText = chapter2QuestionTitle,
                                StataExportCaption = chapter2QuestionVariable,
                                QuestionType = QuestionType.Text,
                                ConditionExpression = chapter2QuestionConditionExpression
                            }
                        }
                    }
                }
            };
        }

        public static IPublishedEvent<MultimediaQuestionUpdated> MultimediaQuestionUpdatedEvent(string questionId, string questionVariable, string questionTitle, string questionConditionExpression)
        {
            return ToPublishedEvent(new MultimediaQuestionUpdated()
            {
                QuestionId = Guid.Parse(questionId),
                VariableName = questionVariable,
                Title = questionTitle,
                EnablementCondition = questionConditionExpression
            });
        }

        public static Questionnaire Questionnaire(Guid? questionnaireId = null,
            string title = "Questionnnaire Title", Guid? responsibleId = null)
        {
            return new Questionnaire(
                publicKey: questionnaireId ?? Guid.Parse("ddddaaaaaaaaaaaaaaaaaaaaaaaabbbb"),
                title: title,
                createdBy: responsibleId ?? Guid.Parse("ddddccccccccccccccccccccccccbbbb"));
        }

        public static Questionnaire Questionnaire(QuestionnaireDocument questionnaireDocument, Guid? responsibleId = null)
        {
            return new Questionnaire(
                createdBy: responsibleId ?? Guid.NewGuid(),
                source: questionnaireDocument);
        }

        public static EventContext EventContext()
        {
            return new EventContext();
        }

        public static QuestionnaireDocument QuestionnaireDocument(Guid? id = null, bool usesCSharp = false, IEnumerable<IComposite> children = null)
        {
            return new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children != null ? children.ToList() : new List<IComposite>(),
                UsesCSharp = usesCSharp,
            };
        }

        public static INumericQuestion NumericQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            bool isInteger = false, int? countOfDecimalPlaces = null, int? maxValue = null)
        {
            return new NumericQuestion("Question N")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                IsInteger = isInteger,
                CountOfDecimalPlaces = countOfDecimalPlaces,
                MaxValue = maxValue,
            };
        }

        public static ITextListQuestion TextListQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            int? maxAnswerCount = null)
        {
            return new TextListQuestion("Question TL")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                MaxAnswerCount = maxAnswerCount,
            };
        }

        public static TextQuestion TextQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string mask = null)
        {
            return new TextQuestion("Question T")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Mask = mask,
            };
        }

        public static IMultyOptionsQuestion MultipleOptionsQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            bool areAnswersOrdered = false, int? maxAllowedAnswers = null)
        {
            return new MultyOptionsQuestion("Question MO")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                AreAnswersOrdered = areAnswersOrdered,
                MaxAllowedAnswers = maxAllowedAnswers,
            };
        }

        public static InterviewsFeedDenormalizer InterviewsFeedDenormalizer(IReadSideRepositoryWriter<InterviewFeedEntry> feedEntryWriter = null,
            IReadSideKeyValueStorage<InterviewData> interviewsRepository = null, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepository = null)
        {
            return new InterviewsFeedDenormalizer(feedEntryWriter ?? Substitute.For<IReadSideRepositoryWriter<InterviewFeedEntry>>(),
                interviewsRepository ?? Substitute.For<IReadSideKeyValueStorage<InterviewData>>(), interviewSummaryRepository ?? Substitute.For<IReadSideRepositoryWriter<InterviewSummary>>());
        }

        public static QuestionnaireFeedDenormalizer QuestionnaireFeedDenormalizer(IReadSideRepositoryWriter<QuestionnaireFeedEntry> questionnaireFeedWriter)
        {
            return new QuestionnaireFeedDenormalizer(questionnaireFeedWriter);
        }

        public static HeadquartersLoginService HeadquartersLoginService(IHeadquartersUserReader headquartersUserReader = null,
            Func<HttpMessageHandler> messageHandler = null,
            ILogger logger = null,
            ICommandService commandService = null,
            IHeadquartersSettings headquartersSettings = null,
            IPasswordHasher passwordHasher = null)
        {
            return new HeadquartersLoginService(logger ?? Substitute.For<ILogger>(),
                commandService ?? Substitute.For<ICommandService>(),
                messageHandler ?? Substitute.For<Func<HttpMessageHandler>>(),
                headquartersSettings ?? HeadquartersSettings(),
                headquartersUserReader ?? Substitute.For<IHeadquartersUserReader>(),
                passwordHasher: passwordHasher ?? Substitute.For<IPasswordHasher>());
        }

        public static UserChangedFeedReader UserChangedFeedReader(IHeadquartersSettings settings = null,
            Func<HttpMessageHandler> messageHandler = null)
        {
            return new UserChangedFeedReader(settings ?? HeadquartersSettings(),
                messageHandler ?? Substitute.For<Func<HttpMessageHandler>>(), HeadquartersPullContext());
        }

        public static HeadquartersPullContext HeadquartersPullContext()
        {
            return new HeadquartersPullContext(Substitute.For<IPlainStorageAccessor<SynchronizationStatus>>());
        }

        public static HeadquartersPushContext HeadquartersPushContext()
        {
            return new HeadquartersPushContext(Substitute.For<IPlainStorageAccessor<SynchronizationStatus>>());
        }

        public static InterviewsSynchronizer InterviewsSynchronizer(
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepositoryWriter = null,
            IQueryableReadSideRepositoryReader<ReadyToSendToHeadquartersInterview> readyToSendInterviewsRepositoryWriter = null,
            Func<HttpMessageHandler> httpMessageHandler = null,
            IEventStore eventStore = null,
            ILogger logger = null,
            IJsonUtils jsonUtils = null,
            ICommandService commandService = null,
            HeadquartersPushContext headquartersPushContext = null,
            IQueryableReadSideRepositoryReader<UserDocument> userDocumentStorage = null,
            IQueryablePlainStorageAccessor<LocalInterviewFeedEntry> plainStorage = null,
            IHeadquartersInterviewReader headquartersInterviewReader = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null,
            IInterviewSynchronizationFileStorage interviewSynchronizationFileStorage = null,
            IArchiveUtils archiver = null)
        {
            return new InterviewsSynchronizer(
                Mock.Of<IAtomFeedReader>(),
                HeadquartersSettings(),
                logger ?? Mock.Of<ILogger>(),
                commandService ?? Mock.Of<ICommandService>(),
                plainStorage ?? Mock.Of<IQueryablePlainStorageAccessor<LocalInterviewFeedEntry>>(),
                userDocumentStorage ?? Mock.Of<IQueryableReadSideRepositoryReader<UserDocument>>(),
                plainQuestionnaireRepository ??
                    Mock.Of<IPlainQuestionnaireRepository>(
                        _ => _.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) == new QuestionnaireDocument()),
                headquartersInterviewReader ?? Mock.Of<IHeadquartersInterviewReader>(),
                HeadquartersPullContext(),
                headquartersPushContext ?? HeadquartersPushContext(),
                eventStore ?? Mock.Of<IEventStore>(),
                jsonUtils ?? Mock.Of<IJsonUtils>(),
                interviewSummaryRepositoryWriter ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                readyToSendInterviewsRepositoryWriter ?? Mock.Of<IQueryableReadSideRepositoryReader<ReadyToSendToHeadquartersInterview>>(),
                httpMessageHandler ?? Mock.Of<Func<HttpMessageHandler>>(),
                interviewSynchronizationFileStorage ??
                    Mock.Of<IInterviewSynchronizationFileStorage>(
                        _ => _.GetBinaryFilesFromSyncFolder() == new List<InterviewBinaryDataDescriptor>()),
                 archiver: archiver ?? Mock.Of<IArchiveUtils>());
        }

        public static IHeadquartersSettings HeadquartersSettings(Uri loginServiceUri = null,
            Uri usersChangedFeedUri = null,
            Uri interviewsFeedUri = null,
            string questionnaireDetailsEndpoint = "",
            string questionnaireAssemblyEndpoint = "",
            string accessToken = "",
            Uri interviewsPushUrl = null)
        {
            var headquartersSettingsMock = new Mock<IHeadquartersSettings>();
            headquartersSettingsMock.SetupGet(x => x.BaseHqUrl).Returns(loginServiceUri ?? new Uri("http://localhost/"));
            headquartersSettingsMock.SetupGet(x => x.UserChangedFeedUrl).Returns(usersChangedFeedUri ?? new Uri("http://localhost/"));
            headquartersSettingsMock.SetupGet(x => x.InterviewsFeedUrl).Returns(interviewsFeedUri ?? new Uri("http://localhost/"));
            headquartersSettingsMock.SetupGet(x => x.QuestionnaireDetailsEndpoint).Returns(questionnaireDetailsEndpoint);
            headquartersSettingsMock.SetupGet(x => x.QuestionnaireAssemblyEndpoint).Returns(questionnaireAssemblyEndpoint);
            headquartersSettingsMock.SetupGet(x => x.AccessToken).Returns(accessToken);
            headquartersSettingsMock.SetupGet(x => x.InterviewsPushUrl).Returns(interviewsPushUrl ?? new Uri("http://localhost/"));
            headquartersSettingsMock.SetupGet(x => x.FilePushUrl).Returns(new Uri("http://localhost/"));
            headquartersSettingsMock.SetupGet(x => x.QuestionnaireChangedFeedUrl).Returns(new Uri("http://localhost/"));
            headquartersSettingsMock.SetupGet(x => x.LoginServiceEndpointUrl).Returns(new Uri("http://localhost/"));
            return headquartersSettingsMock.Object;
        }

        public static CommittedEvent CommittedEvent(string origin = null, Guid? eventSourceId = null, object payload = null,
            Guid? eventIdentifier = null, long eventSequence = 1)
        {
            return new CommittedEvent(
                Guid.Parse("33330000333330000003333300003333"),
                origin,
                eventIdentifier ?? Guid.Parse("44440000444440000004444400004444"),
                eventSourceId ?? Guid.Parse("55550000555550000005555500005555"),
                eventSequence,
                new DateTime(2014, 10, 22),
                payload ?? "some payload");
        }

        public static InterviewSummary InterviewSummary()
        {
            return new InterviewSummary();
        }

        public static Synchronizer Synchronizer(IInterviewsSynchronizer interviewsSynchronizer = null)
        {
            return new Synchronizer(
                Mock.Of<ILocalFeedStorage>(),
                Mock.Of<IUserChangedFeedReader>(),
                Mock.Of<ILocalUserFeedProcessor>(),
                interviewsSynchronizer ?? Mock.Of<IInterviewsSynchronizer>(),
                Mock.Of<IQuestionnaireSynchronizer>(),
                HeadquartersPullContext(),
                HeadquartersPushContext());
        }

        public static HQSyncController HQSyncController(
            ISynchronizer synchronizer = null,
            IGlobalInfoProvider globalInfoProvider = null,
            HeadquartersPushContext headquartersPushContext = null)
        {
            return new HQSyncController(
                Mock.Of<ICommandService>(),
                Mock.Of<ILogger>(),
                HeadquartersPullContext(),
                headquartersPushContext ?? HeadquartersPushContext(),
                Mock.Of<IScheduler>(),
                synchronizer ?? Mock.Of<ISynchronizer>(),
                globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>());
        }

        public static RosterInstancesAdded RosterInstancesAdded(Guid? rosterGroupId = null)
        {
            return new RosterInstancesAdded(new[]
                {
                    new AddedRosterInstance(rosterGroupId ?? Guid.NewGuid(), new decimal[0], 0.0m, null)
                });
        }

        public static RosterInstancesRemoved RosterInstancesRemoved(Guid? rosterGroupId = null)
        {
            return new RosterInstancesRemoved(new[]
                {
                    new RosterInstance(rosterGroupId ?? Guid.NewGuid(), new decimal[0], 0.0m)
                });
        }

        public static RosterInstancesTitleChanged RosterInstancesTitleChanged(Guid? rosterId = null, string rosterTitle = null)
        {
            return new RosterInstancesTitleChanged(
                new[]
                {
                    new ChangedRosterInstanceTitleDto(new RosterInstance(rosterId ?? Guid.NewGuid(), new decimal[0], 0.0m), rosterTitle ?? "title")
                });
        }

        public static IPublishedEvent<InterviewCreated> InterviewCreatedEvent(string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion));
        }

        public static IPublishedEvent<InterviewFromPreloadedDataCreated> InterviewFromPreloadedDataCreatedEvent(string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewFromPreloadedDataCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion));
        }

        public static IPublishedEvent<InterviewOnClientCreated> InterviewOnClientCreatedEvent(string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewOnClientCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion));
        }

        public static IPublishedEvent<InterviewStatusChanged> InterviewStatusChangedEvent(InterviewStatus status,
            string comment = null,
            Guid? interviewId = null)
        {
            return ToPublishedEvent(new InterviewStatusChanged(status, comment), interviewId ?? Guid.NewGuid());
        }

        public static IPublishedEvent<SupervisorAssigned> SupervisorAssignedEvent(string userId = null,
            string supervisorId = null)
        {
            return
                ToPublishedEvent(new SupervisorAssigned(userId: GetGuidIdByStringId(userId),
                    supervisorId: GetGuidIdByStringId(supervisorId)));
        }

        public static IPublishedEvent<InterviewerAssigned> InterviewerAssignedEvent(string userId = null,
            string interviewerId = null)
        {
            return
                ToPublishedEvent(new InterviewerAssigned(userId: GetGuidIdByStringId(userId),
                    interviewerId: GetGuidIdByStringId(interviewerId)));
        }

        public static IPublishedEvent<InterviewDeleted> InterviewDeletedEvent(string userId = null)
        {
            return ToPublishedEvent(new InterviewDeleted(userId: GetGuidIdByStringId(userId)));
        }

        public static IPublishedEvent<InterviewHardDeleted> InterviewHardDeletedEvent(string userId = null)
        {
            return ToPublishedEvent(new InterviewHardDeleted(userId: GetGuidIdByStringId(userId)));
        }

        public static IPublishedEvent<InterviewRestored> InterviewRestoredEvent(string userId = null)
        {
            return ToPublishedEvent(new InterviewRestored(userId: GetGuidIdByStringId(userId)));
        }

        public static IPublishedEvent<InterviewRestarted> InterviewRestartedEvent(string userId = null)
        {
            return ToPublishedEvent(new InterviewRestarted(userId: GetGuidIdByStringId(userId), restartTime: DateTime.Now));
        }

        public static IPublishedEvent<InterviewCompleted> InterviewCompletedEvent(string userId = null)
        {
            return ToPublishedEvent(new InterviewCompleted(userId: GetGuidIdByStringId(userId), completeTime: DateTime.Now));
        }

        public static IPublishedEvent<InterviewRejected> InterviewRejectedEvent(string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRejected(userId: GetGuidIdByStringId(userId), comment: comment));
        }

        public static IPublishedEvent<InterviewApproved> InterviewApprovedEvent(string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewApproved(userId: GetGuidIdByStringId(userId), comment: comment));
        }

        public static IPublishedEvent<InterviewRejectedByHQ> InterviewRejectedByHQEvent(string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRejectedByHQ(userId: GetGuidIdByStringId(userId), comment: comment));
        }

        public static IPublishedEvent<InterviewApprovedByHQ> InterviewApprovedByHQEvent(string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewApprovedByHQ(userId: GetGuidIdByStringId(userId), comment: comment));
        }

        public static IPublishedEvent<SynchronizationMetadataApplied> SynchronizationMetadataAppliedEvent(string userId = null,
            InterviewStatus status = InterviewStatus.Created, string questionnaireId = null,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta = null, bool createdOnClient = false)
        {
            return
                ToPublishedEvent(new SynchronizationMetadataApplied(userId: GetGuidIdByStringId(userId), status: status,
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: 1, featuredQuestionsMeta: featuredQuestionsMeta,
                    createdOnClient: createdOnClient, comments: null));
        }

        private static Guid GetGuidIdByStringId(string stringId)
        {
            return string.IsNullOrEmpty(stringId) ? Guid.NewGuid() : Guid.Parse(stringId);
        }

        public static InterviewData InterviewData(bool createdOnClient = false,
            InterviewStatus status = InterviewStatus.Created,
            Guid? interviewId = null)
        {
            var result = new InterviewData();
            result.CreatedOnClient = createdOnClient;
            result.Status = status;
            result.InterviewId = interviewId.GetValueOrDefault();
            return result;
        }

        public static WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Questionnaire Questionnaire(Guid creatorId, QuestionnaireDocument document)
        {
            return new WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Questionnaire(new Guid(), document, false, "base65 string of assembly");
        }

        public static EnablementChanges EnablementChanges(List<Core.SharedKernels.DataCollection.Identity> groupsToBeDisabled = null, List<Core.SharedKernels.DataCollection.Identity> groupsToBeEnabled = null,
            List<Core.SharedKernels.DataCollection.Identity> questionsToBeDisabled = null, List<Core.SharedKernels.DataCollection.Identity> questionsToBeEnabled = null)
        {
            return new EnablementChanges(
                groupsToBeDisabled ?? new List<Core.SharedKernels.DataCollection.Identity>(),
                groupsToBeEnabled ?? new List<Core.SharedKernels.DataCollection.Identity>(),
                questionsToBeDisabled ?? new List<Core.SharedKernels.DataCollection.Identity>(),
                questionsToBeEnabled ?? new List<Core.SharedKernels.DataCollection.Identity>());
        }

        public static InterviewState InterviewState(InterviewStatus? status = null, List<AnswerComment> answerComments = null)
        {
            return new InterviewState(Guid.NewGuid(), 1, status ?? InterviewStatus.SupervisorAssigned, new Dictionary<string, object>(),
                new Dictionary<string, Tuple<Guid, decimal[], decimal[]>>(), new Dictionary<string, Tuple<Guid, decimal[], decimal[][]>>(),
                new Dictionary<string, Tuple<decimal, string>[]>(), new HashSet<string>(),
                answerComments ?? new List<AnswerComment>(),
                new HashSet<string>(),
                new HashSet<string>(), new Dictionary<string, DistinctDecimalList>(),
                new HashSet<string>(), new HashSet<string>(), true, Mock.Of<IInterviewExpressionState>());
        }

        public static Core.SharedKernels.DataCollection.Identity Identity(Guid id, decimal[] rosterVector)
        {
            return new Core.SharedKernels.DataCollection.Identity(id, rosterVector);
        }

        public static IQuestionnaireRepository QuestionnaireRepositoryStubWithOneQuestionnaire(
            Guid questionnaireId, IQuestionnaire questionaire = null)
        {
            questionaire = questionaire ?? Mock.Of<IQuestionnaire>();

            return Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionaire
                && repository.GetHistoricalQuestionnaire(questionnaireId, questionaire.Version) == questionaire
                && repository.GetHistoricalQuestionnaire(questionnaireId, 1) == questionaire);
        }

        public static IPublishableEvent PublishableEvent(Guid? eventSourceId = null)
        {
            return Mock.Of<IPublishableEvent>(_ => _.Payload == new object() && _.EventSourceId == (eventSourceId ?? Guid.NewGuid()));
        }

        public static NcqrCompatibleEventDispatcher NcqrCompatibleEventDispatcher(Type[] handlersToIgnore = null)
        {
            return new NcqrCompatibleEventDispatcher(Mock.Of<IEventStore>(), handlersToIgnore ?? new Type[]{});
        }

        public static ImportFromDesigner ImportFromDesignerCommand(Guid responsibleId, string base64StringOfAssembly)
        {
            return new ImportFromDesigner(responsibleId, new QuestionnaireDocument(), false, base64StringOfAssembly);
        }
    }
}