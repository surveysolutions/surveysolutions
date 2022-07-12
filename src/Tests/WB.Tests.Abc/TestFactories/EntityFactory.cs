using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events;
using Moq;
using Ncqrs.Eventing;
using NodaTime;
using NodaTime.Extensions;
using NUnit.Framework;
using ReflectionMagic;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Preloading;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Workspaces;
using AttachmentContent = WB.Core.BoundedContexts.Headquarters.Views.Questionnaire.AttachmentContent;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Abc.TestFactories
{
    internal static class EntityFactoryExtensions
    {
        public static QuestionnaireDocument WithEntityMap(this QuestionnaireDocument doc)
        {
            int counter = 0;
            doc.EntitiesIdMap = doc.GetAllQuestions().ToDictionary(q => q.PublicKey, q => counter++);
            return doc;
        }
    }

    internal class EntityFactory
    {
        public Answer Answer(string answer, decimal value, decimal? parentValue = null)
            => new Answer
            {
                AnswerText = answer,
                AnswerValue = value.ToString(),
                ParentValue = parentValue?.ToString(),
                AnswerCode = value
            };

        public AnsweredQuestionSynchronizationDto AnsweredQuestionSynchronizationDto(
            Guid? questionId = null, decimal[] rosterVector = null, object answer = null, params CommentSynchronizationDto[] comments)
            => new AnsweredQuestionSynchronizationDto(
                questionId ?? Guid.NewGuid(),
                rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty,
                answer,
                comments ?? new CommentSynchronizationDto[0],
                null);

        public AnsweredYesNoOption AnsweredYesNoOption(decimal value, bool answer)
            => new AnsweredYesNoOption(value, answer);

        public Attachment Attachment(string attachmentHash)
            => new Attachment { ContentId = attachmentHash , AttachmentId = Guid.NewGuid()};

        public Translation Translation(Guid translationId, string translationName)
            => new Translation { Id = translationId, Name = translationName};

        public WB.Core.SharedKernels.Questionnaire.Api.AttachmentContent AttachmentContent_Enumerator(string id)
            => new WB.Core.SharedKernels.Questionnaire.Api.AttachmentContent
            {
                Id = id
            };

        public AttachmentContent AttachmentContent_SurveyManagement(string contentHash = null, string contentType = null, byte[] content = null)
            => new AttachmentContent
            {
                ContentHash = contentHash ?? "content id",
                ContentType = contentType,
                Content = content ?? new byte[] {1, 2, 3}
            };

        public AttachmentContentData AttachmentContentData(byte[] content)
            => new AttachmentContentData
            {
                Content = content,
            };

        public AttachmentContentMetadata AttachmentContentMetadata(string contentType, string id = null)
            => new AttachmentContentMetadata
            {
                Id = id ?? Guid.NewGuid().ToString(),
                ContentType = contentType,
            };

        public CategoricalOption CategoricalQuestionOption(int value, string title, int? parentValue = null)
            => new CategoricalOption
            {
                Value = value,
                Title = title,
                ParentValue = parentValue
            };

        public ChangedLinkedOptions ChangedLinkedOptions(Guid questionId, decimal[] questionRosterVector = null, RosterVector[] options = null)
            => new ChangedLinkedOptions(
                new Identity(questionId, questionRosterVector ?? new decimal[0]),
                options ?? new RosterVector[0]);

        public List<HeaderColumn> ColumnHeaders(string[] columnNames)
        {
            return columnNames?.Select(x => new HeaderColumn() { Name = x, Title = x }).ToList() ?? new List<HeaderColumn>();
        }

        public CommentedStatusHistoryView CommentedStatusHistroyView(
            InterviewStatus status = InterviewStatus.InterviewerAssigned, string comment = null, DateTime? timestamp = null)
            => new CommentedStatusHistoryView
            {
                Status = status,
                Comment = comment,
                Date = timestamp ?? DateTime.Now,
            };
        
        public CommentSynchronizationDto CommentSynchronizationDto(string text = "hello!", Guid? userId = null, UserRoles? userRole = null)
        {
            return new CommentSynchronizationDto
            {
                Text = text,
                UserId = userId ?? Guid.NewGuid(),
                UserRole = userRole ?? UserRoles.Interviewer
            };
        }

        public CompositeCollection<T> CompositeCollection<T>()
            => new CompositeCollection<T>();

        public CompositeCollection<T> CompositeCollection<T>(params T[] items)
        {
            var compositeCollection = new CompositeCollection<T>();
            items.ForEach(item => compositeCollection.Add(item));
            return compositeCollection;
        }

        public DataExportProcessDetails DataExportProcessDetails(QuestionnaireIdentity questionnaireIdentity = null, DataExportFormat? format = null)
            => new DataExportProcessDetails(
                format ?? DataExportFormat.Tabular,
                questionnaireIdentity ?? new QuestionnaireIdentity(Guid.NewGuid(), 1),
                "some questionnaire");

        public InterviewTreeDateTimeQuestion InterviewTreeDateTimeQuestion(DateTime answer, bool isTimestampQuestion = false)
            => new InterviewTreeDateTimeQuestion(answer, isTimestampQuestion);

        public DateTimeQuestion DateTimeQuestion(
            Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string variable = null, string validationMessage = null, string text = null,
            QuestionScope scope = QuestionScope.Interviewer, bool preFilled = false, bool hideIfDisabled = false, bool isTimestamp = false)
            => new DateTimeQuestion()
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                QuestionText = text,
                StataExportCaption = variable,
                QuestionScope = scope,
                Featured = preFilled,
                IsTimestamp = isTimestamp
            };

        public EnablementChanges EnablementChanges(
            List<Identity> groupsToBeDisabled = null,
            List<Identity> groupsToBeEnabled = null,
            List<Identity> questionsToBeDisabled = null,
            List<Identity> questionsToBeEnabled = null)
            => new EnablementChanges(
                groupsToBeDisabled ?? new List<Identity>(),
                groupsToBeEnabled ?? new List<Identity>(),
                questionsToBeDisabled ?? new List<Identity>(),
                questionsToBeEnabled ?? new List<Identity>(),
                new List<Identity>(),
                new List<Identity>(),
                new List<Identity>(),
                new List<Identity>());

        public ValidityChanges ValidityChanges()
        {
            return new ValidityChanges(new List<Identity>(), new List<Identity>());
        }

        public EventBusSettings EventBusSettings()
            => new EventBusSettings
            {
                EventHandlerTypesWithIgnoredExceptions = new Type[] {},
                DisabledEventHandlerTypes = new Type[] {},
            };

        public ExportedQuestionHeaderItem ExportedQuestionHeaderItem(Guid? questionId = null, string variableName = "var")
            => new ExportedQuestionHeaderItem
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ColumnHeaders = new List<HeaderColumn>() { new HeaderColumn(){Name = variableName, Title = variableName}}
            };

        public FailedValidationCondition FailedValidationCondition(int? failedConditionIndex = null)
            => new FailedValidationCondition(failedConditionIndex ?? 1117);

        public Group FixedRoster(Guid? rosterId = null,
            string enablementCondition = null,
            IEnumerable<string> obsoleteFixedTitles = null,
            IEnumerable<IComposite> children = null,
            string variable = "roster_var",
            string title = "Roster X",
            RosterDisplayMode displayMode = RosterDisplayMode.SubSection,
            FixedRosterTitle[] fixedTitles = null) => Create.Entity.Roster(
                        rosterId: rosterId,
                        children: children,
                        title: title,
                        variable: variable,
                        enablementCondition: enablementCondition,
                        fixedRosterTitles: fixedTitles,
                        displayMode: displayMode,
                        fixedTitles: obsoleteFixedTitles?.ToArray() ?? new[] { "Fixed Roster 1", "Fixed Roster 2", "Fixed Roster 3" });


        public FixedRosterTitle[] FixedTitles(params int[] codes)
        {
            return codes.Select(c => FixedTitle(c)).ToArray();
        }

        public FixedRosterTitle FixedTitle(int value, string title = null)
            => new FixedRosterTitle(value, title ?? $"Fixed title {value}");

        public GeoPosition GeoPosition(double? latitude = null, double? longitude = null, double? accuracy = null, double? altitude = null,
            DateTimeOffset? timestamp = null)
            => new GeoPosition(latitude ?? 1, longitude ?? 2, accuracy ?? 3, altitude ?? 4, timestamp ?? new DateTimeOffset(new DateTime(1984, 4, 18)));

        public GpsCoordinateQuestion GpsCoordinateQuestion(Guid? questionId = null, string variable = "var1", bool isPrefilled = false, string title = null,
            string enablementCondition = null, string validationExpression = null, bool hideIfDisabled = false, string label=null, QuestionScope scope = QuestionScope.Interviewer)
            => new GpsCoordinateQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = variable,
                QuestionScope = scope,
                Featured = isPrefilled,
                QuestionText = title,
                ValidationExpression = validationExpression,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                VariableLabel = label
            };

        public Group Group(
            Guid? groupId = null,
            string title = "Group X",
            string variable = null,
            string enablementCondition = null,
            bool hideIfDisabled = false,
            IEnumerable<IComposite> children = null)
            => new Group(title)
            {
                PublicKey = groupId ?? Guid.NewGuid(),
                VariableName = variable,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
            };

        public HeaderStructureForLevel HeaderStructureForLevel()
            => new HeaderStructureForLevel { LevelScopeVector = new ValueVector<Guid>() };

        public Identity Identity(string id, RosterVector rosterVector)
            => Create.Entity.Identity(Guid.Parse(id), rosterVector);

        public Identity Identity(Guid? id = null, RosterVector rosterVector = null)
            => new Identity(
                id ?? Guid.NewGuid(),
                rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty);

        public Identity Identity(int id, RosterVector rosterVector = null)
            => new Identity(
                Guid.Parse(this.SpamIntToStringOfLength(id)),
                rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty);

        private string SpamIntToStringOfLength(int number, int length = 32)
        {
            var result = number.ToString();
            return string.Concat(Enumerable.Repeat(result, (int) Math.Ceiling(length / (double) result.Length))).Substring(0, 32);
        }

        public InterviewBinaryDataDescriptor InterviewBinaryDataDescriptor(Guid? interviewId = null, string fileName = null)
            => new InterviewBinaryDataDescriptor(interviewId ?? Guid.NewGuid(), fileName ?? "test.jpeg", null, 
                () => Task.FromResult(Array.Empty<byte>()));

        public InterviewCommentedStatus InterviewCommentedStatus(
            InterviewExportedAction status = InterviewExportedAction.ApprovedBySupervisor,
            string originatorName = "inter",
            UserRoles originatorRole = UserRoles.Interviewer,
            Guid? statusId = null, 
            Guid? interviewerId = null, 
            Guid? supervisorId = null, 
            DateTime? timestamp = null, 
            TimeSpan? timeSpanWithPreviousStatus = null,
            InterviewSummary interviewSummary = null)
            => new InterviewCommentedStatus
            {
                Id = statusId ?? Guid.NewGuid(),
                Status = status,
                Timestamp = timestamp ?? DateTime.Now,
                InterviewerId = interviewerId ?? Guid.NewGuid(),
                SupervisorId = supervisorId ?? Guid.NewGuid(),
                StatusChangeOriginatorRole = originatorRole,
                StatusChangeOriginatorName = originatorName,
                InterviewerName = "inter",
                SupervisorName = "supervisor",
                TimeSpanWithPreviousStatus = timeSpanWithPreviousStatus,
                InterviewSummary = interviewSummary
            };

        public InterviewDataExportLevelView InterviewDataExportLevelView(Guid interviewId, params InterviewDataExportRecord[] records)
            => new InterviewDataExportLevelView(new ValueVector<Guid>(), "test", records);

        public InterviewDataExportRecord InterviewDataExportRecord(
            Guid interviewId,
            string levelName = "",
            string[] referenceValues = null,
            string[] parentLevelIds = null,
            string[] systemVariableValues = null,
            string[][] answers = null,
            string id = null)
            => new InterviewDataExportRecord(
               id ?? interviewId.FormatGuid(), 
               referenceValues?? new string[0],
               parentLevelIds ?? new string[0],
               systemVariableValues ?? new string[0])
               { 
                   Answers = answers ?? new string[][]{},
                   LevelName = levelName,
                   InterviewId = interviewId,
                   Id = id
               };

        public InterviewItemId InterviewItemId(Guid id, decimal[] rosterVector = null)
            => new InterviewItemId(id, rosterVector);

        public InterviewSummary InterviewSummary()
            => new InterviewSummary();


        public InterviewSummary InterviewSummary(
            Guid? interviewId = null,
            Guid? questionnaireId = null,
            long? questionnaireVersion = null,
            InterviewStatus? status = null,
            Guid? responsibleId = null,
            Guid? teamLeadId = null,
            string responsibleName = null,
            string teamLeadName = null,
            UserRoles role = UserRoles.Interviewer,
            string key = null,
            DateTime? updateDate = null,
            bool? wasCreatedOnClient = null,
            DateTime? receivedByInterviewerAtUtc = null,
            int? assignmentId = null,
            bool wasCompleted = false,
            int? errorsCount = 0,
            TimeSpan? interviewingTotalTime = null,
            string questionnaireVariable = "automation",
            IEnumerable<InterviewCommentedStatus> statuses = null,
            IEnumerable<TimeSpanBetweenStatuses> timeSpans = null)
        {
            var qId = questionnaireId ?? Guid.NewGuid();
            var qVersion = questionnaireVersion ?? 1;
            return new InterviewSummary
            {
                InterviewId = interviewId ?? Guid.NewGuid(),
                QuestionnaireId = qId,
                QuestionnaireVersion = qVersion,
                Status = status.GetValueOrDefault(),
                ResponsibleId = responsibleId.GetValueOrDefault(),
                ResponsibleName = string.IsNullOrWhiteSpace(responsibleName) ? responsibleId.FormatGuid() : responsibleName,
                SupervisorId = teamLeadId.GetValueOrDefault(),
                SupervisorName = string.IsNullOrWhiteSpace(teamLeadName) ? teamLeadId.FormatGuid() : teamLeadName,
                ResponsibleRole = role,
                Key = key,
                UpdateDate = updateDate ?? new DateTime(2017, 3, 23).ToUniversalTime(),
                WasCreatedOnClient = wasCreatedOnClient ?? false,
                ReceivedByInterviewerAtUtc = receivedByInterviewerAtUtc,
                AssignmentId = assignmentId,
                QuestionnaireIdentity = new QuestionnaireIdentity(qId, qVersion).ToString(),
                WasCompleted = wasCompleted,
                InterviewDuration = interviewingTotalTime,
                InterviewCommentedStatuses = statuses?.ToList() ?? new List<InterviewCommentedStatus>(),
                QuestionnaireVariable = questionnaireVariable,
                TimeSpansBetweenStatuses = timeSpans != null ? timeSpans.ToHashSet() : new HashSet<TimeSpanBetweenStatuses>(),
            };
        }

        public InterviewSynchronizationDto InterviewSynchronizationDto(
            Guid? questionnaireId = null,
            long? questionnaireVersion = null,
            Guid? userId = null,
            Guid? supervisorId = null,
            AnsweredQuestionSynchronizationDto[] answers = null,
            HashSet<InterviewItemId> disabledGroups = null,
            HashSet<InterviewItemId> disabledQuestions = null,
            HashSet<InterviewItemId> validQuestions = null,
            HashSet<InterviewItemId> invalidQuestions = null,
            HashSet<InterviewItemId> readonlyQuestions = null,
            Guid? interviewId = null,
            Dictionary<Identity, IList<FailedValidationCondition>> failedValidationConditions = null,
            InterviewStatus status = InterviewStatus.InterviewerAssigned,
            bool? wasCompleted = false,
            List<Identity> disabledStaticTexts = null,
            List<Identity> validStaticTexts = null,
            List<KeyValuePair<Identity, List<FailedValidationCondition>>> invalidStaticTexts = null,
            Dictionary<InterviewItemId, object> variables = null,
            HashSet<InterviewItemId> disabledVariables = null,
            InterviewKey interviewKey = null,
            int? assignmentId = null)
        {
            return new InterviewSynchronizationDto(
                interviewId ?? Guid.NewGuid(),
                status,
                "",
                null,
                null,
                userId ?? Guid.NewGuid(),
                supervisorId ?? Guid.NewGuid(),
                questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion ?? 1,
                answers ?? Array.Empty<AnsweredQuestionSynchronizationDto>(),
                disabledGroups ?? new HashSet<InterviewItemId>(),
                disabledQuestions ?? new HashSet<InterviewItemId>(),
                disabledStaticTexts ?? new List<Identity>(),
                validQuestions ?? new HashSet<InterviewItemId>(),
                invalidQuestions ?? new HashSet<InterviewItemId>(),
                readonlyQuestions ?? new HashSet<InterviewItemId>(),
                validStaticTexts ?? new List<Identity>(),
                invalidStaticTexts ?? new List<KeyValuePair<Identity, List<FailedValidationCondition>>>(),
                failedValidationConditions?.ToList() ?? new List<KeyValuePair<Identity, IList<FailedValidationCondition>>>(),
                variables ?? new Dictionary<InterviewItemId, object>(),
                disabledVariables ?? new HashSet<InterviewItemId>(),
                wasCompleted ?? false)
            {
                InterviewKey = interviewKey,
                AssignmentId = assignmentId
            };
        }

        public InterviewView InterviewView(Guid? prefilledQuestionId = null, 
            Guid? interviewId = null, 
            string questionnaireId = null, 
            InterviewStatus? status = null,
            string questionaireTitle = null,
            int? assignmentId = null,
            bool? canBeDeleted = null,
            Guid? responsibleId = null,
            DateTime? receivedByInterviewerAt = null,
            DateTime? fromHqSyncDateTime = null)
        {
            interviewId = interviewId ?? Guid.NewGuid();
            return new InterviewView
            {
                Id = interviewId.FormatGuid(),
                InterviewId = interviewId.Value,
                QuestionnaireId = questionnaireId ?? Create.Entity.QuestionnaireIdentity().ToString(),
                LocationQuestionId = prefilledQuestionId,
                QuestionnaireTitle = questionaireTitle ?? "Questionnaire ",
                Status = status ?? InterviewStatus.InterviewerAssigned,
                Assignment = assignmentId,
                CanBeDeleted = canBeDeleted ?? true,
                ResponsibleId = responsibleId.GetValueOrDefault(),
                ReceivedByInterviewerAtUtc = receivedByInterviewerAt,
                FromHqSyncDateTime = fromHqSyncDateTime,
                Mode = InterviewMode.CAPI
            };
        }

        public InterviewSequenceView InterviewSequenceView(Guid id, int sequence)
        {
            return new InterviewSequenceView() {Id = id, ReceivedFromServerSequence = sequence};
        }

        public DataExportVariable LabeledVariable(string variableName = "var", string label = "lbl", Guid? questionId = null, params VariableValueLabel[] variableValueLabels)
            => new DataExportVariable(variableName, label, questionId, variableValueLabels, ExportValueType.Unknown);

        public LastInterviewStatus LastInterviewStatus(InterviewStatus status = InterviewStatus.ApprovedBySupervisor)
            => new LastInterviewStatus("entry-id", status);

        public LookupTable LookupTable(string tableName, string fileName = null)
            => new LookupTable
            {
                TableName = tableName,
                FileName = fileName ?? "lookup.tab"
            };

        public Macro Macro(string name, string content = null, string description = null)
            => new Macro
            {
                Name = name,
                Content = content,
                Description = description
            };

        public MultimediaQuestion MultimediaQuestion(Guid? questionId = null, string enablementCondition = null,
            string validationExpression = null, string variable = null, string validationMessage = null, string text = null,
            QuestionScope scope = QuestionScope.Interviewer, bool hideIfDisabled = false)
            => new MultimediaQuestion()
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = variable,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                QuestionText = text
            };

        public InterviewTreeMultiOptionQuestion InterviewTreeMultiOptionQuestion(decimal[] answer)
            => new InterviewTreeMultiOptionQuestion(answer);

        public MultyOptionsQuestion MultipleOptionsQuestion(Guid? questionId = null, string enablementCondition = null,
            string validationExpression = null, bool areAnswersOrdered = false, int? maxAllowedAnswers = null, Guid? linkedToQuestionId = null, Guid? linkedToRosterId = null,
            bool isYesNo = false, bool hideIfDisabled = false, string optionsFilterExpression = null, 
            Answer[] textAnswers = null, 
            string variable = "mo_question",
            string linkedFilterExpression = null,
            Guid? categoryId = null,
            params int[] answers)
            => new MultyOptionsQuestion("Question MO")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                AreAnswersOrdered = areAnswersOrdered,
                MaxAllowedAnswers = maxAllowedAnswers,
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                LinkedFilterExpression = linkedFilterExpression,
                YesNoView = isYesNo,
                CategoriesId = categoryId,
                Answers = textAnswers?.ToList() ?? answers.Select(a => Create.Entity.Answer(a.ToString(), a)).ToList(),
                Properties = new QuestionProperties(false, false)
                {
                    OptionsFilterExpression = optionsFilterExpression
                }
            };

        public MultyOptionsQuestion MultyOptionsQuestion(Guid? id = null,
            IEnumerable<Answer> options = null, Guid? linkedToQuestionId = null, string variable = null, bool yesNoView = false,
            string enablementCondition = null, 
            string validationExpression = null, 
            Guid? linkedToRosterId = null,
            bool areAnswersOrdered = false,
            string optionsFilter = null,
            string linkedFilter = null,
            int? maxAllowedAnswers = null,
            Guid? categoryId = null)
            => new MultyOptionsQuestion
            {
                PublicKey = id ?? Guid.NewGuid(),
                Answers = linkedToQuestionId.HasValue ? null : new List<Answer>(options ?? new Answer[] { }),
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                StataExportCaption = variable,
                YesNoView = yesNoView,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                AreAnswersOrdered = areAnswersOrdered,
                LinkedFilterExpression = linkedFilter,
                MaxAllowedAnswers = maxAllowedAnswers,
                Properties = { OptionsFilterExpression = optionsFilter },
                CategoriesId = categoryId,
            };

        public NumericQuestion NumericIntegerQuestion(Guid? id = null,
            string variable = "numeric_question",
            string enablementCondition = null,
            string validationExpression = null,
            QuestionScope scope = QuestionScope.Interviewer,
            bool isPrefilled = false,
            bool hideIfDisabled = false,
            bool useFormatting = false,
            string questionText = null,
            IEnumerable<ValidationCondition> validationConditions = null, 
            Guid? linkedToRosterId = null,
            IEnumerable<Answer> specialValues = null)
            => new NumericQuestion
            {
                QuestionText = questionText ?? "text",
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
                Answers = new List<Answer>(specialValues ?? new Answer[] { })
            };

        public NumericQuestion NumericQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            bool isInteger = false, int? countOfDecimalPlaces = null, string variableName = "var1", bool prefilled = false, string title = null,
            IEnumerable<Answer> options = null)
            => new NumericQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                IsInteger = isInteger,
                CountOfDecimalPlaces = countOfDecimalPlaces,
                StataExportCaption = variableName,
                Featured = prefilled,
                QuestionText = title,
                Answers = new List<Answer>(options ?? new Answer[] { })
            };

        public NumericQuestion NumericRealQuestion(Guid? id = null,
            string variable = null,
            string enablementCondition = null,
            string validationExpression = null,
            bool useFomatting = false,
            IEnumerable<ValidationCondition> validationConditions = null,
            int? countOfDecimalPlaces = null,
            IEnumerable<Answer> specialValues = null)
            => new NumericQuestion
            {
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = false,
                UseFormatting = useFomatting,
                ConditionExpression = enablementCondition,
                ValidationConditions = validationConditions?.ToList() ?? new List<ValidationCondition>(),
                ValidationExpression = validationExpression,
                CountOfDecimalPlaces = countOfDecimalPlaces,
                Answers = new List<Answer>(specialValues ?? new Answer[] { })
            };

        public Answer Option(string value = null, string text = null, string parentValue = null, Guid? id = null)
            => new Answer
            {
                AnswerText = text ?? "text",
                AnswerValue = value ?? "1",
                ParentValue = parentValue
            };

        public Answer Option(int value, string text = null, string parentValue = null)
            => new Answer
            {
                AnswerText = text ?? $"Option {value}",
                AnswerCode = value,
                AnswerValue = value.ToString(),
                ParentValue = parentValue
            };

        public Answer OptionByCode(int value, string text = null, decimal? parentCode = null)
            => new Answer
            {
                AnswerText = text ?? $"Option {value}",
                AnswerCode = value,
                ParentCode = parentCode
            };

        public IEnumerable<Answer> Options(params int[] values)
        {
            return values.Select(value => Create.Entity.Option(value));
        }

        public PlainQuestionnaire PlainQuestionnaire(QuestionnaireDocument questionnaireDocument)
            => Create.Entity.PlainQuestionnaire(document: questionnaireDocument);

        public PlainQuestionnaire PlainQuestionnaire(QuestionnaireDocument document = null, long version = 1)
            => Create.Entity.PlainQuestionnaire(document, version, null);

        public PlainQuestionnaire PlainQuestionnaire(params IComposite[] children)
            => Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(null, null, children), 1L, null);

        public PlainQuestionnaire PlainQuestionnaire(QuestionnaireDocument document, long version, 
            Translation translation = null, 
            ISubstitutionService substitutionService = null, 
            IQuestionOptionsRepository questionOptionsRepository = null)
        {
            if (document != null)
            {
                document.IsUsingExpressionStorage = true;
                document.ExpressionsPlayOrder = document.ExpressionsPlayOrder 
                    ?? Create.Service.ExpressionsPlayOrderProvider().GetExpressionsPlayOrder(
                    document.AsReadOnly().AssignMissingVariables());
            }

            var plainQuestionnaire = new PlainQuestionnaire(document, version, questionOptionsRepository ?? Mock.Of<IQuestionOptionsRepository>(), 
                substitutionService ?? Mock.Of<ISubstitutionService>(), translation ?? document.Translations.FirstOrDefault());
            plainQuestionnaire.ExpressionStorageType = typeof(DummyInterviewExpressionStorage);
            return plainQuestionnaire;
        }

        public QRBarcodeQuestion QRBarcodeQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string variable = null, string validationMessage = null, string text = null, QuestionScope scope = QuestionScope.Interviewer, bool preFilled = false,
            bool hideIfDisabled = false)
            => new QRBarcodeQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                QuestionText = text,
                StataExportCaption = variable,
                QuestionScope = scope,
                Featured = preFilled
            };


        public AreaQuestion GeographyQuestion(Guid? id = null,
            string variable = "georgaphy_question",
            string enablementCondition = null,
            string validationExpression = null,
            QuestionScope scope = QuestionScope.Interviewer,
            bool isPrefilled = false,
            bool hideIfDisabled = false,
            bool useFormatting = false,
            string questionText = null,
            IEnumerable<ValidationCondition> validationConditions = null)
            => new AreaQuestion
            {
                QuestionText = questionText ?? "text",
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                QuestionScope = scope,
                Featured = isPrefilled,
                ValidationConditions = validationConditions?.ToList() ?? new List<ValidationCondition>()
            };

        public QuestionnaireBrowseItem QuestionnaireBrowseItem(
            Guid? questionnaireId = null, long? version = null, QuestionnaireIdentity questionnaireIdentity = null,
            string title = "Questionnaire Browse Item X", bool disabled = false, bool deleted = false, bool allowExportVariables = true, 
            string variable = "questionnaire")
            => new QuestionnaireBrowseItem
            {
                QuestionnaireId = questionnaireIdentity?.QuestionnaireId ?? questionnaireId ?? Guid.NewGuid(),
                Version = questionnaireIdentity?.Version ?? version ?? 1,
                Title = title,
                Disabled = disabled,
                IsDeleted = deleted,
                AllowExportVariables = allowExportVariables,
                Variable = variable
            };

        public QuestionnaireBrowseItem QuestionnaireBrowseItem(QuestionnaireDocument questionnaire, bool supportsAssignments = true, bool allowExportVariables = true, string comment = null, Guid? importedBy = null)
            => new QuestionnaireBrowseItem(questionnaire, 1, false, 1, supportsAssignments, allowExportVariables, comment, importedBy);

        public QuestionnaireDocument QuestionnaireDocument(Guid? id = null,
            string title = null,
            params IComposite[] children) => new QuestionnaireDocument
        {
            HideIfDisabled = true,
            PublicKey = id ?? Guid.NewGuid(),
            Title = title ?? "<Untitled>",
            Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
        }.WithEntityMap();
        
        public QuestionnaireDocument QuestionnaireDocumentWithCover(Guid? id = null,
            Guid? coverId = null,
            params IComposite[] children)
        {
            var document = new QuestionnaireDocument
            {
                HideIfDisabled = true,
                PublicKey = id ?? Guid.NewGuid(),
                Title = "<Untitled>",
                Children = children?.ToReadOnlyCollection() ??
                                                     new ReadOnlyCollection<IComposite>(new List<IComposite>())
            }.WithEntityMap();
            var cover = document.Children.First();
            ((Group)cover).PublicKey = coverId ?? Guid.NewGuid();
            document.CoverPageSectionId = cover.PublicKey;
            return document;
        }

        public QuestionnaireDocument QuestionnaireDocumentWithHideIfDisabled(Guid? id = null, bool hideIfDisabled = true, params IComposite[] children) => new QuestionnaireDocument
        {
            HideIfDisabled = hideIfDisabled,
            PublicKey = id ?? Guid.NewGuid(),
            Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
        }.WithEntityMap();

        public QuestionnaireDocument QuestionnaireDocumentWithAttachments(Guid? chapterId = null, params Attachment[] attachments)
            => new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter") { PublicKey = chapterId.GetValueOrDefault() }
                }.ToReadOnlyCollection(),
                Attachments = attachments.ToList()
            }.WithEntityMap();

        public QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Guid? chapterId = null, params IComposite[] children)
            => this.QuestionnaireDocumentWithOneChapter(chapterId, null, children).WithEntityMap();

        public QuestionnaireDocument QuestionnaireDocumentWithOneChapter(params IComposite[] children)
            => this.QuestionnaireDocumentWithOneChapter(null, null, children).WithEntityMap();

        public QuestionnaireDocument QuestionnaireDocumentWithOneChapterAndLanguages(Guid chapterId, string[] languages, params IComposite[] children)
            => new QuestionnaireDocument
            {
                PublicKey = Guid.NewGuid(),
                IsUsingExpressionStorage = true,
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = chapterId,
                        Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
                    }
                }.ToReadOnlyCollection(),
                Translations = new List<Translation>(languages.Select(x=>Create.Entity.Translation(Guid.NewGuid(), x)))
            }.WithEntityMap();

        public QuestionnaireDocument QuestionnaireDocumentWithOneChapterAndLanguages(Guid chapterId, 
            List<Translation> translations, Guid? defaultTranslation, params IComposite[] children)
            => new QuestionnaireDocument
            {
                PublicKey = Guid.NewGuid(),
                IsUsingExpressionStorage = true,
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = chapterId,
                        Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
                    }
                }.ToReadOnlyCollection(),
                DefaultTranslation = defaultTranslation,
                Translations = translations
            }.WithEntityMap();

        public QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Guid? chapterId = null, Guid? id = null, params IComposite[] children)
            => new QuestionnaireDocument
            {
                Title = "Questionnaire",
                VariableName = "MyQuestionnaire",
                IsUsingExpressionStorage = true,
                PublicKey = id ?? Guid.NewGuid(),
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = chapterId.GetValueOrDefault(),
                        Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
                    }
                }.ToReadOnlyCollection()
            }.WithEntityMap();

        public QuestionnaireDocument QuestionnaireDocumentWithOneQuestion(Guid? questionId = null, Guid? questionnaireId = null)
           => this.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: Create.Entity.TextQuestion(questionId));

        public QuestionnaireExportStructure QuestionnaireExportStructure(Guid? questionnaireId = null, long? version = null)
            => new QuestionnaireExportStructure
            {
                QuestionnaireId = questionnaireId ?? Guid.Empty,
                Version = version ?? 0
            };

        public QuestionnaireIdentity QuestionnaireIdentity(Guid? questionnaireId = null, long? questionnaireVersion = null)
            => new QuestionnaireIdentity(questionnaireId ?? Guid.NewGuid(), questionnaireVersion ?? 7);

        public QuestionnaireLevelLabels QuestionnaireLevelLabels(string levelName = "level", params DataExportVariable[] variableLabels)
            => new QuestionnaireLevelLabels(levelName, variableLabels);

        public ReadSideCacheSettings ReadSideCacheSettings(int cacheSizeInEntities = 128, int storeOperationBulkSize = 8)
            => new ReadSideCacheSettings(cacheSizeInEntities, storeOperationBulkSize);

        public InterviewTreeDoubleQuestion InterviewTreeDoubleQuestion(double answer = 42.42)
            => new InterviewTreeDoubleQuestion(answer);

        public Group MultiRoster(
            Guid? rosterId = null,
            string title = "Multi roster",
            string variable = "_multi_roster_var",
            Guid? rosterSizeQuestionId = null,
            string enablementCondition = null,
            IEnumerable<IComposite> children = null)
        {
            Group group = Create.Entity.Group(
                groupId: rosterId,
                title: title,
                variable: variable,
                enablementCondition: enablementCondition,
                children: children);

            group.IsRoster = true;
            group.RosterSizeSource = RosterSizeSourceType.Question;
            group.RosterSizeQuestionId = rosterSizeQuestionId;

            return group;
        }

        public Group ListRoster(
            Guid? rosterId = null,
            string title = "List roster",
            string variable = "_list_roster_var",
            Guid? rosterSizeQuestionId = null,
            string enablementCondition = null,
            IEnumerable<IComposite> children = null)
        {
            Group group = Create.Entity.Group(
                groupId: rosterId,
                title: title,
                variable: variable,
                enablementCondition: enablementCondition,
                children: children);

            group.IsRoster = true;
            group.RosterSizeSource = RosterSizeSourceType.Question;
            group.RosterSizeQuestionId = rosterSizeQuestionId;

            return group;
        }

        public Group NumericRoster(
            Guid? rosterId = null,
            string title = "Numeric roster",
            string variable = "_numeric_roster_var",
            Guid? rosterSizeQuestionId = null,
            Guid? rosterTitleQuestionId = null,
            string enablementCondition = null,
            IEnumerable<IComposite> children = null,
            RosterDisplayMode displayMode = RosterDisplayMode.SubSection)
        {
            Group group = Create.Entity.Group(
                groupId: rosterId,
                title: title,
                variable: variable,
                enablementCondition: enablementCondition,
                children: children);

            group.IsRoster = true;
            group.DisplayMode = displayMode;
            group.RosterSizeSource = RosterSizeSourceType.Question;
            group.RosterSizeQuestionId = rosterSizeQuestionId;
            group.RosterTitleQuestionId = rosterTitleQuestionId;

            return group;
        }

        private static int rostersCounter = 0;
        public Group Roster(
            Guid? rosterId = null,
            string title = "Roster X",
            string variable = null,
            string enablementCondition = null,
            string[] fixedTitles = null,
            IEnumerable<IComposite> children = null,
            RosterSizeSourceType? rosterSizeSourceType = null,
            Guid? rosterSizeQuestionId = null,
            Guid? rosterTitleQuestionId = null,
            FixedRosterTitle[] fixedRosterTitles = null,
            RosterDisplayMode displayMode = RosterDisplayMode.SubSection,
            bool hideIfDisabled = false)
        {
            Group group = Create.Entity.Group(
                groupId: rosterId,
                title: title,
                variable: variable ?? "rost_" + rostersCounter++,
                enablementCondition: enablementCondition,
                children: children);
            group.DisplayMode = displayMode;
            group.IsRoster = true;
            group.RosterSizeSource = rosterSizeSourceType ?? (rosterSizeQuestionId.HasValue ? RosterSizeSourceType.Question : RosterSizeSourceType.FixedTitles);
            group.HideIfDisabled = hideIfDisabled;
            if (group.RosterSizeSource == RosterSizeSourceType.FixedTitles)
            {
                if (fixedRosterTitles == null)
                {
                    group.FixedRosterTitles =
                        (fixedTitles ?? new[] { "Roster X-1", "Roster X-2", "Roster X-3" }).Select(
                            (x, i) => Create.Entity.FixedTitle(i, x)).ToArray();
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

        public RosterVector RosterVector(params decimal[] coordinates)
            => new RosterVector(coordinates ?? Enumerable.Empty<decimal>());

        public SingleQuestion SingleOptionQuestion(
            Guid? questionId = null,
            string variable = null,
            string enablementCondition = null,
            string validationExpression = null,
            Guid? linkedToQuestionId = null,
            Guid? cascadeFromQuestionId = null,
            decimal[] answerCodes = null,
            decimal[] parentCodes = null,
            string title = null,
            bool hideIfDisabled = false,
            string linkedFilterExpression = null,
            Guid? linkedToRosterId = null,
            bool? isFilteredCombobox = null,
            string optionsFilterExpression = null,
            List<Answer> answers = null,
            bool isPrefilled = false,
            int? showAsListThreshold = null,
            Guid? categoryId = null)
        {
            answers = answers ?? (answerCodes ?? new decimal[] { 1, 2, 3 }).Select(a => Create.Entity.Answer(a.ToString(), a)).ToList();
            if (parentCodes != null)
            {
                for (int i = 0; i < parentCodes.Length; i++)
                {
                    answers[i].ParentValue = parentCodes[i].ToString(CultureInfo.InvariantCulture);
                }
            }
            return new SingleQuestion
            {
                Featured = isPrefilled,
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = variable ?? "single_option_question",
                QuestionText = title ?? "SO Question",
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                CascadeFromQuestionId = cascadeFromQuestionId,
                Answers = answers,
                LinkedFilterExpression = linkedFilterExpression,
                IsFilteredCombobox = isFilteredCombobox,
                Properties = new QuestionProperties(false, false)
                {
                    OptionsFilterExpression = optionsFilterExpression
                },
                ShowAsList = showAsListThreshold.HasValue,
                ShowAsListThreshold = showAsListThreshold,
                CategoriesId = categoryId 
            };
        }

        public SingleQuestion SingleQuestion(Guid? id = null, string variable = null, 
            string enablementCondition = null,
            string validationExpression = null,
            Guid? cascadeFromQuestionId = null,
            List<Answer> options = null, 
            Guid? linkedToQuestionId = null, 
            QuestionScope scope = QuestionScope.Interviewer,
            bool isFilteredCombobox = false, 
            Guid? linkedToRosterId = null,
            string linkedFilter = null,
            string optionsFilter = null,
            bool showAsList = false,
            Guid? categoryId = null)
            => new SingleQuestion
            {
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Answers = options ?? new List<Answer>(),
                CascadeFromQuestionId = cascadeFromQuestionId,
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                QuestionScope = scope,
                IsFilteredCombobox = isFilteredCombobox,
                LinkedFilterExpression = linkedFilter,
                Properties = new QuestionProperties(false, false)
                {
                    OptionsFilterExpression = optionsFilter
                },
                ShowAsList = showAsList,
                CategoriesId = categoryId,
            };

        public StaticText StaticText(
            Guid? publicKey = null,
            string text = "Static Text X",
            string attachmentName = null,
            string enablementCondition = null,
            List<ValidationCondition> validationConditions = null)
            => new StaticText(
                publicKey ?? Guid.NewGuid(),
                text,
                enablementCondition,
                false,
                validationConditions ?? new List<ValidationCondition>(),
                attachmentName);

        public InterviewTreeTextQuestion InterviewTreeTextQuestion(string answer)
            => new InterviewTreeTextQuestion(answer);

        public TextListQuestion TextListQuestion(Guid? questionId = null, 
            string enablementCondition = null, 
            string validationExpression = null,
            int? maxAnswerCount = null, 
            string variable = null, 
            bool hideIfDisabled = false,
            string questionText = null)
            => new TextListQuestion()
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                MaxAnswerCount = maxAnswerCount,
                StataExportCaption = variable,
                QuestionText = questionText
            };

        public TextQuestion TextQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string mask = null,
            string variable = null,
            string validationMessage = null,
            string text = "Question T",
            QuestionScope scope = QuestionScope.Interviewer,
            bool preFilled = false,
            string label = null,
            string instruction = null,
            IEnumerable<ValidationCondition> validationConditions = null,
            bool hideIfDisabled = false)
            => new TextQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Mask = mask,
                QuestionText = text,
                StataExportCaption = variable ?? "vv" + Guid.NewGuid().ToString("N"),
                QuestionScope = scope,
                Featured = preFilled,
                VariableLabel = label,
                Instructions = instruction,
                ValidationConditions = validationConditions?.ToList().ConcatWithOldConditionIfNotEmpty(validationExpression, validationMessage)
            };

        public TimeSpanBetweenStatuses TimeSpanBetweenStatuses(
            Guid? interviewerId = null, Guid? supervisorId = null, DateTime? timestamp = null, TimeSpan? timeSpanWithPreviousStatus = null,
            InterviewExportedAction endStatus = InterviewExportedAction.ApprovedByHeadquarter)
            => new TimeSpanBetweenStatuses
            {
                BeginStatus = InterviewExportedAction.InterviewerAssigned,
                EndStatus = endStatus,
                EndStatusTimestamp = timestamp ?? DateTime.Now,
                InterviewerId = interviewerId ?? Guid.NewGuid(),
                SupervisorId = supervisorId ?? Guid.NewGuid(),
                TimeSpan = timeSpanWithPreviousStatus ?? new TimeSpan()
            };


        public UserView UserView(Guid? userId = null, Guid? supervisorId = null, bool? isArchived = null,
            string userName = "name", bool isLockedByHQ = false, UserRoles role = UserRoles.Interviewer,
            string deviceId = null)
            => new UserView
            {
                PublicKey = userId ?? Guid.NewGuid(),
                IsArchived = isArchived ?? false,
                UserName = userName,
                IsLockedByHQ = isLockedByHQ,
                Supervisor = new UserLight(supervisorId ?? Guid.NewGuid(), "supervisor"),
                Roles = new SortedSet<UserRoles>(new[] {role})
            };

        public UserViewLite UserViewLite(Guid? userId = null, Guid? supervisorId = null, bool? isArchived = null,
            string userName = "name", bool isLockedByHQ = false, UserRoles role = UserRoles.Interviewer,
            string deviceId = null)
            => new UserViewLite
            {
                PublicKey = userId ?? Guid.NewGuid(),
                UserName = userName,
                Supervisor = new UserLight(supervisorId ?? Guid.NewGuid(), "supervisor"),
                Roles = new SortedSet<UserRoles>(new[] {role})
            };

        public HqRole HqRole(UserRoles role) => new HqRole
        {
            Id = role.ToUserId(),
            Name = role.ToString()
        };

        public HqUser HqUser(Guid? userId = null, Guid? supervisorId = null, bool? isArchived = null,
            string userName = "name", bool isLockedByHQ = false, UserRoles role = UserRoles.Interviewer,
            string deviceId = null, string passwordHash = null, string passwordHashSha1 = null, string interviewerVersion = null,
            int? interviewerBuild = null,
            bool lockedBySupervisor = false,
            string securityStamp = null, string[] workspaces = null)
        {

            var user = new HqUser
            {
                Id = userId ?? Guid.NewGuid(),
                IsArchived = isArchived ?? false,
                UserName = userName,
                IsLockedByHeadquaters = isLockedByHQ,
                FullName = string.Empty,
                IsLockedBySupervisor = lockedBySupervisor,
                WorkspaceProfile = new WorkspaceUserProfile(),
                Profile = new HqUserProfile
                {
                    DeviceId = deviceId,
                    DeviceAppBuildVersion = interviewerBuild,
                    DeviceAppVersion = interviewerVersion
                },
                PasswordHash = passwordHash,
                PasswordHashSha1 = passwordHashSha1,
                Roles = new List<HqRole> { Create.Entity.HqRole(role) },

                SecurityStamp = securityStamp ?? Guid.NewGuid().ToString()
            };

            var userProfile = user.WorkspaceProfile.AsDynamic();
            userProfile.SupervisorId = supervisorId;
            userProfile.DeviceId = deviceId;
            userProfile.DeviceAppBuildVersion = interviewerBuild;
            userProfile.DeviceAppVersion = interviewerVersion;

            workspaces ??= new[] {WorkspaceConstants.DefaultWorkspaceName};

            foreach (var workspace in workspaces)
            {
                var ws = new Workspace(workspace, workspace);
                user.Workspaces.Add(new WorkspacesUsers(ws, user, supervisorId != null ? new HqUser{Id = supervisorId.Value}: null));
            }
            
            return user;
        }

        public UserView UserDocument()
            => Create.Entity.UserDocument(userId: null);

        public UserView UserDocument(Guid? userId = null, Guid? supervisorId = null, bool? isArchived = null, string userName = "name", bool isLockedByHQ = false)
        {
            var user = new UserView { PublicKey = userId ?? Guid.NewGuid(), IsArchived = isArchived ?? false, UserName = userName, IsLockedByHQ = isLockedByHQ };
            if (supervisorId.HasValue)
            {
                user.Roles.Add(UserRoles.Interviewer);
                user.Supervisor = new UserLight(supervisorId.Value, "supervisor");
            }
            else
            {
                user.Roles.Add(UserRoles.Supervisor);
            }
            return user;
        }

        public UserLight UserLight(Guid? userId = null)
            => new UserLight(userId ?? Guid.NewGuid(), "test");

        public UserToImport UserToImport(
            string login = "test", string supervisor = "", string password = "P@$$w0rd$less", string email = "", string phoneNumber = "",
            string role = null, string fullName = null, string workspace = null)
            => new UserToImport
            {
                Login = login,
                Supervisor = supervisor,
                Role = role ?? (string.IsNullOrEmpty(supervisor) ? "supervisor" : "interviewer"),
                Password = password,
                Email = email,
                PhoneNumber = phoneNumber,
                FullName = fullName,
                Workspace = workspace,
            };

        public UserPreloadingSettings UserPreloadingSettings()
            => new UserPreloadingSettings(
                10000, "^[a-zA-Z0-9_]{3,15}$",
                @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$",
                @"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$", 100, 15,
                @"^[\p{L} '.-]+$");

        public UserImportVerificationError UserPreloadingVerificationError()
            => new UserImportVerificationError();

        public ValidationCondition ValidationCondition(string expression = "self != null", string message = "should be answered", ValidationSeverity severity = ValidationSeverity.Error)
            => new ValidationCondition(expression, message) { Severity =  severity };

        public ValidationCondition WarningCondition(string expression = "self != null", string message = "warning about unanswered question") 
            => this.ValidationCondition(expression, message, ValidationSeverity.Warning);

        public Variable Variable(Guid? id = null, VariableType type = VariableType.LongInteger, string variableName = "v1", string expression = "2*2", bool doNotExport = false)
            => new Variable(
                id ?? Guid.NewGuid(),
                new VariableData(type, variableName, expression, null, doNotExport));

        public VariableValueLabel VariableValueLabel(string value = "1", string label = "l1")
            => new VariableValueLabel(value, label);

        public InterviewTreeYesNoQuestion InterviewTreeYesNoQuestion(AnsweredYesNoOption[] answer)
            => new InterviewTreeYesNoQuestion(answer);

        public MultyOptionsQuestion YesNoQuestion(Guid? questionId = null, int[] answers = null, bool ordered = false,
            int? maxAnswersCount = null, string variable = null)
        {
            var yesNo = Create.Entity.MultipleOptionsQuestion(
                isYesNo: true,
                questionId: questionId,
                answers: answers ?? new int[] {},
                areAnswersOrdered: ordered,
                maxAllowedAnswers: maxAnswersCount);
            yesNo.StataExportCaption = variable;
            return yesNo;
        }

        public Enumerator.Native.Questionnaire.TranslationInstance TranslationInstance(string value = null,
            Guid? translationId = null, 
            QuestionnaireIdentity questionnaireId = null,
            Guid? entityId = null, 
            string translationIndex = null, 
            TranslationType? type = null)
        {
            return new Enumerator.Native.Questionnaire.TranslationInstance
            {
                Value = value,
                TranslationId = translationId ?? Guid.NewGuid(),
                QuestionnaireId = questionnaireId ?? Create.Entity.QuestionnaireIdentity(),
                QuestionnaireEntityId = entityId ?? Guid.NewGuid(),
                TranslationIndex = translationIndex,
                Type = type ?? TranslationType.Unknown
            };
        }

        public QuestionnairePdf QuestionnairePdf()
        {
            return new QuestionnairePdf
            {
                Content = new byte[]{4,4,4}
            };
        }

        public WB.Core.SharedKernels.Enumerator.Views.TranslationInstance TranslationInstance_Enumetaror(string value = null,
            Guid? tranlationId = null,
            string questionnaireId = null,
            Guid? entityId = null,
            string translationIndex = null,
            TranslationType? type = null)
        {
            return new TranslationInstance
            {
                Id = Guid.NewGuid().ToString(),
                Value = value,
                TranslationId = tranlationId ?? Guid.NewGuid(),
                QuestionnaireId = questionnaireId ?? Create.Entity.QuestionnaireIdentity().ToString(),
                QuestionnaireEntityId = entityId ?? Guid.NewGuid(),
                TranslationIndex = translationIndex,
                Type = type ?? TranslationType.Unknown
            };
        }

        public TranslationDto TranslationDto(string value = null,
            Guid? translationId = null,
            string questionnaireId = null,
            Guid? entityId = null,
            string translationIndex = null,
            TranslationType? type = null)
        {
            return new TranslationDto
            {
                Value = value,
                TranslationId = translationId ?? Guid.NewGuid(),
                QuestionnaireEntityId = entityId ?? Guid.NewGuid(),
                TranslationIndex = translationIndex,
                Type = type ?? TranslationType.Unknown
            };
        }

        public RosterSynchronizationDto RosterSynchronizationDto(Guid rosterId,
            RosterVector outerScopeRosterVector = null, decimal? rosterInstanceId = null, int? sortIndex = null,
            string rosterTitle = null)
            => new RosterSynchronizationDto(rosterId,
                    outerScopeRosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty,
                    rosterInstanceId ?? 0, sortIndex ?? 0, rosterTitle ?? "roster title");
        
        public PrefilledQuestionView PrefilledQuestionView(Guid? interviewId = null, 
            string answer = null, 
            Guid? questionId = null,
            string questionText = null)
        {
            var prefilledQuestionView = new PrefilledQuestionView
            {
                InterviewId = interviewId ?? Guid.NewGuid(),
                Answer = answer,
                QuestionId = questionId ?? Guid.NewGuid(),
                QuestionText = questionText
            };
            prefilledQuestionView.Id = $"{prefilledQuestionView.InterviewId:N}${prefilledQuestionView.QuestionId}";
            return prefilledQuestionView;
        }

        public QuestionnaireView QuestionnaireView(QuestionnaireIdentity questionnaireIdentity)
        {
            return new QuestionnaireView { Id = questionnaireIdentity.ToString()};
        }

        public InterviewTreeRoster InterviewTreeRoster(Identity rosterIdentity, bool isDisabled = false, string rosterTitle = null,
            RosterType rosterType = RosterType.Fixed, Guid? rosterSizeQuestion = null, Identity rosterTitleQuestionIdentity = null,
            params IInterviewTreeNode[] children)
        {
            var titleWithSubstitutions = Create.Entity.SubstitutionText(rosterIdentity, "Title");
            var roster =  new InterviewTreeRoster(rosterIdentity, titleWithSubstitutions, rosterType: rosterType,
                rosterSizeQuestion: rosterSizeQuestion,
                rosterTitleQuestionIdentity: rosterTitleQuestionIdentity,
                childrenReferences: Enumerable.Empty<QuestionnaireItemReference>()) {RosterTitle = rosterTitle};
            roster.SetChildren(children.ToList());
            return roster;
        }

        public InterviewTreeSubSection InterviewTreeSubSection(Identity groupIdentity, bool isDisabled = false, 
            params IInterviewTreeNode[] children)
        {
            var titleWithSubstitutions = Create.Entity.SubstitutionText(groupIdentity, "Title");
            var subSection = new InterviewTreeSubSection(groupIdentity, titleWithSubstitutions, Enumerable.Empty<QuestionnaireItemReference>());
            subSection.AddChildren(children);
            if (isDisabled) subSection.Disable();
            return subSection;
        }

        public InterviewTreeSection InterviewTreeSection(Identity sectionIdentity = null, bool isDisabled = false, params IInterviewTreeNode[] children)
        {
            sectionIdentity = sectionIdentity ?? Create.Entity.Identity(Guid.NewGuid());

            var titleWithSubstitutions = Create.Entity.SubstitutionText(sectionIdentity, "Title");
            var section = new InterviewTreeSection(sectionIdentity, titleWithSubstitutions, Enumerable.Empty<QuestionnaireItemReference>());
            section.AddChildren(children);
            if (isDisabled)
                section.Disable();
            return section;
        }

        public InterviewTreeStaticText InterviewTreeStaticText(Identity staticTextIdentity, bool isDisabled = false)
        {
            var titleWithSubstitutions = Create.Entity.SubstitutionText(staticTextIdentity, "Title");
            var staticText = new InterviewTreeStaticText(staticTextIdentity, titleWithSubstitutions);
            if (isDisabled) staticText.Disable();
            return staticText;
        }

        public InterviewTreeVariable InterviewTreeVariable(Identity variableIdentity, bool isDisabled = false, object value = null)
        {
            var variable = new InterviewTreeVariable(variableIdentity);
            if (isDisabled) variable.Disable();
            variable.SetValue(value);
            return variable;
        }

        public InterviewTreeQuestion InterviewTreeQuestion_SingleOption(Identity questionIdentity,
            bool isDisabled = false, string title = "title", string instructions = "inst", string variableName = "var", int? answer = null)
        {
            var question = this.InterviewTreeQuestion(questionIdentity, isDisabled, title,instructions,
                variableName, QuestionType.SingleOption, answer, null, null, false, false);
            if (isDisabled) question.Disable();
            return question;
        }

        public InterviewTreeQuestion InterviewTreeQuestion(Identity questionIdentity, bool isDisabled = false, 
            string title = "title", string instructions = "instructions",
            string variableName = "var", QuestionType questionType = QuestionType.Text, 
            object answer = null, IEnumerable<RosterVector> linkedOptions = null,
            Guid? cascadingParentQuestionId = null, 
            bool isYesNo = false, 
            bool isDecimal = false, 
            Guid? linkedSourceId = null, 
            Guid[] questionsUsingForSubstitution = null,
            bool? isTimestamp = false)
        {
            var titleWithSubstitutions = Create.Entity.SubstitutionText(questionIdentity, title);
            var instructionsWithSubstitutions = Create.Entity.SubstitutionText(questionIdentity, instructions);
            
            var question = new InterviewTreeQuestion(questionIdentity, titleWithSubstitutions, instructionsWithSubstitutions, 
                variableName, questionType, answer, linkedOptions, 
                cascadingParentQuestionId, isYesNo,  isDecimal, false, isTimestamp ?? false, linkedSourceId);

            if (isDisabled) question.Disable();
            return question;
        }

        public SubstitutionText SubstitutionText(Identity identity, 
            string title,
            List<SubstitutionVariable> variables = null)
        {
            return new SubstitutionText(identity, "self", title, variables ?? new List<SubstitutionVariable>(), Mock.Of<ISubstitutionService>(), Mock.Of<IVariableToUIStringService>());
        }

        public InterviewTree InterviewTree(Guid? interviewId = null, IQuestionnaire questionnaire = null,
            params InterviewTreeSection[] sections)
        {
            var tree = new InterviewTree(
                interviewId ?? Guid.NewGuid(),
                questionnaire ?? Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument()),
                Create.Service.SubstitutionTextFactory());

            tree.SetSections(sections);

            return tree;
        }

        public CategoricalFixedSingleOptionAnswer SingleOptionAnswer(int answer)
        {
            return CategoricalFixedSingleOptionAnswer.FromInt(answer);
        }

        public CategoricalFixedMultiOptionAnswer MultiOptionAnswer(params int[] selectedOptions)
        {
            return CategoricalFixedMultiOptionAnswer.Convert(selectedOptions);
        }

        
        public TextListAnswer ListAnswer(params int[] answers)
        {
            return TextListAnswer.FromTextListAnswerRows(answers.Select(x => new TextListAnswerRow(x, $"answer #{x}")));
        }

        public TextAnswer TextQuestionAnswer(string answer)
        {
            return TextAnswer.FromString(answer);
        }

        public CategoricalLinkedSingleOptionAnswer LinkedSingleOptionAnswer(RosterVector selectedValue)
        {
            return CategoricalLinkedSingleOptionAnswer.FromRosterVector(selectedValue);
        }

        public NumericIntegerAnswer NumericIntegerAnswer(int i)
            => Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers
                    .NumericIntegerAnswer.FromInt(i);

        public YesNoAnswer YesNoAnswer(IEnumerable<AnsweredYesNoOption> answer)
            => Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers
                    .YesNoAnswer.FromAnsweredYesNoOptions(answer);

        public PreloadedLevelDto PreloadedLevelDto(RosterVector rosterVector, params PreloadedAnswer[] answeres)
        {
            return new PreloadedLevelDto(rosterVector, answeres.ToDictionary(answer => answer.Id, answer => answer.Answer));
        }

        public NavigationIdentity NavigationIdentity(Identity navigateTo)
            => new NavigationIdentity() {TargetScreen = ScreenType.Group, TargetGroup = navigateTo};

        public ScreenChangedEventArgs ScreenChangedEventArgs(ScreenType targetStage = ScreenType.Group, Identity targetGroup = null)
            => new ScreenChangedEventArgs(targetStage, targetGroup, null, ScreenType.Group, null);
        
        public FeaturedQuestionItem FeaturedQuestionItem(Guid? id = null, string title = "title", string caption = "caption") 
            => new FeaturedQuestionItem(id ?? Guid.NewGuid(), title,  caption);

        public SampleUploadView SampleUploadView(Guid? questionnaireId = null, int? version = null, List<FeaturedQuestionItem> featuredQuestionItems = null) 
            => new SampleUploadView(questionnaireId ?? Guid.NewGuid(), version ?? 1, featuredQuestionItems, null, null);

        public AnswerNotifier AnswerNotifier(ViewModelEventRegistry liteEventRegistry)
            => new AnswerNotifier(liteEventRegistry);

        public ChangedVariable ChangedVariable(Identity changedVariable, object newValue)
            => new ChangedVariable(changedVariable, newValue);

        public InterviewTextListAnswers InterviewTextListAnswers(IEnumerable<Tuple<decimal, string>> answers)
        {
            return new InterviewTextListAnswers(answers);
        }

        public RosterVector RosterVector(int[] coordinates)
        {
            return new RosterVector(coordinates);
        }

        public CompanyLogo HqCompanyLogo(bool withContent = true)
        {
            var hqCompanyLogo = new CompanyLogo();
            return hqCompanyLogo;
        }

        public InterviewKey InterviewKey(int key = 289)
        {
            return new InterviewKey(key);
        }

        public BrokenInterviewPackagesView BrokenInterviewPackagesView(params BrokenInterviewPackageView[] brokenPackages)
        {
            return new BrokenInterviewPackagesView
            {
                Items = brokenPackages,
                TotalCount = brokenPackages.Length
            };
        }

        public BrokenInterviewPackageView BrokenInterviewPackageView()
        {
            return new BrokenInterviewPackageView();
        }

        public BrokenInterviewPackage BrokenInterviewPackage(DateTime? processingDate = null, DateTime? incomingDate = null,
            InterviewDomainExceptionType? exceptionType =null)
        {
            return new BrokenInterviewPackage
            {
                ProcessingDate = processingDate ?? DateTime.Now,
                IncomingDate = incomingDate ?? DateTime.Now,
                ExceptionType = exceptionType?.ToString() ?? "Unexpected"
            };
        }

        public InterviewPackage InterviewPackage(Guid? interviewId = null, params IEvent[] events)
        {
            var serializer = new JsonAllTypesSerializer();
            var aggregateRootEvents = events?.Select(x =>  Create.Event.AggregateRootEvent(x)).ToArray() ?? new AggregateRootEvent[0];
            return new InterviewPackage
            {
                InterviewId = interviewId ?? Guid.NewGuid(),
                Events = serializer.Serialize(aggregateRootEvents)
            };
        }

        public AssignmentApiView AssignmentApiView(int id, int? quantity, QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new AssignmentApiView
            {
                Id = id,
                Quantity = quantity,
                QuestionnaireId = questionnaireIdentity
            };
        }

        public AssignmentApiDocumentBuilder AssignmentApiDocument(int id, int? quantity, QuestionnaireIdentity questionnaireIdentity = null,
            Guid? responsibleId = null)
        {
            return new AssignmentApiDocumentBuilder(new AssignmentApiDocument
            {
                Id = id,
                Quantity = quantity,
                QuestionnaireId = questionnaireIdentity,
                ResponsibleId = responsibleId ?? Guid.Empty
            });
        }

        public class AssignmentApiDocumentBuilder
        {
            private readonly AssignmentApiDocument _entity;
            private static readonly NewtonInterviewAnswerJsonSerializer serializer = new NewtonInterviewAnswerJsonSerializer();

            public AssignmentApiDocumentBuilder(AssignmentApiDocument entity)
            {
                _entity = entity;
            }

            public AssignmentApiDocumentBuilder WithAnswer(Identity questionId, string answer, string serializedAnswer = null, 
                AbstractAnswer answerAsObj = null, double? latitude = null, double? longtitude = null)
            {
                this._entity.Answers.Add(new AssignmentApiDocument.InterviewSerializedAnswer
                { 
                    Identity = questionId,
                    SerializedAnswer = serializedAnswer ?? (answerAsObj == null ? null :serializer.Serialize(answerAsObj))
                });

                if (latitude != null && longtitude != null)
                {
                    this._entity.LocationQuestionId = questionId.Id;
                    this._entity.LocationLatitude = latitude;
                    this._entity.LocationLongitude = longtitude;
                }

                return this;
            }

            public AssignmentApiDocument Build() => this._entity;
        }

        public AssignmentToImport AssignmentToImport(int? id = null, string password = null)
        {
            return new AssignmentToImport
            {
                Id = id ?? 0,
                Password = password
            };
        }

        public Assignment Assignment(int? id = null,
            QuestionnaireIdentity questionnaireIdentity = null,
            int? quantity = null,
            Guid? assigneeSupervisorId = null,
            string responsibleName = null,
            IEnumerable<InterviewSummary> interviewSummary = null,
            string questionnaireTitle = null, 
            DateTime? updatedAt = null,
            Guid? responsibleId = null,
            List<string> protectedVariables = null,
            string email = null,
            string password = null,
            bool? webMode = null,
            bool isArchived = false,
            Guid? publicKey = null,
            bool isAudioRecordingEnabled = false,
            List<InterviewAnswer> answers = null,
            List<IdentifyingAnswer> identifyingAnswers = null
            )
        {
            var result = new Assignment();

            result.Quantity = quantity;
            result.Id = id ?? 0;
            result.PublicKey = publicKey ?? Guid.NewGuid();
            result.QuestionnaireId = questionnaireIdentity ?? Create.Entity.QuestionnaireIdentity();
            result.Archived = isArchived;

            var readonlyUser = new ReadonlyUser
            {
            };
            readonlyUser.RoleIds.Add(UserRoles.Interviewer.ToUserId());

            var readonlyProfile = new WorkspaceUserProfile();
            readonlyUser.AsDynamic().ReadonlyProfile = readonlyProfile;
            result.AsDynamic().Responsible = readonlyUser;

            if (assigneeSupervisorId.HasValue)
            {
                readonlyProfile.AsDynamic().SupervisorId = assigneeSupervisorId;
            }

            if (!string.IsNullOrEmpty(responsibleName))
            {
                readonlyUser.AsDynamic().Name = responsibleName;
            }

            if (!string.IsNullOrWhiteSpace(questionnaireTitle))
            {
                result.AsDynamic().Questionnaire = Create.Entity.QuestionnaireBrowseItem(questionnaireId: questionnaireIdentity?.QuestionnaireId, 
                    version: questionnaireIdentity?.Version, title: questionnaireTitle);
            }

            if (updatedAt.HasValue)
            {
                result.AsDynamic().UpdatedAtUtc = updatedAt.Value;
            }

            if(interviewSummary != null)
                result.AsDynamic().InterviewSummaries =  new HashSet<InterviewSummary>(interviewSummary);
            if (responsibleId.HasValue)
            {
                result.ResponsibleId = responsibleId.Value;
            }
            result.ProtectedVariables = protectedVariables;
            result.Email = email;
            result.Password = password;
            result.WebMode = webMode;
            result.AudioRecording = isAudioRecordingEnabled;
            result.Answers = answers ?? new List<InterviewAnswer>();
            result.IdentifyingData = identifyingAnswers ?? new List<IdentifyingAnswer>();

            return result;
        }

        public IdentifyingAnswer IdentifyingAnswer(Assignment assignment = null, Identity identity = null, 
            string answer = null,
            string answerAsString = null,
            string variable = null)
        {
            var result = new IdentifyingAnswer();
            
            dynamic dynamic = result.AsDynamic();
            dynamic.Assignment = assignment;
            dynamic.Identity = identity;
            dynamic.Answer = answer;
            dynamic.AnswerAsString = answerAsString;
            dynamic.VariableName = variable;
            return result;
        }

        public AssignmentDocumentBuilder AssignmentDocument(int id, int? quantity = null,
            int interviewsCount = 0, string questionnaireIdentity = null, Guid? responsibleId = null, Guid? originalResponsibleId = null)
        {
            return new AssignmentDocumentBuilder(new AssignmentDocument
            {
                Id = id,
                Quantity = quantity,
                QuestionnaireId = questionnaireIdentity,
                ResponsibleId = responsibleId ?? Guid.Empty,
                OriginalResponsibleId = originalResponsibleId ?? Guid.Empty,
                CreatedInterviewsCount = interviewsCount
            });
        }

        public class AssignmentDocumentBuilder
        {
            private readonly AssignmentDocument _entity;

            public AssignmentDocumentBuilder(AssignmentDocument entity)
            {
                _entity = entity;
            }

            public AssignmentDocumentBuilder WithAnswer(Identity identity, string answer, bool identifying = false, string serializedAnswer = null, int? sortOrder = null)
            {
                this._entity.Answers = this._entity.Answers ?? new List<AssignmentDocument.AssignmentAnswer>();
                this._entity.IdentifyingAnswers = this._entity.IdentifyingAnswers ?? new List<AssignmentDocument.AssignmentAnswer>();
                
                var assignmentAnswer = new AssignmentDocument.AssignmentAnswer
                {
                    AssignmentId = this._entity.Id,
                    AnswerAsString = answer,
                    IsIdentifying = identifying,
                    SerializedAnswer = serializedAnswer,
                    Identity = identity,
                    SortOrder = sortOrder
                };

                this._entity.Answers.Add(assignmentAnswer);

                if (identifying)
                {
                    this._entity.IdentifyingAnswers.Add(assignmentAnswer);
                }

                return this;
            }

            public AssignmentDocumentBuilder WithTitle(string title)
            {
                this._entity.Title = title;
                return this;
            }

            public AssignmentDocumentBuilder WithResponsible(Guid responsibleId)
            {
                this._entity.ResponsibleId = responsibleId;
                return this;
            }

            public AssignmentDocument Build() => this._entity;
        }

        public InterviewAnswer InterviewAnswer(Identity identity, AbstractAnswer answer)
        {
            return new InterviewAnswer
            {
                Identity = identity,
                Answer = answer
            };
        }

        public AssignmentImportStatus AssignmentImportStatus()
        {
            return new AssignmentImportStatus();
        }

        public QuestionnaireExportStructure QuestionnaireExportStructure(QuestionnaireDocument questionnaire)
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor
                .Setup(x => x.MakeStataCompatibleFileName(It.IsAny<string>()))
                .Returns((string f) => f);

            var exportViewFactory = new ExportViewFactory(
                fileSystemAccessor.Object,
                Mock.Of<IQuestionnaireStorage>(s => s.GetQuestionnaireDocument(It.IsAny<QuestionnaireIdentity>()) == questionnaire),
                new RosterStructureService(),
                Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>());
            return exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(Guid.NewGuid(), 1));
        }

        public AudioQuestion AudioQuestion(Guid? qId = null, string variable = "audio_question")
        {
            return new AudioQuestion
            {
                PublicKey = qId?? Guid.NewGuid(),
                StataExportCaption = variable,
                QuestionScope = QuestionScope.Interviewer,
            };
        }

        public SampleImportSettings SampleImportSettings(int limit = 1) => new SampleImportSettings(limit);

        public AssignmentImportData AssignmentImportData(Guid interviewerId, Guid? supervisorId = null, params PreloadedLevelDto[] levels)
        {
            return new AssignmentImportData
            {
                InterviewerId = interviewerId,
                SupervisorId = supervisorId,
                Quantity = 1,
                PreloadedData = new PreloadedDataDto(levels ?? new PreloadedLevelDto[0])
            };
        }

        public AssignmentImportData AssignmentImportData(Guid interviewerId, Guid? supervisorId = null, params InterviewAnswer[] answers)
        {
            var levels = answers.GroupBy(x => x.Identity.RosterVector)
                .Select(x => new PreloadedLevelDto(x.Key, x.ToDictionary(a => a.Identity.Id, a => a.Answer)))
                .ToArray();

            return new AssignmentImportData
            {
                InterviewerId = interviewerId,
                SupervisorId = supervisorId,
                Quantity = 1,
                PreloadedData = new PreloadedDataDto(levels)
            };
        }

        public CumulativeReportStatusChange CumulativeReportStatusChange(string entryId, 
            Guid questionnaireId, long questionnaireVersion, DateTime date, InterviewStatus status, int changeValue,
            Guid interviewId, long eventSequence)
        {
            return new CumulativeReportStatusChange(entryId, questionnaireId, questionnaireVersion, date, status,
                changeValue, interviewId, eventSequence);
        }

        public SyncSettings SyncSettings()
        {
            return new SyncSettings("hq");
        }

        public InterviewTreeVariableDiff InterviewTreeVariableDiff(InterviewTreeVariable sourceVariable, InterviewTreeVariable targetVariable)
        {
            return new InterviewTreeVariableDiff(sourceVariable, targetVariable);
        }

        public DeviceSyncInfo DeviceSyncInfo(Guid interviewerId, string deviceId)
        {
            return new DeviceSyncInfo
            {
                SyncDate = DateTime.UtcNow,
                InterviewerId = interviewerId,
                DeviceId = deviceId,
                LastAppUpdatedDate = DateTime.UtcNow.AddDays(-30),
                DeviceModel = "DeviceModel",
                DeviceType = "DeviceType",
                AndroidVersion = "Android",
                DeviceLanguage = "DeviceLanguage",
                DeviceBuildNumber = "DeviceBuildNumber",
                DeviceSerialNumber = "DeviceSerialNumber",
                DeviceManufacturer = "DeviceManufacturer",
                DBSizeInfo = 73 * 1024 * 1024,
                AndroidSdkVersion = 25,
                AndroidSdkVersionName = "AndroidSdkVersionName",
                DeviceDate = DateTime.UtcNow.AddHours(-1),
                AppVersion = "AppVersion",
                AppBuildVersion = 1697,
                MobileSignalStrength = 7,
                AppOrientation = "AppOrientation",
                MobileOperator = "MobileOperator",
                NetworkSubType = "NetworkSubType",
                NetworkType = "NetworkType",
                BatteryChargePercent = 88,
                BatteryPowerSource = "BatteryPowerSource",
                IsPowerInSaveMode = false,
                DeviceLocationLat = 14.15,
                DeviceLocationLong = 16.17,
                NumberOfStartedInterviews = 10,
                RAMFreeInBytes = 50 * 1024 * 1024,
                RAMTotalInBytes = 1024 * 1024 * 1024,
                StorageFreeInBytes = 5 * 1024 * 1024,
                StorageTotalInBytes = 2000 * 1024 * 1024,
                Statistics = new SyncStatistics
                {
                    UploadedInterviewsCount = 0,
                    DownloadedInterviewsCount = 0,
                    DownloadedQuestionnairesCount = 0,
                    RejectedInterviewsOnDeviceCount  = 0,
                    NewInterviewsOnDeviceCount  = 0,
                    NewAssignmentsCount = 0,
                    RemovedAssignmentsCount = 0,
                    RemovedInterviewsCount  = 0,
                    AssignmentsOnDeviceCount  = 0,
                    TotalUploadedBytes  = 0,
                    TotalDownloadedBytes = 0,
                    TotalConnectionSpeed = 0,
                    TotalSyncDuration  = TimeSpan.FromSeconds(5),
                    SyncFinishDate = DateTime.UtcNow
                }
            };
        }

        public InterviewEntity InterviewEntity(Guid? interviewId = null, EntityType entityType = EntityType.Question, Identity identity = null, 
            int[] invalidValidations = null, bool isEnabled = true, int? asInt = null)
        {
            return new InterviewEntity
            {
                InterviewId = interviewId ?? Guid.NewGuid(),
                EntityType = entityType,
                Identity = identity ?? Create.Identity(),
                InvalidValidations = invalidValidations ?? Array.Empty<int>(),
                IsEnabled = isEnabled,
                AsInt = asInt
            };
        }

        public PreloadedFile PreloadedFile(string questionnaireOrRosterName = null, params PreloadingRow[] rows)
            => this.PreloadedFile(null, questionnaireOrRosterName, rows);

        public PreloadedFile PreloadedFile(string fileName, string questionnaireOrRosterName, params PreloadingRow[] rows)
        {
            var columns = rows.SelectMany(x => x.Cells).OfType<PreloadingValue>().Select(x => x.Column).ToArray();
            return new PreloadedFile
            {
                FileInfo = Create.Entity.PreloadedFileInfo(columns, fileName, questionnaireOrRosterName),
                Rows = rows
            };
        }

        public PreloadedFileInfo PreloadedFileInfo(string[] columns = null, string fileName = null, string questionnaireOrRosterName = null) => new PreloadedFileInfo
        {
            Columns = columns,
            FileName = fileName,
            QuestionnaireOrRosterName = questionnaireOrRosterName ?? "Questionnaire"
        };

        public PreloadingAssignmentRow PreloadingAssignmentRow(string fileName,
            AssignmentResponsible responsible = null, 
            AssignmentQuantity quantity = null,
            AssignmentRosterInstanceCode[] rosterInstanceCodes = null,
            AssignmentInterviewId interviewId = null,
            string questionnaireOrRosterName = null,
            AssignmentEmail assignmentEmail = null,
            AssignmentPassword assignmentPassword = null,
            AssignmentWebMode assignmentWebMode = null,
            params BaseAssignmentValue[] answers) => new PreloadingAssignmentRow
        {
            FileName = fileName,
            QuestionnaireOrRosterName = questionnaireOrRosterName ?? "Questionnaire",
            Responsible = responsible,
            Quantity = quantity,
            RosterInstanceCodes = rosterInstanceCodes,
            InterviewIdValue = interviewId,
            Answers = answers,
            Email = assignmentEmail,
            Password = assignmentPassword,
            WebMode = assignmentWebMode
            };

        public AssignmentResponsible AssignmentResponsible(string responsibleName, UserToVerify userInfo = null) => new AssignmentResponsible
        {
            Value = responsibleName,
            Column = ServiceColumns.ResponsibleColumnName,
            Responsible = userInfo
        };

        public AssignmentEmail AssignmentEmail(string email) => new AssignmentEmail
        {
            Value = email,
            Column = ServiceColumns.EmailColumnName
        };

        public AssignmentWebMode AssignmentWebMode(bool? webMode) => new AssignmentWebMode
        {
            WebMode = webMode,
            Value = webMode == true ? "1" : "",
            Column = ServiceColumns.WebModeColumnName
        };

        public AssignmentPassword AssignmentPassword(string password) => new AssignmentPassword
        {
            Value = password,
            Column = ServiceColumns.PasswordColumnName
        };

        public AssignmentQuantity AssignmentQuantity(string quantity = null, int? parsedQuantity = null) => new AssignmentQuantity
        {
            Value = string.IsNullOrEmpty(quantity) ? parsedQuantity?.ToString() ?? "" : quantity,
            Column = ServiceColumns.AssignmentsCountColumnName,
            Quantity = parsedQuantity
        };

        public AssignmentInterviewId AssignmentInterviewId(string interviewId) => new AssignmentInterviewId
        {
            Value = interviewId,
            Column = ServiceColumns.InterviewId
        };

        public UserToVerify UserToVerify(bool isLocked = false, Guid? interviewerId = null, Guid? supervisorId = null, Guid? hqId = null) => new UserToVerify
        {
            IsLocked = isLocked,
            InterviewerId = interviewerId,
            SupervisorId = supervisorId,
            HeadquartersId = hqId
        };

        public AssignmentTextAnswer AssignmentTextAnswer(string column, string value) => new AssignmentTextAnswer
        {
            Column = column,
            VariableName = column.ToLower(),
            Value = value
        };

        public AssignmentIntegerAnswer AssignmentIntegerAnswer(string
            column, int? answer = null, string value = null) => new AssignmentIntegerAnswer
        {
            Column = column,
            VariableName = column.ToLower(),
            Value = answer.HasValue ? answer.ToString() : value,
            Answer = answer
        };

        public AssignmentCategoricalSingleAnswer AssignmentCategoricalSingleAnswer(string
            column, int? answer = null, string value = null) => new AssignmentCategoricalSingleAnswer
            {
            Column = column,
            VariableName = column.ToLower(),
            Value = answer.HasValue ? answer.ToString() : value,
            OptionCode = answer
        };

        public AssignmentRosterInstanceCode AssignmentRosterInstanceCode(string
            column, int? answer = null, string value = null, string variable = null) => new AssignmentRosterInstanceCode
            {
            Column = column,
            VariableName = variable ?? column.ToLower(),
            Value = answer.HasValue ? answer.ToString() : value,
            Code = answer
        };

        public AssignmentDoubleAnswer AssignmentDoubleAnswer(string
            column, double? answer = null, string value = null, string variable = null) => new AssignmentDoubleAnswer
        {
            Column = column,
            VariableName = string.IsNullOrEmpty(variable) ? column.ToLower() : variable,
            Value = answer.HasValue ? answer.ToString() : value,
            Answer = answer
        };

        public AssignmentMultiAnswer AssignmentMultiAnswer(string variable, params AssignmentAnswer[] values) => new AssignmentMultiAnswer
        {
            VariableName = variable,
            Values = values
        };

        public AssignmentGpsAnswer AssignmentGpsAnswer(string variable, params AssignmentAnswer[] values) => new AssignmentGpsAnswer
        {
            VariableName = variable,
            Values = values
        };

        public AssignmentDateTimeAnswer AssignmentDateTimeAnswer(string
            column, DateTime? answer = null, string value = null) => new AssignmentDateTimeAnswer
        {
            Column = column,
            VariableName = column.ToLower(),
            Value = answer.HasValue ? answer.ToString() : value,
            Answer = answer
        };

        public PreloadingAssignmentRow PreloadingAssignmentRow(string fileName, int row, string interviewId,
            params AssignmentRosterInstanceCode[] rosterInstanceCodes) => new PreloadingAssignmentRow
        {
            FileName = fileName,
            Row = row,
            InterviewIdValue = string.IsNullOrWhiteSpace(interviewId) ? null : Create.Entity.AssignmentInterviewId(interviewId),
            RosterInstanceCodes = rosterInstanceCodes,
            QuestionnaireOrRosterName = Path.GetFileNameWithoutExtension(fileName)
        };

        public PreloadingAssignmentRow PreloadingAssignmentRow(string fileName, int row, string interviewId, string questionnaireOrRosterName,
            params AssignmentRosterInstanceCode[] rosterInstanceCodes) => new PreloadingAssignmentRow
        {
            FileName = fileName,
            Row = row,
            InterviewIdValue = string.IsNullOrWhiteSpace(interviewId) ? null : Create.Entity.AssignmentInterviewId(interviewId),
            RosterInstanceCodes = rosterInstanceCodes,
            QuestionnaireOrRosterName = questionnaireOrRosterName
            };

        public AssignmentRosterInstanceCode AssignmentRosterInstanceCode(string column, int code) =>
            new AssignmentRosterInstanceCode
            {
                Column = column,
                VariableName = column,
                Code = code,
                Value = code.ToString()
            };

        public PreloadingRow PreloadingRow(params PreloadingCell[] cells) => this.PreloadingRow(1, cells);

        public PreloadingRow PreloadingRow(int rowIndex = 1, params PreloadingCell[] cells) => new PreloadingRow
        {
            RowIndex = rowIndex,
            Cells = cells
        };

        public PreloadingValue PreloadingValue(string variableName, string value, string columnName) => new PreloadingValue
        {
            VariableOrCodeOrPropertyName = variableName.ToLower(),
            Value = value,
            Column = columnName ?? variableName
        };

        public PreloadingValue PreloadingValue(string variableName, string value)
            => PreloadingValue(variableName, value, null);

        public PreloadingCompositeValue PreloadingCompositeValue(string variableName, params PreloadingValue[] values) => new PreloadingCompositeValue
        {
            VariableOrCodeOrPropertyName = variableName.ToLower(),
            Values = values
        };

        public GetReportCategoricalPivotReportItem GetReportCategoricalPivotReportItem(int row, int col, long count)
        {
            return new GetReportCategoricalPivotReportItem
            {
                RowValue = row,
                ColValue = col,
                Count = count
            };
        }

        public AuditLogEntityFactory AuditLogEntity => new AuditLogEntityFactory();

        public InterviewerDocument InterviewerDocument(Guid interviewerId, string login = null, bool isLockedBySv = false) =>
            new InterviewerDocument
            {
                Id = interviewerId.ToString(),
                InterviewerId = interviewerId,
                UserName = login,
                IsLockedBySupervisor = isLockedBySv
            };

        public class AuditLogEntityFactory
        {
            private readonly Random rnd;
            private readonly JsonAllTypesSerializer serializer;

            public AuditLogEntityFactory()
            {
                this.rnd = new Random();
                this.serializer = new JsonAllTypesSerializer();
            }

            public LogoutAuditLogEntity LogoutAuditLogEntity(string userName) 
                => new LogoutAuditLogEntity(userName);

            public CloseInterviewAuditLogEntity CloseInterviewAuditLogEntity(Guid? interviewId = null, string interviewKey = null) 
                => new CloseInterviewAuditLogEntity(interviewId ?? Guid.NewGuid(), interviewKey ?? new InterviewKey(rnd.Next()).ToString());

            public AuditLogEntityApiView AuditLogEntitiesApiView(
                IAuditLogEntity entity, 
                int? id = null, 
                Guid? responsibleId = null, 
                string responsible = null,
                DateTime? dateTime = null, 
                DateTime? dateTimeUtc = null) =>
                new AuditLogEntityApiView
                {
                    Id = id ?? rnd.Next(),
                    Type = entity.Type,
                    PayloadType = entity.GetType().Name,
                    ResponsibleId = responsibleId ?? Guid.NewGuid(),
                    Payload = serializer.Serialize(entity),
                    ResponsibleName = responsible ?? Guid.NewGuid().ToString(),
                    Time = dateTime ?? DateTime.Now,
                    TimeUtc = dateTimeUtc ?? DateTime.UtcNow
                };
        }

        public EnumeratorSynchonizationContext EnumeratorSynchonizationContext(
            IProgress<SyncProgressInfo> progress = null)
            => new EnumeratorSynchonizationContext
            {
                Statistics = new SynchronizationStatistics(),
                CancellationToken = CancellationToken.None,
                Progress = progress ?? Mock.Of<IProgress<SyncProgressInfo>>()
            };

        public InterviewerAssignmentDashboardItemViewModel InterviewerAssignmentDashboardItemViewModel(
            IServiceLocator serviceLocator,
            IViewModelNavigationService viewModelNavigationService = null)
        {
            return new InterviewerAssignmentDashboardItemViewModel(serviceLocator, 
                viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>());
        }

        public DashboardSubTitleViewModel DashboardSubTitleViewModel()
        {
            return new DashboardSubTitleViewModel();
        }

        public InterviewTreeQuestion InterviewTreeSingleOptionQuestion(Identity singleOptionQuestionIdentity, int answer)
        {
            var result = Create.Entity.InterviewTreeQuestion(singleOptionQuestionIdentity, answer: answer,
                questionType: QuestionType.SingleOption);
            return result;
        }

        public InterviewGpsAnswer InterviewGpsAnswer(Guid interviewId, double latitude, double longitude)
        {
            return new InterviewGpsAnswer
            {
                InterviewId = interviewId,
                Latitude = latitude,
                Longitude = longitude
            };
        }

        public InterviewerDailyTrafficUsage InterviewerDailyTrafficUsage(
            int uploadedBytes = 0,
            int downloadedBytes = 0,
            int year = 0,
            int month = 0,
            int day = 0)
        {
            return new InterviewerDailyTrafficUsage
            {
                UploadedBytes = uploadedBytes,
                DownloadedBytes = downloadedBytes,
                Year = year,
                Month = month,
                Day = day
            };
        }

        public InterviewerSettings InterviewerSettings(bool autoUpdateEnabled = false)
        {
            return new InterviewerSettings
            {
                AutoUpdateEnabled = autoUpdateEnabled,
            };
        }

        public InterviewGpsAnswerWithTimeStamp InterviewGpsAnswerWithTimeStamp(
            Guid interviewId,
            double latitude,
            double longitude,
            Guid entityId,
            DateTime? timestamp = null,
            InterviewStatus status = InterviewStatus.Completed,
            bool idenifying = false)

        {
            return new InterviewGpsAnswerWithTimeStamp
            {
                InterviewId = interviewId,
                Latitude = latitude,
                Longitude = longitude,
                EntityId = entityId,
                Timestamp = timestamp ?? DateTime.Now,
                Status = status,
                Idenifying = idenifying
            };
        }

        public OptionWithSearchTerm OptionWithSearchTerm(int value, string title = null) => new OptionWithSearchTerm
        {
            Value = value,
            Title = title
        };

        public Invitation Invitation(int id, 
            Assignment assignment, 
            string token = null,
            string interviewId = null,
            InterviewSummary interview = null)
        {
            var invitation = new Invitation();

            var asDynamic = invitation.AsDynamic();
            asDynamic.Id = id;
            asDynamic.AssignmentId = assignment.Id;
            asDynamic.Assignment = assignment;
            asDynamic.Token = token;
            asDynamic.InterviewId = interviewId;
            asDynamic.Interview = interview;
            
            return invitation;
        }

        public WebInterviewEmailTemplate EmailTemplate(string subject = null, string message = null, string passwordDescription = null, string linkText = null)
        {
            return new WebInterviewEmailTemplate(subject ?? "Subject", message ?? "Message", passwordDescription ?? "Password", linkText ?? "LINK");
        }

        public PersonalizedWebInterviewEmail PersonalizedEmail(string subject = null, string message = null)
        {
            var email = new PersonalizedWebInterviewEmail("Subject", "Message", "password:");

            var asDynamic = email.AsDynamic();
            asDynamic.Subject = subject ?? "Subject";
            asDynamic.MessageText = message ?? "Message text";
            asDynamic.MessageHtml = message ?? "Message html";

            return email;
        }

        public CategoriesItem CategoriesItem(string text, int id, int? parentId = null)
        {
            return new CategoriesItem()
            {
                Id = id,
                Text = text,
                ParentId = parentId
            };
        }

        public Categories Categories(Guid id)
        {
            return new Categories() { Id = id };
        }

        public OptionView OptionView(QuestionnaireIdentity questionnaireId, int value, string text, int? parentId, Guid categoryId)
        {
            return new OptionView()
            {
                CategoryId = categoryId.FormatGuid(),
                Value = value,
                Title = text,
                ParentValue = parentId,
                QuestionnaireId = questionnaireId.ToString()
            };
        }

        public ReusableCategoriesDto ReusableCategoriesDto(Guid? id = null, int count = 5)
        {
            return new ReusableCategoriesDto()
            {
                Id = id ?? Guid.NewGuid(),
                Options = Enumerable.Range(1, count).Select(i => CategoriesItem(i.ToString(), i)).ToList()
            };
        }


        public ReusableCategoriesDto ReusableCategoriesDto(Guid? id, List<CategoriesItem> items)
        {
            return new ReusableCategoriesDto()
            {
                Id = id ?? Guid.NewGuid(),
                Options = items
            };
        }

        public ReusableCategoricalOptions ReusableCategoricalOptions(QuestionnaireIdentity questionnaireId,
            Guid categoriesId, int value, string text = "option", int? parentValue = null, int? sortIndex = null) => new ReusableCategoricalOptions
        {
            CategoriesId = categoriesId,
            QuestionnaireId = questionnaireId,
            ParentValue = parentValue,
            Text = text,
            Value = value,
            SortIndex = sortIndex ?? 0
        };

        public CategoricalOption CategoricalOption(string title, int value)
        {
            return new CategoricalOption()
            {
                Title = title,
                Value = value,
            };
        }

        public InterviewApiView InterviewApiView(Guid id, Guid? lastEventId)
        {
            return new InterviewApiView()
            {
                Id = id,
                LastEventId = lastEventId,
            };
        }

        public InterviewUploadState InterviewUploadState(
            Guid responsibleId,
            bool isEventsUploaded = false,
            HashSet<string> imagesQuestionsMd5 = null,
            HashSet<string> audioQuestionsFilesMd5 = null,
            HashSet<string> audioAuditFilesMd5 = null
            )
        {
            return new InterviewUploadState()
            {
                IsEventsUploaded = isEventsUploaded,
                ImagesFilesNames = new HashSet<string>(),
                AudioFilesNames = new HashSet<string>(),
                ImageQuestionsFilesMd5 = imagesQuestionsMd5 ?? new HashSet<string>(),
                AudioQuestionsFilesMd5 = audioQuestionsFilesMd5 ?? new HashSet<string>(),
                AudioAuditFilesMd5 = audioAuditFilesMd5 ?? new HashSet<string>(),
                ResponsibleId = responsibleId,
            };
        }

        public InterviewPackageContainer InterviewPackageContainer(Guid interviewId, params CommittedEvent[] events)
        {
            return new InterviewPackageContainer(interviewId, events.ToReadOnlyCollection());
        }

        public MapBrowseItem MapBrowseItem(string fileName)
        {
            return new MapBrowseItem
            {
                Id = fileName,
                FileName = fileName
            };
        }

        public CalendarEvent CalendarEvent(Guid? id = null, int? assignmentId = null,
            Guid? interviewId = null, string interviewKey = null,
            bool isCompleted = false, bool isDeleted = false, bool isSynchronized = false,
            DateTimeOffset? start = null, string tomeZoneId = null, string comment = null,
            DateTime? lastUpdate = null)
        {
            return new CalendarEvent()
            {
                Id = id ?? Guid.NewGuid(),
                AssignmentId = assignmentId ?? 7,
                Comment = comment,
                InterviewId = interviewId,
                InterviewKey = interviewKey,
                IsCompleted = isCompleted,
                IsDeleted = isDeleted,
                IsSynchronized = isSynchronized,
                LastEventId = Guid.NewGuid(),
                LastUpdateDateUtc = lastUpdate ?? DateTime.UtcNow,
                Start = start ?? DateTimeOffset.UtcNow,
                StartTimezone = tomeZoneId,
                UserId = Guid.NewGuid(),
            };
        }

        public WB.Core.BoundedContexts.Headquarters.CalendarEvents.CalendarEvent CalendarEvent(Guid? publicKey = null, 
            int? assignmentId = null,
            Guid? interviewId = null, string interviewKey = null,
            DateTimeOffset? start = null, string tomeZoneId = null, string comment = null,
            DateTimeOffset? lastUpdate = null, DateTime? deletedAtUtc = null, DateTime? completedAtUtc = null,
            DateTime? updateDateUtc = null)
        {
            var date = (start ?? DateTimeOffset.UtcNow).ToInstant();
            DateTimeZone zone = null;
            if (tomeZoneId != null)
                zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tomeZoneId);
            if (zone == null)
                zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var zonedDate = new ZonedDateTime(date, zone);

            var calendarEvent = new WB.Core.BoundedContexts.Headquarters.CalendarEvents.CalendarEvent(
                publicKey: publicKey ?? Guid.NewGuid(),
                assignmentId: assignmentId ?? 7,
                comment: comment,
                interviewId: interviewId,
                interviewKey: interviewKey ?? String.Empty,
                start: zonedDate,
                userId: Guid.NewGuid(),
                updateDate: lastUpdate ?? DateTimeOffset.UtcNow
            );
            calendarEvent.DeletedAtUtc = deletedAtUtc;
            calendarEvent.CompletedAtUtc = completedAtUtc;
            calendarEvent.UpdateDateUtc = updateDateUtc ?? DateTime.UtcNow;
            
            return calendarEvent;
        }

        public Workspace Workspace(string name = null, bool? disabled = false)
        {
            var ws  = new Workspace(name ?? Guid.NewGuid().FormatGuid(), (name ?? "") + " Display name1");
            
            if(disabled == true)
                ws.Disable();
            return ws;
        }
    }
}
