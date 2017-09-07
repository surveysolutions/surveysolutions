using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events;
using Moq;
using ReflectionMagic;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Storage;
using AttachmentContent = WB.Core.BoundedContexts.Headquarters.Views.Questionnaire.AttachmentContent;
using CompanyLogo = WB.UI.Headquarters.Models.CompanyLogo.CompanyLogo;
using TranslationInstance = WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations.TranslationInstance;

namespace WB.Tests.Abc.TestFactories
{
    internal class EntityFactory
    {
        public Answer Answer(string answer, decimal value, decimal? parentValue = null)
            => new Answer
            {
                AnswerText = answer,
                AnswerValue = value.ToString(),
                ParentValue = parentValue?.ToString()
            };

        public AnsweredQuestionSynchronizationDto AnsweredQuestionSynchronizationDto(
            Guid? questionId = null, decimal[] rosterVector = null, object answer = null, params CommentSynchronizationDto[] comments)
            => new AnsweredQuestionSynchronizationDto(
                questionId ?? Guid.NewGuid(),
                rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty,
                answer,
                comments ?? new CommentSynchronizationDto[0]);

        public AnsweredYesNoOption AnsweredYesNoOption(decimal value, bool answer)
            => new AnsweredYesNoOption(value, answer);

        public Attachment Attachment(string attachmentHash)
            => new Attachment { ContentId = attachmentHash };

        public Translation Translation(Guid translationId, string translationName)
            => new Translation { Id = translationId, Name = translationName};

        public Core.SharedKernels.Enumerator.Views.AttachmentContent AttachmentContent_Enumerator(string id)
            => new Core.SharedKernels.Enumerator.Views.AttachmentContent
            {
                Id = id,
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

        public AttachmentContentMetadata AttachmentContentMetadata(string contentType)
            => new AttachmentContentMetadata
            {
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

        public CommentedStatusHistroyView CommentedStatusHistroyView(
            InterviewStatus status = InterviewStatus.InterviewerAssigned, string comment = null, DateTime? timestamp = null)
            => new CommentedStatusHistroyView
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

        public DataExportProcessDetails DataExportProcessDetails(QuestionnaireIdentity questionnaireIdentity = null)
            => new DataExportProcessDetails(
                DataExportFormat.Tabular,
                questionnaireIdentity ?? new QuestionnaireIdentity(Guid.NewGuid(), 1),
                "some questionnaire");

        public InterviewTreeDateTimeQuestion InterviewTreeDateTimeQuestion(DateTime answer, bool isTimestampQuestion = false)
            => new InterviewTreeDateTimeQuestion(answer, isTimestampQuestion);

        public DateTimeQuestion DateTimeQuestion(
            Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string variable = null, string validationMessage = null, string text = null,
            QuestionScope scope = QuestionScope.Interviewer, bool preFilled = false, bool hideIfDisabled = false, bool isTimestamp = false)
            => new DateTimeQuestion("Question DT")
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

        public ExportedHeaderItem ExportedHeaderItem(Guid? questionId = null, string variableName = "var")
            => new ExportedHeaderItem
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ColumnNames = new[] { variableName },
            };

        public FailedValidationCondition FailedValidationCondition(int? failedConditionIndex = null)
            => new FailedValidationCondition(failedConditionIndex ?? 1117);

        public Group FixedRoster(Guid? rosterId = null,
            string enablementCondition = null,
            IEnumerable<string> obsoleteFixedTitles = null,
            IEnumerable<IComposite> children = null,
            string variable = "roster_var",
            string title = "Roster X",
            FixedRosterTitle[] fixedTitles = null) => Create.Entity.Roster(
                        rosterId: rosterId,
                        children: children,
                        title: title,
                        variable: variable,
                        enablementCondition: enablementCondition,
                        fixedRosterTitles: fixedTitles,
                        fixedTitles: obsoleteFixedTitles?.ToArray() ?? new[] { "Fixed Roster 1", "Fixed Roster 2", "Fixed Roster 3" });

        public FixedRosterTitle FixedTitle(decimal value, string title = null)
            => new FixedRosterTitle(value, title ?? $"Fixed title {value}");

        public GeoPosition GeoPosition()
            => new GeoPosition(1, 2, 3, 4, new DateTimeOffset(new DateTime(1984, 4, 18)));

        public GpsCoordinateQuestion GpsCoordinateQuestion(Guid? questionId = null, string variable = "var1", bool isPrefilled = false, string title = null,
            string enablementCondition = null, string validationExpression = null, bool hideIfDisabled = false, string label=null)
            => new GpsCoordinateQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = variable,
                QuestionType = QuestionType.GpsCoordinates,
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

        public InterviewTreeIntegerQuestion InterviewTreeIntegerQuestion(int answer = 42)
            => new InterviewTreeIntegerQuestion(answer);

        public InterviewBinaryDataDescriptor InterviewBinaryDataDescriptor()
            => new InterviewBinaryDataDescriptor(Guid.NewGuid(), "test.jpeg", null, () => new byte[0]);

        public InterviewComment InterviewComment(string comment = null)
            => new InterviewComment { Comment = comment };
        
        public InterviewCommentaries InterviewCommentaries(Guid? questionnaireId = null, long? questionnaireVersion = null, params InterviewComment[] comments)
            => new InterviewCommentaries
            {
                QuestionnaireId = (questionnaireId ?? Guid.NewGuid()).FormatGuid(),
                QuestionnaireVersion = questionnaireVersion ?? 1,
                Commentaries = new List<InterviewComment>(comments)
            };

        public InterviewCommentedStatus InterviewCommentedStatus(
            InterviewExportedAction status = InterviewExportedAction.ApprovedBySupervisor,
            string originatorName = "inter",
            UserRoles originatorRole = UserRoles.Interviewer,
            Guid? statusId = null, 
            Guid? interviewerId = null, 
            Guid? supervisorId = null, 
            DateTime? timestamp = null, 
            TimeSpan? timeSpanWithPreviousStatus = null)
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
                TimeSpanWithPreviousStatus = timeSpanWithPreviousStatus
            };

        public InterviewData InterviewData(
            bool createdOnClient = false,
            InterviewStatus status = InterviewStatus.Created,
            Guid? interviewId = null,
            Guid? responsibleId = null,
            Guid? questionnaireId = null)
            => new InterviewData
            {
                CreatedOnClient = createdOnClient,
                Status = status,
                InterviewId = interviewId.GetValueOrDefault(),
                ResponsibleId = responsibleId.GetValueOrDefault(),
                QuestionnaireId = questionnaireId ?? Guid.NewGuid()
            };

        public InterviewData InterviewData(params InterviewQuestion[] topLevelQuestions)
        {
            var interviewData = new InterviewData { InterviewId = Guid.NewGuid() };
            interviewData.Levels.Add("#", new InterviewLevel(new ValueVector<Guid>(), null, new decimal[0]));
            foreach (var interviewQuestion in topLevelQuestions)
            {
                interviewData.Levels["#"].QuestionsSearchCache.Add(interviewQuestion.Id, interviewQuestion);
            }
            return interviewData;
        }

        public InterviewDataExportLevelView InterviewDataExportLevelView(Guid interviewId, params InterviewDataExportRecord[] records)
            => new InterviewDataExportLevelView(new ValueVector<Guid>(), "test", records);

        public InterviewDataExportRecord InterviewDataExportRecord(
            Guid interviewId,
            string levelName = "",
            string[] referenceValues = null,
            string[] parentLevelIds = null,
            string[] systemVariableValues = null,
            string[] answers = null)
            => new InterviewDataExportRecord(interviewId.FormatGuid(), 
               referenceValues?? new string[0],
               parentLevelIds ?? new string[0],
               systemVariableValues ?? new string[0])
               { 
                   Answers = answers ?? new string[0],
                   LevelName = levelName,
                   InterviewId = interviewId
               };

        public InterviewDataExportView InterviewDataExportView(
            Guid? interviewId = null,
            Guid? questionnaireId = null,
            long questionnaireVersion = 1,
            params InterviewDataExportLevelView[] levels)
            => new InterviewDataExportView(interviewId ?? Guid.NewGuid(), levels);

        public InterviewItemId InterviewItemId(Guid id, decimal[] rosterVector = null)
            => new InterviewItemId(id, rosterVector);

        public InterviewLinkedQuestionOptions InterviewLinkedQuestionOptions(params ChangedLinkedOptions[] options)
        {
            var result = new InterviewLinkedQuestionOptions();

            foreach (var changedLinkedQuestion in options)
            {
                result.LinkedQuestionOptions[changedLinkedQuestion.QuestionId.ToString()] = changedLinkedQuestion.Options;
            }

            return result;
        }

        public InterviewQuestion InterviewQuestion(Guid? questionId = null, object answer = null)
        {
            var interviewQuestion = new InterviewQuestion(questionId ?? Guid.NewGuid()) { Answer = answer };
            if (answer != null)
            {
                interviewQuestion.QuestionState |= QuestionState.Answered;
            }
            return interviewQuestion;
        }



        public InterviewStatusTimeSpans InterviewStatusTimeSpans(Guid? questionnaireId = null,
            long? questionnaireVersion = null, string interviewId = null, params TimeSpanBetweenStatuses[] timeSpans)
            => new InterviewStatusTimeSpans
            {
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                QuestionnaireVersion = questionnaireVersion ?? 1,
                TimeSpansBetweenStatuses = timeSpans.ToHashSet(),
                InterviewId = interviewId
            };

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
            bool receivedByInterviewer = false,
            int? assignmentId = null,
            bool wasCompleted = false,
            IEnumerable<InterviewCommentedStatus> statuses = null)
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
                TeamLeadId = teamLeadId.GetValueOrDefault(),
                TeamLeadName = string.IsNullOrWhiteSpace(teamLeadName) ? teamLeadId.FormatGuid() : teamLeadName,
                ResponsibleRole = role,
                Key = key,
                UpdateDate = updateDate ?? new DateTime(2017, 3, 23),
                WasCreatedOnClient = wasCreatedOnClient ?? false,
                ReceivedByInterviewer = receivedByInterviewer,
                AssignmentId = assignmentId,
                QuestionnaireIdentity = new QuestionnaireIdentity(qId, qVersion).ToString(),
                WasCompleted = wasCompleted,
                InterviewCommentedStatuses = statuses?.ToList() ?? new List<InterviewCommentedStatus>(),
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
            Dictionary<InterviewItemId, RosterSynchronizationDto[]> rosterGroupInstances = null,
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
                answers ?? new AnsweredQuestionSynchronizationDto[0],
                disabledGroups ?? new HashSet<InterviewItemId>(),
                disabledQuestions ?? new HashSet<InterviewItemId>(),
                disabledStaticTexts ?? new List<Identity>(),
                validQuestions ?? new HashSet<InterviewItemId>(),
                invalidQuestions ?? new HashSet<InterviewItemId>(),
                readonlyQuestions ?? new HashSet<InterviewItemId>(),
                validStaticTexts ?? new List<Identity>(),
                invalidStaticTexts ?? new List<KeyValuePair<Identity, List<FailedValidationCondition>>>(),
                rosterGroupInstances ?? new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(),
                failedValidationConditions?.ToList() ??
                new List<KeyValuePair<Identity, IList<FailedValidationCondition>>>(),
                new Dictionary<InterviewItemId, RosterVector[]>(),
                variables ?? new Dictionary<InterviewItemId, object>(),
                disabledVariables ?? new HashSet<InterviewItemId>(),
                wasCompleted ?? false)
            {
                InterviewKey = interviewKey,
                AssignmentId = assignmentId
            };
        }

        public InterviewView InterviewView(Guid? prefilledQuestionId = null, Guid? interviewId = null, string questionnaireId = null, InterviewStatus? status = null)
        {
            interviewId = interviewId ?? Guid.NewGuid();
            return new InterviewView
            {
                Id = interviewId.FormatGuid(),
                InterviewId = interviewId.Value,
                QuestionnaireId = questionnaireId,
                LocationQuestionId = prefilledQuestionId,
                Status = status ?? default(InterviewStatus)
            };
        }

        public LabeledVariable LabeledVariable(string variableName = "var", string label = "lbl", Guid? questionId = null, params VariableValueLabel[] variableValueLabels)
            => new LabeledVariable(variableName, label, questionId, variableValueLabels);

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
            => new MultimediaQuestion("Question T")
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

        public InterviewTreeMultiOptionQuestion InterviewTreeMultiOptionQuestion(decimal[] answer)
            => new InterviewTreeMultiOptionQuestion(answer);

        public MultyOptionsQuestion MultipleOptionsQuestion(Guid? questionId = null, string enablementCondition = null,
            string validationExpression = null, bool areAnswersOrdered = false, int? maxAllowedAnswers = null, Guid? linkedToQuestionId = null, Guid? linkedToRosterId = null,
            bool isYesNo = false, bool hideIfDisabled = false, string optionsFilterExpression = null, 
            Answer[] textAnswers = null, 
            string variable = "mo_question",
            string linkedFilterExpression = null,
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
                QuestionType = QuestionType.MultyOption,
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                LinkedFilterExpression = linkedFilterExpression,
                YesNoView = isYesNo,
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
            string linkedFilter = null)
            => new MultyOptionsQuestion
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
                AreAnswersOrdered = areAnswersOrdered,
                LinkedFilterExpression = linkedFilter,
                Properties = { OptionsFilterExpression = optionsFilter }
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
            IEnumerable<ValidationCondition> validationConditions = null, Guid? linkedToRosterId = null)
            => new NumericQuestion
            {
                QuestionText = questionText ?? "text",
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

        public NumericQuestion NumericQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            bool isInteger = false, int? countOfDecimalPlaces = null, string variableName = "var1", bool prefilled = false, string title = null)
            => new NumericQuestion("Question N")
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

        public NumericQuestion NumericRealQuestion(Guid? id = null,
            string variable = null,
            string enablementCondition = null,
            string validationExpression = null,
            bool useFomatting = false,
            IEnumerable<ValidationCondition> validationConditions = null,
            int? countOfDecimalPlaces = null)
            => new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = false,
                UseFormatting = useFomatting,
                ConditionExpression = enablementCondition,
                ValidationConditions = validationConditions?.ToList() ?? new List<ValidationCondition>(),
                ValidationExpression = validationExpression,
                CountOfDecimalPlaces = countOfDecimalPlaces
            };

        public Answer Option(string value = null, string text = null, string parentValue = null, Guid? id = null)
            => new Answer
            {
                PublicKey = id ?? Guid.NewGuid(),
                AnswerText = text ?? "text",
                AnswerValue = value ?? "1",
                ParentValue = parentValue
            };

        public Answer Option(int value, string text = null)
            => new Answer
            {
                AnswerText = text ?? $"Option {value}",
                AnswerCode = value,
            };

        public IEnumerable<Answer> Options(params int[] values)
        {
            return values.Select(value => Create.Entity.Option(value));
        }

        public ParaDataExportProcessDetails ParaDataExportProcess()
            => new ParaDataExportProcessDetails(DataExportFormat.Tabular);

        public PlainQuestionnaire PlainQuestionnaire(QuestionnaireDocument questionnaireDocument)
            => Create.Entity.PlainQuestionnaire(document: questionnaireDocument);

        public PlainQuestionnaire PlainQuestionnaire(QuestionnaireDocument document = null, long version = 1)
            => Create.Entity.PlainQuestionnaire(document, version, null);

        public PlainQuestionnaire PlainQuestionnaire(QuestionnaireDocument document, long version, Translation translation = null)
        {
            if (document != null)
            {
                document.IsUsingExpressionStorage = true;
                document.ExpressionsPlayOrder = document.ExpressionsPlayOrder ?? Create.Service.ExpressionsPlayOrderProvider().GetExpressionsPlayOrder(
                    document.AsReadOnly().AssignMissingVariables());
            }
            return new PlainQuestionnaire(document, version, translation);
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
                QuestionType = QuestionType.QRBarcode,
                StataExportCaption = variable,
                QuestionScope = scope,
                Featured = preFilled
            };

        public IQuestion Question(
            Guid? questionId = null,
            string variable = "question",
            string enablementCondition = null,
            string validationExpression = null,
            string validationMessage = null,
            string title = null,
            QuestionType questionType = QuestionType.Text,
            IList<ValidationCondition> validationConditions = null,
            params Answer[] answers)
            => new TextQuestion(title ?? "Question X")
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

        public QuestionnaireBrowseItem QuestionnaireBrowseItem(
            Guid? questionnaireId = null, long? version = null, QuestionnaireIdentity questionnaireIdentity = null,
            string title = "Questionnaire Browse Item X", bool disabled = false, bool deleted = false)
            => new QuestionnaireBrowseItem
            {
                QuestionnaireId = questionnaireIdentity?.QuestionnaireId ?? questionnaireId ?? Guid.NewGuid(),
                Version = questionnaireIdentity?.Version ?? version ?? 1,
                Title = title,
                Disabled = disabled,
                IsDeleted = deleted
            };

        public QuestionnaireBrowseItem QuestionnaireBrowseItem(QuestionnaireDocument questionnaire, bool supportsAssignments = true)
            => new QuestionnaireBrowseItem(questionnaire, 1, false, 1, supportsAssignments);

        public QuestionnaireDocument QuestionnaireDocument(Guid? id = null, params IComposite[] children)
            => new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
            };

        public QuestionnaireDocument QuestionnaireDocument(Guid? id = null, bool usesCSharp = false, IEnumerable<IComposite> children = null)
            => new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>()),
            };

        public QuestionnaireDocument QuestionnaireDocumentWithAttachments(Guid? chapterId = null, params Attachment[] attachments)
            => new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter") { PublicKey = chapterId.GetValueOrDefault() }
                }.ToReadOnlyCollection(),
                Attachments = attachments.ToList()
            };

        public QuestionnaireDocument QuestionnaireDocumentWithTranslations(Guid? chapterId = null, params Translation[] translations)
            => new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter") { PublicKey = chapterId.GetValueOrDefault() }
                }.ToReadOnlyCollection(),
                Translations = translations.ToList()
            };

        public QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Guid? chapterId = null, params IComposite[] children)
            => this.QuestionnaireDocumentWithOneChapter(chapterId, null, children);

        public QuestionnaireDocument QuestionnaireDocumentWithOneChapter(params IComposite[] children)
            => this.QuestionnaireDocumentWithOneChapter(null, null, children);

        public QuestionnaireDocument QuestionnaireDocumentWithOneChapterAndLanguages(Guid chapterId, string[] languages, params IComposite[] children)
            => new QuestionnaireDocument
            {
                PublicKey = Guid.NewGuid(),
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = chapterId,
                        Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
                    }
                }.ToReadOnlyCollection(),
                Translations = new List<Translation>(languages.Select(x=>Create.Entity.Translation(Guid.NewGuid(), x)))
            };

        public QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Guid? chapterId = null, Guid? id = null, params IComposite[] children)
            => new QuestionnaireDocument
            {
                Title = "Questionnaire",
                PublicKey = id ?? Guid.NewGuid(),
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = chapterId.GetValueOrDefault(),
                        Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>())
                    }
                }.ToReadOnlyCollection()
            };

        public QuestionnaireDocument QuestionnaireDocumentWithOneQuestion(Guid? questionId = null, Guid? questionnaireId = null)
           => this.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion(questionId));

        public QuestionnaireExportStructure QuestionnaireExportStructure(Guid? questionnaireId = null, long? version = null)
            => new QuestionnaireExportStructure
            {
                QuestionnaireId = questionnaireId ?? Guid.Empty,
                Version = version ?? 0
            };

        public QuestionnaireIdentity QuestionnaireIdentity(Guid? questionnaireId = null, long? questionnaireVersion = null)
            => new QuestionnaireIdentity(questionnaireId ?? Guid.NewGuid(), questionnaireVersion ?? 7);

        public QuestionnaireLevelLabels QuestionnaireLevelLabels(string levelName = "level", params LabeledVariable[] variableLabels)
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
            FixedRosterTitle[] fixedRosterTitles = null)
        {
            Group group = Create.Entity.Group(
                groupId: rosterId,
                title: title,
                variable: variable ?? "rost_" + rostersCounter++,
                enablementCondition: enablementCondition,
                children: children);

            group.IsRoster = true;
            group.RosterSizeSource = rosterSizeSourceType ?? (rosterSizeQuestionId.HasValue ? RosterSizeSourceType.Question : RosterSizeSourceType.FixedTitles);

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
            bool isPrefilled = false)
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
                QuestionType = QuestionType.SingleOption,
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                CascadeFromQuestionId = cascadeFromQuestionId,
                Answers = answers,
                LinkedFilterExpression = linkedFilterExpression,
                IsFilteredCombobox = isFilteredCombobox,
                Properties = new QuestionProperties(false, false)
                {
                    OptionsFilterExpression = optionsFilterExpression
                }
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
            string optionsFilter = null)
            => new SingleQuestion
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
                IsFilteredCombobox = isFilteredCombobox,
                LinkedFilterExpression = linkedFilter,
                Properties = new QuestionProperties(false, false)
                {
                    OptionsFilterExpression = optionsFilter
                }
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
            => new TextListQuestion("Question TL")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                MaxAnswerCount = maxAnswerCount,
                QuestionType = QuestionType.TextList,
                StataExportCaption = variable,
                QuestionText = questionText
            };

        public TextQuestion TextQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string mask = null,
            string variable = "text_question",
            string validationMessage = null,
            string text = "Question T",
            QuestionScope scope = QuestionScope.Interviewer,
            bool preFilled = false,
            string label = null,
            string instruction = null,
            IEnumerable<ValidationCondition> validationConditions = null,
            bool hideIfDisabled = false)
            => new TextQuestion(text)
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

        public HqUser HqUser(Guid? userId = null, Guid? supervisorId = null, bool? isArchived = null,
            string userName = "name", bool isLockedByHQ = false, UserRoles role = UserRoles.Interviewer,
            string deviceId = null, string passwordHash = null, string passwordHashSha1 = null, string interviewerVersion = null, int? interviewerBuild = null)
        {
            var user = new HqUser
            {
                Id = userId ?? Guid.NewGuid(),
                IsArchived = isArchived ?? false,
                UserName = userName,
                IsLockedByHeadquaters = isLockedByHQ,
                Profile = new HqUserProfile
                {
                    SupervisorId = supervisorId,
                    DeviceId = deviceId,
                    DeviceAppBuildVersion = interviewerBuild,
                    DeviceAppVersion = interviewerVersion
                },
                PasswordHash = passwordHash,
                PasswordHashSha1 = passwordHashSha1
            };
            user.Roles.Add(new HqUserRole {UserId = user.Id, RoleId = role.ToUserId()});

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

        public UserPreloadingDataRecord UserPreloadingDataRecord(
            string login = "test", string supervisor = "", string password = "test", string email = "", string phoneNumber = "",
            string role = null, string fullName = null)
            => new UserPreloadingDataRecord
            {
                Login = login,
                Supervisor = supervisor,
                Role = role ?? (string.IsNullOrEmpty(supervisor) ? "supervisor" : "interviewer"),
                Password = password,
                Email = email,
                PhoneNumber = phoneNumber,
                FullName = fullName 
            };

        public UserPreloadingProcess UserPreloadingProcess(string userPreloadingProcessId = null,
            UserPrelodingState state = UserPrelodingState.Uploaded, int recordsCount = 0, params UserPreloadingDataRecord[] dataRecords)
        {
            var result = new UserPreloadingProcess
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
            => new UserPreloadingSettings(
                5, 5, 12, 1, 10000, 100, 100, "^[a-zA-Z0-9_]{3,15}$",
                @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$",
                "^(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z]).*$",
                @"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$", 100, 15,
                UserModel.PersonNameRegex);

        public UserPreloadingVerificationError UserPreloadingVerificationError()
            => new UserPreloadingVerificationError();

        public ValidationCondition ValidationCondition(string expression = "self != null", string message = "should be answered")
            => new ValidationCondition(expression, message);

        public Variable Variable(Guid? id = null, VariableType type = VariableType.LongInteger, string variableName = "v1", string expression = "2*2")
            => new Variable(
                id ?? Guid.NewGuid(),
                new VariableData(type, variableName, expression, null));

        public VariableValueLabel VariableValueLabel(string value = "1", string label = "l1")
            => new VariableValueLabel(value, label);

        public YesNoAnswers YesNoAnswers(decimal[] allOptionCodes, YesNoAnswersOnly yesNoAnswersOnly = null)
            => new YesNoAnswers(allOptionCodes, yesNoAnswersOnly);
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

        public TranslationInstance TranslationInstance(string value = null,
            Guid? translationId = null, 
            QuestionnaireIdentity questionnaireId = null,
            Guid? entityId = null, 
            string translationIndex = null, 
            TranslationType? type = null)
        {
            return new TranslationInstance
            {
                Value = value,
                TranslationId = translationId ?? Guid.NewGuid(),
                QuestionnaireId = questionnaireId ?? Create.Entity.QuestionnaireIdentity(),
                QuestionnaireEntityId = entityId ?? Guid.NewGuid(),
                TranslationIndex = translationIndex,
                Type = type ?? TranslationType.Unknown
            };
        }

        public WB.Core.SharedKernels.Enumerator.Views.TranslationInstance TranslationInstance_Enumetaror(string value = null,
            Guid? tranlationId = null,
            string questionnaireId = null,
            Guid? entityId = null,
            string translationIndex = null,
            TranslationType? type = null)
        {
            return new WB.Core.SharedKernels.Enumerator.Views.TranslationInstance
            {
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

        public MapReportPoint MapReportPoint(string id, double latitude, double longitude, Guid? interviewId = null, Guid? questionnaireId = null, string variable = "var", long version = 1)
        {
            return new MapReportPoint(id)
            {
                Latitude = latitude,
                Longitude = longitude,
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                QuestionnaireVersion = version,
                Variable = variable,
                InterviewId = interviewId ?? Guid.NewGuid()
            };
        }

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
            bool isDisabled = false, string title = "title", string variableName = "var", int? answer = null)
        {
            var question = this.InterviewTreeQuestion(questionIdentity, isDisabled, title, variableName, QuestionType.SingleOption, answer, null, null, false, false);
            if (isDisabled) question.Disable();
            return question;
        }

        public InterviewTreeQuestion InterviewTreeQuestion(Identity questionIdentity, bool isDisabled = false, string title = "title",
            string variableName = "var", QuestionType questionType = QuestionType.Text, object answer = null, IEnumerable<RosterVector> linkedOptions = null,
            Guid? cascadingParentQuestionId = null, bool isYesNo = false, bool isDecimal = false, Guid? linkedSourceId = null, Guid[] questionsUsingForSubstitution = null)
        {
            var titleWithSubstitutions = Create.Entity.SubstitutionText(questionIdentity, title);
            var question = new InterviewTreeQuestion(questionIdentity, titleWithSubstitutions, variableName, questionType, answer, linkedOptions, 
                cascadingParentQuestionId, isYesNo,  isDecimal, false, false, linkedSourceId);

            if (isDisabled) question.Disable();
            return question;
        }

        public SubstitutionText SubstitutionText(Identity identity, 
            string title,
            SubstitutionVariables variables = null)
        {
            return new SubstitutionText(identity, title, variables ?? new SubstitutionVariables(), Mock.Of<ISubstitutionService>(), Mock.Of<IVariableToUIStringService>());
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
            return CategoricalFixedMultiOptionAnswer.FromInts(selectedOptions);
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
            => new SampleUploadView(questionnaireId ?? Guid.NewGuid(), version ?? 1, featuredQuestionItems);

        public AnswerNotifier AnswerNotifier(LiteEventRegistry liteEventRegistry)
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
            if (withContent)
            {
                hqCompanyLogo.Logo = Guid.NewGuid().ToByteArray();
            }

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

        public InterviewPackage InterviewPackage(Guid? interviewId = null, AggregateRootEvent[] events = null)
        {
            var serializer = new JsonAllTypesSerializer();
            return new InterviewPackage
            {
                InterviewId = interviewId ?? Guid.NewGuid(),
                Events = events != null ? serializer.Serialize(events) : serializer.Serialize(new AggregateRootEvent[0])
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

        public AssignmentApiDocumentBuilder AssignmentApiDocument(int id, int? quantity, QuestionnaireIdentity questionnaireIdentity = null)
        {
            return new AssignmentApiDocumentBuilder(new AssignmentApiDocument
            {
                Id = id,
                Quantity = quantity,
                QuestionnaireId = questionnaireIdentity
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

        public Assignment Assignment(int? id = null,
            QuestionnaireIdentity questionnaireIdentity = null,
            int? quantity = null,
            Guid? assigneeSupervisorId = null,
            string responsibleName = null,
            ISet<InterviewSummary> interviewSummary = null,
            string questionnaireTitle = null, DateTime? updatedAt = null)
        {
            var result = new Assignment();
            var asDynamic = result.AsDynamic();
            asDynamic.Quantity = quantity;
            asDynamic.Id = id ?? 0;
            result.QuestionnaireId = questionnaireIdentity;

            var readonlyUser = new ReadonlyUser() { RoleIds = { UserRoles.Interviewer.ToUserId() } };
            var readonlyProfile = new ReadonlyProfile();
            
            readonlyUser.AsDynamic().ReadonlyProfile = readonlyProfile;
            asDynamic.Responsible = readonlyUser;

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
                result.Questionnaire = new QuestionnaireLiteViewItem
                {
                    Id = questionnaireIdentity?.Id,
                    Title = questionnaireTitle
                };
            }

            if (updatedAt.HasValue)
            {
                result.AsDynamic().UpdatedAtUtc = updatedAt.Value;
            }

            if(interviewSummary != null)
                asDynamic.InterviewSummaries = interviewSummary;
            asDynamic.Answers = null;

            return result;
        }

        public IdentifyingAnswer IdentifyingAnswer(Assignment assignment = null, Identity identity = null, string answer = null, string answerAsString = null)
        {
            var result = new IdentifyingAnswer();
            
            dynamic dynamic = result.AsDynamic();
            dynamic.Assignment = assignment;
            dynamic.Identity = identity;
            dynamic.Answer = answer;
            dynamic.AnswerAsString = answerAsString;
            return result;
        }

        public AssignmentDocumentBuilder AssignmentDocument(int id, int? quantity = null, int interviewsCount = 0, string questionnaireIdentity = null)
        {
            return new AssignmentDocumentBuilder(new AssignmentDocument
            {
                Id = id,
                Quantity = quantity,
                QuestionnaireId = questionnaireIdentity
            });
        }

        public class AssignmentDocumentBuilder
        {
            private readonly AssignmentDocument _entity;

            public AssignmentDocumentBuilder(AssignmentDocument entity)
            {
                _entity = entity;
            }

            public AssignmentDocumentBuilder WithAnswer(Identity identity, string answer, bool identifying = false)
            {
                this._entity.Answers = this._entity.Answers ?? new List<AssignmentDocument.AssignmentAnswer>();
                this._entity.IdentifyingAnswers = this._entity.IdentifyingAnswers ?? new List<AssignmentDocument.AssignmentAnswer>();
                
                var assignmentAnswer = new AssignmentDocument.AssignmentAnswer
                {
                    AssignmentId = this._entity.Id,
                    AnswerAsString = answer,
                    IsIdentifying = identifying,
                    Identity = identity
                };

                this._entity.Answers.Add(assignmentAnswer);

                if (identifying)
                {
                    this._entity.IdentifyingAnswers.Add(assignmentAnswer);
                }

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
                Mock.Of<IExportQuestionService>(),
                Mock.Of<IQuestionnaireStorage>(),
                new RosterStructureService());
            return exportViewFactory.CreateQuestionnaireExportStructure(questionnaire, new QuestionnaireIdentity(Guid.NewGuid(), 1));
        }

        public ExportQuestionService ExportQuestionService()
        {
            return new ExportQuestionService();
        }

        public AudioQuestion AudioQuestion(Guid qId, string variable)
        {
            return new AudioQuestion
            {
                PublicKey = qId,
                StataExportCaption = variable,
                QuestionScope = QuestionScope.Interviewer,
                QuestionType = QuestionType.Audio
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
    }
}