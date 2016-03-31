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
using MvvmCross.Platform.Core;
using MvvmCross.Plugins.Messenger;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ncqrs.Spec;
using NSubstitute;
using Quartz;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V5.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
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
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
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
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandTransformation;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using WB.UI.Designer.Models;
using WB.UI.Supervisor.Controllers;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;
using Questionnaire = WB.Core.BoundedContexts.Designer.Aggregates.Questionnaire;
using QuestionnaireDeleted = WB.Core.SharedKernels.DataCollection.Events.Questionnaire.QuestionnaireDeleted;
using QuestionnaireVersion = WB.Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireVersion;
using QuestionnaireView = WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireView;
using SynchronizationStatus = WB.Core.BoundedContexts.Supervisor.Synchronization.SynchronizationStatus;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.Synchronization.SyncStorage;
using TemplateImported = designer::Main.Core.Events.Questionnaire.TemplateImported;
using designer::WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V6.Templates;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Portable.CustomCollections;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.SurveyManagement.Commands;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Shared.Web.Membership;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using AttachmentContent = WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire.AttachmentContent;
using AttachmentsController = WB.Core.SharedKernels.SurveyManagement.Web.Controllers.AttachmentsController;

namespace WB.Tests.Unit
{
    internal static partial class Create
    {
        public static InterviewAnswersCommandValidator InterviewAnswersCommandValidator(IInterviewSummaryViewFactory interviewSummaryViewFactory = null)
        {
            return new InterviewAnswersCommandValidator(interviewSummaryViewFactory ?? Mock.Of<IInterviewSummaryViewFactory>());
        }

        public static AccountDocument AccountDocument(string userName="")
        {
            return new AccountDocument() { UserName = userName };
        }

        public static DataExportProcessDetails AllDataExportProcess(QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new DataExportProcessDetails(
                DataExportFormat.Tabular,
                questionnaireIdentity ?? new QuestionnaireIdentity(Guid.NewGuid(), 1),
                "some questionnaire");
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

        public static AnsweredQuestionSynchronizationDto AnsweredQuestionSynchronizationDto(
            Guid? questionId = null, decimal[] rosterVector = null, object answer = null)
        {
            return new AnsweredQuestionSynchronizationDto(
                questionId ?? Guid.NewGuid(),
                rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty,
                answer ?? "42",
                "no comment");
        }

        public static AnsweredYesNoOption AnsweredYesNoOption(decimal value, bool answer)
        {
            return new AnsweredYesNoOption(value, answer);
        }

        public static AnswerNotifier AnswerNotifier()
        {
            return new AnswerNotifier(Create.LiteEventRegistry());
        }

        public static IAnswerToStringService AnswerToStringService()
        {
            return new AnswerToStringService();
        }

        public static ArchiveUserCommad ArchiveUserCommad(Guid userId)
        {
            return new ArchiveUserCommad(userId);
        }

        public static AtomFeedReader AtomFeedReader(Func<HttpMessageHandler> messageHandler = null, IHeadquartersSettings settings = null)
        {
            return new AtomFeedReader(
                messageHandler ?? Mock.Of<Func<HttpMessageHandler>>(),
                settings ?? Mock.Of<IHeadquartersSettings>());
        }

        public static AttachmentContentService AttachmentContentService(IPlainStorageAccessor<AttachmentContent> attachmentContentPlainStorage)
        {
            return new AttachmentContentService(attachmentContentPlainStorage ?? Mock.Of<IPlainStorageAccessor<AttachmentContent>>());
        }

        public static AttachmentsController AttachmentsController(IAttachmentContentService attachmentContentService)
        {
            return new AttachmentsController(attachmentContentService);
        }

        public static AttachmentContent AttachmentContent(string contentHash = null, string contentType = null, byte[] content = null)
        {
            return new AttachmentContent
            {
                ContentHash = contentHash ?? "content id",
                ContentType = contentType ,
                Content = content ?? new byte[] {1, 2, 3}
            };
        }

        public static Attachment Attachment(string attachementHash) => new Attachment { ContentId = attachementHash };

        public static CategoricalQuestionOption CascadingOptionModel(int value, string title, int parentValue)
        {
            return new CategoricalQuestionOption
                   {
                       Value = value,
                       Title = title,
                       ParentValue = parentValue
                   };
        }

        public static Group Chapter(string title = "Chapter X",Guid? chapterId=null, bool hideIfDisabled = false, IEnumerable<IComposite> children = null)
        {
            return Create.Group(
                title: title,
                groupId: chapterId,
                hideIfDisabled: hideIfDisabled,
                children: children);
        }

        public static Group Section(string title = "Section X", Guid? sectionId = null, IEnumerable<IComposite> children = null)
            => Create.Group(
                title: title,
                groupId: sectionId,
                children: children);

        public static CodeGenerationSettings CodeGenerationSettings()
        {
            return new CodeGenerationSettings(additionInterfaces: new[] { "IInterviewExpressionStateV5" },
                namespaces: new[]
                {
                    "WB.Core.SharedKernels.DataCollection.V2",
                    "WB.Core.SharedKernels.DataCollection.V2.CustomFunctions",
                    "WB.Core.SharedKernels.DataCollection.V3.CustomFunctions",
                    "WB.Core.SharedKernels.DataCollection.V4",
                    "WB.Core.SharedKernels.DataCollection.V4.CustomFunctions",
                    "WB.Core.SharedKernels.DataCollection.V5",
                    "WB.Core.SharedKernels.DataCollection.V5.CustomFunctions"
                },
                isLookupTablesFeatureSupported: true)
            {
                ExpressionStateBodyGenerator = expressionStateModel => new InterviewExpressionStateTemplateV5(expressionStateModel).TransformText()
            };
        }

        public static CodeGenerationSettings CodeGenerationSettingsV6()
        {
            return new CodeGenerationSettings(additionInterfaces: new[] { "IInterviewExpressionStateV6" },
                namespaces: new[]
                {
                    "WB.Core.SharedKernels.DataCollection.V2",
                    "WB.Core.SharedKernels.DataCollection.V2.CustomFunctions",
                    "WB.Core.SharedKernels.DataCollection.V3.CustomFunctions",
                    "WB.Core.SharedKernels.DataCollection.V4",
                    "WB.Core.SharedKernels.DataCollection.V4.CustomFunctions",
                    "WB.Core.SharedKernels.DataCollection.V5",
                    "WB.Core.SharedKernels.DataCollection.V5.CustomFunctions",
                    "WB.Core.SharedKernels.DataCollection.V5"
                },
                isLookupTablesFeatureSupported: true)
            {
                ExpressionStateBodyGenerator = expressionStateModel => new InterviewExpressionStateTemplateV6(expressionStateModel).TransformText()
            };
        }

        public static CodeGenerator CodeGenerator(
            IMacrosSubstitutionService macrosSubstitutionService = null,
            IExpressionProcessor expressionProcessor = null,
            ILookupTableService lookupTableService = null)
        {
            return new CodeGenerator(
                macrosSubstitutionService ?? Create.DefaultMacrosSubstitutionService(),
                expressionProcessor ?? ServiceLocator.Current.GetInstance<IExpressionProcessor>(),
                lookupTableService ?? ServiceLocator.Current.GetInstance<ILookupTableService>(),
                Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ICompilerSettings>());
        }

        public static CommittedEvent CommittedEvent(string origin = null, Guid? eventSourceId = null, IEvent payload = null,
            Guid? eventIdentifier = null, int eventSequence = 1, Guid? commitId = null)
        {
            return new CommittedEvent(
                commitId ?? Guid.NewGuid(),
                origin,
                eventIdentifier ?? Guid.Parse("44440000444440000004444400004444"),
                eventSourceId ?? Guid.Parse("55550000555550000005555500005555"),
                eventSequence,
                new DateTime(2014, 10, 22),
                0,
                payload ?? Mock.Of<IEvent>());
        }

        public static CreateInterviewCommand CreateInterviewCommand()
        {
            return new CreateInterviewCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, DateTime.Now,
                Guid.NewGuid(), 1);
        }

        public static AnswerTextQuestionCommand AnswerTextQuestionCommand(Guid interviewId, Guid userId, string answer = "")
        {
            return new AnswerTextQuestionCommand(
                interviewId: interviewId, 
                userId: userId, 
                questionId: Guid.NewGuid(), 
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answer: answer);
        }

        public static AnswerNumericIntegerQuestionCommand AnswerNumericIntegerQuestionCommand(Guid interviewId, Guid userId, int answer = 0)
        {
            return new AnswerNumericIntegerQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answer: answer);
        }

        public static AnswerNumericRealQuestionCommand AnswerNumericRealQuestionCommand(Guid interviewId, Guid userId, decimal answer = 0)
        {
            return new AnswerNumericRealQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answer: answer);
        }

        public static AnswerSingleOptionQuestionCommand AnswerSingleOptionQuestionCommand(Guid interviewId, Guid userId, decimal answer = 0)
        {
            return new AnswerSingleOptionQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                selectedValue: answer);
        }

        public static AnswerSingleOptionLinkedQuestionCommand AnswerSingleOptionLinkedQuestionCommand(Guid interviewId, Guid userId, decimal[] answer = null)
        {
            return new AnswerSingleOptionLinkedQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                selectedRosterVector: answer);
        }

        public static AnswerMultipleOptionsQuestionCommand AnswerMultipleOptionsQuestionCommand(Guid interviewId, Guid userId, decimal[] answer = null)
        {
            return new AnswerMultipleOptionsQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                selectedValues: answer);
        }

        public static AnswerMultipleOptionsLinkedQuestionCommand AnswerMultipleOptionsLinkedQuestionCommand(Guid interviewId, Guid userId, decimal[][] answer = null)
        {
            return new AnswerMultipleOptionsLinkedQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                selectedRosterVectors: answer);
        }

        public static AnswerYesNoQuestion AnswerYesNoQuestion(Guid interviewId, Guid userId, IEnumerable<AnsweredYesNoOption> answer = default(IEnumerable<AnsweredYesNoOption>))
        {
            return new AnswerYesNoQuestion(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answeredOptions: answer);
        }
        
        public static AnswerDateTimeQuestionCommand AnswerDateTimeQuestionCommand(Guid interviewId, Guid userId, DateTime answer = default(DateTime))
        {
            return new AnswerDateTimeQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answer: answer);
        }

        public static AnswerGeoLocationQuestionCommand AnswerGeoLocationQuestionCommand(Guid interviewId, Guid userId, double latitude = 0, 
            double longitude = 0, double accuracy = 0, double altitude = 0, DateTimeOffset timestamp = default(DateTimeOffset))
        {
            return new AnswerGeoLocationQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                longitude: longitude,
                latitude: latitude,
                accuracy: accuracy,
                altitude: altitude,
                timestamp: timestamp);
        }

        public static AnswerPictureQuestionCommand AnswerPictureQuestionCommand(Guid interviewId, Guid userId, string answer = "")
        {
            return new AnswerPictureQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                pictureFileName: answer);
        }

        public static AnswerQRBarcodeQuestionCommand AnswerQRBarcodeQuestionCommand(Guid interviewId, Guid userId, string answer = "")
        {
            return new AnswerQRBarcodeQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answer: answer);
        }

        public static AnswerTextListQuestionCommand AnswerTextListQuestionCommand(Guid interviewId, Guid userId, Tuple<decimal, string>[] answer = null)
        {
            return new AnswerTextListQuestionCommand(
                interviewId: interviewId,
                userId: userId,
                questionId: Guid.NewGuid(),
                rosterVector: new decimal[0],
                answerTime: DateTime.UtcNow,
                answers: answer);
        }

        public static CreateInterviewControllerCommand CreateInterviewControllerCommand()
        {
            return new CreateInterviewControllerCommand()
            {
                AnswersToFeaturedQuestions = new List<UntypedQuestionAnswer>()
            };
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


        public static CreateUserCommand CreateUserCommand(UserRoles role = UserRoles.Operator, string userName = "name", Guid? supervisorId=null)
        {
            return new CreateUserCommand(Guid.NewGuid(), userName, "pass", "e@g.com", new[] { role }, false, false, Create.UserLight(supervisorId), "", ""); 
        }

        public static CumulativeChartDenormalizer CumulativeChartDenormalizer(
            IReadSideKeyValueStorage<LastInterviewStatus> lastStatusesStorage = null,
            IReadSideRepositoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage = null,
            IReadSideKeyValueStorage<InterviewReferences> interviewReferencesStorage = null)
        {
            return new CumulativeChartDenormalizer(
                lastStatusesStorage ?? Mock.Of<IReadSideKeyValueStorage<LastInterviewStatus>>(),
                cumulativeReportStatusChangeStorage ?? Mock.Of<IReadSideRepositoryWriter<CumulativeReportStatusChange>>(),
                interviewReferencesStorage ?? Mock.Of<IReadSideKeyValueStorage<InterviewReferences>>());
        }

        public static InterviewEventHandler DashboardDenormalizer(
            IAsyncPlainStorage<InterviewView> interviewViewRepository = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null)
        {
            return new InterviewEventHandler(
                interviewViewRepository ?? Mock.Of<IAsyncPlainStorage<InterviewView>>(),
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>());
        }

        public static Core.SharedKernels.SurveyManagement.Implementation.Aggregates.Questionnaire DataCollectionQuestionnaire(
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null)
        {
            return new Core.SharedKernels.SurveyManagement.Implementation.Aggregates.Questionnaire(
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                new ReferenceInfoForLinkedQuestionsFactory(),
                new QuestionnaireRosterStructureFactory(),
                Mock.Of<IExportViewFactory>());
        }

        public static DateTimeQuestion DateTimeQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string variable = null, string validationMessage = null, string text = null, QuestionScope scope = QuestionScope.Interviewer, 
            bool preFilled = false, bool hideIfDisabled = false)
        {
            return new DateTimeQuestion("Question DT")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                QuestionText = text,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = variable,
                QuestionScope = scope,
                Featured = preFilled
            };
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

        public static IDesignerEngineVersionService DesignerEngineVersionService()
        {
            return new DesignerEngineVersionService();
        }

        public static DownloadQuestionnaireRequest DownloadQuestionnaireRequest(Guid? questionnaireId, QuestionnaireVersion questionnaireVersion = null)
        {
            return new DownloadQuestionnaireRequest()
            {
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                SupportedVersion = questionnaireVersion ?? new QuestionnaireVersion()
            };
        }

        public static EnablementChanges EnablementChanges(
            List<Identity> groupsToBeDisabled = null, 
            List<Identity> groupsToBeEnabled = null,
            List<Identity> questionsToBeDisabled = null, 
            List<Identity> questionsToBeEnabled = null)
        {
            return new EnablementChanges(
                groupsToBeDisabled ?? new List<Identity>(),
                groupsToBeEnabled ?? new List<Identity>(),
                questionsToBeDisabled ?? new List<Identity>(),
                questionsToBeEnabled ?? new List<Identity>());
        }

        public static EnumerationStageViewModel EnumerationStageViewModel(
            IInterviewViewModelFactory interviewViewModelFactory = null,
            IPlainQuestionnaireRepository questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            ISubstitutionService substitutionService = null,
            ILiteEventRegistry eventRegistry = null,
            IMvxMessenger messenger = null,
            IUserInterfaceStateService userInterfaceStateService = null,
            IMvxMainThreadDispatcher mvxMainThreadDispatcher = null)
            => new EnumerationStageViewModel(
                interviewViewModelFactory ?? Mock.Of<IInterviewViewModelFactory>(),
                questionnaireRepository ?? Stub<IPlainQuestionnaireRepository>.WithNotEmptyValues,
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                substitutionService ?? Mock.Of<ISubstitutionService>(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                userInterfaceStateService ?? Mock.Of<IUserInterfaceStateService>(),
                mvxMainThreadDispatcher ?? Stub.MvxMainThreadDispatcher());

        public static EventContext EventContext()
        {
            return new EventContext();
        }

        public static ExportedHeaderItem ExportedHeaderItem(Guid? questionId=null, string variableName="var")
        {
            return new ExportedHeaderItem()
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ColumnNames = new[] {variableName}
            };
        }

        public static ExportedQuestion ExportedQuestion()
        {
            return new ExportedQuestion() {Answers = new string[0]};
        }

        public static FailedValidationCondition FailedValidationCondition(int? failedConditionIndex = null)
            => new FailedValidationCondition(failedConditionIndex ?? 1117);

        public static FileSystemIOAccessor FileSystemIOAccessor()
            => new FileSystemIOAccessor();

        public static FixedRosterTitle FixedRosterTitle(decimal value, string title)
        {
            return new FixedRosterTitle(value, title);
        }

        public static GenerationResult GenerationResult(bool success=false)
        {
            return new GenerationResult() {Success = success};
        }

        public static GeoPosition GeoPosition()
        {
            return new GeoPosition(1, 2, 3, 4, new DateTimeOffset(new DateTime(1984,4,18)));
        }

        private static Guid GetGuidIdByStringId(string stringId)
        {
            return string.IsNullOrEmpty(stringId) ? Guid.NewGuid() : Guid.Parse(stringId);
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

        public static GpsCoordinateQuestion GpsCoordinateQuestion(Guid? questionId = null, string variable = "var1", bool isPrefilled=false, string title = null,
            string enablementCondition = null, string validationExpression = null, bool hideIfDisabled = false)
        {
            return new GpsCoordinateQuestion()
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = variable,
                QuestionType = QuestionType.GpsCoordinates,
                Featured = isPrefilled,
                QuestionText = title,
                ValidationExpression = validationExpression,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
            };
        }

        public static Group Group(
            Guid? groupId = null,
            string title = "Group X",
            string variable = null,
            string enablementCondition = null,
            bool hideIfDisabled = false,
            IEnumerable<IComposite> children = null)
        {
            return new Group(title)
            {
                PublicKey = groupId ?? Guid.NewGuid(),
                VariableName = variable,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                Children = children != null ? children.ToList() : new List<IComposite>(),
            };
        }

        public static IPublishedEvent<GroupBecameARoster> GroupBecameARosterEvent(string groupId)
        {
            return ToPublishedEvent(new GroupBecameARoster(responsibleId: new Guid(), groupId: Guid.Parse(groupId)));
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

        public static IPublishedEvent<GroupDeleted> GroupDeletedEvent(string groupId)
        {
            return ToPublishedEvent(new GroupDeleted()
            {
                GroupPublicKey = Guid.Parse(groupId)
            });
        }

        public static IPublishedEvent<GroupStoppedBeingARoster> GroupStoppedBeingARosterEvent(string groupId)
        {
            return ToPublishedEvent(new GroupStoppedBeingARoster(responsibleId: new Guid(), groupId: Guid.Parse(groupId)));
        }

        public static IPublishedEvent<GroupUpdated> GroupUpdatedEvent(string groupId, string groupTitle)
        {
            return ToPublishedEvent(new GroupUpdated()
            {
                GroupPublicKey = Guid.Parse(groupId),
                GroupText = groupTitle
            });
        }

        public static HeaderStructureForLevel HeaderStructureForLevel()
        {
            return new HeaderStructureForLevel() {LevelScopeVector = new ValueVector<Guid>()};
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

        public static HeadquartersPullContext HeadquartersPullContext()
        {
            return new HeadquartersPullContext(Substitute.For<IPlainKeyValueStorage<SynchronizationStatus>>());
        }

        public static HeadquartersPushContext HeadquartersPushContext()
        {
            return new HeadquartersPushContext(Substitute.For<IPlainKeyValueStorage<SynchronizationStatus>>());
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

        public static HybridEventBus HybridEventBus(ILiteEventBus liteEventBus = null, IEventBus cqrsEventBus = null)
        {
            return new HybridEventBus(
                liteEventBus ?? Mock.Of<ILiteEventBus>(),
                cqrsEventBus ?? Mock.Of<IEventBus>());
        }

        public static Identity Identity(string id, RosterVector rosterVector)
            => Create.Identity(Guid.Parse(id), rosterVector);

        public static Identity Identity(Guid id, RosterVector rosterVector
            ) => new Identity(id, rosterVector);

        public static Interview Interview(Guid? interviewId = null, IPlainQuestionnaireRepository questionnaireRepository = null,
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null)
        {
            var interview = new Interview(
                Mock.Of<ILogger>(),
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                expressionProcessorStatePrototypeProvider ?? Stub.InterviewExpressionStateProvider());

            interview.SetId(interviewId ?? Guid.NewGuid());

            return interview;
        }

        public static IPublishedEvent<InterviewApprovedByHQ> InterviewApprovedByHQEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewApprovedByHQ(userId: GetGuidIdByStringId(userId), comment: comment), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewApproved> InterviewApprovedEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewApproved(userId: GetGuidIdByStringId(userId), comment: comment, approveTime: DateTime.Now), eventSourceId: interviewId);
        }

        public static InterviewBinaryDataDescriptor InterviewBinaryDataDescriptor()
        {
            return new InterviewBinaryDataDescriptor(Guid.NewGuid(), "test.jpeg", () => new byte[0]);
        }

        public static InterviewComment InterviewComment(string comment=null)
        {
            return new InterviewComment() {Comment = comment};
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

        public static IPublishedEvent<InterviewCompleted> InterviewCompletedEvent(Guid? interviewId = null, string userId = null, string comment = null, Guid? eventId = null)
        {
            return ToPublishedEvent(new InterviewCompleted(userId: GetGuidIdByStringId(userId), completeTime: DateTime.Now, comment: comment), eventSourceId: interviewId, eventId: eventId);
        }

        public static IPublishedEvent<InterviewCreated> InterviewCreatedEvent(Guid? interviewId = null, string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion), eventSourceId: interviewId);
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

        public static InterviewDataExportLevelView InterviewDataExportLevelView(Guid interviewId, params InterviewDataExportRecord[] records)
        {
            return new InterviewDataExportLevelView(new ValueVector<Guid>(), "test", records);
        }

        public static InterviewDataExportRecord InterviewDataExportRecord(
            Guid interviewId,
            params ExportedQuestion[] questions)
        {
            return new InterviewDataExportRecord("test", new string[0], new string[0], new string [0])
            {
                Answers = questions.Select(x => String.Join("\n", x)).ToArray(), 
                LevelName = ""
            };
        }

        public static InterviewDataExportView InterviewDataExportView(
            Guid? interviewId = null, 
            Guid? questionnaireId = null, 
            long questionnaireVersion = 1, 
            params InterviewDataExportLevelView[] levels)
        {
            return new InterviewDataExportView(interviewId ?? Guid.NewGuid(), levels);
        }

        public static IPublishedEvent<InterviewDeleted> InterviewDeletedEvent(string userId = null, string origin = null, Guid? interviewId = null)
        {
            return ToPublishedEvent(new InterviewDeleted(userId: GetGuidIdByStringId(userId)), origin: origin, eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewerAssigned> InterviewerAssignedEvent(Guid? interviewId=null, string userId = null,
            string interviewerId = null)
        {
            return
                ToPublishedEvent(new InterviewerAssigned(userId: GetGuidIdByStringId(userId),
                    interviewerId: GetGuidIdByStringId(interviewerId), assignTime: DateTime.Now), eventSourceId: interviewId);
        }

        public static InterviewExportedDataRecord InterviewExportedDataRecord()
        {
            return new InterviewExportedDataRecord();
        }

        public static IPublishedEvent<InterviewFromPreloadedDataCreated> InterviewFromPreloadedDataCreatedEvent(Guid? interviewId = null, string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewFromPreloadedDataCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewHardDeleted> InterviewHardDeletedEvent(string userId = null, Guid? interviewId = null)
        {
            return ToPublishedEvent(new InterviewHardDeleted(userId: GetGuidIdByStringId(userId)), eventSourceId: interviewId);
        }

        public static InterviewItemId InterviewItemId(Guid id, decimal[] rosterVector = null)
        {
            return new InterviewItemId(id, rosterVector);
        }

        public static IPublishedEvent<InterviewOnClientCreated> InterviewOnClientCreatedEvent(Guid? interviewId = null, string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewOnClientCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion), eventSourceId: interviewId);
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

        public static InterviewReferences InterviewReferences(
            Guid? questionnaireId = null,
            long? questionnaireVersion = null) 
            => new InterviewReferences(
                Guid.NewGuid(),
                questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion ?? 301);

        public static InterviewReferencesDenormalizer InterviewReferencesDenormalizer()
        {
            return new InterviewReferencesDenormalizer(
                Mock.Of<IReadSideKeyValueStorage<InterviewReferences>>());
        }

        public static IPublishedEvent<InterviewRejectedByHQ> InterviewRejectedByHQEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRejectedByHQ(userId: GetGuidIdByStringId(userId), comment: comment), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewRejected> InterviewRejectedEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRejected(userId: GetGuidIdByStringId(userId), comment: comment, rejectTime: DateTime.Now), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewRestarted> InterviewRestartedEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRestarted(userId: GetGuidIdByStringId(userId), restartTime: DateTime.Now, comment: comment), eventSourceId: interviewId);
        }

        public static IPublishedEvent<InterviewRestored> InterviewRestoredEvent(Guid? interviewId = null, string userId = null,
            string origin = null)
        {
            return ToPublishedEvent(new InterviewRestored(userId: GetGuidIdByStringId(userId)), origin: origin, eventSourceId: interviewId);
        }

        public static InterviewsFeedDenormalizer InterviewsFeedDenormalizer(IReadSideRepositoryWriter<InterviewFeedEntry> feedEntryWriter = null,
            IReadSideKeyValueStorage<InterviewData> interviewsRepository = null, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepository = null)
        {
            return new InterviewsFeedDenormalizer(feedEntryWriter ?? Substitute.For<IReadSideRepositoryWriter<InterviewFeedEntry>>(),
                interviewsRepository ?? Substitute.For<IReadSideKeyValueStorage<InterviewData>>(), interviewSummaryRepository ?? Substitute.For<IReadSideRepositoryWriter<InterviewSummary>>());
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
            IQueryableReadSideRepositoryReader<UserDocument> userDocumentStorage = null, IPlainStorageAccessor<LocalInterviewFeedEntry> plainStorage = null,
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
                plainStorage ?? Mock.Of<IPlainStorageAccessor<LocalInterviewFeedEntry>>(),
                userDocumentStorage ?? Mock.Of<IQueryableReadSideRepositoryReader<UserDocument>>(),
                plainQuestionnaireRepository ??
                    Mock.Of<IPlainQuestionnaireRepository>(
                        _ => _.GetQuestionnaireDocument(It.IsAny<Guid>(), It.IsAny<long>()) == new QuestionnaireDocument()),
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

        public static IPublishedEvent<InterviewStatusChanged> InterviewStatusChangedEvent(InterviewStatus status,
            string comment = null,
            Guid? interviewId = null)
        {
            return ToPublishedEvent(new InterviewStatusChanged(status, comment), interviewId ?? Guid.NewGuid());
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

        public static InterviewSynchronizationDto InterviewSynchronizationDto(
            Guid? questionnaireId = null,
            long? questionnaireVersion = null,
            Guid? userId = null, 
            AnsweredQuestionSynchronizationDto[] answers = null,
            HashSet<InterviewItemId> disabledGroups = null,
            HashSet<InterviewItemId> disabledQuestions = null,
            HashSet<InterviewItemId> validQuestions = null,
            HashSet<InterviewItemId> invalidQuestions = null,
            Guid? interviewId = null,
            Dictionary<Identity, IList<FailedValidationCondition>> failedValidationConditions = null,
            InterviewStatus status = InterviewStatus.SupervisorAssigned,
            Dictionary<InterviewItemId, RosterSynchronizationDto[]> rosterGroupInstances = null,
            bool? wasCompleted = false)
        {
            return new InterviewSynchronizationDto(
                interviewId ?? Guid.NewGuid(),
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
                rosterGroupInstances ?? new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(), 
                failedValidationConditions?.ToList() ?? new List<KeyValuePair<Identity, IList<FailedValidationCondition>>>(),
                wasCompleted ?? false);
        }

        public static InterviewView InterviewView(Guid? prefilledQuestionId = null)
        {
            return new InterviewView()
            {
                GpsLocation = new InterviewGpsLocationView
                {
                    PrefilledQuestionId = prefilledQuestionId
                }
            };
        }

        public static KeywordsProvider KeywordsProvider()
        {
            return new KeywordsProvider(Create.SubstitutionService());
        }

        public static LabeledVariable LabeledVariable(string variableName="var", string label="lbl", Guid? questionId=null, params VariableValueLabel[] variableValueLabels)
        {
            return new LabeledVariable(variableName, label, questionId, variableValueLabels);
        }

        public static LastInterviewStatus LastInterviewStatus(InterviewStatus status = InterviewStatus.ApprovedBySupervisor)
            => new LastInterviewStatus("entry-id", status);

        public static LiteEventBus LiteEventBus(ILiteEventRegistry liteEventRegistry = null, IEventStore eventStore = null)
            => new LiteEventBus(
                liteEventRegistry ?? Stub<ILiteEventRegistry>.WithNotEmptyValues,
                eventStore ?? Mock.Of<IEventStore>());

        public static ILiteEventRegistry LiteEventRegistry()
            => new LiteEventRegistry();

        public static LookupTable LookupTable(string tableName, string fileName = null)
        {
            return new LookupTable
            {
                TableName = tableName,
                FileName = fileName ?? "lookup.tab"
            };
        }

        public static LookupTableContent LookupTableContent(string[] variableNames, params LookupTableRow[] rows)
        {
            return new LookupTableContent
            {
                VariableNames = variableNames,
                Rows = rows
            };
        }

        public static LookupTableRow LookupTableRow(long rowcode, decimal?[] values)
        {
            return new LookupTableRow
            {
                RowCode = rowcode,
                Variables = values
            };
        }

        public static LookupTableService LookupTableService(
            IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage = null, 
            IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage = null)
        {
            return new LookupTableService(
                lookupTableContentStorage ?? Mock.Of<IPlainKeyValueStorage<LookupTableContent>>(),
                documentStorage ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>());
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

        public static MacrosSubstitutionService MacrosSubstitutionService()
            => new MacrosSubstitutionService();

        public static MapReportDenormalizer MapReportDenormalizer(
            IReadSideRepositoryWriter<MapReportPoint> mapReportPointStorage = null,
            IReadSideKeyValueStorage<InterviewReferences> interviewReferencesStorage = null,
            QuestionnaireQuestionsInfo questionnaireQuestionsInfo=null,
            QuestionnaireDocument questionnaireDocument=null)
            => new MapReportDenormalizer(
                interviewReferencesStorage ?? new TestInMemoryWriter<InterviewReferences>(),
                mapReportPointStorage ?? new TestInMemoryWriter<MapReportPoint>(),
                Mock.Of<IPlainQuestionnaireRepository>(_=>_.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) == questionnaireDocument),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireQuestionsInfo>>(_=>_.GetById(Moq.It.IsAny<string>())== questionnaireQuestionsInfo));

        public static MultimediaQuestion MultimediaQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string variable = null, string validationMessage = null, string text = null, QuestionScope scope = QuestionScope.Interviewer
            , bool hideIfDisabled = false)
        {
            return new MultimediaQuestion("Question T")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionType = QuestionType.Multimedia,
                StataExportCaption = variable,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                QuestionText = text
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

        public static MultiOptionAnswer MultiOptionAnswer(Guid questionId, decimal[] rosterVector)
        {
            return new MultiOptionAnswer(questionId, rosterVector);
        }

        public static IMultyOptionsQuestion MultipleOptionsQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            bool areAnswersOrdered = false, int? maxAllowedAnswers = null, Guid? linkedToQuestionId = null, bool isYesNo = false, bool hideIfDisabled = false,
            params decimal[] answers)
        {
            return new MultyOptionsQuestion("Question MO")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = "mo_question",
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                AreAnswersOrdered = areAnswersOrdered,
                MaxAllowedAnswers = maxAllowedAnswers,
                QuestionType = QuestionType.MultyOption,
                LinkedToQuestionId = linkedToQuestionId,
                YesNoView = isYesNo,
                Answers = answers.Select(a => Create.Answer(a.ToString(), a)).ToList()
            };
        }

        public static MultyOptionsQuestion MultyOptionsQuestion(Guid? id = null, 
            IEnumerable<Answer> options = null, Guid? linkedToQuestionId = null, string variable = null, bool yesNoView=false,
            string enablementCondition = null, string validationExpression = null, Guid? linkedToRosterId =null)
        {
            return new MultyOptionsQuestion
            {
                QuestionType = QuestionType.MultyOption,
                PublicKey = id ?? Guid.NewGuid(),
                Answers = linkedToQuestionId.HasValue ? null : new List<Answer>(options ?? new Answer[] { }),
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                StataExportCaption = variable,
                YesNoView = yesNoView,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression
            };
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

        public static NewUserCreated NewUserCreated(UserRoles role = UserRoles.Operator, Guid? supervisorId=null)
        {
            return new NewUserCreated() { Roles = new[] { role }, Supervisor = Create.UserLight(supervisorId) };
        }

        public static NumericQuestion NumericIntegerQuestion(Guid? id = null, string variable = "numeric_question", string enablementCondition = null, 
            string validationExpression = null, QuestionScope scope = QuestionScope.Interviewer, bool isPrefilled = false,
            bool hideIfDisabled = false, IEnumerable<ValidationCondition> validationConditions = null, Guid? linkedToRosterId = null)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = true,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                QuestionScope = scope,
                Featured = isPrefilled,
                ValidationConditions = validationConditions?.ToList() ?? new List<ValidationCondition>(),
                LinkedToRosterId = linkedToRosterId,
            };
        }

        public static INumericQuestion NumericQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null, 
            bool isInteger = false, int? countOfDecimalPlaces = null, string variableName = "var1", bool prefilled = false, string title=null)
        {
            return new NumericQuestion("Question N")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                IsInteger = isInteger,
                CountOfDecimalPlaces = countOfDecimalPlaces,
                QuestionType = QuestionType.Numeric,
                StataExportCaption = variableName,
                Featured = prefilled,
                QuestionText = title
            };
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

        public static NumericQuestion NumericRealQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null, IEnumerable<ValidationCondition> validationConditions = null)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = false,
                ConditionExpression = enablementCondition,
                ValidationConditions = validationConditions?.ToList() ?? new List<ValidationCondition>(),
                ValidationExpression = validationExpression
            };
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

        public static Answer Option(string value = null, string text = null, string parentValue = null, Guid? id = null)
        {
            return new Answer
            {
                PublicKey = id ?? Guid.NewGuid(),
                AnswerText = text ?? "text",
                AnswerValue = value ?? "1",
                ParentValue = parentValue
            };
        }

        public static ParaDataExportProcessDetails ParaDataExportProcess()
        {
            return new ParaDataExportProcessDetails(DataExportFormat.Tabular);
        }

        public static PdfGroupView PdfGroupView()
        {
            return new PdfGroupView();
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

        public static PlainQuestionnaire PlainQuestionnaire(QuestionnaireDocument document = null, long version = 19)
        {
            return new PlainQuestionnaire(
                document: document,
                version: version);
        }

        public static PreloadedDataService PreloadedDataService(QuestionnaireDocument questionnaire)
        {
            return new PreloadedDataService(
                    new ExportViewFactory(
                        new QuestionnaireRosterStructureFactory(), new FileSystemIOAccessor())
                        .CreateQuestionnaireExportStructure(questionnaire, 1), new QuestionnaireRosterStructureFactory().CreateQuestionnaireRosterStructure(questionnaire, 1), questionnaire,
                    new QuestionDataParser(),
                    new UserViewFactory(new  TestInMemoryWriter<UserDocument>()),
                    Mock.Of<ITransactionManagerProvider>());

        }

        public static IPublishableEvent PublishableEvent(Guid? eventSourceId = null, IEvent payload = null)
        {
            return Mock.Of<IPublishableEvent>(_ => _.Payload == (payload ?? Mock.Of<IEvent>()) && _.EventSourceId == (eventSourceId ?? Guid.NewGuid()));
        }

        public static QRBarcodeQuestion QRBarcodeQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string variable = null, string validationMessage = null, string text = null, QuestionScope scope = QuestionScope.Interviewer, bool preFilled = false, 
            bool hideIfDisabled = false)
        {
            return new QRBarcodeQuestion()
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                QuestionText = text,
                QuestionType = QuestionType.QRBarcode,
                StataExportCaption = variable,
                QuestionScope = scope,
                Featured = preFilled
            };
        }

        public static IPublishedEvent<QRBarcodeQuestionCloned> QRBarcodeQuestionClonedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            string questionConditionExpression = null, string sourceQuestionId = null,
            IList<ValidationCondition> validationConditions = null)
        {
            return ToPublishedEvent(new QRBarcodeQuestionCloned()
            {
                QuestionId = GetQuestionnaireItemId(questionId),
                ParentGroupId = GetQuestionnaireItemId(parentGroupId),
                VariableName = questionVariable,
                Title = questionTitle,
                EnablementCondition = questionConditionExpression,
                SourceQuestionId = GetQuestionnaireItemId(sourceQuestionId),
                TargetIndex = 0,
                ValidationConditions = validationConditions ?? new List<ValidationCondition>()
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

        public static IQuestion Question(
            Guid? questionId = null,
            string variable = "question",
            string enablementCondition = null,
            string validationExpression = null,
            string validationMessage = null,
            QuestionType questionType = QuestionType.Text,
            IList<ValidationCondition> validationConditions = null,
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
                Answers = answers.ToList(),
                ValidationConditions = validationConditions ?? new List<ValidationCondition>()
            };
        }

        public static IPublishedEvent<QuestionnaireDeleted> QuestionaireDeleted(Guid questionnaireId, long version)
        {
            return ToPublishedEvent(new QuestionnaireDeleted{QuestionnaireVersion = version}, eventSourceId: questionnaireId);
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

        public static IPublishedEvent<QuestionCloned> QuestionClonedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            QuestionType questionType = QuestionType.Text, string questionConditionExpression = null,
            string sourceQuestionId = null,
            IList<ValidationCondition> validationConditions = null, bool hideIfDisabled = false)
        {
            return ToPublishedEvent(new QuestionCloned(
                publicKey : GetQuestionnaireItemId(questionId),
                groupPublicKey : GetQuestionnaireItemId(parentGroupId),
                stataExportCaption : questionVariable,
                questionText : questionTitle,
                questionType : questionType,
                conditionExpression : questionConditionExpression,
                hideIfDisabled: hideIfDisabled,
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
                linkedToRosterId: null,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                mask:null,
                maxAllowedAnswers: null,
                isFilteredCombobox: null,
                cascadeFromQuestionId: null,
                sourceQuestionnaireId: null,
                maxAnswerCount: null,
                countOfDecimalPlaces: null,
                validationConditions: validationConditions,
                linkedFilterExpression: null
            ));
        }

        public static IPublishedEvent<QuestionDeleted> QuestionDeletedEvent(string questionId = null)
        {
            return ToPublishedEvent(new QuestionDeleted(GetQuestionnaireItemId(questionId)));
        }

        public static Questionnaire Questionnaire(IExpressionProcessor expressionProcessor = null)
        {
            return new Questionnaire(
                new QuestionnaireEntityFactory(),
                Mock.Of<ILogger>(),
                Mock.Of<IClock>(),
                expressionProcessor ?? Mock.Of<IExpressionProcessor>(),
                Create.SubstitutionService(),
                Create.KeywordsProvider(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<IAttachmentService>());
        }

        public static IPublishedEvent<QuestionnaireAssemblyImported> QuestionnaireAssemblyImported(Guid questionnaireId, long version)
        {
            return ToPublishedEvent(new QuestionnaireAssemblyImported { Version = version }, eventSourceId: questionnaireId);
        }

        public static QuestionnaireBrowseItem QuestionnaireBrowseItem(Guid? questionnaireId = null, string title = "Questionnaire Browse Item X")
        {
            return new QuestionnaireBrowseItem
            {
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                Version = 1,
                Title = title,
            };
        }

        public static QuestionnaireBrowseItem QuestionnaireBrowseItem(QuestionnaireDocument questionnaire)
        {
            return new QuestionnaireBrowseItem(questionnaire, 1, false,1);
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

        public static IPublishedEvent<Main.Core.Events.Questionnaire.QuestionnaireDeleted> QuestionnaireDeleted(Guid questionnaireId)
        {
            return ToPublishedEvent(new Main.Core.Events.Questionnaire.QuestionnaireDeleted(),eventSourceId: questionnaireId);
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

        public static QuestionnaireDocument QuestionnaireDocumentWithAttachments(Guid? chapterId = null, params Attachment[] attachments)
        {
            var result = new QuestionnaireDocument();
            var chapter = new Group("Chapter") { PublicKey = chapterId.GetValueOrDefault() };

            result.Children.Add(chapter);

            result.Attachments = attachments.ToList();

            return result;
        }

        public static QuestionnaireExpressionStateModelFactory QuestionnaireExecutorTemplateModelFactory(
            IMacrosSubstitutionService macrosSubstitutionService = null,
            IExpressionProcessor expressionProcessor = null,
            ILookupTableService lookupTableService = null)
        {
            return new QuestionnaireExpressionStateModelFactory(
                macrosSubstitutionService ?? Create.DefaultMacrosSubstitutionService(),
                expressionProcessor ?? ServiceLocator.Current.GetInstance<IExpressionProcessor>(),
                lookupTableService ?? ServiceLocator.Current.GetInstance<ILookupTableService>());
        }

        public static QuestionnaireExportStructure QuestionnaireExportStructure(Guid? questionnaireId = null, long? version = null)
        {
            return new QuestionnaireExportStructure
            {
                QuestionnaireId = questionnaireId ?? Guid.Empty,
                Version = version ?? 0
            };
        }

        public static QuestionnaireFeedDenormalizer QuestionnaireFeedDenormalizer(IReadSideRepositoryWriter<QuestionnaireFeedEntry> questionnaireFeedWriter)
        {
            return new QuestionnaireFeedDenormalizer(questionnaireFeedWriter);
        }

        public static QuestionnaireIdentity QuestionnaireIdentity()
        {
            return Create.QuestionnaireIdentity(Guid.NewGuid());
        }

        public static QuestionnaireIdentity QuestionnaireIdentity(Guid questionnaireId)
        {
            return new QuestionnaireIdentity(questionnaireId, 7);
        }

        public static QuestionnaireImportService QuestionnaireImportService(IPlainQuestionnaireRepository plainKeyValueStorage = null)
        {
            return new QuestionnaireImportService(Mock.Of<IPlainQuestionnaireRepository>(),
                Mock.Of<IQuestionnaireAssemblyFileAccessor>());
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

        public static QuestionnaireKeyValueStorage QuestionnaireKeyValueStorage(IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocumentViewRepository = null)
        {
            return new QuestionnaireKeyValueStorage(
                questionnaireDocumentViewRepository ?? Mock.Of<IAsyncPlainStorage<QuestionnaireDocumentView>>());
        }

        public static QuestionnaireLevelLabels QuestionnaireLevelLabels(string levelName="level", params LabeledVariable[] variableLabels)
        {
            return new QuestionnaireLevelLabels(levelName, variableLabels);
        }

        public static QuestionnaireNameValidator QuestionnaireNameValidator(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null)
        {
            return new QuestionnaireNameValidator(
                questionnaireBrowseItemStorage ?? Stub<IPlainStorageAccessor<QuestionnaireBrowseItem>>.WithNotEmptyValues);
        }

        public static IPlainQuestionnaireRepository QuestionnaireRepositoryStubWithOneQuestionnaire(
            Guid questionnaireId, IQuestionnaire questionnaire = null, long? questionnaireVersion = null)
        {
            questionnaire = questionnaire ?? Mock.Of<IQuestionnaire>();

            return Mock.Of<IPlainQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion ?? questionnaire.Version) == questionnaire
                && repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion ?? 1) == questionnaire
                && repository.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire);
        }

        public static QuestionnaireSharedPersons QuestionnaireSharedPersons(Guid? questionnaireId = null)
        {
            return  new QuestionnaireSharedPersons(questionnaireId ?? Guid.NewGuid());
        }

        public static QuestionnaireStateTracker QuestionnaireStateTacker()
        {
            return new QuestionnaireStateTracker();
        }

        public static IPublishedEvent<QuestionnaireUpdated> QuestionnaireUpdatedEvent(string questionnaireId,
            string questionnaireTitle,
            bool isPublic = false)
        {
            return ToPublishedEvent(new QuestionnaireUpdated() { Title = questionnaireTitle, IsPublic = isPublic }, new Guid(questionnaireId));
        }

        public static QuestionnaireView QuestionnaireView(Guid? createdBy)
            => Create.QuestionnaireView(new QuestionnaireDocument { CreatedBy = createdBy ?? Guid.NewGuid( )});

        public static QuestionnaireView QuestionnaireView(QuestionnaireDocument questionnaireDocument)
            => new QuestionnaireView(questionnaireDocument);

        public static ReadSideCacheSettings ReadSideCacheSettings(int cacheSizeInEntities = 128, int storeOperationBulkSize = 8)
            => new ReadSideCacheSettings(true, "folder", cacheSizeInEntities, storeOperationBulkSize);

        public static ReadSideSettings ReadSideSettings()
            => new ReadSideSettings(readSideVersion: 0);

        public static RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions RebuildReadSideCqrsPostgresTransactionManager()
            => new RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions();

        public static RoslynExpressionProcessor RoslynExpressionProcessor() => new RoslynExpressionProcessor();

        public static Group FixedRoster(Guid? rosterId = null, IEnumerable<string> fixedTitles = null, IEnumerable<IComposite> children = null)
            => Create.Roster(
                rosterId: rosterId,
                children: children,
                fixedTitles: fixedTitles?.ToArray() ?? new[] { "Fixed Roster 1", "Fixed Roster 2", "Fixed Roster 3" });

        public static Group Roster(
            Guid? rosterId = null, 
            string title = "Roster X", 
            string variable = "roster_var", 
            string enablementCondition = null,
            string[] fixedTitles = null, 
            IEnumerable<IComposite> children = null,
            RosterSizeSourceType rosterSizeSourceType = RosterSizeSourceType.FixedTitles,
            Guid? rosterSizeQuestionId = null, 
            Guid? rosterTitleQuestionId = null,
            FixedRosterTitle[] fixedRosterTitles = null)
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
            {
                if (fixedRosterTitles == null)
                {
                    group.RosterFixedTitles = fixedTitles ?? new[] { "Roster X-1", "Roster X-2", "Roster X-3" };
                }
                else
                {
                    group.FixedRosterTitles = fixedRosterTitles;
                }
            }

            group.RosterSizeQuestionId = rosterSizeQuestionId;
            group.RosterTitleQuestionId = rosterTitleQuestionId;

            return group;
        }

        public static IPublishedEvent<RosterChanged> RosterChanged(string groupId)
        {
            return ToPublishedEvent(new RosterChanged(responsibleId: new Guid(), groupId: Guid.Parse(groupId)));
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

        public static RosterVector RosterVector(params decimal[] coordinates)
        {
            return new RosterVector(coordinates ?? Enumerable.Empty<decimal>());
        }

        public static IPublishedEvent<SharedPersonFromQuestionnaireRemoved> SharedPersonFromQuestionnaireRemoved(Guid questionnaireId, Guid personId)
        {
            return ToPublishedEvent(new SharedPersonFromQuestionnaireRemoved() { PersonId = personId }, questionnaireId);
        }

        public static IPublishedEvent<SharedPersonToQuestionnaireAdded> SharedPersonToQuestionnaireAdded(Guid questionnaireId, Guid personId)
        {
            return ToPublishedEvent(new SharedPersonToQuestionnaireAdded() { PersonId = personId }, questionnaireId);
        }

        public static SingleOptionLinkedQuestionViewModel SingleOptionLinkedQuestionViewModel(
            IQuestionnaire questionnaire = null,
            IStatefulInterview interview = null,
            ILiteEventRegistry eventRegistry = null,
            QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionState = null,
            AnsweringViewModel answering = null)
        {
            var userIdentity = Mock.Of<IUserIdentity>(y => y.UserId == Guid.NewGuid());
            questionnaire = questionnaire ?? Mock.Of<IQuestionnaire>();
            interview = interview ?? Mock.Of<IStatefulInterview>();

            return new SingleOptionLinkedQuestionViewModel(
                Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity),
                Mock.Of<IPlainQuestionnaireRepository>(_ => _.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>()) == questionnaire),
                Mock.Of<IStatefulInterviewRepository>(_ => _.Get(It.IsAny<string>()) == interview),
                Create.AnswerToStringService(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                Stub.MvxMainThreadDispatcher(),
                questionState ?? Stub<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>.WithNotEmptyValues,
                answering ?? Mock.Of<AnsweringViewModel>());
        }

        public static SingleQuestion SingleOptionQuestion(Guid? questionId = null, string variable = null, string enablementCondition = null, string validationExpression = null,
            Guid? linkedToQuestionId = null, Guid? cascadeFromQuestionId = null, decimal[] answerCodes = null, string title=null, bool hideIfDisabled = false, string linkedFilterExpression=null,
            Guid? linkedToRosterId=null)
        {
            return new SingleQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = variable ?? "single_option_question",
                QuestionText = title??"SO Question",
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                QuestionType = QuestionType.SingleOption,
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                CascadeFromQuestionId = cascadeFromQuestionId,
                Answers = (answerCodes ?? new decimal[] { 1, 2, 3 }).Select(a => Create.Answer(a.ToString(), a)).ToList(),
                LinkedFilterExpression= linkedFilterExpression
            };
        }

        public static SingleQuestion SingleQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null,
            Guid? cascadeFromQuestionId = null, List<Answer> options = null, Guid? linkedToQuestionId = null, QuestionScope scope = QuestionScope.Interviewer, 
            bool isFilteredCombobox = false, Guid? linkedToRosterId = null)
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
                LinkedToRosterId = linkedToRosterId,
                QuestionScope = scope,
                IsFilteredCombobox = isFilteredCombobox
            };
        }

        public static StatefulInterview StatefulInterview(Guid? questionnaireId = null, long? questionnaireVersion = null,
            Guid? userId = null, IPlainQuestionnaireRepository questionnaireRepository = null)
        {
            questionnaireId = questionnaireId ?? Guid.NewGuid();
            var statefulInterview = new StatefulInterview(
                Mock.Of<ILogger>(),
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                Stub<IInterviewExpressionStatePrototypeProvider>.WithNotEmptyValues)
            {
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId.Value, questionnaireVersion ?? 1),
            };

            statefulInterview.Apply(new InterviewCreated(userId ?? Guid.NewGuid(), questionnaireId.Value, questionnaireVersion ?? 1));

            return statefulInterview;
        }

        public static StatefulInterview StatefulInterview(Guid? questionnaireId = null, Guid? userId = null,
    IQuestionnaire questionnaire = null)
        {
            questionnaireId = questionnaireId ?? Guid.NewGuid();
            var statefulInterview = new StatefulInterview(
                Mock.Of<ILogger>(),
                Mock.Of<IPlainQuestionnaireRepository>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire),
                Stub<IInterviewExpressionStatePrototypeProvider>.WithNotEmptyValues)
            {
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId.Value, 1),
            };

            statefulInterview.Apply(new InterviewCreated(userId ?? Guid.NewGuid(), questionnaireId.Value, 1));

            return statefulInterview;
        }

        public static IStatefulInterviewRepository StatefulInterviewRepository(IAggregateRootRepository aggregateRootRepository, ILiteEventBus liteEventBus = null)
        {
            return new StatefulInterviewRepository(
                aggregateRootRepository: aggregateRootRepository ?? Mock.Of<IAggregateRootRepository>(),
                eventBus: liteEventBus ?? Mock.Of<ILiteEventBus>());
        }

        public static StaticText StaticText(
            Guid? staticTextId = null,
            string text = "Static Text X",
            string attachmentName = null)
        {
            return new StaticText(staticTextId ?? Guid.NewGuid(), text, attachmentName);
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

        public static IPublishedEvent<StaticTextUpdated> StaticTextUpdatedEvent(string entityId = null, string text = null)
        {
            return ToPublishedEvent(new StaticTextUpdated()
            {
                EntityId = GetQuestionnaireItemId(entityId),
                Text = text
            });
        }

        public static ISubstitutionService SubstitutionService()
        {
            return new SubstitutionService();
        }

        public static IPublishedEvent<SupervisorAssigned> SupervisorAssignedEvent(Guid? interviewId = null, string userId = null,
            string supervisorId = null)
        {
            return
                ToPublishedEvent(new SupervisorAssigned(userId: GetGuidIdByStringId(userId),
                    supervisorId: GetGuidIdByStringId(supervisorId)), eventSourceId: interviewId);
        }

        public static IAsyncExecutor SyncAsyncExecutor()
        {
            return new SyncAsyncExecutorStub();
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

        public static SynchronizeInterviewEventsCommand SynchronizeInterviewEventsCommand()
        {
            return new SynchronizeInterviewEventsCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1,
                new IEvent[0], InterviewStatus.Completed, true);
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

        public static IPublishedEvent<TemplateImported> TemplateImportedEvent(
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
            return ToPublishedEvent(new TemplateImported()
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

        public static ITextListQuestion TextListQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            int? maxAnswerCount = null, string variable=null, bool hideIfDisabled = false)
        {
            return new TextListQuestion("Question TL")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                MaxAnswerCount = maxAnswerCount,
                QuestionType = QuestionType.TextList,
                StataExportCaption = variable
            };
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

        public static TextQuestion TextQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string mask = null, 
            string variable = "text_question", 
            string validationMessage = null, 
            string text = "Question T", 
            QuestionScope scope = QuestionScope.Interviewer, 
            bool preFilled=false,
            string label=null,
            string instruction=null,
            IEnumerable<ValidationCondition> validationConditions = null, 
            bool hideIfDisabled = false)
            
        {
            return new TextQuestion(text)
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Mask = mask,
                QuestionText = text,
                QuestionType = QuestionType.Text,
                StataExportCaption = variable,
                QuestionScope = scope,
                Featured = preFilled,
                VariableLabel = label,
                Instructions = instruction,
                ValidationConditions = validationConditions?.ToList().ConcatWithOldConditionIfNotEmpty(validationExpression, validationMessage)
            };
        }

        public static IPublishedEvent<TextQuestionAnswered> TextQuestionAnsweredEvent(Guid? interviewId = null, string userId = null)
        {
            return
                ToPublishedEvent(new TextQuestionAnswered(GetGuidIdByStringId(userId), Guid.NewGuid(), new decimal[0],
                    DateTime.Now, "tttt"));
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

        public static IPublishedEvent<T> ToPublishedEvent<T>(this T @event, 
            Guid? eventSourceId = null,
            string origin = null,
            DateTime? eventTimeStamp = null,
            Guid? eventId = null)
            where T : class, IEvent
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

        public static TransactionManagerProvider TransactionManagerProvider(
            Func<ICqrsPostgresTransactionManager> transactionManagerFactory = null,
            Func<ICqrsPostgresTransactionManager> noTransactionTransactionManagerFactory = null,
            ICqrsPostgresTransactionManager rebuildReadSideTransactionManager = null)
        {
            return new TransactionManagerProvider(
                transactionManagerFactory ?? (() => Mock.Of<ICqrsPostgresTransactionManager>()),
                noTransactionTransactionManagerFactory ?? (() => Mock.Of<ICqrsPostgresTransactionManager>()),
                rebuildReadSideTransactionManager ?? Mock.Of<ICqrsPostgresTransactionManager>(),
                rebuildReadSideTransactionManager ?? Mock.Of<ICqrsPostgresTransactionManager>(),
                Create.ReadSideCacheSettings());
        }

        public static IPublishedEvent<UnapprovedByHeadquarters> UnapprovedByHeadquartersEvent(Guid? interviewId = null, string userId = null, string comment = null)
        {
            return ToPublishedEvent(new UnapprovedByHeadquarters(userId: GetGuidIdByStringId(userId), comment: comment), eventSourceId: interviewId);
        }

        public static UnarchiveUserCommand UnarchiveUserCommand(Guid userId)
        {
            return new UnarchiveUserCommand(userId);
        }

        public static UncommittedEvent UncommittedEvent(IEvent payload)
        {
            return new UncommittedEvent(Guid.NewGuid(), Guid.NewGuid(), 1, 1, DateTime.Now, payload);
        }

        public static User User()
        {
            return new User();
        }

        public static UserArchived UserArchived()
        {
           return new UserArchived();
        }

        public static UserChangedFeedReader UserChangedFeedReader(IHeadquartersSettings settings = null,
            Func<HttpMessageHandler> messageHandler = null)
        {
            return new UserChangedFeedReader(settings ?? HeadquartersSettings(),
                messageHandler ?? Substitute.For<Func<HttpMessageHandler>>(), HeadquartersPullContext());
        }

        public static UserDocument UserDocument(Guid? userId = null, Guid? supervisorId = null, bool? isArchived = null, string userName="name", bool isLockedByHQ = false)
        {
            var user = new UserDocument() { PublicKey = userId ?? Guid.NewGuid(), IsArchived = isArchived ?? false, UserName = userName, IsLockedByHQ = isLockedByHQ };
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

        public static UserLight UserLight(Guid? userId = null)
        {
            return new UserLight(userId ?? Guid.NewGuid(), "test");
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

        public static UserPreloadingSettings UserPreloadingSettings()
        {
            return new UserPreloadingSettings(5, 5, 12, 1, 10000, 100, 100, "^[a-zA-Z0-9_]{3,15}$",
                @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$",
                "^(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z]).*$",
                @"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$");
        }

        public static UserPreloadingVerificationError UserPreloadingVerificationError()
        {
            return new UserPreloadingVerificationError();
        }

        public static VariableValueLabel VariableValueLabel(string value="1", string label="l1")
        {
            return new VariableValueLabel(value, label);
        }

        public static QuestionnaireVerificationMessage VerificationError(string code, string message, params QuestionnaireVerificationReference[] questionnaireVerificationReferences)
        {
            return QuestionnaireVerificationMessage.Error(code, message, questionnaireVerificationReferences);
        }

        public static QuestionnaireVerificationMessage VerificationWarning(string code, string message, params QuestionnaireVerificationReference[] questionnaireVerificationReferences)
        {
            return QuestionnaireVerificationMessage.Warning(code, message, questionnaireVerificationReferences);
        }

        public static VerificationMessage VerificationMessage(string code, string message, params VerificationReferenceEnriched[] references)
        {
            return new VerificationMessage
            {
                Code = code,
                Message = message,
                References = references.ToList()
            };
        }

        public static QuestionnaireVerificationReference VerificationReference(Guid? id = null, QuestionnaireVerificationReferenceType type = QuestionnaireVerificationReferenceType.Question)
        {
            return new QuestionnaireVerificationReference(type, id ?? Guid.NewGuid());
        }

        public static VerificationReferenceEnriched VerificationReferenceEnriched(QuestionnaireVerificationReferenceType type, Guid id, string title)
        {
            return new VerificationReferenceEnriched
            {
                Type = type,
                ItemId = id.FormatGuid(),
                Title = title
            };
        }

        public static YesNoAnswer YesNoAnswer(Guid questionId, decimal[] rosterVector)
        {
            return new YesNoAnswer(questionId, rosterVector);
        }

        public static YesNoAnswers YesNoAnswers(decimal[] allOptionCodes, YesNoAnswersOnly yesNoAnswersOnly = null)
        {
            return new YesNoAnswers(allOptionCodes: allOptionCodes, yesNoAnswersOnly: yesNoAnswersOnly);
        }

        public static IMultyOptionsQuestion YesNoQuestion(Guid? questionId = null, decimal[] answers = null)
        {
            return Create.MultipleOptionsQuestion(
                isYesNo: true,
                questionId: questionId,
                answers: answers ?? new decimal[] {});
        }

        internal static class Command
        {
            public static AddLookupTable AddLookupTable(Guid questionnaireId, Guid lookupTableId, Guid responsibleId, string lookupTableName = "table")
            {
                return new AddLookupTable(questionnaireId, lookupTableName, null, lookupTableId, responsibleId);
            }

            public static AddMacro AddMacro(Guid questionnaire, Guid? macroId = null, Guid? userId = null)
            {
                return new AddMacro(questionnaire, macroId ?? Guid.NewGuid(), userId ?? Guid.NewGuid());
            }

            public static AnswerYesNoQuestion AnswerYesNoQuestion(Guid? userId = null,
                Guid? questionId = null, RosterVector rosterVector = null, AnsweredYesNoOption[] answeredOptions = null,
                DateTime? answerTime = null)
            {
                return new AnswerYesNoQuestion(
                    interviewId: Guid.NewGuid(),
                    userId: userId ?? Guid.NewGuid(),
                    questionId: questionId ?? Guid.NewGuid(),
                    rosterVector: rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty,
                    answerTime: answerTime ?? DateTime.UtcNow,
                    answeredOptions: answeredOptions ?? new AnsweredYesNoOption[] {});
            }

            public static DeleteLookupTable DeleteLookupTable(Guid questionnaireId, Guid lookupTableId, Guid responsibleId)
            {
                return new DeleteLookupTable(questionnaireId, lookupTableId, responsibleId);
            }

            public static DeleteMacro DeleteMacro(Guid questionnaire, Guid? macroId = null, Guid? userId = null)
            {
                return new DeleteMacro(questionnaire, macroId ?? Guid.NewGuid(), userId ?? Guid.NewGuid());
            }

            public static ImportFromDesigner ImportFromDesigner(Guid? questionnaireId = null, string title = "Questionnaire X",
                Guid? responsibleId = null, string base64StringOfAssembly = "<base64>assembly</base64> :)",
                long questionnaireContentVersion = 1)
            {
                return new ImportFromDesigner(
                    responsibleId ?? Guid.NewGuid(),
                    new QuestionnaireDocument
                    {
                        PublicKey = questionnaireId ?? Guid.NewGuid(),
                        Title = title,
                    },
                    false,
                    base64StringOfAssembly,
                    questionnaireContentVersion);
            }

            public static LinkUserToDevice LinkUserToDeviceCommand(Guid userId, string deviceId)
            {
                return new LinkUserToDevice(userId, deviceId);
            }

            public static UpdateLookupTable UpdateLookupTable(Guid questionnaireId, Guid lookupTableId, Guid responsibleId, string lookupTableName = "table")
            {
                return new UpdateLookupTable(questionnaireId, lookupTableId, responsibleId, lookupTableName,"file");
            }

            internal static UpdateMacro UpdateMacro(Guid questionnaireId, Guid macroId, string name, string content, string description, Guid? userId)
            {
                return new UpdateMacro(questionnaireId, macroId, name, content, description, userId ?? Guid.NewGuid());
            }

            public static UpdateStaticText UpdateStaticText(Guid questionnaireId, Guid entityId, string text, string attachmentName, Guid responsibleId)
            {
                return new UpdateStaticText(questionnaireId, entityId, text, attachmentName, responsibleId);
            }
        }

        private class SyncAsyncExecutorStub : IAsyncExecutor
        {
            public void ExecuteAsync(Action action)
            {
                action.Invoke();
            }
        }

        public static TeamViewFactory TeamViewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader = null,
            IQueryableReadSideRepositoryReader<UserDocument> usersReader = null)
        {
            return new TeamViewFactory(interviewSummaryReader, usersReader);
        }

        public static CommentedStatusHistroyView CommentedStatusHistroyView(InterviewStatus status=InterviewStatus.InterviewerAssigned, string comment=null, DateTime? timestamp=null)
        {
            return new CommentedStatusHistroyView()
            {
                Status = status,
                Comment = comment,
                Date = timestamp ?? DateTime.Now
            };
        }

        public static InterviewRoster InterviewRoster(Guid? rosterId=null, decimal[] rosterVector=null, string rosterTitle="titile")
        {
            return new InterviewRoster()
            {
                Id = rosterId ?? Guid.NewGuid(),
                IsDisabled = false,
                RosterVector = rosterVector ?? new decimal[0],
                Title = rosterTitle
            };
        }

        public static ValidationCondition ValidationCondition(string expression = "self != null", string message = "should be answered")
        {
            return new ValidationCondition(expression, message);
        }

        public static CommandPostprocessor CommandPostprocessor(
            IMembershipUserService membershipUserService, 
            IRecipientNotifier recipientNotifier, 
            IAccountRepository accountRepository, 
            IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage, 
            ILogger logger,
            IAttachmentService attachmentService = null,
            ILookupTableService lookupTableService = null)
        {
             return new CommandPostprocessor(
                membershipUserService,
                recipientNotifier,
                accountRepository,
                documentStorage,
                logger,
                attachmentService ?? Mock.Of<IAttachmentService>(),
                lookupTableService ?? Mock.Of<ILookupTableService>());
        }

        public static InterviewEventStreamOptimizer InterviewEventStreamOptimizer()
            => new InterviewEventStreamOptimizer();

        public static InterviewLinkedQuestionOptions InterviewLinkedQuestionOptions(params ChangedLinkedOptions[] options)
        {
            var result = new InterviewLinkedQuestionOptions();

            foreach (var changedLinkedQuestion in options)
            {
                result.LinkedQuestionOptions[changedLinkedQuestion.QuestionId.ToString()] = changedLinkedQuestion.Options;
            }

            return result;
        }

        public static ChangedLinkedOptions ChangedLinkedOptions(Guid questionId,decimal[] questionRosterVector=null, RosterVector[] options=null)
        {
            return new ChangedLinkedOptions(new Identity(questionId, questionRosterVector ?? new decimal[0]),
                options ?? new RosterVector[0]);
        }
        
        public static AttachmentContentMetadata AttachmentContentMetadata(string contentType)
        {
            return new AttachmentContentMetadata()
            {
                ContentType = contentType,
            };
        }
        
        public static AttachmentContentData AttachmentContentData(byte[] content)
        {
            return new AttachmentContentData()
            {
                Content = content,
            };
        }

        public static AttachmentViewModel AttachmentViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAttachmentContentStorage attachmentContentStorage)
        {
            return new AttachmentViewModel(
                questionnaireRepository: questionnaireRepository,
                interviewRepository: interviewRepository,
                attachmentContentStorage: attachmentContentStorage);
        }

        public static WB.Core.SharedKernels.Enumerator.Views.AttachmentContent Enumerator_AttachmentContent(string id)
        {
            return new Core.SharedKernels.Enumerator.Views.AttachmentContent()
            {
                Id = id,

            };
        }

        public static IAggregateRootRepository AggregateRootRepository(IEventStore eventStore = null, ISnapshotStore snapshotStore = null, IDomainRepository repository = null)
        {
            return new AggregateRootRepository(eventStore, snapshotStore, repository);
        }

        public static ISnapshotStore SnapshotStore(Guid aggregateRootId, Snapshot snapshot = null)
        {
            return Mock.Of<ISnapshotStore>(_ => _.GetSnapshot(aggregateRootId, Moq.It.IsAny<int>()) == snapshot);
        }

        public static IEventStore EventStore(Guid eventSourceId, IEnumerable<CommittedEvent> committedEvents)
        {
            return Mock.Of<IEventStore>(_ =>
                _.ReadFrom(eventSourceId, Moq.It.IsAny<int>(), Moq.It.IsAny<int>()) == new CommittedEventStream(eventSourceId, committedEvents));
        }

        public static IDomainRepository DomainRepository(IAggregateSnapshotter aggregateSnapshotter = null, IServiceLocator serviceLocator = null)
        {
            return new DomainRepository(
                aggregateSnapshotter: aggregateSnapshotter ?? Mock.Of<IAggregateSnapshotter>(),
                serviceLocator: serviceLocator ?? Mock.Of<IServiceLocator>());
        }

        public static IAggregateSnapshotter AggregateSnapshotter(AggregateRoot aggregateRoot = null, bool isARLoadedFromSnapshotSuccessfully = false)
        {
            return Mock.Of<IAggregateSnapshotter>(_ =>
                _.TryLoadFromSnapshot(Moq.It.IsAny<Type>(), Moq.It.IsAny<Snapshot>(),
                    Moq.It.IsAny<CommittedEventStream>(), out aggregateRoot) ==
                isARLoadedFromSnapshotSuccessfully);
        }

        public static IStatefulInterviewRepository StatefulInterviewRepositoryWith(IStatefulInterview interview)
        {
            var result = Substitute.For<IStatefulInterviewRepository>();
            result.Get(null).ReturnsForAnyArgs(interview);
            return result;
        }
    }
}