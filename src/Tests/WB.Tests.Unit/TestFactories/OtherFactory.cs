extern alias designer;

using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.User;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using System.Collections.Generic;
using System.Globalization;
using MvvmCross.Platform.Core;
using MvvmCross.Plugins.Messenger;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ncqrs.Spec;
using NSubstitute;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V5.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Storage;
using WB.UI.Designer.Models;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;
using Questionnaire = WB.Core.BoundedContexts.Designer.Aggregates.Questionnaire;
using QuestionnaireVersion = WB.Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireVersion;
using QuestionnaireView = WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireView;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using designer::WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V6.Templates;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Core.GenericSubdomains.Portable.CustomCollections;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Hybrid.Implementation;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using AttachmentContent = WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire.AttachmentContent;
using AttachmentsController = WB.Core.SharedKernels.SurveyManagement.Web.Controllers.AttachmentsController;

namespace WB.Tests.Unit.TestFactories
{
    internal class OtherFactory
    {
        public DataExportProcessDetails AllDataExportProcess(QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new DataExportProcessDetails(
                DataExportFormat.Tabular,
                questionnaireIdentity ?? new QuestionnaireIdentity(Guid.NewGuid(), 1),
                "some questionnaire");
        }

        public Answer Answer(string answer, decimal value, decimal? parentValue = null)
        {
            return new Answer()
            {
                AnswerText = answer,
                AnswerValue = value.ToString(),
                ParentValue = parentValue.HasValue ? parentValue.ToString() : null
            };
        }

        public AnsweredQuestionSynchronizationDto AnsweredQuestionSynchronizationDto(
            Guid? questionId = null, decimal[] rosterVector = null, object answer = null)
        {
            return new AnsweredQuestionSynchronizationDto(
                questionId ?? Guid.NewGuid(),
                rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty,
                answer ?? "42",
                "no comment");
        }

        public AnsweredYesNoOption AnsweredYesNoOption(decimal value, bool answer)
        {
            return new AnsweredYesNoOption(value, answer);
        }

        public AttachmentsController AttachmentsController(IAttachmentContentService attachmentContentService)
        {
            return new AttachmentsController(attachmentContentService);
        }

        public AttachmentContent AttachmentContent(string contentHash = null, string contentType = null, byte[] content = null)
        {
            return new AttachmentContent
            {
                ContentHash = contentHash ?? "content id",
                ContentType = contentType ,
                Content = content ?? new byte[] {1, 2, 3}
            };
        }

        public Attachment Attachment(string attachementHash) => new Attachment { ContentId = attachementHash };

        public CategoricalQuestionOption CategoricalQuestionOption(int value, string title, int? parentValue = null)
        {
            return new CategoricalQuestionOption
                   {
                       Value = value,
                       Title = title,
                       ParentValue = parentValue
                   };
        }

        public CommittedEvent CommittedEvent(string origin = null, Guid? eventSourceId = null, IEvent payload = null,
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

        public Core.SharedKernels.SurveyManagement.Implementation.Aggregates.Questionnaire DataCollectionQuestionnaire(
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null,
            IFileSystemAccessor fileSystemAccessor = null)
            => new Core.SharedKernels.SurveyManagement.Implementation.Aggregates.Questionnaire(
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                new ReferenceInfoForLinkedQuestionsFactory(),
                new QuestionnaireRosterStructureFactory(),
                questionnaireBrowseItemStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(),
                Mock.Of<IPlainKeyValueStorage<ReferenceInfoForLinkedQuestions>>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireRosterStructure>>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireQuestionsInfo>>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>());

        public DateTimeQuestion DateTimeQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
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

        public EnablementChanges EnablementChanges(
            List<Identity> groupsToBeDisabled = null, 
            List<Identity> groupsToBeEnabled = null,
            List<Identity> questionsToBeDisabled = null, 
            List<Identity> questionsToBeEnabled = null)
        {
            return new EnablementChanges(
                groupsToBeDisabled ?? new List<Identity>(),
                groupsToBeEnabled ?? new List<Identity>(),
                questionsToBeDisabled ?? new List<Identity>(),
                questionsToBeEnabled ?? new List<Identity>(),
                new List<Identity>(),
                new List<Identity>());
        }

        public EnumerationStageViewModel EnumerationStageViewModel(
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

        public EventContext EventContext()
        {
            return new EventContext();
        }

        public ExportedHeaderItem ExportedHeaderItem(Guid? questionId=null, string variableName="var")
        {
            return new ExportedHeaderItem()
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ColumnNames = new[] {variableName}
            };
        }

        public FailedValidationCondition FailedValidationCondition(int? failedConditionIndex = null)
            => new FailedValidationCondition(failedConditionIndex ?? 1117);

        public FixedRosterTitle FixedRosterTitle(decimal value, string title)
        {
            return new FixedRosterTitle(value, title);
        }

        public GeoPosition GeoPosition()
        {
            return new GeoPosition(1, 2, 3, 4, new DateTimeOffset(new DateTime(1984,4,18)));
        }

        public GpsCoordinateQuestion GpsCoordinateQuestion(Guid? questionId = null, string variable = "var1", bool isPrefilled=false, string title = null,
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

        public Group Group(
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

        public Variable Variable(Guid? id = null, VariableType type = VariableType.LongInteger, string variableName = "v1", string expression = "2*2")
        {
            return new Variable(publicKey: id ?? Guid.NewGuid(),
                variableData: new VariableData(type: type, name: variableName, expression: expression));
        }

        public HeaderStructureForLevel HeaderStructureForLevel()
        {
            return new HeaderStructureForLevel() {LevelScopeVector = new ValueVector<Guid>()};
        }

        public Identity Identity(string id, RosterVector rosterVector)
            => Create.Other.Identity(Guid.Parse(id), rosterVector);

        public Identity Identity(Guid id, RosterVector rosterVector
            ) => new Identity(id, rosterVector);

        public Interview Interview(Guid? interviewId = null, IPlainQuestionnaireRepository questionnaireRepository = null,
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null)
        {
            var interview = new Interview(
                Mock.Of<ILogger>(),
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                expressionProcessorStatePrototypeProvider ?? Stub.InterviewExpressionStateProvider());

            interview.SetId(interviewId ?? Guid.NewGuid());

            return interview;
        }

        public InterviewBinaryDataDescriptor InterviewBinaryDataDescriptor()
        {
            return new InterviewBinaryDataDescriptor(Guid.NewGuid(), "test.jpeg", () => new byte[0]);
        }

        public InterviewComment InterviewComment(string comment=null)
        {
            return new InterviewComment() {Comment = comment};
        }

        public InterviewCommentaries InterviewCommentaries(Guid? questionnaireId = null, long? questionnaireVersion = null, params InterviewComment[] comments)
        {
            return new InterviewCommentaries()
            {
                QuestionnaireId = (questionnaireId ?? Guid.NewGuid()).FormatGuid(),
                QuestionnaireVersion = questionnaireVersion ?? 1,
                Commentaries = new List<InterviewComment>(comments)
            };
        }

        public InterviewCommentedStatus InterviewCommentedStatus(
            Guid? statusId = null, 
            Guid? interviewerId = null, 
            Guid? supervisorId = null,
            DateTime? timestamp = null, 
            TimeSpan? timeSpanWithPreviousStatus = null, 
            InterviewExportedAction status = InterviewExportedAction.ApprovedBySupervisor)
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

        public InterviewData InterviewData(bool createdOnClient = false,
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

        public InterviewData InterviewData(params InterviewQuestion[] topLevelQuestions)
        {
            var interviewData = new InterviewData() { InterviewId = Guid.NewGuid() };
            interviewData.Levels.Add("#", new InterviewLevel(new ValueVector<Guid>(), null, new decimal[0]));
            foreach (var interviewQuestion in topLevelQuestions)
            {
                interviewData.Levels["#"].QuestionsSearchCache.Add(interviewQuestion.Id, interviewQuestion);
            }
            return interviewData;
        }

        public InterviewDataExportLevelView InterviewDataExportLevelView(Guid interviewId, params InterviewDataExportRecord[] records)
        {
            return new InterviewDataExportLevelView(new ValueVector<Guid>(), "test", records);
        }

        public InterviewDataExportRecord InterviewDataExportRecord(
            Guid interviewId,
            params ExportedQuestion[] questions)
        {
            return new InterviewDataExportRecord("test", new string[0], new string[0], new string [0])
            {
                Answers = questions.Select(x => String.Join("\n", x)).ToArray(), 
                LevelName = ""
            };
        }

        public InterviewDataExportView InterviewDataExportView(
            Guid? interviewId = null, 
            Guid? questionnaireId = null, 
            long questionnaireVersion = 1, 
            params InterviewDataExportLevelView[] levels)
        {
            return new InterviewDataExportView(interviewId ?? Guid.NewGuid(), levels);
        }

        public InterviewItemId InterviewItemId(Guid id, decimal[] rosterVector = null)
        {
            return new InterviewItemId(id, rosterVector);
        }

        public InterviewQuestion InterviewQuestion(Guid? questionId = null, object answer = null)
        {
            var interviewQuestion = new InterviewQuestion(questionId ?? Guid.NewGuid());
            interviewQuestion.Answer = answer;
            if (answer != null)
            {
                interviewQuestion.QuestionState = interviewQuestion.QuestionState | QuestionState.Answered;
            }
            return interviewQuestion;
        }

        public InterviewReferences InterviewReferences(
            Guid? questionnaireId = null,
            long? questionnaireVersion = null) 
            => new InterviewReferences(
                Guid.NewGuid(),
                questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion ?? 301);

        public InterviewStatuses InterviewStatuses(Guid? interviewid=null, Guid? questionnaireId=null, long? questionnaireVersion=null,params InterviewCommentedStatus[] statuses)
        {
            return new InterviewStatuses()
            {
                InterviewId = (interviewid??Guid.NewGuid()).FormatGuid(),
                InterviewCommentedStatuses = statuses.ToList(),
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                QuestionnaireVersion = questionnaireVersion ?? 1
            };
        }

        public InterviewStatusTimeSpans InterviewStatusTimeSpans(Guid? questionnaireId = null, long? questionnaireVersion = null, string interviewId = null, params TimeSpanBetweenStatuses[] timeSpans)
        {
            return new InterviewStatusTimeSpans()
            {
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                QuestionnaireVersion = questionnaireVersion ?? 1,
                TimeSpansBetweenStatuses = timeSpans.ToHashSet(),
                InterviewId = interviewId
            };
        }

        public InterviewSummary InterviewSummary() // needed since overload cannot be used in lambda expression
        {
            return new InterviewSummary();
        }

        public InterviewSummary InterviewSummary(
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

        public InterviewSynchronizationDto InterviewSynchronizationDto(
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
            bool? wasCompleted = false,
            List<Identity> disabledStaticTexts = null,
            List<Identity> validStaticTexts = null,
            List<KeyValuePair<Identity, List<FailedValidationCondition>>> invalidStaticTexts = null,
            Dictionary<InterviewItemId, object> variables=null,
            HashSet<InterviewItemId> disabledVariables=null)
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
                disabledStaticTexts,
                validQuestions ?? new HashSet<InterviewItemId>(),
                invalidQuestions ?? new HashSet<InterviewItemId>(),
                validStaticTexts,
                invalidStaticTexts,
                rosterGroupInstances ?? new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(),
                failedValidationConditions?.ToList() ?? new List<KeyValuePair<Identity, IList<FailedValidationCondition>>>(),
                new Dictionary<InterviewItemId, RosterVector[]>(),
                variables??new Dictionary<InterviewItemId, object>(),
                disabledVariables??new HashSet<InterviewItemId>(),  
                wasCompleted ?? false);
        }

        public InterviewView InterviewView(Guid? prefilledQuestionId = null)
        {
            return new InterviewView()
            {
                GpsLocation = new InterviewGpsLocationView
                {
                    PrefilledQuestionId = prefilledQuestionId
                }
            };
        }

        public LabeledVariable LabeledVariable(string variableName="var", string label="lbl", Guid? questionId=null, params VariableValueLabel[] variableValueLabels)
        {
            return new LabeledVariable(variableName, label, questionId, variableValueLabels);
        }

        public LastInterviewStatus LastInterviewStatus(InterviewStatus status = InterviewStatus.ApprovedBySupervisor)
            => new LastInterviewStatus("entry-id", status);

        public LookupTable LookupTable(string tableName, string fileName = null)
        {
            return new LookupTable
            {
                TableName = tableName,
                FileName = fileName ?? "lookup.tab"
            };
        }

        public Macro Macro(string name, string content = null, string description = null)
        {
            return new Macro
            {
                Name = name,
                Content = content,
                Description = description
            };
        }

        public MultimediaQuestion MultimediaQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
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

        public MultiOptionAnswer MultiOptionAnswer(Guid questionId, decimal[] rosterVector)
        {
            return new MultiOptionAnswer(questionId, rosterVector);
        }

        public IMultyOptionsQuestion MultipleOptionsQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
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
                Answers = answers.Select(a => Create.Other.Answer(a.ToString(), a)).ToList()
            };
        }

        public MultyOptionsQuestion MultyOptionsQuestion(Guid? id = null, 
            IEnumerable<Answer> options = null, Guid? linkedToQuestionId = null, string variable = null, bool yesNoView=false,
            string enablementCondition = null, string validationExpression = null, Guid? linkedToRosterId =null, bool areAnswersOrdered=false)
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
                ValidationExpression = validationExpression,
                AreAnswersOrdered = areAnswersOrdered
            };
        }

        public NavigationState NavigationState(IStatefulInterviewRepository interviewRepository = null)
        {
            var result = new NavigationState(
                Mock.Of<ICommandService>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IUserInteractionService>(),
                Mock.Of<IUserInterfaceStateService>());
            return result;
        }

        public NumericQuestion NumericIntegerQuestion(Guid? id = null, 
            string variable = "numeric_question", 
            string enablementCondition = null, 
            string validationExpression = null, 
            QuestionScope scope = QuestionScope.Interviewer, 
            bool isPrefilled = false,
            bool hideIfDisabled = false,
            bool useFormatting = false, 
            IEnumerable<ValidationCondition> validationConditions = null, Guid? linkedToRosterId = null)
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
                UseFormatting = useFormatting,
                ValidationConditions = validationConditions?.ToList() ?? new List<ValidationCondition>(),
                LinkedToRosterId = linkedToRosterId,
            };
        }

        public INumericQuestion NumericQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null, 
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

        public NumericQuestion NumericRealQuestion(Guid? id = null, 
            string variable = null, 
            string enablementCondition = null, 
            string validationExpression = null, 
            bool useFomatting = false,
            IEnumerable<ValidationCondition> validationConditions = null)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = false,
                UseFormatting = useFomatting,
                ConditionExpression = enablementCondition,
                ValidationConditions = validationConditions?.ToList() ?? new List<ValidationCondition>(),
                ValidationExpression = validationExpression
            };
        }

        public Answer Option(string value = null, string text = null, string parentValue = null, Guid? id = null)
        {
            return new Answer
            {
                PublicKey = id ?? Guid.NewGuid(),
                AnswerText = text ?? "text",
                AnswerValue = value ?? "1",
                ParentValue = parentValue
            };
        }

        public ParaDataExportProcessDetails ParaDataExportProcess()
        {
            return new ParaDataExportProcessDetails(DataExportFormat.Tabular);
        }

        public PlainQuestionnaire PlainQuestionnaire(QuestionnaireDocument document = null, long version = 19)
        {
            return new PlainQuestionnaire(
                document: document,
                version: version);
        }

        public IPublishableEvent PublishableEvent(Guid? eventSourceId = null, IEvent payload = null)
        {
            return Mock.Of<IPublishableEvent>(_ => _.Payload == (payload ?? Mock.Of<IEvent>()) && _.EventSourceId == (eventSourceId ?? Guid.NewGuid()));
        }

        public QRBarcodeQuestion QRBarcodeQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
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

        public IQuestion Question(
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

        public QuestionnaireBrowseItem QuestionnaireBrowseItem(
            Guid? questionnaireId = null, long? version = null, QuestionnaireIdentity questionnaireIdentity = null,
            string title = "Questionnaire Browse Item X", bool disabled = false)
            => new QuestionnaireBrowseItem
            {
                QuestionnaireId = questionnaireIdentity?.QuestionnaireId ?? questionnaireId ?? Guid.NewGuid(),
                Version = questionnaireIdentity?.Version ?? version ?? 1,
                Title = title,
                Disabled = disabled,
            };

        public QuestionnaireBrowseItem QuestionnaireBrowseItem(QuestionnaireDocument questionnaire)
        {
            return new QuestionnaireBrowseItem(questionnaire, 1, false,1);
        }

        public QuestionnaireDocument QuestionnaireDocument(Guid? id = null, params IComposite[] children)
        {
            return new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children?.ToList() ?? new List<IComposite>(),
            };
        }

        public QuestionnaireDocument QuestionnaireDocument(Guid? id = null, bool usesCSharp = false, IEnumerable<IComposite> children = null)
        {
            return new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children?.ToList() ?? new List<IComposite>(),
                UsesCSharp = usesCSharp,
            };
        }

        public QuestionnaireDocument QuestionnaireDocumentWithOneChapter(params IComposite[] children)
        {
            return QuestionnaireDocumentWithOneChapter(null, children);
        }

        public QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Guid? chapterId = null, params IComposite[] children)
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

        public QuestionnaireDocument QuestionnaireDocumentWithAttachments(Guid? chapterId = null, params Attachment[] attachments)
        {
            var result = new QuestionnaireDocument();
            var chapter = new Group("Chapter") { PublicKey = chapterId.GetValueOrDefault() };

            result.Children.Add(chapter);

            result.Attachments = attachments.ToList();

            return result;
        }

        public QuestionnaireExportStructure QuestionnaireExportStructure(Guid? questionnaireId = null, long? version = null)
        {
            return new QuestionnaireExportStructure
            {
                QuestionnaireId = questionnaireId ?? Guid.Empty,
                Version = version ?? 0
            };
        }

        public QuestionnaireIdentity QuestionnaireIdentity(Guid? questionnaireId = null, long? questionnaireVersion = null)
        {
            return new QuestionnaireIdentity(questionnaireId ?? Guid.NewGuid(), questionnaireVersion ?? 7);
        }

        public QuestionnaireLevelLabels QuestionnaireLevelLabels(string levelName="level", params LabeledVariable[] variableLabels)
        {
            return new QuestionnaireLevelLabels(levelName, variableLabels);
        }

        public IPlainQuestionnaireRepository QuestionnaireRepositoryStubWithOneQuestionnaire(
            Guid questionnaireId, IQuestionnaire questionnaire = null, long? questionnaireVersion = null)
        {
            questionnaire = questionnaire ?? Mock.Of<IQuestionnaire>();

            return Mock.Of<IPlainQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion ?? questionnaire.Version) == questionnaire
                && repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion ?? 1) == questionnaire
                && repository.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire);
        }

        public ReadSideCacheSettings ReadSideCacheSettings(int cacheSizeInEntities = 128, int storeOperationBulkSize = 8)
            => new ReadSideCacheSettings(true, "folder", cacheSizeInEntities, storeOperationBulkSize);

        public ReadSideSettings ReadSideSettings()
            => new ReadSideSettings(readSideVersion: 0);

        public Group FixedRoster(Guid? rosterId = null, IEnumerable<string> fixedTitles = null, IEnumerable<IComposite> children = null)
            => Create.Other.Roster(
                rosterId: rosterId,
                children: children,
                fixedTitles: fixedTitles?.ToArray() ?? new[] { "Fixed Roster 1", "Fixed Roster 2", "Fixed Roster 3" });

        public Group Roster(
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
            Group group = Create.Other.Group(
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

        public RosterInstancesRemoved RosterInstancesRemoved(Guid? rosterGroupId = null)
        {
            return new RosterInstancesRemoved(new[]
                {
                    new RosterInstance(rosterGroupId ?? Guid.NewGuid(), new decimal[0], 0.0m)
                });
        }

        public RosterInstancesTitleChanged RosterInstancesTitleChanged(Guid? rosterId = null, 
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

        public RosterVector RosterVector(params decimal[] coordinates)
        {
            return new RosterVector(coordinates ?? Enumerable.Empty<decimal>());
        }

        public SingleOptionLinkedQuestionViewModel SingleOptionLinkedQuestionViewModel(
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
                Create.Service.AnswerToStringService(),
                eventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                Stub.MvxMainThreadDispatcher(),
                questionState ?? Stub<QuestionStateViewModel<SingleOptionLinkedQuestionAnswered>>.WithNotEmptyValues,
                answering ?? Mock.Of<AnsweringViewModel>());
        }

        public SingleQuestion SingleOptionQuestion(Guid? questionId = null, string variable = null, string enablementCondition = null, string validationExpression = null,
            Guid? linkedToQuestionId = null,
            Guid? cascadeFromQuestionId = null, 
            decimal[] answerCodes = null, 
            decimal[] parentCodes = null,
            string title = null, 
            bool hideIfDisabled = false, 
            string linkedFilterExpression = null,
            Guid? linkedToRosterId = null)
        {
            var answers = (answerCodes ?? new decimal[] { 1, 2, 3 }).Select(a => Create.Other.Answer(a.ToString(), a)).ToList();
            if (parentCodes != null)
            {
                for (int i = 0; i < parentCodes.Length; i++)
                {
                    answers[i].ParentValue = parentCodes[i].ToString(CultureInfo.InvariantCulture);
                }
            }
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
                Answers = answers,
                LinkedFilterExpression= linkedFilterExpression
            };
        }

        public SingleQuestion SingleQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null,
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

        public StatefulInterview StatefulInterview(Guid? questionnaireId = null, 
            long? questionnaireVersion = null,
            Guid? userId = null, 
            IPlainQuestionnaireRepository questionnaireRepository = null)
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

        public StatefulInterview StatefulInterview(Guid? questionnaireId = null, Guid? userId = null,
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

        public StaticText StaticText(
            Guid? publicKey = null,
            string text = "Static Text X",
            string attachmentName = null,
            List<ValidationCondition> validationConditions = null)
        {
            return new StaticText(publicKey ?? Guid.NewGuid(), text, null, false, validationConditions ?? new List<ValidationCondition>(), attachmentName);
        }

        public TextAnswer TextAnswer(string answer)
        {
            return Create.Other.TextAnswer(answer, null, null);
        }

        public TextAnswer TextAnswer(string answer, Guid? questionId, decimal[] rosterVector)
        {
            var masedMaskedTextAnswer = new TextAnswer(questionId ?? Guid.NewGuid(), rosterVector ?? Empty.RosterVector);

            if (answer != null)
            {
                masedMaskedTextAnswer.SetAnswer(answer);
            }

            return masedMaskedTextAnswer;
        }

        public ITextListQuestion TextListQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
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

        public TextQuestion TextQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
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

        public TimeSpanBetweenStatuses TimeSpanBetweenStatuses(Guid? interviewerId = null, Guid? supervisorId = null, DateTime? timestamp = null, TimeSpan? timeSpanWithPreviousStatus = null, InterviewExportedAction endStatus= InterviewExportedAction.ApprovedByHeadquarter)
        {
            return new TimeSpanBetweenStatuses()
            {
                BeginStatus = InterviewExportedAction.InterviewerAssigned,
                EndStatus = endStatus,
                EndStatusTimestamp = timestamp ?? DateTime.Now,
                InterviewerId = interviewerId ?? Guid.NewGuid(),
                SupervisorId = supervisorId ?? Guid.NewGuid(),
                TimeSpan = timeSpanWithPreviousStatus?? new TimeSpan()
            };
        }

        public UncommittedEvent UncommittedEvent(Guid? eventSourceId = null, 
            IEvent payload = null,
            int sequence = 1,
            int initialVersion = 1)
        {
            return new UncommittedEvent(Guid.NewGuid(), eventSourceId ?? Guid.NewGuid(), sequence, initialVersion, DateTime.Now, payload);
        }

        public User User(Guid? userId=null)
        {
            var user = new User();
            user.SetId(userId??Guid.NewGuid());
            return user;
        }

        public UserDocument UserDocument(Guid? userId = null, Guid? supervisorId = null, bool? isArchived = null, string userName="name", bool isLockedByHQ = false)
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

        public UserLight UserLight(Guid? userId = null)
        {
            return new UserLight(userId ?? Guid.NewGuid(), "test");
        }

        public UserPreloadingDataRecord UserPreloadingDataRecord(string login = "test", string supervisor = "", string password = "test", string email="", string phoneNumber="", string role=null)
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

        public UserPreloadingProcess UserPreloadingProcess(string userPreloadingProcessId = null,
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

        public UserPreloadingSettings UserPreloadingSettings()
        {
            return new UserPreloadingSettings(5, 5, 12, 1, 10000, 100, 100, "^[a-zA-Z0-9_]{3,15}$",
                @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$",
                "^(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z]).*$",
                @"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$");
        }

        public UserPreloadingVerificationError UserPreloadingVerificationError()
        {
            return new UserPreloadingVerificationError();
        }

        public VariableValueLabel VariableValueLabel(string value="1", string label="l1")
        {
            return new VariableValueLabel(value, label);
        }

        public YesNoAnswer YesNoAnswer(Guid questionId, decimal[] rosterVector)
        {
            return new YesNoAnswer(questionId, rosterVector);
        }

        public YesNoAnswers YesNoAnswers(decimal[] allOptionCodes, YesNoAnswersOnly yesNoAnswersOnly = null)
        {
            return new YesNoAnswers(allOptionCodes: allOptionCodes, yesNoAnswersOnly: yesNoAnswersOnly);
        }

        public IMultyOptionsQuestion YesNoQuestion(Guid? questionId = null, decimal[] answers = null)
        {
            return Create.Other.MultipleOptionsQuestion(
                isYesNo: true,
                questionId: questionId,
                answers: answers ?? new decimal[] {});
        }

        public CommentedStatusHistroyView CommentedStatusHistroyView(InterviewStatus status=InterviewStatus.InterviewerAssigned, string comment=null, DateTime? timestamp=null)
        {
            return new CommentedStatusHistroyView()
            {
                Status = status,
                Comment = comment,
                Date = timestamp ?? DateTime.Now
            };
        }

        public InterviewRoster InterviewRoster(Guid? rosterId=null, decimal[] rosterVector=null, string rosterTitle="titile")
        {
            return new InterviewRoster()
            {
                Id = rosterId ?? Guid.NewGuid(),
                IsDisabled = false,
                RosterVector = rosterVector ?? new decimal[0],
                Title = rosterTitle
            };
        }

        public ValidationCondition ValidationCondition(string expression = "self != null", string message = "should be answered")
        {
            return new ValidationCondition(expression, message);
        }

        public InterviewLinkedQuestionOptions InterviewLinkedQuestionOptions(params ChangedLinkedOptions[] options)
        {
            var result = new InterviewLinkedQuestionOptions();

            foreach (var changedLinkedQuestion in options)
            {
                result.LinkedQuestionOptions[changedLinkedQuestion.QuestionId.ToString()] = changedLinkedQuestion.Options;
            }

            return result;
        }

        public ChangedLinkedOptions ChangedLinkedOptions(Guid questionId,decimal[] questionRosterVector=null, RosterVector[] options=null)
        {
            return new ChangedLinkedOptions(new Identity(questionId, questionRosterVector ?? new decimal[0]),
                options ?? new RosterVector[0]);
        }
        
        public AttachmentContentMetadata AttachmentContentMetadata(string contentType)
        {
            return new AttachmentContentMetadata()
            {
                ContentType = contentType,
            };
        }
        
        public AttachmentContentData AttachmentContentData(byte[] content)
        {
            return new AttachmentContentData()
            {
                Content = content,
            };
        }

        public AttachmentViewModel AttachmentViewModel(
            IPlainQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAttachmentContentStorage attachmentContentStorage)
        {
            return new AttachmentViewModel(
                questionnaireRepository: questionnaireRepository,
                interviewRepository: interviewRepository,
                attachmentContentStorage: attachmentContentStorage);
        }

        public WB.Core.SharedKernels.Enumerator.Views.AttachmentContent Enumerator_AttachmentContent(string id)
        {
            return new Core.SharedKernels.Enumerator.Views.AttachmentContent()
            {
                Id = id,

            };
        }

        public ISnapshotStore SnapshotStore(Guid aggregateRootId, Snapshot snapshot = null)
        {
            return Mock.Of<ISnapshotStore>(_ => _.GetSnapshot(aggregateRootId, Moq.It.IsAny<int>()) == snapshot);
        }

        public IEventStore EventStore(Guid eventSourceId, IEnumerable<CommittedEvent> committedEvents)
        {
            return Mock.Of<IEventStore>(_ =>
                _.Read(eventSourceId, Moq.It.IsAny<int>()) == new CommittedEventStream(eventSourceId, committedEvents));
        }

        public IAggregateSnapshotter AggregateSnapshotter(EventSourcedAggregateRoot aggregateRoot = null, bool isARLoadedFromSnapshotSuccessfully = false)
        {
            return Mock.Of<IAggregateSnapshotter>(_ =>
                _.TryLoadFromSnapshot(Moq.It.IsAny<Type>(), Moq.It.IsAny<Snapshot>(),
                    Moq.It.IsAny<CommittedEventStream>(), out aggregateRoot) ==
                isARLoadedFromSnapshotSuccessfully);
        }

        public IStatefulInterviewRepository StatefulInterviewRepositoryWith(IStatefulInterview interview)
        {
            var result = Substitute.For<IStatefulInterviewRepository>();
            result.Get(null).ReturnsForAnyArgs(interview);
            return result;
        }

        public EventBusSettings EventBusSettings() => new EventBusSettings
        {
            EventHandlerTypesWithIgnoredExceptions = new Type[] {},
            DisabledEventHandlerTypes = new Type[] {},
        };
    }
}