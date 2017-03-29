using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewDetailsViewFactory : IInterviewDetailsViewFactory
    {
        private readonly IUserViewFactory userStore;
        private readonly IChangeStatusFactory changeStatusFactory;
        private readonly IInterviewPackagesService incomingSyncPackagesQueue;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewDataRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;

        private class ValidationView
        {
            public string Message { get; set; }
            public int FailedValidationIndex { get; set; }
        }

        public InterviewDetailsViewFactory(
            IUserViewFactory userStore,
            IChangeStatusFactory changeStatusFactory,
            IInterviewPackagesService incomingSyncPackagesQueue,
            IQuestionnaireStorage questionnaireStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository,
            IReadSideKeyValueStorage<InterviewData>  interviewDataRepository,
            ISubstitutionService substitutionService)
        {
            this.userStore = userStore;
            this.changeStatusFactory = changeStatusFactory;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.questionnaireStorage = questionnaireStorage;
            this.interviewSummaryRepository = interviewSummaryRepository;
            this.interviewDataRepository = interviewDataRepository;
            this.substitutionService = substitutionService;
            this.statefulInterviewRepository = statefulInterviewRepository;
        }

        public DetailsViewModel GetInterviewDetails(Guid interviewId, InterviewDetailsFilter filter, Identity currentGroupIdentity)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());
            InterviewSummary interviewSummary = this.interviewSummaryRepository.GetById(interviewId);
            var interviewData = this.interviewDataRepository.GetById(interviewId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(interviewSummary.QuestionnaireId,
                interviewSummary.QuestionnaireVersion);
            var responsible = this.userStore.GetUser(new UserViewInputModel(interviewSummary.ResponsibleId));

            var rootNode = new InterviewGroupView(Identity.Create(questionnaire.PublicKey, RosterVector.Empty))
            {
                Title = questionnaire.Title
            };
            var interviewGroupViews = rootNode.ToEnumerable()
                                              .Concat(interview.GetAllGroupsAndRosters().Select(this.ToGroupView)).ToList();

            var interviewEntityViews = this.GetFilteredEntities(interview, interviewData, questionnaire, currentGroupIdentity, filter);
            if (filter != InterviewDetailsFilter.All)
                interviewEntityViews = this.GetEntitiesWithoutEmptyGroupsAndRosters(interviewEntityViews);

            return new DetailsViewModel
            {
                Filter = filter,
                SelectedGroupId = currentGroupIdentity,
                FilteredEntities = interviewEntityViews,
                InterviewDetails = new InterviewDetailsView
                {
                    Groups = interviewGroupViews,
                    Responsible = new UserLight(interviewSummary.ResponsibleId, responsible?.UserName ?? "<UNKNOWN>"),
                    Title = questionnaire.Title,
                    Description = questionnaire.Description,
                    PublicKey = interviewSummary.InterviewId,
                    Status = interview.Status,
                    ReceivedByInterviewer = interviewSummary.ReceivedByInterviewer,
                    CurrentTranslation = interview.Language,
                    IsAssignedToInterviewer = interviewSummary.IsAssignedToInterviewer
                },
                Statistic = new DetailsStatisticView
                {
                    AnsweredCount = interview.CountAllEnabledAnsweredQuestions(),
                    AllCount = interview.CountAllEnabledQuestions(),
                    CommentedCount = interview.GetCommentedBySupervisorAllQuestions().Count(),
                    EnabledCount = interview.CountAllEnabledQuestions(),
                    FlaggedCount = interviewData.Levels.Sum(lvl => lvl.Value.QuestionsSearchCache.Values.Count(q => q.IsFlagged())),
                    InvalidCount = interview.CountAllInvalidEntities(),
                    SupervisorsCount = interview.CountEnabledSupervisorQuestions(),
                    HiddenCount = interview.CountEnabledHiddenQuestions(),
                },
                History = this.changeStatusFactory.Load(new ChangeStatusInputModel {InterviewId = interviewId}),
                HasUnprocessedSyncPackages = this.incomingSyncPackagesQueue.HasPendingPackageByInterview(interviewId),
                Translations = questionnaire.Translations.Select(ToTranslationView).ToReadOnlyCollection(),
                InterviewKey = interviewSummary.Key,
                QuestionnaireName = questionnaire.Title,
                QuestionnaireVersion = interviewSummary.QuestionnaireVersion
            };
        }

        private IEnumerable<InterviewEntityView> GetEntitiesWithoutEmptyGroupsAndRosters(IEnumerable<InterviewEntityView> interviewEntityViews)
        {
            var questionViews = interviewEntityViews.OfType<InterviewQuestionView>().ToList();
            var staticTextViews = interviewEntityViews.OfType<InterviewStaticTextView>().ToList();

            foreach (var interviewEntityView in interviewEntityViews)
            {
                if (interviewEntityView is InterviewStaticTextView || interviewEntityView is InterviewQuestionView)
                    yield return interviewEntityView;

                var groupView = interviewEntityView as InterviewGroupView;
                if (groupView == null) continue;

                if (questionViews.Any(question => question.ParentId == groupView.Id) ||
                    staticTextViews.Any(staticText => staticText.ParentId == groupView.Id))
                    yield return groupView;
            }
        }

        private IEnumerable<InterviewEntityView> GetFilteredEntities(IStatefulInterview interview,
            InterviewData interviewData, QuestionnaireDocument questionnaire, Identity currentGroupIdentity,
            InterviewDetailsFilter filter)
        {
            var groupEntities = currentGroupIdentity == null || currentGroupIdentity.Id == questionnaire.PublicKey
                ? interview.GetAllSections()
                : (interview.GetGroup(currentGroupIdentity) as IInterviewTreeNode).ToEnumerable();

            foreach (var entity in groupEntities.TreeToEnumerableDepthFirst(x => x.Children))
            {
                if (!IsEntityInFilter(filter, entity, interviewData)) continue;

                var question = entity as InterviewTreeQuestion;
                var group = entity as InterviewTreeGroup;
                var staticText = entity as InterviewTreeStaticText;

                if (question != null) yield return this.ToQuestionView(question, questionnaire, interview, interviewData);
                else if (group != null) yield return this.ToGroupView(group);
                else if (staticText != null) yield return this.ToStaticTextView(staticText, questionnaire);
            }
        }

        private static InterviewAttachmentViewModel ToAttachmentView(QuestionnaireDocument questionnaire,
            StaticText questionnaireStaticText)
        {
            var attachmentName = questionnaireStaticText.AttachmentName;
            if (string.IsNullOrWhiteSpace(attachmentName)) return null;

            var attachmentHash = questionnaire.Attachments?.Find(x => x.Name == attachmentName)?.ContentId;
            if (string.IsNullOrWhiteSpace(attachmentHash)) return null;

            return new InterviewAttachmentViewModel
            {
                ContentId = attachmentHash,
                ContentName = attachmentName,
            };
        }

        private InterviewEntityView ToQuestionView(InterviewTreeQuestion interviewQuestion, QuestionnaireDocument questionnaire, 
            IStatefulInterview interview, InterviewData interviewData)
        {
            var questionnaireQuestion = questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == interviewQuestion.Identity.Id);

            return new InterviewQuestionView
            {
                Id = interviewQuestion.Identity,
                ParentId = interviewQuestion.Parent.Identity,
                Title = interviewQuestion.Title.Text,
                IsAnswered = interviewQuestion.IsAnswered(),
                IsValid = interviewQuestion.IsValid,
                AnswerString = interviewQuestion.GetAnswerAsString(),
                QuestionType = questionnaireQuestion.QuestionType,
                IsFeatured = interviewQuestion.IsPrefilled,
                LinkedToQuestionId = questionnaireQuestion.LinkedToQuestionId,
                LinkedToRosterId = questionnaireQuestion.LinkedToRosterId,
                Scope = questionnaireQuestion.QuestionScope,
                Variable = interviewQuestion.VariableName,
                Settings = ToQuestionSettingsView(questionnaireQuestion, questionnaire),
                Comments = interviewQuestion.AnswerComments.Select(ToCommentView).ToList(),
                IsEnabled = !interviewQuestion.IsDisabled(),
                IsReadOnly = !(interviewQuestion.IsSupervisors && interview.Status < InterviewStatus.ApprovedByHeadquarters),
                Options = ToOptionsView(questionnaireQuestion, interviewQuestion, interview),
                Answer = ToAnswerView(interviewQuestion),
                IsFlagged = GetIsFlagged(interviewQuestion, interviewData),
                FailedValidationMessages = GetFailedValidationMessages(
                    interviewQuestion.FailedValidations?.Select(
                        (x, index) => ToValidationView(interviewQuestion.ValidationMessages, x, index)),
                    questionnaireQuestion.ValidationConditions).ToList()
            };
        }

        private static bool GetIsFlagged(InterviewTreeQuestion interviewQuestion, InterviewData interviewData)
        {
            var levelId = InterviewEventHandlerFunctional.CreateLevelIdFromPropagationVector(
                    interviewQuestion.Identity.RosterVector);

            if (!interviewData.Levels.ContainsKey(levelId)) return false;
            if (!interviewData.Levels[levelId].QuestionsSearchCache.ContainsKey(interviewQuestion.Identity.Id)) return false;

            return interviewData.Levels[levelId].QuestionsSearchCache[interviewQuestion.Identity.Id].IsFlagged();
        }

        private static object ToAnswerView(InterviewTreeQuestion interviewQuestion)
        {
            if (!interviewQuestion.IsAnswered()) return null;

            if (interviewQuestion.IsYesNo)
                return interviewQuestion.AsYesNo.GetAnswer().ToAnsweredYesNoOptions().ToArray();

            if (interviewQuestion.IsMultiFixedOption)
                return interviewQuestion.AsMultiFixedOption.GetAnswer().ToDecimals().ToArray();

            if (interviewQuestion.IsMultiLinkedOption)
                return interviewQuestion.AsMultiLinkedOption.GetAnswer().ToRosterVectorArray();

            if (interviewQuestion.IsSingleFixedOption)
                return interviewQuestion.AsSingleFixedOption.GetAnswer().SelectedValue;

            if (interviewQuestion.IsSingleLinkedOption)
                return interviewQuestion.AsSingleLinkedOption.GetAnswer().SelectedValue;

            if (interviewQuestion.IsGps)
                return interviewQuestion.AsGps.GetAnswer().Value;

            if (interviewQuestion.IsText)
                return interviewQuestion.AsText.GetAnswer().Value;

            if (interviewQuestion.IsInteger)
                return interviewQuestion.AsInteger.GetAnswer().Value;

            if (interviewQuestion.IsDouble)
                return interviewQuestion.AsDouble.GetAnswer().Value;

            return null;
        }

        private List<QuestionOptionView> ToOptionsView(IQuestion questionnaireQuestion, InterviewTreeQuestion interviewQuestion, IStatefulInterview interview)
        {
            if ((interviewQuestion.IsSingleFixedOption || interviewQuestion.IsMultiFixedOption))
            {
                var options = questionnaireQuestion.Answers?.Select(a => new QuestionOptionView
                {
                    Value = int.Parse(a.AnswerValue),
                    Label = a.AnswerText
                })?.ToList() ?? new List<QuestionOptionView>();

                var optionsToMarkAsSelected = new List<int>();
                if (interviewQuestion.IsSingleFixedOption && interviewQuestion.AsSingleFixedOption.IsAnswered)
                    optionsToMarkAsSelected.Add(interviewQuestion.AsSingleFixedOption.GetAnswer().SelectedValue);

                if (interviewQuestion.IsMultiFixedOption && interviewQuestion.AsMultiFixedOption.IsAnswered)
                {
                    optionsToMarkAsSelected.AddRange(interviewQuestion.AsMultiFixedOption.GetAnswer().CheckedValues);
                }

                foreach (var selectedValue in optionsToMarkAsSelected)
                {
                    var selectedOption = options.FirstOrDefault(x => (int) x.Value == selectedValue);
                    if (selectedOption == null) continue;
                    selectedOption.IsChecked = true;
                    selectedOption.Index = optionsToMarkAsSelected.IndexOf(selectedValue) + 1;
                }

                return options;
            }

            if (interviewQuestion.IsLinked)
            {
                var optionsToMarkAsSelected = new List<RosterVector>();
                if (interviewQuestion.IsSingleLinkedOption && interviewQuestion.AsSingleLinkedOption.IsAnswered)
                {
                    optionsToMarkAsSelected.Add(interviewQuestion.AsSingleLinkedOption.GetAnswer().SelectedValue);
                }
                if (interviewQuestion.IsMultiLinkedOption && interviewQuestion.AsMultiLinkedOption.IsAnswered)
                {
                    optionsToMarkAsSelected.AddRange(interviewQuestion.AsMultiLinkedOption.GetAnswer().CheckedValues);
                }

                var options = interviewQuestion.AsLinked.Options.Select(x => new QuestionOptionView
                {
                    Value = x,
                    Label = interview.GetLinkedOptionTitle(interviewQuestion.Identity, x),
                    IsChecked = optionsToMarkAsSelected.Contains(x),
                    Index = optionsToMarkAsSelected.IndexOf(x) + 1
                }).ToList();

                return options;
            }

            if (interviewQuestion.IsLinkedToListQuestion)
            {
                var optionsToMarkAsSelected = new List<int>();
                if (interviewQuestion.IsSingleLinkedToList && interviewQuestion.AsSingleLinkedToList.IsAnswered)
                {
                    optionsToMarkAsSelected.Add(interviewQuestion.AsSingleLinkedToList.GetAnswer().SelectedValue);
                }
                if (interviewQuestion.IsMultiLinkedToList && interviewQuestion.AsMultiLinkedToList.IsAnswered)
                {
                    optionsToMarkAsSelected.AddRange(interviewQuestion.AsMultiLinkedToList.GetAnswer().CheckedValues);
                }
                var listQuestion = interview.FindQuestionInQuestionBranch(interviewQuestion.AsLinkedToList.LinkedSourceId, interviewQuestion.Identity);

                var options = interviewQuestion.AsLinkedToList.Options.Select(x => new QuestionOptionView
                {
                    Value = Convert.ToInt32(x),
                    Label = listQuestion.AsTextList.GetTitleByItemCode(x),
                    IsChecked = optionsToMarkAsSelected.Contains(Convert.ToInt32(x)),
                    Index = optionsToMarkAsSelected.IndexOf(Convert.ToInt32(x)) + 1
                }).ToList();

                return options;
            }

            if (interviewQuestion.IsTextList && interviewQuestion.AsTextList.IsAnswered)
            {
                return interviewQuestion.AsTextList.GetAnswer().Rows.Select(x => new QuestionOptionView
                {
                    Value = Convert.ToInt32(x.Value),
                    Label = x.Text
                }).ToList();
            }

            return new List<QuestionOptionView>();
        }

        private dynamic ToQuestionSettingsView(IQuestion question, QuestionnaireDocument questionnaire)
        {
            bool treatAsLinkedToList = questionnaire.Find<TextListQuestion>(q => q.PublicKey == question.LinkedToQuestionId) != null;

            var numericQuestion = question as INumericQuestion;
            if (numericQuestion != null)
            {
                return new NumericQuestionSettings
                {
                    IsInteger = numericQuestion.IsInteger,
                    CountOfDecimalPlaces = numericQuestion.CountOfDecimalPlaces,
                    UseFormating = numericQuestion.UseFormatting
                };
            }

            var categoricalMultiQuestion = question as MultyOptionsQuestion;
            if (categoricalMultiQuestion != null)
            {
                return new MultiQuestionSettings
                {
                    YesNoQuestion = categoricalMultiQuestion.YesNoView,
                    AreAnswersOrdered = categoricalMultiQuestion.AreAnswersOrdered,
                    MaxAllowedAnswers = categoricalMultiQuestion.MaxAllowedAnswers,
                    IsLinkedToRoster = (categoricalMultiQuestion.LinkedToRosterId.HasValue ||
                         (categoricalMultiQuestion.LinkedToQuestionId.HasValue && !treatAsLinkedToList)),
                    IsLinkedToListQuestion = treatAsLinkedToList
                };
            }

            var categoricalSingleQuestion = question as SingleQuestion;
            if (categoricalSingleQuestion != null)
            {
                return new SingleQuestionSettings
                {
                    IsFilteredCombobox = categoricalSingleQuestion.IsFilteredCombobox ?? false,
                    IsCascade = categoricalSingleQuestion.CascadeFromQuestionId.HasValue,
                    IsLinkedToRoster = (categoricalSingleQuestion.LinkedToRosterId.HasValue ||
                        (categoricalSingleQuestion.LinkedToQuestionId.HasValue && !treatAsLinkedToList)),
                    IsLinkedToListQuestion = treatAsLinkedToList
                };
            }

            var textQuestion = question as TextQuestion;
            if (textQuestion != null)
            {
               return new TextQuestionSettings
                {
                    Mask = textQuestion.Mask
                };
            }

            var dateTimeQuestion = question as DateTimeQuestion;
            if (dateTimeQuestion != null)
            {
                return new DateTimeQuestionSettings
                {
                    IsTimestamp = question.IsTimestamp
                };
            }

            return null;
        }

        private InterviewQuestionCommentView ToCommentView(AnswerComment comment) => new InterviewQuestionCommentView
        {
            Text = comment.Comment,
            CommenterId = comment.UserId,
            CommenterRole = comment.UserRole,
            CommenterName = this.userStore.GetUser(new UserViewInputModel(comment.UserId))?.UserName ?? "<UNKNOWN>",
            Date = comment.CommentTime
        };

        private InterviewEntityView ToStaticTextView(InterviewTreeStaticText interviewStaticText, QuestionnaireDocument questionnaire)
        {
            var questionnaireStaticText = questionnaire.FirstOrDefault<StaticText>(st => st.PublicKey == interviewStaticText.Identity.Id);
            var attachment = ToAttachmentView(questionnaire, questionnaireStaticText);

            return new InterviewStaticTextView
            {
                Id = interviewStaticText.Identity,
                ParentId = interviewStaticText.Parent.Identity,
                Text = interviewStaticText.Title.Text,
                IsEnabled = !interviewStaticText.IsDisabled(),
                IsValid = interviewStaticText.IsValid,
                FailedValidationMessages = GetFailedValidationMessages(
                    interviewStaticText.FailedValidations?.Select(
                        (x, index) => ToValidationView(interviewStaticText.ValidationMessages, x, index)),
                    questionnaireStaticText.ValidationConditions).ToList(),
                Attachment = attachment
            };
        }

        private static ValidationView ToValidationView(SubstitionText[] validationMessages,
            FailedValidationCondition validationCondition, int failedValidationIndex)
            => new ValidationView
            {
                FailedValidationIndex = validationCondition.FailedConditionIndex,
                Message = validationMessages[failedValidationIndex].Text
            };

        private static IEnumerable<ValidationCondition> GetFailedValidationMessages(
            IEnumerable<ValidationView> interviewValidations, IList<ValidationCondition> questionnaireValidations)
        {
            if (interviewValidations == null) yield break;

            foreach (var failedValidation in interviewValidations)
            {
                var validationExpression =
                    questionnaireValidations[failedValidation.FailedValidationIndex].Expression;
                var validationMessage = failedValidation.Message;

                yield return new ValidationCondition(validationExpression, validationMessage);
            }
        }

        private static InterviewTranslationView ToTranslationView(Translation translation)
            => new InterviewTranslationView { Id = translation.Id, Name = translation.Name };

        private InterviewGroupView ToGroupView(InterviewTreeGroup group) => new InterviewGroupView
        {
                Id = group.Identity,
                Title = this.ToGroupTitleView(@group),
                Depth = group.Parents?.Count() + 1 ?? 1
        };

        private string ToGroupTitleView(InterviewTreeGroup group)
        {
            var roster = group as InterviewTreeRoster;

            return roster != null
                ? $"{roster.Title.Text} - {roster.RosterTitle ?? this.substitutionService.DefaultSubstitutionText}"
                : @group.Title.Text;
        }

        private static bool IsEntityInFilter(InterviewDetailsFilter? filter, IInterviewTreeNode entity, InterviewData interviewData)
        {
            var question = entity as InterviewTreeQuestion;

            if (question != null)
            {
                switch (filter)
                {
                    case InterviewDetailsFilter.Answered:
                        return question.IsAnswered();
                    case InterviewDetailsFilter.Unanswered:
                        return !question.IsDisabled() && !question.IsAnswered();
                    case InterviewDetailsFilter.Commented:
                        return question.AnswerComments?.Any() ?? false;
                    case InterviewDetailsFilter.Enabled:
                        return !question.IsDisabled();
                    case InterviewDetailsFilter.Flagged:
                        return GetIsFlagged(question, interviewData);
                    case InterviewDetailsFilter.Invalid:
                        return !question.IsValid;
                    case InterviewDetailsFilter.Supervisors:
                        return question.IsSupervisors;
                    case InterviewDetailsFilter.Hidden:
                        return question.IsHidden;
                }
            }

            var staticText = entity as InterviewTreeStaticText;
            if (staticText != null)
            {
                switch (filter)
                {
                    case InterviewDetailsFilter.Enabled:
                        return !staticText.IsDisabled();
                    case InterviewDetailsFilter.Invalid:
                        return !staticText.IsValid;
                    case InterviewDetailsFilter.Flagged:
                        return false;
                    case InterviewDetailsFilter.All:
                        return true;
                    default:
                        return false;
                }
            }
            return true;
        }

        public Guid GetFirstChapterId(Guid id)
        {
            var interview = this.statefulInterviewRepository.Get(id.FormatGuid());
            return interview.FirstSection.Identity.Id;
        }
    }
}