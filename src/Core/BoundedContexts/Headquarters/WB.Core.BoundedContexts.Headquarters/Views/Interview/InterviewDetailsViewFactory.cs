using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
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
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewDetailsViewFactory : IInterviewDetailsViewFactory
    {
        private readonly IUserViewFactory userStore;
        private readonly IChangeStatusFactory changeStatusFactory;
        private readonly IInterviewPackagesService incomingSyncPackagesQueue;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IInterviewFactory interviewFactory;
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
            ISubstitutionService substitutionService,
            IInterviewFactory interviewFactory)
        {
            this.userStore = userStore;
            this.changeStatusFactory = changeStatusFactory;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.questionnaireStorage = questionnaireStorage;
            this.interviewSummaryRepository = interviewSummaryRepository;
            this.substitutionService = substitutionService;
            this.interviewFactory = interviewFactory;
            this.statefulInterviewRepository = statefulInterviewRepository;
        }

        public DetailsViewModel GetInterviewDetails(Guid interviewId, InterviewDetailsFilter questionsTypes, Identity currentGroupIdentity)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());
            InterviewSummary interviewSummary = this.interviewSummaryRepository.GetById(interviewId);

            Identity[] flaggedQuestionIds = this.interviewFactory.GetFlaggedQuestionIds(interviewId)
                .Where(x => interview.GetQuestion(x) != null).ToArray();

            var questionnaireIdentity = new QuestionnaireIdentity(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, interview.Language);
            var questionnaireDocument = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);

            var responsible = this.userStore.GetUser(new UserViewInputModel(interviewSummary.ResponsibleId));

            var rootNode = new InterviewGroupView(Identity.Create(questionnaire.QuestionnaireId, RosterVector.Empty))
            {
                Title = questionnaire.Title
            };
            var interviewGroupViews = rootNode.ToEnumerable()
                                              .Concat(interview.GetAllGroupsAndRosters().Select(this.ToGroupView)).ToList();

            var interviewEntityViews = this.GetFilteredEntities(interview, flaggedQuestionIds, questionnaire, questionnaireDocument, currentGroupIdentity, questionsTypes);
            if (questionsTypes != InterviewDetailsFilter.All)
                interviewEntityViews = this.GetEntitiesWithoutEmptyGroupsAndRosters(interviewEntityViews);

            return new DetailsViewModel
            {
                QuestionsTypes = questionsTypes,
                SelectedGroupId = currentGroupIdentity,
                FilteredEntities = interviewEntityViews,
                InterviewDetails = new InterviewDetailsView
                {
                    Groups = interviewGroupViews,
                    Responsible = new UserLight(interviewSummary.ResponsibleId, responsible?.UserName ?? "<UNKNOWN>"),
                    Title = questionnaire.Title,
                    Description = questionnaireDocument.Description,
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
                    CommentedCount = interview.GetAllCommentedEnabledQuestions().Count(),
                    EnabledCount = interview.CountAllEnabledQuestions(),
                    FlaggedCount = flaggedQuestionIds.Length,
                    InvalidCount = interview.CountAllInvalidEntities(),
                    SupervisorsCount = interview.CountEnabledSupervisorQuestions(),
                    HiddenCount = interview.CountEnabledHiddenQuestions(),
                    UnansweredCount = interview.CountAllEnabledUnansweredQuestions()
                },
                History = this.changeStatusFactory.Load(new ChangeStatusInputModel {InterviewId = interviewId}),
                HasUnprocessedSyncPackages = this.incomingSyncPackagesQueue.HasPendingPackageByInterview(interviewId),
                Translations = questionnaire.GetTranslationLanguages().Select(ToTranslationView).ToReadOnlyCollection(),
                InterviewKey = interviewSummary.Key,
                QuestionnaireName = questionnaire.Title,
                QuestionnaireVersion = interviewSummary.QuestionnaireVersion
            };
        }

        private IEnumerable<InterviewEntityView> GetEntitiesWithoutEmptyGroupsAndRosters(IEnumerable<InterviewEntityView> interviewEntityViews)
        {
            var allEntities = interviewEntityViews.ToList();
            var parentsOfQuestions = allEntities.OfType<InterviewQuestionView>().Select(x => x.ParentId).ToHashSet();
            var parentsOfStaticTexts = allEntities.OfType<InterviewStaticTextView>().Select(x => x.ParentId).ToHashSet();

            foreach (var interviewEntityView in allEntities)
            {
                if (interviewEntityView is InterviewGroupView groupView)
                {
                    if (parentsOfQuestions.Contains(groupView.Id) || parentsOfStaticTexts.Contains(groupView.Id))
                        yield return groupView;
                }
                else
                {
                    yield return interviewEntityView;
                }
            }
        }

        private IEnumerable<InterviewEntityView> GetFilteredEntities(IStatefulInterview interview,
            Identity[] flaggedQuestionIds, IQuestionnaire questionnaire, QuestionnaireDocument questionnaireDocument, Identity currentGroupIdentity,
            InterviewDetailsFilter questionsTypes)
        {
            var groupEntities = currentGroupIdentity == null || currentGroupIdentity.Id == questionnaire.QuestionnaireId
                ? interview.GetAllSections()
                : (interview.GetGroup(currentGroupIdentity) as IInterviewTreeNode).ToEnumerable().Where(n => n != null);

            foreach (var entity in this.GetQuestionsFirstAndGroupsAfterFrom(groupEntities))
            {
                if (!IsEntityInFilter(questionsTypes, entity, flaggedQuestionIds)) continue;

                switch (entity)
                {
                    case InterviewTreeQuestion question:
                        yield return this.ToQuestionView(question, questionnaire, questionnaireDocument, interview, flaggedQuestionIds);
                        break;
                    case InterviewTreeGroup group:
                        yield return this.ToGroupView(@group);
                        break;
                    case InterviewTreeStaticText staticText:
                        yield return this.ToStaticTextView(interview, staticText, questionnaire, questionnaireDocument);
                        break;
                }
            }
        }

        private IEnumerable<IInterviewTreeNode> GetQuestionsFirstAndGroupsAfterFrom(IEnumerable<IInterviewTreeNode> groups)
        {
            var itemsQueue = new Stack<IInterviewTreeNode>(groups.Reverse());

            while (itemsQueue.Count > 0)
            {
                var currentItem = itemsQueue.Pop();

                yield return currentItem;

                IEnumerable<IInterviewTreeNode> childItems = currentItem.Children;

                if (childItems != null)
                {
                    var reverseChildItems = childItems.Reverse().ToList();
                    var childItemsIncOrrectOrder = 
                        reverseChildItems.Where(child =>  child is InterviewTreeGroup)
                        .Concat(reverseChildItems.Where(child => !(child is InterviewTreeGroup)));

                    foreach (var childItem in childItemsIncOrrectOrder)
                    {
                        itemsQueue.Push(childItem);
                    }
                }
            }
        }

        private static InterviewAttachmentViewModel ToAttachmentView(IQuestionnaire questionnaire, Guid staticTextId)
        {
            var attachment = questionnaire.GetAttachmentForEntity(staticTextId);
            
            if (attachment == null) return null;

            return new InterviewAttachmentViewModel
            {
                ContentId = attachment.ContentId,
                ContentName = attachment.Name
            };
        }

        private InterviewEntityView ToQuestionView(InterviewTreeQuestion interviewQuestion, IQuestionnaire questionnaire, 
            QuestionnaireDocument questionnaireDocument, IStatefulInterview interview, Identity[] flaggedQuestionIds)
        {
            var questionnaireQuestion = questionnaireDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == interviewQuestion.Identity.Id);
            
            return new InterviewQuestionView
            {
                Id = interviewQuestion.Identity,
                ParentId = interviewQuestion.Parent.Identity,
                Title = interviewQuestion.Title.BrowserReadyText,
                IsAnswered = interviewQuestion.IsAnswered(),
                IsValid = interviewQuestion.IsValid,
                AnswerString = GetAnswerAsString(interviewQuestion, questionnaire), 
                QuestionType = questionnaire.GetQuestionType(interviewQuestion.Identity.Id),
                IsFeatured = interviewQuestion.IsPrefilled,
                LinkedToQuestionId = questionnaire.IsQuestionLinked(interviewQuestion.Identity.Id) ? questionnaire.GetQuestionReferencedByLinkedQuestion(interviewQuestion.Identity.Id) : (Guid?)null,
                LinkedToRosterId = questionnaire.IsQuestionLinkedToRoster(interviewQuestion.Identity.Id) ? questionnaire.GetRosterReferencedByLinkedQuestion(interviewQuestion.Identity.Id) : (Guid?)null,
                Scope = questionnaire.GetQuestionScope(interviewQuestion.Identity.Id),
                Variable = interviewQuestion.VariableName,
                Settings = ToQuestionSettingsView(questionnaireQuestion),
                Comments = interviewQuestion.AnswerComments.Select(ToCommentView).ToList(),
                IsEnabled = !interviewQuestion.IsDisabled(),
                IsReadOnly = !(interviewQuestion.IsSupervisors && interview.Status < InterviewStatus.ApprovedByHeadquarters),
                Options = ToOptionsView(interviewQuestion, interview),
                Answer = ToAnswerView(interviewQuestion),
                IsFlagged = GetIsFlagged(interviewQuestion, flaggedQuestionIds),
                FailedValidationMessages = GetFailedValidationMessages(
                    interviewQuestion.FailedValidations?.Select(
                        (x, index) => ToValidationView(interviewQuestion.ValidationMessages, x, index)),
                    questionnaireQuestion.ValidationConditions).ToList()
            };
        }

        private string GetAnswerAsString(InterviewTreeQuestion interviewQuestion, IQuestionnaire questionnaire)
        {
            if (!interviewQuestion.IsAnswered())
                return string.Empty;

            if (interviewQuestion.InterviewQuestionType == InterviewQuestionType.Integer)
            {
                var integerValue = interviewQuestion.GetAsInterviewTreeIntegerQuestion().GetAnswer().Value;
                return questionnaire.ShouldUseFormatting(interviewQuestion.Identity.Id)
                    ? integerValue.ToString("N0", CultureInfo.InvariantCulture)
                    : integerValue.ToString(CultureInfo.InvariantCulture);
            }

            if (interviewQuestion.InterviewQuestionType == InterviewQuestionType.Double)
            {
                var doubleValue = interviewQuestion.GetAsInterviewTreeDoubleQuestion().GetAnswer().Value;
                return questionnaire.ShouldUseFormatting(interviewQuestion.Identity.Id)
                    ? $"{doubleValue:0,0.#################}"
                    : doubleValue.ToString(CultureInfo.InvariantCulture);
            }

            if (interviewQuestion.IsDateTime)
            {
                var interviewTreeDateTimeQuestion = interviewQuestion.GetAsInterviewTreeDateTimeQuestion();
                DateTime? dateTime = interviewTreeDateTimeQuestion.GetAnswer()?.Value;
                return AnswerUtils.AnswerToString(dateTime, isTimestamp: interviewTreeDateTimeQuestion.IsTimestamp);
            }

            return interviewQuestion.GetAnswerAsString();
        }

        private static bool GetIsFlagged(InterviewTreeQuestion interviewQuestion, Identity[] flaggedQuestionIds) 
            => flaggedQuestionIds.Contains(interviewQuestion.Identity);

        private static object ToAnswerView(InterviewTreeQuestion interviewQuestion)
        {
            if (!interviewQuestion.IsAnswered()) return null;

            if (interviewQuestion.IsYesNo)
                return interviewQuestion.GetAsInterviewTreeYesNoQuestion().GetAnswer().ToAnsweredYesNoOptions().ToArray();

            if (interviewQuestion.IsMultiFixedOption)
                return interviewQuestion.GetAsInterviewTreeMultiOptionQuestion().GetAnswer().ToDecimals().ToArray();

            if (interviewQuestion.IsMultiLinkedOption)
                return interviewQuestion.GetAsInterviewTreeMultiLinkedToRosterQuestion().GetAnswer().ToRosterVectorArray();

            if (interviewQuestion.IsSingleFixedOption)
                return interviewQuestion.GetAsInterviewTreeSingleOptionQuestion().GetAnswer().SelectedValue;

            if (interviewQuestion.IsSingleLinkedOption)
                return interviewQuestion.GetAsInterviewTreeSingleLinkedToRosterQuestion().GetAnswer().SelectedValue;

            if (interviewQuestion.IsGps)
                return interviewQuestion.GetAsInterviewTreeGpsQuestion().GetAnswer().Value;

            if (interviewQuestion.IsText)
                return interviewQuestion.GetAsInterviewTreeTextQuestion().GetAnswer().Value;

            if (interviewQuestion.IsInteger)
                return interviewQuestion.GetAsInterviewTreeIntegerQuestion().GetAnswer().Value;

            if (interviewQuestion.IsDouble)
                return interviewQuestion.GetAsInterviewTreeDoubleQuestion().GetAnswer().Value;

            if (interviewQuestion.IsArea)
                return interviewQuestion.GetAsInterviewTreeAreaQuestion().GetAnswer().Value;

            if (interviewQuestion.IsAudio)
                return interviewQuestion.GetAsInterviewTreeAudioQuestion().GetAnswer().FileName;

            return null;
        }

        private List<QuestionOptionView> ToOptionsView(InterviewTreeQuestion interviewQuestion, IStatefulInterview interview)
        {
            if (interviewQuestion.IsSingleFixedOption || interviewQuestion.IsMultiFixedOption)
            {
                var options = interview.GetTopFilteredOptionsForQuestion(interviewQuestion.Identity, null, null, int.MaxValue)?.Select(a => new QuestionOptionView
                              {
                                  Value = a.Value,
                                  Label = a.Title
                              })?.ToList() ?? new List<QuestionOptionView>(); ;

                var optionsToMarkAsSelected = new List<int>();
                if (interviewQuestion.IsSingleFixedOption && interviewQuestion.IsAnswered())
                    optionsToMarkAsSelected.Add(interviewQuestion.GetAsInterviewTreeSingleOptionQuestion().GetAnswer().SelectedValue);

                if (interviewQuestion.IsMultiFixedOption && interviewQuestion.IsAnswered())
                    optionsToMarkAsSelected.AddRange(interviewQuestion.GetAsInterviewTreeMultiOptionQuestion().GetAnswer().CheckedValues);

                foreach (var selectedValue in optionsToMarkAsSelected)
                {
                    var selectedOption = options.FirstOrDefault(x => (int)x.Value == selectedValue);
                    if (selectedOption == null) continue;
                    selectedOption.IsChecked = true;
                    selectedOption.Index = optionsToMarkAsSelected.IndexOf(selectedValue) + 1;
                }

                return options;
            }

            if (interviewQuestion.IsLinked)
            {
                var optionsToMarkAsSelected = new List<RosterVector>();
                if (interviewQuestion.IsSingleLinkedOption && interviewQuestion.IsAnswered())
                {
                    optionsToMarkAsSelected.Add(interviewQuestion.GetAsInterviewTreeSingleLinkedToRosterQuestion().GetAnswer().SelectedValue);
                }
                if (interviewQuestion.IsMultiLinkedOption && interviewQuestion.IsAnswered())
                {
                    optionsToMarkAsSelected.AddRange(interviewQuestion.GetAsInterviewTreeMultiLinkedToRosterQuestion().GetAnswer().CheckedValues);
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
                if (interviewQuestion.IsSingleLinkedToList && interviewQuestion.IsAnswered())
                {
                    optionsToMarkAsSelected.Add(interviewQuestion.GetAsInterviewTreeSingleOptionLinkedToListQuestion().GetAnswer().SelectedValue);
                }
                if (interviewQuestion.IsMultiLinkedToList && interviewQuestion.IsAnswered())
                {
                    optionsToMarkAsSelected.AddRange(interviewQuestion.GetAsInterviewTreeMultiOptionLinkedToListQuestion().GetAnswer().CheckedValues);
                }
                var listQuestion = interview.FindQuestionInQuestionBranch(interviewQuestion.AsLinkedToList.LinkedSourceId, interviewQuestion.Identity);

                var options = interviewQuestion.AsLinkedToList.Options.Select(x => new QuestionOptionView
                {
                    Value = Convert.ToInt32(x),
                    Label = (listQuestion.GetAsInterviewTreeTextListQuestion()).GetTitleByItemCode(x),
                    IsChecked = optionsToMarkAsSelected.Contains(Convert.ToInt32(x)),
                    Index = optionsToMarkAsSelected.IndexOf(Convert.ToInt32(x)) + 1
                }).ToList();

                return options;
            }

            if (interviewQuestion.IsTextList && interviewQuestion.IsAnswered())
            {
                return interviewQuestion.GetAsInterviewTreeTextListQuestion().GetAnswer().Rows.Select(x => new QuestionOptionView
                {
                    Value = Convert.ToInt32(x.Value),
                    Label = x.Text
                }).ToList();
            }

            if (interviewQuestion.IsYesNo)
            {
                var options = interview.GetTopFilteredOptionsForQuestion(interviewQuestion.Identity, null, null, 200)?.Select(a => new QuestionOptionView
                {
                    Value = a.Value,
                    Label = a.Title
                })?.ToList() ?? new List<QuestionOptionView>();

                return options;
            }

            return new List<QuestionOptionView>();
        }

        private dynamic ToQuestionSettingsView(IQuestion question)
        {
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
                    IsLinkedToRoster = categoricalMultiQuestion.LinkedToRosterId.HasValue
                };
            }

            var categoricalSingleQuestion = question as SingleQuestion;
            if (categoricalSingleQuestion != null)
            {
                return new SingleQuestionSettings
                {
                    IsFilteredCombobox = categoricalSingleQuestion.IsFilteredCombobox ?? false,
                    IsCascade = categoricalSingleQuestion.CascadeFromQuestionId.HasValue,
                    IsLinkedToRoster = categoricalSingleQuestion.LinkedToRosterId.HasValue,
                    
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

        private InterviewEntityView ToStaticTextView(IStatefulInterview interview, InterviewTreeStaticText interviewStaticText, IQuestionnaire questionnaire,
            QuestionnaireDocument questionnaireDocument)
        {
            var attachment = ToAttachmentView(questionnaire, interviewStaticText.Identity.Id);
            var questionnaireStaticText = questionnaireDocument.FirstOrDefault<StaticText>(st => st.PublicKey == interviewStaticText.Identity.Id);

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

        private static ValidationView ToValidationView(SubstitutionText[] validationMessages,
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

        private static InterviewTranslationView ToTranslationView(string translation)
            => new InterviewTranslationView { /*Id = translation.Id,*/ Name = translation };

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

        private static bool IsEntityInFilter(InterviewDetailsFilter? filter, IInterviewTreeNode entity, Identity[] flaggedQuestionIds)
        {
            var question = entity as InterviewTreeQuestion;

            if (question != null)
            {
                switch (filter)
                {
                    case InterviewDetailsFilter.Answered:
                        return !question.IsHidden && !question.IsDisabled() && question.IsAnswered();
                    case InterviewDetailsFilter.Unanswered:
                        return !question.IsHidden && !question.IsDisabled() && !question.IsAnswered();
                    case InterviewDetailsFilter.Commented:
                        return !question.IsHidden && !question.IsDisabled() && (question.AnswerComments?.Any() ?? false);
                    case InterviewDetailsFilter.Enabled:
                        return !question.IsHidden && !question.IsDisabled();
                    case InterviewDetailsFilter.Flagged:
                        return !question.IsHidden && !question.IsDisabled() && GetIsFlagged(question, flaggedQuestionIds);
                    case InterviewDetailsFilter.Invalid:
                        return !question.IsHidden && !question.IsDisabled() && !question.IsValid;
                    case InterviewDetailsFilter.Supervisors:
                        return !question.IsHidden && !question.IsDisabled() && question.IsSupervisors;
                    case InterviewDetailsFilter.Hidden:
                        return !question.IsDisabled() && question.IsHidden;
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