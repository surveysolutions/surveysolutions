extern alias designer;

using System;
using System.Linq;
using System.Net.Http;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;

using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using System.Collections.Generic;

using Microsoft.Practices.ServiceLocation;

using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ncqrs.Spec;
using NHibernate;
using NSubstitute;
using Quartz;

using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.OldDashboardCapability;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Supervisor;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom.Implementation;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.BoundedContexts.Supervisor.Users.Implementation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Hybrid.Implementation;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandTransformation;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using WB.UI.Interviewer.ViewModel.Dashboard;
using WB.UI.Supervisor.Controllers;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;
using Questionnaire = WB.Core.BoundedContexts.Designer.Aggregates.Questionnaire;
using QuestionnaireDeleted = WB.Core.SharedKernels.DataCollection.Events.Questionnaire.QuestionnaireDeleted;
using QuestionnaireVersion = WB.Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireVersion;

namespace WB.Tests.Unit
{
    internal static class Create
    {
        public static IPublishedEvent<T> ToPublishedEvent<T>(this T @event, 
            Guid? eventSourceId = null,
            string origin = null,
            DateTime? eventTimeStamp = null,
            Guid? eventId = null)
            where T : class
        {
            var mock = new Mock<IPublishedEvent<T>>();
            var eventIdentifier = eventId ?? Guid.NewGuid();
            mock.Setup(x => x.Payload).Returns(@event);
            mock.Setup(x => x.EventSourceId).Returns(eventSourceId ?? Guid.NewGuid());
            mock.Setup(x => x.Origin).Returns(origin);
            mock.Setup(x => x.EventIdentifier).Returns(eventIdentifier);
            mock.Setup(x => x.EventTimeStamp).Returns((eventTimeStamp ?? DateTime.Now));
            var publishableEventMock =mock.As<IUncommittedEvent>();
            publishableEventMock.Setup(x => x.Payload).Returns(@event);
            return mock.Object;
        }

        internal static class Command
        {
            public static LinkUserToDevice LinkUserToDeviceCommand(Guid userId, string deviceId)
            {
                return new LinkUserToDevice(userId, deviceId);
            }

            public static ImportFromSupervisor ImportFromSupervisor(IQuestionnaireDocument source)
            {
                return new ImportFromSupervisor(source);
            }

            public static AddMacro AddMacro(Guid questionnaire, Guid? macroId = null, Guid? userId = null)
            {
                return new AddMacro(questionnaire, macroId ?? Guid.NewGuid(), userId ?? Guid.NewGuid());
            }

            public static DeleteMacro DeleteMacro(Guid questionnaire, Guid? macroId = null, Guid? userId = null)
            {
                return new DeleteMacro(questionnaire, macroId ?? Guid.NewGuid(), userId ?? Guid.NewGuid());
            }

            internal static UpdateMacro UpdateMacro(Guid questionnaireId, Guid macroId, string name, string content, string description, Guid? userId)
            {
                return new UpdateMacro(questionnaireId, macroId, name, content, description, userId ?? Guid.NewGuid());
            }

            public static AnswerYesNoQuestion AnswerYesNoQuestion(Guid? userId = null,
                Guid? questionId = null, RosterVector rosterVector = null, AnsweredYesNoOption[] answeredOptions = null,
                DateTime? answerTime = null)
            {
                return new AnswerYesNoQuestion(
                    interviewId: Guid.NewGuid(),
                    userId: userId ?? Guid.NewGuid(),
                    questionId: questionId ?? Guid.NewGuid(),
                    rosterVector: rosterVector ?? WB.Core.SharedKernels.DataCollection.RosterVector.Empty,
                    answerTime: answerTime ?? DateTime.UtcNow,
                    answeredOptions: answeredOptions ?? new AnsweredYesNoOption[] {});
            }
        }

        internal static class Event
        {
            internal static class Designer
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

            public static RosterChanged RosterChanged(Guid rosterId, RosterSizeSourceType rosterType, FixedRosterTitle[] titles)
            {
                return new RosterChanged(Guid.NewGuid(), rosterId)
                {
                    RosterSizeQuestionId = null,
                    RosterSizeSource = rosterType,
                    FixedRosterTitles = titles,
                    RosterTitleQuestionId = null
                };
            }

            public static NewQuestionAdded AddTextQuestion(Guid questionId, Guid parentId)
            {
                return NewQuestionAdded(
                    publicKey: questionId,
                    groupPublicKey: parentId,
                    questionText: null,
                    questionType: QuestionType.Text,
                    stataExportCaption: null,
                    variableLabel: null,
                    featured: false,
                    questionScope: QuestionScope.Interviewer,
                    conditionExpression: null,
                    validationExpression: null,
                    validationMessage: null,
                    instructions: null,
                    responsibleId: Guid.NewGuid(),
                    linkedToQuestionId: null,
                    isFilteredCombobox: false,
                    cascadeFromQuestionId: null,
                    capital: false,
                    answerOrder: null,
                    answers: null,
                    isInteger: null);
            }

            public static NumericQuestionChanged UpdateNumericIntegerQuestion(Guid questionId, string variableName, string enablementCondition = null, string validationExpression = null)
            {
                return NumericQuestionChanged
                (
                    publicKey : questionId,
                    stataExportCaption : variableName,
                    isInteger : true,
                    conditionExpression : enablementCondition,
                    validationExpression : validationExpression
                );
            }

            public static QuestionChanged QuestionChanged(Guid questionId, string variableName, QuestionType questionType)
            {
                return QuestionChanged(
                    publicKey : questionId,
                    stataExportCaption : variableName,
                    questionType : questionType
                );
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

            public static GroupsEnabled GroupsEnabled(Guid? id = null, decimal[] rosterVector = null)
            {
                var identities = new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
                };
                return new GroupsEnabled(identities);
            }

            public static AnswersDeclaredInvalid AnswersDeclaredInvalid(Guid? id = null, decimal[] rosterVector = null)
            {
                var identities = new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]),
                };
                return new AnswersDeclaredInvalid(identities);
            }


            public static QuestionsEnabled QuestionsEnabled(Guid? id = null, decimal[] rosterVector = null)
            {
                var identities = new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
                };
                return new QuestionsEnabled(identities);
            }

            public static RosterInstancesAdded RosterInstancesAdded(Guid rosterId, params decimal[][] fullRosterVectors)
            {
                AddedRosterInstance[] instances =
                    fullRosterVectors
                        .Select(fullRosterVector => new AddedRosterInstance(
                            rosterId,
                            outerRosterVector: fullRosterVector.Take(fullRosterVector.Length - 1).ToArray(),
                            rosterInstanceId: fullRosterVector.Last(),
                            sortIndex: null))
                        .ToArray();

                return new RosterInstancesAdded(instances);
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

            public static RosterInstancesTitleChanged RosterInstancesTitleChanged(Guid? rosterId = null, 
                decimal[] rosterVector = null,
                string rosterTitle = null)
            {
                return new RosterInstancesTitleChanged(
                    new[]
                {
                    new ChangedRosterInstanceTitleDto(
                        new RosterInstance(rosterId ?? Guid.NewGuid(), 
                            rosterVector != null ? rosterVector.WithoutLast().ToArray() : new decimal[0], 
                            rosterVector != null ? rosterVector.Last() : 0.0m), 
                        rosterTitle ?? "title")
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

            public static IPublishedEvent<UserLinkedToDevice> UserLinkedToDevice(Guid userId, string deviceId, DateTime eventTimeStamp)
            {
                return new UserLinkedToDevice
                {
                    DeviceId = deviceId
                }.ToPublishedEvent(eventSourceId: userId, eventTimeStamp: eventTimeStamp);
            }

            public static IPublishedEvent<UserUnlockedBySupervisor> UserUnlockedBySupervisor(Guid userId, Guid? eventId = null)
            {
                return new UserUnlockedBySupervisor
                {
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public static IPublishedEvent<UserLockedBySupervisor> UserLockedBySupervisor(Guid userId, Guid? eventId = null)
            {
                return new UserLockedBySupervisor
                {
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public static IPublishedEvent<UserUnlocked> UserUnlocked(Guid userId, Guid? eventId = null)
            {
                return new UserUnlocked
                {
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public static IPublishedEvent<UserLocked> UserLocked(Guid userId, Guid? eventId = null)
            {
                return new UserLocked
                {
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public static IPublishedEvent<UserChanged> UserChanged(Guid userId, string password, string email, Guid? eventId = null)
            {
                return new UserChanged
                {
                    PasswordHash = password,
                    Email = email
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public static IPublishedEvent<NewUserCreated> NewUserCreated(Guid userId, 
                string name, 
                string password, 
                string email,
                bool islockedBySupervisor, 
                bool isLocked,
                Guid? eventId = null)
            {
                return new NewUserCreated
                {
                    Name = name,
                    Password = password,
                    Email = email,
                    IsLockedBySupervisor = islockedBySupervisor,
                    IsLocked = isLocked,
                    Roles = new[] { UserRoles.Operator }
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public static SingleOptionQuestionAnswered SingleOptionQuestionAnswered(Guid questionId, decimal[] rosterVector, decimal answer, Guid? userId = null)
            {
                return new SingleOptionQuestionAnswered(userId ?? Guid.NewGuid(), questionId, rosterVector, DateTime.UtcNow, answer);
            }

            public static IPublishedEvent<InterviewStatusChanged> InterviewStatusChanged(Guid interviewId, 
                InterviewStatus status, 
                string comment = "hello",
                Guid? eventId = null)
            {
                return new InterviewStatusChanged(status, comment)
                    .ToPublishedEvent(eventSourceId: interviewId, eventId: eventId);
            }

            public static IPublishedEvent<InterviewerAssigned> InterviewerAssigned(Guid interviewId, Guid userId, Guid interviewerId)
            {
                return new InterviewerAssigned(userId, interviewerId, DateTime.Now)
                        .ToPublishedEvent(eventSourceId: interviewId);
            }

            public static IPublishedEvent<InterviewHardDeleted> InterviewHardDeleted(Guid interviewId, Guid userId)
            {
                return new InterviewHardDeleted(userId)
                        .ToPublishedEvent(eventSourceId: interviewId);
            }

            public static TextQuestionAnswered TextQuestionAnswered(Guid questionId, decimal[] rosterVector, string answer)
            {
                return new TextQuestionAnswered(Guid.NewGuid(), questionId, rosterVector, DateTime.Now, answer);
            }

            public static AnswersRemoved AnswersRemoved(params Identity[] questions)
            {
                return new AnswersRemoved(questions);
            }

            public static Identity Identity(Guid id, decimal[] rosterVector)
            {
                return new Identity(id, rosterVector);
            }

            public static MultipleOptionsLinkedQuestionAnswered MultipleOptionsLinkedQuestionAnswered(Guid? questionId = null,
                decimal[] rosterVector = null,
                decimal[][] selectedRosterVectors = null)
            {
                return new MultipleOptionsLinkedQuestionAnswered(Guid.NewGuid(), 
                    questionId ?? Guid.NewGuid(),
                    rosterVector ?? new decimal[]{},
                    DateTime.Now, 
                    selectedRosterVectors ?? new decimal[][]{});
            }

            public static InterviewSynchronized InterviewSynchronized(InterviewSynchronizationDto synchronizationDto)
            {
                return new InterviewSynchronized(synchronizationDto);
            }

            public static InterviewReceivedByInterviewer InterviewReceivedByInterviewer()
            {
                return new InterviewReceivedByInterviewer();
            }

            public static InterviewReceivedBySupervisor InterviewReceivedBySupervisor()
            {
                return new InterviewReceivedBySupervisor();
            }

            public static GeoLocationQuestionAnswered GeoLocationQuestionAnswered(Identity question, double latitude, double longitude)
            {
                return new GeoLocationQuestionAnswered(
                    Guid.NewGuid(), question.Id, question.RosterVector, DateTime.UtcNow, latitude, longitude, 1, 1, DateTimeOffset.Now);
            }

            public static InterviewOnClientCreated InterviewOnClientCreated(
                Guid? questionnaireId = null, long? questionnaireVersion = null)
            {
                return new InterviewOnClientCreated(
                    Guid.NewGuid(),
                    questionnaireId ?? Guid.NewGuid(),
                    questionnaireVersion ?? 1);
            }

            public static IPublishedEvent<MacroAdded> MacroAdded(Guid questionnaireId, Guid entityId, Guid? responsibleId = null)
            {
                return new MacroAdded(entityId, responsibleId ?? Guid.NewGuid())
                    .ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public static IPublishedEvent<MacroDeleted> MacroDeleted(Guid questionnaireId, Guid entityId, Guid? responsibleId = null)
            {
                return new MacroDeleted(entityId, responsibleId ?? Guid.NewGuid())
                    .ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public static IPublishedEvent<MacroUpdated> MacroUpdated(Guid questionnaireId, Guid entityId, 
                string name, string content, string description,
                Guid? responsibleId = null)
            {
                return new MacroUpdated(entityId, name, content, description, responsibleId ?? Guid.NewGuid())
                    .ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public static NewQuestionAdded NewQuestionAdded(Guid publicKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
            string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string variableLabel = null, string validationExpression = null, string validationMessage = null,
            QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
            QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
            string mask = null, int? maxAllowedAnswers = null, bool? yesNoView = null, bool? areAnswersOrdered = null)
            {
                return new NewQuestionAdded(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: questionScope,
                    conditionExpression: conditionExpression,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: capital,
                    isInteger: isInteger,
                    questionType: questionType,
                    answerOrder: answerOrder,
                    answers: answers,
                    linkedToQuestionId: linkedToQuestionId,
                    areAnswersOrdered: areAnswersOrdered,
                    yesNoView: yesNoView,
                    maxAllowedAnswers: maxAllowedAnswers,
                    mask: mask,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: cascadeFromQuestionId);
            }


            public static QuestionCloned QuestionCloned(Guid publicKey, Guid sourceQuestionId, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
                string stataExportCaption = null, Guid? linkedToQuestionId = null, string variableLabel = null, bool capital = false, string validationExpression = null, string validationMessage = null,
                QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
                QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
                Guid? sourceQuestionnaireId = null, int targetIndex = 0, int? maxAnswerCount = null, int? countOfDecimalPlaces = null)
            {
                return new QuestionCloned(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: questionScope,
                    conditionExpression: conditionExpression,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: capital,
                    isInteger: isInteger,
                    questionType: questionType,
                    answerOrder: answerOrder,
                    answers: answers,
                    linkedToQuestionId: null,
                    areAnswersOrdered: null,
                    yesNoView: null,
                    maxAllowedAnswers: null,
                    mask: null,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: cascadeFromQuestionId,
                    sourceQuestionnaireId: sourceQuestionnaireId,
                    sourceQuestionId: sourceQuestionId,
                    targetIndex: targetIndex,
                    maxAnswerCount: maxAnswerCount,
                    countOfDecimalPlaces: countOfDecimalPlaces);
            }

            public static QuestionChanged QuestionChanged(Guid publicKey, Guid targetGroupKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
                string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
                QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
                QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null)
            {
                return new QuestionChanged(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: null,
                    featured: featured,
                    questionScope: questionScope,
                    conditionExpression: conditionExpression,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: capital,
                    isInteger: isInteger,
                    questionType: questionType,
                    answerOrder: answerOrder,
                    answers: answers,
                    linkedToQuestionId: null,
                    areAnswersOrdered: null,
                    yesNoView: null,
                    maxAllowedAnswers: null,
                    mask: null,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: cascadeFromQuestionId,
                    targetGroupKey: targetGroupKey);
            }

            public static NumericQuestionAdded NumericQuestionAdded(Guid publicKey, Guid groupPublicKey,
                bool? isInteger = null,
                string stataExportCaption = null,
                string questionText = null,
                string variableLabel = null,
                bool featured = false,
                string conditionExpression = null,
                string validationExpression = null,
                string validationMessage = null,
                string instructions = null,
                Guid? responsibleId = null,
                int? countOfDecimalPlaces = null,
                QuestionScope? questionScope = null)
            {
                return new NumericQuestionAdded(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: questionScope ?? QuestionScope.Interviewer,
                    conditionExpression: conditionExpression,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId ?? Guid.NewGuid(),
                    capital: false,
                    isInteger: isInteger,
                    countOfDecimalPlaces: countOfDecimalPlaces);
            }

            public static NumericQuestionCloned NumericQuestionCloned(Guid publicKey,
                Guid sourceQuestionId,
                Guid groupPublicKey,
                bool? isInteger = null,
                string stataExportCaption = null,
                string questionText = null,
                string variableLabel = null,
                bool featured = false,
                string conditionExpression = null,
                string validationExpression = null,
                string validationMessage = null,
                string instructions = null,
                Guid? responsibleId = null,
                int targetIndex = 0,
                QuestionScope scope = QuestionScope.Interviewer)
            {
                return new NumericQuestionCloned(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: scope,
                    conditionExpression: conditionExpression,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: false,
                    isInteger: isInteger,
                    countOfDecimalPlaces: null,
                    sourceQuestionnaireId: null,
                    sourceQuestionId: sourceQuestionId,
                    targetIndex: targetIndex);
            }

            public static NumericQuestionChanged NumericQuestionChanged(
                Guid publicKey,
                bool? isInteger = null,
                string stataExportCaption = null,
                string questionText = null,
                string variableLabel = null,
                bool featured = false,
                string conditionExpression = null,
                string validationExpression = null,
                string validationMessage = null,
                string instructions = null,
                Guid? responsibleId = null)
            {
                return new NumericQuestionChanged(
                    publicKey: publicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: QuestionScope.Interviewer,
                    conditionExpression: conditionExpression,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: false,
                    isInteger: isInteger,
                    countOfDecimalPlaces: null);
            }

            public static QuestionChanged QuestionChanged(Guid publicKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
                string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
                QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
                QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null)
            {
                return new QuestionChanged(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: null,
                    featured: featured,
                    questionScope: questionScope,
                    conditionExpression: conditionExpression,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: capital,
                    isInteger: isInteger,
                    questionType: questionType,
                    answerOrder: answerOrder,
                    answers: answers,
                    linkedToQuestionId: null,
                    areAnswersOrdered: null,
                    yesNoView: null,
                    maxAllowedAnswers: null,
                    mask: null,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: cascadeFromQuestionId,
                    targetGroupKey: Guid.NewGuid());
            }

            internal static InterviewHardDeleted InterviewHardDeleted()
            {
                return new InterviewHardDeleted(
                    userId: Guid.NewGuid());
            }

            public static InterviewCreated InterviewCreated(Guid? questionnaireId = null, long? questionnaireVersion = null)
            {
                return new InterviewCreated(
                    userId: Guid.NewGuid(),
                    questionnaireId: questionnaireId ?? Guid.NewGuid(),
                    questionnaireVersion: questionnaireVersion ?? 7);
            }

            public static YesNoQuestionAnswered YesNoQuestionAnswered(Guid? questionId = null, AnsweredYesNoOption[] answeredOptions = null)
            {
                return new YesNoQuestionAnswered(
                    userId: Guid.NewGuid(),
                    questionId: questionId ?? Guid.NewGuid(),
                    rosterVector: Core.SharedKernels.DataCollection.RosterVector.Empty,
                    answerTimeUtc: DateTime.UtcNow,
                    answeredOptions: answeredOptions ?? new AnsweredYesNoOption[] {});
            }
        }

        public static Group Chapter(string title = "Chapter X",Guid? chapterId=null, IEnumerable<IComposite> children = null)
        {
            return Create.Group(
                title: title,
                groupId: chapterId,
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

        public static StaticText StaticText(
            Guid? staticTextId = null,
            string text = "Static Text X")
        {
            return new StaticText(staticTextId ?? Guid.NewGuid(), text);
        }

        public static IQuestion Question(
            Guid? questionId = null,
            string variable = "question",
            string enablementCondition = null,
            string validationExpression = null,
            string validationMessage = null,
            QuestionType questionType = QuestionType.Text,
            params Answer[] answers)
        {
            return new TextQuestion("Question X")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionType = questionType,
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Answers = answers.ToList()
            };
        }

        public static Answer Answer(string answer, decimal value, decimal? parentValue = null)
        {
            return new Answer()
            {
                AnswerText = answer,
                AnswerValue = value.ToString(),
                ParentValue = parentValue.HasValue ? parentValue.ToString() : null
            };
        }

        public static MultyOptionsQuestion MultyOptionsQuestion(Guid? id = null, 
            IEnumerable<Answer> answers = null, Guid? linkedToQuestionId = null, string variable = null, bool yesNoView=false)
        {
            return new MultyOptionsQuestion
            {
                QuestionType = QuestionType.MultyOption,
                PublicKey = id ?? Guid.NewGuid(),
                Answers = linkedToQuestionId.HasValue ? null : new List<Answer>(answers ?? new Answer[] { }),
                LinkedToQuestionId = linkedToQuestionId,
                StataExportCaption = variable,
                YesNoView = yesNoView
            };
        }

        public static Group Roster(Guid? rosterId = null, string title = "Roster X", string variable = "roster_var", string enablementCondition = null,
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

        public static Group NumericRoster(Guid? rosterId, string variable, Guid? rosterSizeQuestionId, params IComposite[] children)
        {
            Group group = Create.Group(
                groupId: rosterId,
                title: "Roster X",
                variable: variable,
                children: children);

            group.IsRoster = true;
            group.RosterSizeSource = RosterSizeSourceType.Question;
            group.RosterSizeQuestionId = rosterSizeQuestionId;
            return group;
        }

        public static NumericQuestion NumericIntegerQuestion(Guid? id = null, string variable = "numeric_question", string enablementCondition = null, 
            string validationExpression = null, QuestionScope scope = QuestionScope.Interviewer, bool isPrefilled = false)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = true,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                QuestionScope = scope,
                Featured = isPrefilled
            };
        }

        public static SingleQuestion SingleQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null,
            Guid? cascadeFromQuestionId = null, List<Answer> options = null, Guid? linkedToQuestionId = null, QuestionScope scope = QuestionScope.Interviewer)
        {
            return new SingleQuestion
            {
                QuestionType = QuestionType.SingleOption,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Answers = options ?? new List<Answer>(),
                CascadeFromQuestionId = cascadeFromQuestionId,
                LinkedToQuestionId = linkedToQuestionId,
                QuestionScope = scope
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

        public static InterviewSummary InterviewSummary() // needed since overload cannot be used in lambda expression
        {
            return new InterviewSummary();
        }

        public static InterviewSummary InterviewSummary(
            Guid? interviewId=null,
            Guid? questionnaireId = null, 
            long? questionnaireVersion = null,
            InterviewStatus? status = null,
            Guid? responsibleId = null,
            Guid? teamLeadId = null,
            string responsibleName = null,
            string teamLeadName = null,
            UserRoles role = UserRoles.Operator)
        {
            return new InterviewSummary()
            {
                InterviewId = interviewId ?? Guid.NewGuid(),
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                QuestionnaireVersion = questionnaireVersion ?? 1,
                Status = status.GetValueOrDefault(),
                ResponsibleId = responsibleId.GetValueOrDefault(),
                ResponsibleName = string.IsNullOrWhiteSpace(responsibleName) ? responsibleId.FormatGuid() : responsibleName,
                TeamLeadId = teamLeadId.GetValueOrDefault(),
                TeamLeadName = string.IsNullOrWhiteSpace(teamLeadName) ? teamLeadId.FormatGuid() : teamLeadName,
                ResponsibleRole = role
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
            return ToPublishedEvent(new QuestionCloned(
                publicKey : GetQuestionnaireItemId(questionId),
                groupPublicKey : GetQuestionnaireItemId(parentGroupId),
                stataExportCaption : questionVariable,
                questionText : questionTitle,
                questionType : questionType,
                conditionExpression : questionConditionExpression,
                sourceQuestionId : GetQuestionnaireItemId(sourceQuestionId),
                targetIndex: 0,
                featured: false,
                instructions: null,
                responsibleId: Guid.NewGuid(),
                capital: false,
                questionScope: QuestionScope.Interviewer,
                variableLabel: null,
                validationExpression: null,
                validationMessage:  null,
                answerOrder: null,
                answers: null,
                linkedToQuestionId: null,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                mask:null,
                maxAllowedAnswers: null,
                isFilteredCombobox: null,
                cascadeFromQuestionId: null,
                sourceQuestionnaireId: null,
                maxAnswerCount: null,
                countOfDecimalPlaces: null
            ));
        }

        public static IPublishedEvent<QuestionChanged> QuestionChangedEvent(string questionId, string parentGroupId=null,
            string questionVariable = null, string questionTitle = null, QuestionType? questionType = null, string questionConditionExpression = null)
        {
            return ToPublishedEvent(Event.QuestionChanged(
                publicKey : Guid.Parse(questionId),
                groupPublicKey : Guid.Parse(parentGroupId?? Guid.NewGuid().ToString()),
                stataExportCaption : questionVariable,
                questionText : questionTitle,
                questionType : questionType ?? QuestionType.Text,
                conditionExpression : questionConditionExpression
            ));
        }

        public static IPublishedEvent<QuestionnaireCloned> QuestionnaireClonedEvent(string questionnaireId,
            string chapter1Id = null, string chapter1Title = "", string chapter2Id = null, string chapter2Title = "",
            string questionnaireTitle = null, string chapter1GroupId = null, string chapter1GroupTitle = null,
            string chapter2QuestionId = null, string chapter2QuestionTitle = null,
            string chapter2QuestionVariable = null,
            string chapter2QuestionConditionExpression = null,
            string chapter1StaticTextId = null, string chapter1StaticText = null,
            bool? isPublic = null,
            Guid? clonedFromQuestionnaireId=null)
        {
            var result = ToPublishedEvent(new QuestionnaireCloned()
            {
                QuestionnaireDocument =
                    CreateQuestionnaireDocument(questionnaireId: questionnaireId, questionnaireTitle: questionnaireTitle,
                        chapter1Id: chapter1Id ?? Guid.NewGuid().FormatGuid(), chapter1Title: chapter1Title, chapter2Id: chapter2Id ?? Guid.NewGuid().FormatGuid(),
                        chapter2Title: chapter2Title, chapter1GroupId: chapter1GroupId,
                        chapter1GroupTitle: chapter1GroupTitle, chapter2QuestionId: chapter2QuestionId,
                        chapter2QuestionTitle: chapter2QuestionTitle, chapter2QuestionVariable: chapter2QuestionVariable,
                        chapter2QuestionConditionExpression: chapter2QuestionConditionExpression,
                        chapter1StaticTextId: chapter1StaticTextId, chapter1StaticText: chapter1StaticText,
                        isPublic: isPublic ?? false),
                ClonedFromQuestionnaireId = clonedFromQuestionnaireId?? Guid.NewGuid()
            }, new Guid(questionnaireId));
            return result;
        }

        public static IPublishedEvent<designer::Main.Core.Events.Questionnaire.TemplateImported> TemplateImportedEvent(
            string questionnaireId,
            string chapter1Id = null,
            string chapter1Title = null,
            string chapter2Id = null,
            string chapter2Title = null,
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
                        chapter1Id: chapter1Id ?? Guid.NewGuid().FormatGuid(), chapter1Title: chapter1Title,
                        chapter2Id: chapter2Id ?? Guid.NewGuid().FormatGuid(),
                        chapter2Title: chapter2Title, chapter1GroupId: chapter1GroupId,
                        chapter1GroupTitle: chapter1GroupTitle, chapter2QuestionId: chapter2QuestionId,
                        chapter2QuestionTitle: chapter2QuestionTitle, chapter2QuestionVariable: chapter2QuestionVariable,
                        chapter2QuestionConditionExpression: chapter2QuestionConditionExpression,
                        chapter1StaticTextId: chapter1StaticTextId, chapter1StaticText: chapter1StaticText,
                        isPublic: isPublic ?? false)
            }, new Guid(questionnaireId));
        }

        public static IPublishedEvent<QuestionnaireItemMoved> QuestionnaireItemMovedEvent(string itemId,
            string targetGroupId = null, int? targetIndex = null, string questionnaireId=null)
        {
            return ToPublishedEvent(new QuestionnaireItemMoved()
            {
                PublicKey = Guid.Parse(itemId),
                GroupKey = GetQuestionnaireItemParentId(targetGroupId),
                TargetIndex = targetIndex ?? 0
            }, Guid.Parse(questionnaireId??Guid.NewGuid().ToString()));
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
            string questionVariable = null, string questionTitle = null, string questionConditionExpression = null)
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
            string questionVariable = null, string questionTitle = null, string questionConditionExpression = null)
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
            return ToPublishedEvent(Event.NumericQuestionCloned(
                publicKey : GetQuestionnaireItemId(questionId),
                groupPublicKey : GetQuestionnaireItemId(parentGroupId),
                stataExportCaption : questionVariable,
                questionText : questionTitle,
                conditionExpression : questionConditionExpression,
                sourceQuestionId : GetQuestionnaireItemId(sourceQuestionId),
                targetIndex : 0
            ));
        }

        public static IPublishedEvent<NumericQuestionChanged> NumericQuestionChangedEvent(string questionId,
            string questionVariable = null, string questionTitle = null, string questionConditionExpression = null)
        {
            return ToPublishedEvent(Event.NumericQuestionChanged(
                publicKey : Guid.Parse(questionId),
                stataExportCaption : questionVariable,
                questionText : questionTitle,
                conditionExpression : questionConditionExpression
            ));
        }

        public static IPublishedEvent<NumericQuestionAdded> NumericQuestionAddedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            string questionConditionExpression = null)
        {
            return ToPublishedEvent(Event.NumericQuestionAdded(
                publicKey : GetQuestionnaireItemId(questionId),
                groupPublicKey : GetQuestionnaireItemId(parentGroupId),
                stataExportCaption : questionVariable,
                questionText : questionTitle,
                conditionExpression : questionConditionExpression
            ));
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
            return ToPublishedEvent(Event.NewQuestionAdded(
                publicKey : GetQuestionnaireItemId(questionId),
                groupPublicKey : GetQuestionnaireItemId(parentGroupId),
                questionType : questionType,
                stataExportCaption : questionVariable,
                questionText : questionTitle,
                conditionExpression : questionConditionExpression
            ));
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

        public static IPublishedEvent<RosterChanged> RosterChanged(string groupId)
        {
            return ToPublishedEvent(new RosterChanged(responsibleId: new Guid(), groupId: Guid.Parse(groupId)));
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

        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(params IComposite[] children)
        {
            return QuestionnaireDocumentWithOneChapter(null, children);
        }

        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Guid? chapterId = null, params IComposite[] children)
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

        public static IPublishedEvent<MultimediaQuestionUpdated> MultimediaQuestionUpdatedEvent(string questionId, string questionVariable = null, string questionTitle = null, string questionConditionExpression = null)
        {
            return ToPublishedEvent(new MultimediaQuestionUpdated()
            {
                QuestionId = Guid.Parse(questionId),
                VariableName = questionVariable,
                Title = questionTitle,
                EnablementCondition = questionConditionExpression
            });
        }

        public static Questionnaire Questionnaire(IExpressionProcessor expressionProcessor = null)
        {
            return new Questionnaire(
                new QuestionnaireEntityFactory(),
                Mock.Of<ILogger>(),
                Mock.Of<IClock>(),
                expressionProcessor ?? Mock.Of<IExpressionProcessor>(),
                Create.SubstitutionService(),
                Create.KeywordsProvider());
        }

        public static  CodeGenerator CodeGenerator(
            IMacrosSubstitutionService macrosSubstitutionService = null,
            IExpressionProcessor expressionProcessor = null)
        {
            return new CodeGenerator(
                macrosSubstitutionService ?? Create.DefaultMacrosSubstitutionService(),
                expressionProcessor ?? ServiceLocator.Current.GetInstance<IExpressionProcessor>());
        }

        public static IMacrosSubstitutionService DefaultMacrosSubstitutionService()
        {
            var macrosSubstitutionServiceMock = new Mock<IMacrosSubstitutionService>();
            macrosSubstitutionServiceMock.Setup(
                x => x.InlineMacros(It.IsAny<string>(), It.IsAny<IEnumerable<Macro>>()))
                .Returns((string e, IEnumerable<Macro> macros) =>
                {
                    return e;
                });

            return macrosSubstitutionServiceMock.Object;
        }

        public static KeywordsProvider KeywordsProvider()
        {
            return new KeywordsProvider(Create.SubstitutionService());
        }

        public static EventContext EventContext()
        {
            return new EventContext();
        }

        public static QuestionnaireDocument QuestionnaireDocument(Guid? id = null, params IComposite[] children)
        {
            return new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children?.ToList() ?? new List<IComposite>(),
            };
        }

        public static QuestionnaireDocument QuestionnaireDocument(Guid? id = null, bool usesCSharp = false, IEnumerable<IComposite> children = null)
        {
            return new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children?.ToList() ?? new List<IComposite>(),
                UsesCSharp = usesCSharp,
            };
        }

        public static INumericQuestion NumericQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            bool isInteger = false, int? countOfDecimalPlaces = null, string variableName="var1")
        {
            return new NumericQuestion("Question N")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                IsInteger = isInteger,
                CountOfDecimalPlaces = countOfDecimalPlaces,
                QuestionType = QuestionType.Numeric,
                StataExportCaption = variableName
            };
        }

        public static ITextListQuestion TextListQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            int? maxAnswerCount = null, string variable=null)
        {
            return new TextListQuestion("Question TL")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                MaxAnswerCount = maxAnswerCount,
                QuestionType = QuestionType.TextList,
                StataExportCaption = variable
            };
        }

        public static TextQuestion TextQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string mask = null, string variable = null, string validationMessage = null, string text = null, QuestionScope scope = QuestionScope.Interviewer, bool preFilled=false)
            
        {
            return new TextQuestion("Question T")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Mask = mask,
                QuestionText = text,
                QuestionType = QuestionType.Text,
                StataExportCaption = variable,
                QuestionScope = scope,
                Featured = preFilled
            };
        }

        public static SingleQuestion SingleOptionQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            Guid? linkedToQuestionId = null, Guid? cascadeFromQuestionId = null, decimal[] answerCodes = null)
        {
            return new SingleQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = "single_option_question",
                QuestionText = "SO Question",
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                QuestionType = QuestionType.SingleOption,
                LinkedToQuestionId = linkedToQuestionId,
                CascadeFromQuestionId = cascadeFromQuestionId,
                Answers = (answerCodes ?? new decimal[] { 1, 2, 3 }).Select(a => Create.Answer(a.ToString(), a)).ToList()
            };
        }

        public static IMultyOptionsQuestion MultipleOptionsQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            bool areAnswersOrdered = false, int? maxAllowedAnswers = null, Guid? linkedToQuestionId = null, bool isYesNo = false,
            params decimal[] answers)
        {
            return new MultyOptionsQuestion("Question MO")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = "multiple_options_question",
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                AreAnswersOrdered = areAnswersOrdered,
                MaxAllowedAnswers = maxAllowedAnswers,
                QuestionType = QuestionType.MultyOption,
                LinkedToQuestionId = linkedToQuestionId,
                YesNoView = isYesNo,
                Answers = answers.Select(a => Create.Answer(a.ToString(), a)).ToList()
            };
        }

        public static IMultyOptionsQuestion YesNoQuestion(Guid? questionId = null, decimal[] answers = null)
        {
            return Create.MultipleOptionsQuestion(
                isYesNo: true,
                questionId: questionId,
                answers: answers ?? new decimal[] {});
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
            return new HeadquartersPullContext(Substitute.For<IPlainKeyValueStorage<SynchronizationStatus>>());
        }

        public static HeadquartersPushContext HeadquartersPushContext()
        {
            return new HeadquartersPushContext(Substitute.For<IPlainKeyValueStorage<SynchronizationStatus>>());
        }

        public static InterviewsSynchronizer InterviewsSynchronizer(
            IReadSideRepositoryReader<InterviewSummary> interviewSummaryRepositoryReader = null,
            IQueryableReadSideRepositoryReader<ReadyToSendToHeadquartersInterview> readyToSendInterviewsRepositoryReader = null,
            Func<HttpMessageHandler> httpMessageHandler = null,
            IEventStore eventStore = null,
            ILogger logger = null,
            ISerializer serializer = null,
            ICommandService commandService = null,
            HeadquartersPushContext headquartersPushContext = null,
            IQueryableReadSideRepositoryReader<UserDocument> userDocumentStorage = null, WB.Core.Infrastructure.PlainStorage.IPlainStorageAccessor<LocalInterviewFeedEntry> plainStorage = null,
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
                plainStorage ?? Mock.Of<WB.Core.Infrastructure.PlainStorage.IPlainStorageAccessor<LocalInterviewFeedEntry>>(),
                userDocumentStorage ?? Mock.Of<IQueryableReadSideRepositoryReader<UserDocument>>(),
                plainQuestionnaireRepository ??
                    Mock.Of<IPlainQuestionnaireRepository>(
                        _ => _.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) == new QuestionnaireDocument()),
                headquartersInterviewReader ?? Mock.Of<IHeadquartersInterviewReader>(),
                HeadquartersPullContext(),
                headquartersPushContext ?? HeadquartersPushContext(),
                eventStore ?? Mock.Of<IEventStore>(),
                serializer ?? Mock.Of<ISerializer>(),
                interviewSummaryRepositoryReader ?? Mock.Of<IReadSideRepositoryReader<InterviewSummary>>(),
                readyToSendInterviewsRepositoryReader ?? Stub.ReadSideRepository<ReadyToSendToHeadquartersInterview>(),
                httpMessageHandler ?? Mock.Of<Func<HttpMessageHandler>>(),
                interviewSynchronizationFileStorage ??
                    Mock.Of<IInterviewSynchronizationFileStorage>(
                        _ => _.GetImagesByInterviews() == new List<InterviewBinaryDataDescriptor>()),
                archiver ?? Mock.Of<IArchiveUtils>(),
                Mock.Of<IPlainTransactionManager>(),
                Mock.Of<ITransactionManager>());
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
            Guid? eventIdentifier = null, int eventSequence = 1)
        {
            return new CommittedEvent(
                Guid.Parse("33330000333330000003333300003333"),
                origin,
                eventIdentifier ?? Guid.Parse("44440000444440000004444400004444"),
                eventSourceId ?? Guid.Parse("55550000555550000005555500005555"),
                eventSequence,
                new DateTime(2014, 10, 22),
                0,
                payload ?? "some payload");
        }

        public static Synchronizer Synchronizer(IInterviewsSynchronizer interviewsSynchronizer = null)
        {
            return new Synchronizer(
                Mock.Of<ILocalFeedStorage>(),
                Mock.Of<IUserChangedFeedReader>(),
                Mock.Of<ILocalUserFeedProcessor>(),
                interviewsSynchronizer ?? Mock.Of<IInterviewsSynchronizer>(),
                Mock.Of<IQuestionnaireSynchronizer>(),
                Mock.Of<IPlainTransactionManager>(),
                HeadquartersPullContext(),
                HeadquartersPushContext(),
                Mock.Of<ILogger>());
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

        public static RosterInstancesTitleChanged RosterInstancesTitleChanged(Guid? rosterId = null, 
            string rosterTitle = null,
            decimal[] outerRosterVector = null,
            decimal? instanceId= null)
        {
            return new RosterInstancesTitleChanged(
                new[]
                {
                    new ChangedRosterInstanceTitleDto(new RosterInstance(rosterId ?? Guid.NewGuid(), outerRosterVector ?? new decimal[0], instanceId ?? 0.0m), rosterTitle ?? "title")
                });
        }

        public static IPublishedEvent<InterviewCreated> InterviewCreatedEvent(Guid? interviewId = null, string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion), eventSourceId: interviewId);
        }

        public static IPublishedEvent<TextQuestionAnswered> TextQuestionAnsweredEvent(Guid? interviewId = null, string userId = null)
        {
            return
                ToPublishedEvent(new TextQuestionAnswered(GetGuidIdByStringId(userId), Guid.NewGuid(), new decimal[0],
                    DateTime.Now, "tttt"));
        }


        public static IPublishedEvent<InterviewFromPreloadedDataCreated> InterviewFromPreloadedDataCreatedEvent(Guid? interviewId = null, string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewFromPreloadedDataCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewOnClientCreated> InterviewOnClientCreatedEvent(Guid? interviewId = null, string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewOnClientCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewStatusChanged> InterviewStatusChangedEvent(InterviewStatus status,
            string comment = null,
            Guid? interviewId = null)
        {
            return ToPublishedEvent(new InterviewStatusChanged(status, comment), interviewId ?? Guid.NewGuid());
        }

        public static IPublishedEvent<SupervisorAssigned> SupervisorAssignedEvent(Guid? interviewId = null, string userId = null,
            string supervisorId = null)
        {
            return
                ToPublishedEvent(new SupervisorAssigned(userId: GetGuidIdByStringId(userId),
                    supervisorId: GetGuidIdByStringId(supervisorId)), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewerAssigned> InterviewerAssignedEvent(Guid? interviewId=null, string userId = null,
            string interviewerId = null)
        {
            return
                ToPublishedEvent(new InterviewerAssigned(userId: GetGuidIdByStringId(userId),
                    interviewerId: GetGuidIdByStringId(interviewerId), assignTime: DateTime.Now), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewDeleted> InterviewDeletedEvent(string userId = null, string origin = null, Guid? interviewId = null)
        {
            return ToPublishedEvent(new InterviewDeleted(userId: GetGuidIdByStringId(userId)), origin: origin, eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewHardDeleted> InterviewHardDeletedEvent(string userId = null, Guid? interviewId = null)
        {
            return ToPublishedEvent(new InterviewHardDeleted(userId: GetGuidIdByStringId(userId)), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewRestored> InterviewRestoredEvent(Guid? interviewId = null, string userId = null,
            string origin = null)
        {
            return ToPublishedEvent(new InterviewRestored(userId: GetGuidIdByStringId(userId)), origin: origin, eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewRestarted> InterviewRestartedEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRestarted(userId: GetGuidIdByStringId(userId), restartTime: DateTime.Now, comment: comment), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewCompleted> InterviewCompletedEvent(Guid? interviewId = null, string userId = null, string comment = null, Guid? eventId = null)
        {
            return ToPublishedEvent(new InterviewCompleted(userId: GetGuidIdByStringId(userId), completeTime: DateTime.Now, comment: comment), eventSourceId: interviewId, eventId: eventId);
        }

        public static IPublishedEvent<InterviewRejected> InterviewRejectedEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRejected(userId: GetGuidIdByStringId(userId), comment: comment, rejectTime: DateTime.Now), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewApproved> InterviewApprovedEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewApproved(userId: GetGuidIdByStringId(userId), comment: comment, approveTime: DateTime.Now), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewRejectedByHQ> InterviewRejectedByHQEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRejectedByHQ(userId: GetGuidIdByStringId(userId), comment: comment), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewApprovedByHQ> InterviewApprovedByHQEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewApprovedByHQ(userId: GetGuidIdByStringId(userId), comment: comment), eventSourceId: interviewId);
        }

        public static IPublishedEvent<UnapprovedByHeadquarters> UnapprovedByHeadquartersEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new UnapprovedByHeadquarters(userId: GetGuidIdByStringId(userId), comment: comment), eventSourceId: interviewId);
        }

        public static IPublishedEvent<QuestionnaireDeleted> QuestionaireDeleted(Guid questionnaireId, long version)
        {
            return ToPublishedEvent(new QuestionnaireDeleted{QuestionnaireVersion = version}, eventSourceId: questionnaireId);
        }

        public static IPublishedEvent<SharedPersonToQuestionnaireAdded> SharedPersonToQuestionnaireAdded(Guid questionnaireId, Guid personId)
        {
            return ToPublishedEvent(new SharedPersonToQuestionnaireAdded() { PersonId = personId }, questionnaireId);
        }

        public static IPublishedEvent<SharedPersonFromQuestionnaireRemoved> SharedPersonFromQuestionnaireRemoved(Guid questionnaireId, Guid personId)
        {
            return ToPublishedEvent(new SharedPersonFromQuestionnaireRemoved() { PersonId = personId }, questionnaireId);
        }

        public static IPublishedEvent<Main.Core.Events.Questionnaire.QuestionnaireDeleted> QuestionnaireDeleted(Guid questionnaireId)
        {
            return ToPublishedEvent(new Main.Core.Events.Questionnaire.QuestionnaireDeleted(),eventSourceId: questionnaireId);
        }

        public static IPublishedEvent<QuestionnaireAssemblyImported> QuestionnaireAssemblyImported(Guid questionnaireId, long version)
        {
            return ToPublishedEvent(new QuestionnaireAssemblyImported { Version = version }, eventSourceId: questionnaireId);
        }

      public static IPublishedEvent<SynchronizationMetadataApplied> SynchronizationMetadataAppliedEvent(string userId = null,
            InterviewStatus status = InterviewStatus.Created, string questionnaireId = null,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta = null, bool createdOnClient = false)
        {
            return
                ToPublishedEvent(new SynchronizationMetadataApplied(userId: GetGuidIdByStringId(userId), status: status,
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: 1, featuredQuestionsMeta: featuredQuestionsMeta,
                    createdOnClient: createdOnClient, comments: null, rejectedDateTime: null, interviewerAssignedDateTime: null));
        }

        private static Guid GetGuidIdByStringId(string stringId)
        {
            return string.IsNullOrEmpty(stringId) ? Guid.NewGuid() : Guid.Parse(stringId);
        }

        public static InterviewData InterviewData(bool createdOnClient = false,
            InterviewStatus status = InterviewStatus.Created,
            Guid? interviewId = null, 
            Guid? responsibleId = null)
        {
            var result = new InterviewData
                         {
                             CreatedOnClient = createdOnClient,
                             Status = status,
                             InterviewId = interviewId.GetValueOrDefault(),
                             ResponsibleId = responsibleId.GetValueOrDefault()
                         };
            return result;
        }

        public static EnablementChanges EnablementChanges(
            List<WB.Core.SharedKernels.DataCollection.Identity> groupsToBeDisabled = null, 
            List<WB.Core.SharedKernels.DataCollection.Identity> groupsToBeEnabled = null,
            List<WB.Core.SharedKernels.DataCollection.Identity> questionsToBeDisabled = null, 
            List<WB.Core.SharedKernels.DataCollection.Identity> questionsToBeEnabled = null)
        {
            return new EnablementChanges(
                groupsToBeDisabled ?? new List<WB.Core.SharedKernels.DataCollection.Identity>(),
                groupsToBeEnabled ?? new List<WB.Core.SharedKernels.DataCollection.Identity>(),
                questionsToBeDisabled ?? new List<WB.Core.SharedKernels.DataCollection.Identity>(),
                questionsToBeEnabled ?? new List<WB.Core.SharedKernels.DataCollection.Identity>());
        }

        public static InterviewState InterviewState(InterviewStatus? status = null, List<AnswerComment> answerComments = null, Guid? interviewerId=null)
        {
            return new InterviewState(Guid.NewGuid(), 1, status ?? InterviewStatus.SupervisorAssigned, new Dictionary<string, object>(),
                new Dictionary<string, Tuple<Guid, decimal[], decimal[]>>(), new Dictionary<string, Tuple<Guid, decimal[], decimal[][]>>(),
                new Dictionary<string, Tuple<decimal, string>[]>(), new HashSet<string>(),
                answerComments ?? new List<AnswerComment>(),
                new HashSet<string>(),
                new HashSet<string>(), new Dictionary<string, ConcurrentHashSet<decimal>>(),
                new HashSet<string>(), new HashSet<string>(), true, Mock.Of<IInterviewExpressionStateV2>(), interviewerId?? Guid.NewGuid());
        }

        public static Identity Identity(string id, RosterVector rosterVector)
        {
            return Create.Identity(Guid.Parse(id), rosterVector);
        }

        public static Identity Identity(Guid id, RosterVector rosterVector)
        {
            return new Identity(id, rosterVector);
        }

        public static IQuestionnaireRepository QuestionnaireRepositoryStubWithOneQuestionnaire(
            Guid questionnaireId, IQuestionnaire questionaire = null, long? questionnaireVersion = null)
        {
            questionaire = questionaire ?? Mock.Of<IQuestionnaire>();

            return Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionaire
                && repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion ?? questionaire.Version) == questionaire
                && repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion ?? 1) == questionaire);
        }

        public static IPublishableEvent PublishableEvent(Guid? eventSourceId = null, object payload = null)
        {
            return Mock.Of<IPublishableEvent>(_ => _.Payload == (payload ?? new object()) && _.EventSourceId == (eventSourceId ?? Guid.NewGuid()));
        }

        public static NcqrCompatibleEventDispatcher NcqrCompatibleEventDispatcher(EventBusSettings eventBusSettings = null, ILogger logger = null)
        {
            eventBusSettings = eventBusSettings ?? new EventBusSettings
            {
                EventHandlerTypesWithIgnoredExceptions = new Type[0],
                DisabledEventHandlerTypes = new Type[0]
            };

            var ncqrCompatibleEventDispatcher =
                new NcqrCompatibleEventDispatcher(eventStore: Mock.Of<IEventStore>(), eventBusSettings: eventBusSettings, logger: logger ?? Mock.Of<ILogger>());
              ncqrCompatibleEventDispatcher.TransactionManager = Mock.Of<ITransactionManagerProvider>(x => x.GetTransactionManager() == Mock.Of<ITransactionManager>());
            return ncqrCompatibleEventDispatcher;
        }

        private static InProcessEventBus GetInProcessEventBus(EventBusSettings eventBusSettings, ILogger logger,
            EventHandlerExceptionDelegate eventHandlerExceptionDelegate, IEventStore eventStore)
        {
            var inProcessEventBus = new InProcessEventBus(eventStore, eventBusSettings, logger ?? Mock.Of<ILogger>());
            if (eventHandlerExceptionDelegate != null)
            {
                inProcessEventBus.OnCatchingNonCriticalEventHandlerException += eventHandlerExceptionDelegate;
            }

            return inProcessEventBus;
        }

        public static ImportFromDesigner ImportFromDesignerCommand(Guid responsibleId, string base64StringOfAssembly)
        {
            return new ImportFromDesigner(responsibleId, new QuestionnaireDocument(), false, base64StringOfAssembly);
        }

        public static TransactionManagerProvider TransactionManagerProvider(
            Func<ICqrsPostgresTransactionManager> transactionManagerFactory = null,
            Func<ICqrsPostgresTransactionManager> noTransactionTransactionManagerFactory = null,
            ICqrsPostgresTransactionManager rebuildReadSideTransactionManager = null)
        {
            return new TransactionManagerProvider(
                transactionManagerFactory ?? Mock.Of<ICqrsPostgresTransactionManager>,
                noTransactionTransactionManagerFactory ?? Mock.Of<ICqrsPostgresTransactionManager>,
                rebuildReadSideTransactionManager ?? Mock.Of<ICqrsPostgresTransactionManager>());
        }

        public static RebuildReadSideCqrsPostgresTransactionManager RebuildReadSideCqrsPostgresTransactionManager()
        {
            return new RebuildReadSideCqrsPostgresTransactionManager(Mock.Of<ISessionFactory>());
        }
        public static ILiteEventRegistry LiteEventRegistry()
        {
            return new LiteEventRegistry();
        }

        public static ILiteEventBus LiteEventBus(ILiteEventRegistry liteEventRegistry = null,
            IEventStore eventStore = null)
        {
            var eventReg = liteEventRegistry ?? Mock.Of<ILiteEventRegistry>();
            var eventSt = eventStore ?? Mock.Of<IEventStore>();
            return new LiteEventBus(eventReg, eventSt);
        }

        public static UncommittedEvent UncommittedEvent(object payload)
        {
            return new UncommittedEvent(Guid.NewGuid(), Guid.NewGuid(), 1, 1, DateTime.Now, payload);
        }

        public static DownloadQuestionnaireRequest DownloadQuestionnaireRequest(Guid? questionnaireId, QuestionnaireVersion questionnaireVersion = null)
        {
            return new DownloadQuestionnaireRequest()
            {
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                SupportedVersion = questionnaireVersion ?? new QuestionnaireVersion()
            };
        }

        public static QuestionnaireView QuestionnaireView(Guid? createdBy)
        {
            return new QuestionnaireView(new QuestionnaireDocument() {CreatedBy = createdBy ?? Guid.NewGuid()});
        }

        public static QuestionnaireView QuestionnaireView(QuestionnaireDocument questionnaireDocument)
        {
            return new QuestionnaireView(questionnaireDocument);
        }

        public static GenerationResult GenerationResult(bool success=false)
        {
            return new GenerationResult() {Success = success};
        }

        public static QuestionnaireVerificationError QuestionnaireVerificationError()
        {
            return new QuestionnaireVerificationError("ee", "mm", VerificationErrorLevel.General);
        }

        public static QuestionnaireSharedPersons QuestionnaireSharedPersons(Guid? questionnaireId = null)
        {
            return  new QuestionnaireSharedPersons(questionnaireId ?? Guid.NewGuid());
        }

        public static InterviewExportedDataRecord InterviewExportedDataRecord()
        {
            return new InterviewExportedDataRecord();
        }

        public static InterviewDataExportView InterviewDataExportView(
            Guid? interviewId = null, 
            Guid? questionnaireId = null, 
            long questionnaireVersion = 1, 
            params InterviewDataExportLevelView[] levels)
        {
            return new InterviewDataExportView(interviewId ?? Guid.NewGuid(), questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion, levels);
        }

        public static InterviewDataExportLevelView InterviewDataExportLevelView(Guid interviewId, params InterviewDataExportRecord[] records)
        {
            return new InterviewDataExportLevelView(new ValueVector<Guid>(), "test", records, interviewId.FormatGuid());
        }

        public static InterviewDataExportRecord InterviewDataExportRecord(
            Guid interviewId,
            params ExportedQuestion[] questions)
        {
            return new InterviewDataExportRecord(interviewId, "test", new string[0], new string[0],
                questions, new string [0]);
        }

        public static ExportedQuestion ExportedQuestion()
        {
            return new ExportedQuestion() {Answers = new string[0]};
        }

        public static UserDocument UserDocument(Guid? userId = null, Guid? supervisorId = null, bool? isArchived = null, string userName="name")
        {
            var user = new UserDocument() { PublicKey = userId ?? Guid.NewGuid(), IsArchived = isArchived ?? false, UserName = userName };
            if (supervisorId.HasValue)
            {
                user.Roles.Add(UserRoles.Operator);
                user.Supervisor = new UserLight(supervisorId.Value, "supervisor");
            }
            else
            {
                user.Roles.Add(UserRoles.Supervisor);
            }
            return user;
        }

        public static InterviewStatuses InterviewStatuses(Guid? interviewid=null, Guid? questionnaireId=null, long? questionnaireVersion=null,params InterviewCommentedStatus[] statuses)
        {
            return new InterviewStatuses()
            {
                InterviewId = (interviewid??Guid.NewGuid()).FormatGuid(),
                InterviewCommentedStatuses = statuses.ToList(),
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                QuestionnaireVersion = questionnaireVersion ?? 1
            };
        }

        public static InterviewCommentedStatus InterviewCommentedStatus(
            Guid? statusId = null, 
            Guid? interviewerId = null, 
            Guid? supervisorId = null,
            DateTime? timestamp = null, 
            TimeSpan? timeSpanWithPreviousStatus = null, 
            InterviewExportedAction status = InterviewExportedAction.Completed)
        {
            return new InterviewCommentedStatus()
            {
                Id = statusId ?? Guid.NewGuid(),
                Status = status,
                Timestamp = timestamp ?? DateTime.Now,
                InterviewerId = interviewerId??Guid.NewGuid(),
                SupervisorId = supervisorId??Guid.NewGuid(),
                TimeSpanWithPreviousStatus = timeSpanWithPreviousStatus
            };
        }

        public static QuestionnaireImportService QuestionnaireImportService(IPlainKeyValueStorage<QuestionnaireModel> plainKeyValueStorage = null)
        {
            return new QuestionnaireImportService(plainKeyValueStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                Mock.Of<IPlainQuestionnaireRepository>(),
                Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                Mock.Of<IQuestionnaireModelBuilder>());
        }

        public static ISubstitutionService SubstitutionService()
        {
            return new SubstitutionService();
        }

        public static IAnswerToStringService AnswerToStringService()
        {
            return new AnswerToStringService();
        }

        public static QuestionnaireModel QuestionnaireModel(BaseQuestionModel[] questions = null)
        {
            return new QuestionnaireModel
            {
                Questions = questions != null ? questions.ToDictionary(question => question.Id, question => question) : new Dictionary<Guid, BaseQuestionModel>(),
            };
        }

        public static MultiOptionAnswer MultiOptionAnswer(Guid questionId, decimal[] rosterVector)
        {
            return new MultiOptionAnswer(questionId, rosterVector);
        }

        public static YesNoAnswer YesNoAnswer(Guid questionId, decimal[] rosterVector)
        {
            return new YesNoAnswer(questionId, rosterVector);
        }

        public static NavigationState NavigationState(IStatefulInterviewRepository interviewRepository = null)
        {
            var result = new NavigationState(
                Mock.Of<ICommandService>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IUserInteractionService>(),
                Mock.Of<IUserInterfaceStateService>());
            return result;
        }

        public static TextAnswer TextAnswer(string answer)
        {
            return Create.TextAnswer(answer, null, null);
        }

        public static TextAnswer TextAnswer(string answer, Guid? questionId, decimal[] rosterVector)
        {
            var masedMaskedTextAnswer = new TextAnswer(questionId ?? Guid.NewGuid(), rosterVector ?? Empty.RosterVector);

            if (answer != null)
            {
                masedMaskedTextAnswer.SetAnswer(answer);
            }

            return masedMaskedTextAnswer;
        }

        public static SingleOptionLinkedQuestionViewModel SingleOptionLinkedQuestionViewModel(
            QuestionnaireModel questionnaireModel = null,
            IStatefulInterview interview = null,
            ILiteEventRegistry eventRegistry = null,
            QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null)
        {
            var userIdentity = Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid());
            questionnaireModel = questionnaireModel ?? Mock.Of<QuestionnaireModel>();
            interview = interview ?? Mock.Of<IStatefulInterview>();

            return new SingleOptionLinkedQuestionViewModel(
                Mock.Of<IPrincipal>(_
                    => _.CurrentUserIdentity == userIdentity),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(_
                    => _.GetById(It.IsAny<string>()) == questionnaireModel),
                Mock.Of<IStatefulInterviewRepository>(_
                    => _.Get(It.IsAny<string>()) == interview),
                Create.AnswerToStringService(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                Stub.MvxMainThreadDispatcher(),
                questionState ?? Stub<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>.WithNotEmptyValues,
                answering ?? Mock.Of<AnsweringViewModel>(),
                Mock.Of<AnswerNotifier>());
        }

        public static AnswerNotifier AnswerNotifier()
        {
            return new AnswerNotifier(Create.LiteEventRegistry());
        }

        public static TextQuestionModel TextQuestionModel(Guid? questionId)
        {
            return new TextQuestionModel
            {
                Id  = questionId ?? Guid.NewGuid()
            };
        }

        public static LinkedMultiOptionQuestionModel LinkedMultiOptionQuestionModel(Guid? questionId = null, Guid? linkedToQuestionId =null)
        {
            return new LinkedMultiOptionQuestionModel()
            {
                Id =  questionId ?? Guid.NewGuid(),
                LinkedToQuestionId = linkedToQuestionId ?? Guid.NewGuid()
            };
        }
        public static QuestionnaireStateTracker QuestionnaireStateTacker()
        {
            return new QuestionnaireStateTracker();
        }
        public static AccountDocument AccountDocument(string userName="")
        {
            return new AccountDocument() { UserName = userName };
        }

        public static QuestionnaireChangeRecord QuestionnaireChangeRecord(
            string questionnaireId = null,
            QuestionnaireActionType? action = null, 
            Guid? targetId = null, 
            QuestionnaireItemType? targetType = null,
            params QuestionnaireChangeReference[] reference)
        {
            return new QuestionnaireChangeRecord()
            {
                QuestionnaireId = questionnaireId,
                ActionType = action ?? QuestionnaireActionType.Add,
                TargetItemId = targetId ?? Guid.NewGuid(),
                TargetItemType = targetType ?? QuestionnaireItemType.Group,
                References = reference.ToHashSet()
            };
        }

        public static QuestionnaireChangeReference QuestionnaireChangeReference(
            Guid? referenceId = null,
            QuestionnaireItemType? referenceType = null)
        {
            return new QuestionnaireChangeReference()
            {
                ReferenceId = referenceId ?? Guid.NewGuid(),
                ReferenceType = referenceType ?? QuestionnaireItemType.Group
            };
        }

        public static Interview Interview(Guid? interviewId = null, IQuestionnaireRepository questionnaireRepository = null,
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null)
        {
            var interview = new Interview(
                Mock.Of<ILogger>(),
                questionnaireRepository ?? Mock.Of<IQuestionnaireRepository>(),
                expressionProcessorStatePrototypeProvider ?? Stub.InterviewExpressionStateProvider());

            interview.SetId(interviewId ?? Guid.NewGuid());

            return interview;
        }

        public static StatefulInterview StatefulInterview(Guid? questionnaireId = null, Guid? userId = null,
            IQuestionnaireRepository questionnaireRepository = null)
        {
            questionnaireId = questionnaireId ?? Guid.NewGuid();
            var statefulInterview = new StatefulInterview(
                Mock.Of<ILogger>(),
                questionnaireRepository ?? Mock.Of<IQuestionnaireRepository>(),
                Stub<IInterviewExpressionStatePrototypeProvider>.WithNotEmptyValues)
            {
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId.Value, 1),
            };

            statefulInterview.Apply(new InterviewCreated(userId ?? Guid.NewGuid(), questionnaireId.Value, 1));

            return statefulInterview;
        }

        public static CascadingOptionModel CascadingOptionModel(int value, string title, int parentValue)
        {
            return new CascadingOptionModel
                   {
                       Value = value,
                       Title = title,
                       ParentValue = parentValue
                   };
        }

        public static GroupModel GroupModel(Guid id, string title)
        {
            return new GroupModel
            {
                Id = id,
                Title = title
            };
        }
        public static FileSystemIOAccessor FileSystemIOAccessor()
        {
            return new FileSystemIOAccessor();
        }

        public static UserLight UserLight(Guid? userId=null)
        {
            return new UserLight(userId ?? Guid.NewGuid(), "test");
        }

        public static NewUserCreated NewUserCreated(UserRoles role = UserRoles.Operator, Guid? supervisorId=null)
        {
            return new NewUserCreated() { Roles = new[] { role }, Supervisor = Create.UserLight(supervisorId) };
        }

        public static UserArchived UserArchived()
        {
           return new UserArchived();
        }

        public static ArchiveUserCommad ArchiveUserCommad(Guid userId)
        {
            return new ArchiveUserCommad(userId);
        }

        public static CreateUserCommand CreateUserCommand(UserRoles role = UserRoles.Operator, string userName = "name", Guid? supervisorId=null)
        {
            return new CreateUserCommand(Guid.NewGuid(), userName, "pass", "e@g.com", new[] { role }, false, false, Create.UserLight(supervisorId), "", ""); 
        }

        public static UnarchiveUserCommand UnarchiveUserCommand(Guid userId)
        {
            return new UnarchiveUserCommand(userId);
        }

        public static CreateInterviewCommand CreateInterviewCommand()
        {
            return new CreateInterviewCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, DateTime.Now,
                Guid.NewGuid(), 1);
        }

        public static SynchronizeInterviewEventsCommand SynchronizeInterviewEventsCommand()
        {
            return new SynchronizeInterviewEventsCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1,
                new object[0], InterviewStatus.Completed, true);
        }

        public static User User()
        {
            return new User();
        }

        public static GpsCoordinateQuestion GpsCoordinateQuestion(Guid? questionId = null, string variableName = "var1", bool isPrefilled=false)
        {
            return new GpsCoordinateQuestion()
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = variableName,
                QuestionType = QuestionType.GpsCoordinates,
                Featured = isPrefilled
            };
        }

        public static InterviewData InterviewData(params InterviewQuestion[] topLevelQuestions)
        {
            var interviewData = new InterviewData() { InterviewId = Guid.NewGuid() };
            interviewData.Levels.Add("#", new InterviewLevel(new ValueVector<Guid>(), null, new decimal[0]));
            foreach (var interviewQuestion in topLevelQuestions)
            {
                interviewData.Levels["#"].QuestionsSearchCache.Add(interviewQuestion.Id, interviewQuestion);
            }
            return interviewData;
        }

        public static InterviewQuestion InterviewQuestion(Guid? questionId = null, object answer = null)
        {
            var interviewQuestion = new InterviewQuestion(questionId ?? Guid.NewGuid());
            interviewQuestion.Answer = answer;
            if (answer != null)
            {
                interviewQuestion.QuestionState = interviewQuestion.QuestionState | QuestionState.Answered;
            }
            return interviewQuestion;
        }

        public static GeoPosition GeoPosition()
        {
            return new GeoPosition(1, 2, 3, 4, new DateTimeOffset(new DateTime(1984,4,18)));
        }

        public static PreloadedDataService PreloadedDataService(QuestionnaireDocument questionnaire)
        {
            return new PreloadedDataService(
                    new ExportViewFactory(new ReferenceInfoForLinkedQuestionsFactory(),
                        new QuestionnaireRosterStructureFactory(), new FileSystemIOAccessor())
                        .CreateQuestionnaireExportStructure(questionnaire, 1), new QuestionnaireRosterStructureFactory().CreateQuestionnaireRosterStructure(questionnaire, 1), questionnaire,
                    new QuestionDataParser(),
                    new UserViewFactory(new  TestInMemoryWriter<UserDocument>()),
                    Mock.Of<ITransactionManagerProvider>());

        }

        public static InterviewStatusTimeSpans InterviewStatusTimeSpans(Guid? questionnaireId = null, long? questionnaireVersion = null, string interviewId = null, params TimeSpanBetweenStatuses[] timeSpans)
        {
            return new InterviewStatusTimeSpans()
            {
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                QuestionnaireVersion = questionnaireVersion ?? 1,
                TimeSpansBetweenStatuses = timeSpans.ToHashSet(),
                InterviewId = interviewId
            };
        }

        public static TimeSpanBetweenStatuses TimeSpanBetweenStatuses(Guid? interviewerId = null, Guid? supervisorId = null, DateTime? timestamp = null, TimeSpan? timeSpanWithPreviousStatus = null)
        {
            return new TimeSpanBetweenStatuses()
            {
                BeginStatus = InterviewExportedAction.InterviewerAssigned,
                EndStatus = InterviewExportedAction.ApprovedByHeadquarter,
                EndStatusTimestamp = timestamp ?? DateTime.Now,
                InterviewerId = interviewerId ?? Guid.NewGuid(),
                SupervisorId = supervisorId ?? Guid.NewGuid(),
                TimeSpan = timeSpanWithPreviousStatus?? new TimeSpan()
            };
        }

        public static UserPreloadingProcess UserPreloadingProcess(string userPreloadingProcessId = null,
            UserPrelodingState state = UserPrelodingState.Uploaded, int recordsCount=0, params UserPreloadingDataRecord[] dataRecords)
        {
            var result = new UserPreloadingProcess()
            {
                UserPreloadingProcessId = userPreloadingProcessId ?? Guid.NewGuid().FormatGuid(),
                State = state,
                RecordsCount = recordsCount,
                LastUpdateDate = DateTime.Now
            };
            foreach (var userPreloadingDataRecord in dataRecords)
            {
                result.UserPrelodingData.Add(userPreloadingDataRecord);
            }
            return result;
        }

        public static UserPreloadingDataRecord UserPreloadingDataRecord(string login = "test", string supervisor = "", string password = "test", string email="", string phoneNumber="", string role=null)
        {
            return new UserPreloadingDataRecord()
            {
                Login = login,
                Supervisor = supervisor,
                Role = role??(string.IsNullOrEmpty(supervisor) ? "supervisor" : "interviewer"),
                Password = password,
                Email = email,
                PhoneNumber = phoneNumber
            };
        }

        public static UserPreloadingVerificationError UserPreloadingVerificationError()
        {
            return new UserPreloadingVerificationError();
        }

        public static HybridEventBus HybridEventBus(ILiteEventBus liteEventBus = null, IEventBus cqrsEventBus = null)
        {
            return new HybridEventBus(
                liteEventBus ?? Mock.Of<ILiteEventBus>(),
                cqrsEventBus ?? Mock.Of<IEventBus>());
        }

        public static UserPreloadingSettings UserPreloadingSettings()
        {
            return new UserPreloadingSettings(5, 5, 12, 1, 10000, 100, 100, "^[a-zA-Z0-9_]{3,15}$",
                @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$",
                "^(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z]).*$",
                @"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$");
        }

        public static QuestionnaireIdentity QuestionnaireIdentity()
        {
            return new QuestionnaireIdentity(Guid.NewGuid(), 7);
        }

        public static QuestionnaireModelBuilder QuestionnaireModelBuilder()
        {
            return new QuestionnaireModelBuilder();
        }

        public static InterviewSynchronizationDto InterviewSynchronizationDto(
            Guid? questionnaireId = null,
            long? questionnaireVersion = null,
            Guid? userId = null, 
            AnsweredQuestionSynchronizationDto[] answers = null,
            HashSet<InterviewItemId> disabledGroups = null,
            HashSet<InterviewItemId> disabledQuestions = null,
            HashSet<InterviewItemId> validQuestions = null,
            HashSet<InterviewItemId> invalidQuestions = null,
            InterviewStatus status=InterviewStatus.SupervisorAssigned)
        {
            return new InterviewSynchronizationDto(
                Guid.NewGuid(),
                status,
                "", 
                null,
                null,
                userId ?? Guid.NewGuid(),
                questionnaireId ?? Guid.NewGuid(), 
                questionnaireVersion ?? 1, 
                answers ?? new AnsweredQuestionSynchronizationDto[0],
                disabledGroups ?? new HashSet<InterviewItemId>(),
                disabledQuestions ?? new HashSet<InterviewItemId>(),
                validQuestions ?? new HashSet<InterviewItemId>(),
                invalidQuestions ?? new HashSet<InterviewItemId>(), 
                new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(), 
                false);
        }

        public static QuestionnaireExportStructure QuestionnaireExportStructure(Guid? questionnaireId = null, long? version = null)
        {
            return new QuestionnaireExportStructure
            {
                QuestionnaireId = questionnaireId ?? Guid.Empty,
                Version = version ?? 0
            };
        }
        public static Core.SharedKernels.DataCollection.Implementation.Aggregates.Questionnaire DataCollectionQuestionnaire(
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null)
        {
            return new Core.SharedKernels.DataCollection.Implementation.Aggregates.Questionnaire(
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                Mock.Of<IQuestionnaireAssemblyFileAccessor>());
        }

        public static MultimediaQuestion MultimediaQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string variable = null, string validationMessage = null, string text = null, QuestionScope scope = QuestionScope.Interviewer)
        {
            return new MultimediaQuestion("Question T")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionType = QuestionType.Multimedia,
                StataExportCaption = variable,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                QuestionText = text
            };
        }

        public static HeaderStructureForLevel HeaderStructureForLevel()
        {
            return new HeaderStructureForLevel() {LevelScopeVector = new ValueVector<Guid>()};
        }

        public static InterviewCommentaries InterviewCommentaries(Guid? questionnaireId = null, long? questionnaireVersion = null, params InterviewComment[] comments)
        {
            return new InterviewCommentaries()
            {
                QuestionnaireId = (questionnaireId ?? Guid.NewGuid()).FormatGuid(),
                QuestionnaireVersion = questionnaireVersion ?? 1,
                Commentaries = new List<InterviewComment>(comments)
            };
        }

        public static InterviewComment InterviewComment(string comment=null)
        {
            return new InterviewComment() {Comment = comment};
        }

        public static QuestionnaireDTO QuestionnaireDTO()
        {
            return new QuestionnaireDTO();
        }

        public static DashboardDenormalizer DashboardDenormalizer(
            IReadSideRepositoryWriter<QuestionnaireDTO> questionnaireDtoDocumentStorage = null,
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStorage = null)
        {
            return new DashboardDenormalizer(
                questionnaireDtoDocumentStorage ?? Mock.Of<IReadSideRepositoryWriter<QuestionnaireDTO>>(),
                Mock.Of<IReadSideRepositoryWriter<SurveyDto>>(),
                questionnaireStorage ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(),
                Mock.Of<IPlainQuestionnaireRepository>());
        }

        public static QuestionnaireDocumentVersioned QuestionnaireDocumentVersioned(
            QuestionnaireDocument questionnaireDocument, long? version = null)
        {
            return new QuestionnaireDocumentVersioned
            {
                Questionnaire = questionnaireDocument,
                Version = version ?? 77,
            };
        }

        public static AnsweredQuestionSynchronizationDto AnsweredQuestionSynchronizationDto(
            Guid? questionId = null, decimal[] rosterVector = null, object answer = null)
        {
            return new AnsweredQuestionSynchronizationDto(
                questionId ?? Guid.NewGuid(),
                rosterVector ?? WB.Core.SharedKernels.DataCollection.RosterVector.Empty,
                answer ?? "42",
                "no comment");
        }
        public static QuestionnaireBrowseItem QuestionnaireBrowseItem(Guid? questionnaireId=null)
        {
            return new QuestionnaireBrowseItem()
            {
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                Version = 1
            };
        }

        public static QuestionnaireBrowseItem QuestionnaireBrowseItem(QuestionnaireDocument questionnaire)
        {
            return new QuestionnaireBrowseItem(questionnaire, 1, false, 0);
        }

        public static ExportedHeaderItem ExportedHeaderItem(Guid? questionId=null, string variableName="var")
        {
            return new ExportedHeaderItem()
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ColumnNames = new[] {variableName}
            };
        }

        public static ReadSideSettings ReadSideSettings()
        {
            return new ReadSideSettings(readSideVersion: 0);
        }

        public static Macro Macro(string name, string content = null, string description = null)
        {
            return new Macro
            {
                Name = name,
                Content = content,
                Description = description
            };
        }


        public static AnsweredYesNoOption AnsweredYesNoOption(decimal value, bool answer)
        {
            return new AnsweredYesNoOption(value, answer);
        }

        public static ParaDataExportProcessDetails ParaDataExportProcess()
        {
            return new ParaDataExportProcessDetails(DataExportFormat.Tabular);
        }

        public static AllDataExportProcessDetails AllDataExportProcess(QuestionnaireIdentity? questionnaireIdentity = null)
        {
            return new AllDataExportProcessDetails("all data", DataExportFormat.Tabular, questionnaireIdentity ?? new QuestionnaireIdentity(Guid.NewGuid(), 1));
        }

        public static ApprovedDataExportProcessDetails ApprovedDataExportProcess(QuestionnaireIdentity? questionnaireIdentity = null)
        {
            return new ApprovedDataExportProcessDetails("approved data", DataExportFormat.Tabular, questionnaireIdentity ?? new QuestionnaireIdentity(Guid.NewGuid(), 1));
        }

        public static InterviewBinaryDataDescriptor InterviewBinaryDataDescriptor()
        {
            return new InterviewBinaryDataDescriptor(Guid.NewGuid(), "test.jpeg", () => new byte[0]);
        }

        public static IDesignerEngineVersionService DesignerEngineVersionService()
        {
            return new DesignerEngineVersionService();
        }

        public static YesNoAnswers YesNoAnswers(decimal[] allOptionCodes, YesNoAnswersOnly yesNoAnswersOnly = null)
        {
            return new YesNoAnswers(allOptionCodes: allOptionCodes, yesNoAnswersOnly: yesNoAnswersOnly);
        }

        public static PlainQuestionnaire PlainQuestionnaire(QuestionnaireDocument document = null, long version = 19)
        {
            return new PlainQuestionnaire(
                document: document,
                version: version);
        }

        public static RosterVector RosterVector(params decimal[] coordinates)
        {
            return new RosterVector(coordinates ?? Enumerable.Empty<decimal>());
        }

        public static MacrosSubstitutionService MacrosSubstitutionService()
        {
            return new MacrosSubstitutionService();
        }
    }
}