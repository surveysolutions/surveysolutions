using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    public class WebInterviewInterviewEntityFactory : IWebInterviewInterviewEntityFactory
    {
        private readonly IMapper autoMapper;
        private readonly IAuthorizedUser authorizedUser;
        private static readonly Regex HtmlRemovalRegex = new Regex(Constants.HtmlRemovalPattern, RegexOptions.Compiled);

        public WebInterviewInterviewEntityFactory(IMapper autoMapper, IAuthorizedUser authorizedUser)
        {
            this.autoMapper = autoMapper;
            this.authorizedUser = authorizedUser;
        }

        public Sidebar GetSidebarChildSectionsOf(string sectionId, IStatefulInterview interview, string[] parentIds, bool isReviewMode)
        {
            Sidebar result = new Sidebar();
            HashSet<Identity> visibleSections = new HashSet<Identity>();

            if (sectionId != null)
            {
                var currentOpenSection = interview.GetGroup(Identity.Parse(sectionId));

                //roster instance could be removed
                if (currentOpenSection != null)
                {
                    var shownPanels = currentOpenSection.Parents.Union(new[] { currentOpenSection });
                    visibleSections = new HashSet<Identity>(shownPanels.Select(p => p.Identity));
                }
            }

            foreach (var parentId in parentIds.Distinct())
            {
                var children = parentId == null
                    ? interview.GetEnabledSections()
                    : interview.GetGroup(Identity.Parse(parentId))?.Children.OfType<InterviewTreeGroup>().Where(g => !g.IsDisabled());

                foreach (var child in children ?? Array.Empty<InterviewTreeGroup>())
                {
                    var sidebar = autoMapper.Map<InterviewTreeGroup, SidebarPanel>(child, SidebarMapOptions);
                    result.Groups.Add(sidebar);
                }

                void SidebarMapOptions(IMappingOperationOptions<InterviewTreeGroup, SidebarPanel> opts)
                {
                    opts.AfterMap((g, sidebarPanel) =>
                    {
                        ApplyGroupValidity(sidebarPanel.Validity, g, isReviewMode);
                        sidebarPanel.State = GetGroupStatus(g, isReviewMode).ToString();
                        sidebarPanel.Collapsed = !visibleSections.Contains(g.Identity);
                        sidebarPanel.Current = visibleSections.Contains(g.Identity);
                    });
                }
            }

            return result;
        }

        public InterviewEntity GetEntityDetails(string id, IStatefulInterview callerInterview, IQuestionnaire questionnaire, bool isReviewMode)
        {
            var identity = Identity.Parse(id);

            InterviewTreeQuestion question = callerInterview.GetQuestion(identity);

            if (question != null)
            {
                GenericQuestion result = Map<StubEntity>(question);

                switch (question.InterviewQuestionType)
                {
                    case InterviewQuestionType.SingleFixedOption:
                        if (questionnaire.IsQuestionFilteredCombobox(identity.Id))
                        {
                            result = Map<InterviewFilteredQuestion>(question);
                        }
                        else
                        {
                            result = Map<InterviewSingleOptionQuestion>(question, res =>
                            {
                                res.Options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                            });
                        }
                        break;
                    case InterviewQuestionType.Cascading:
                        result = Map<InterviewFilteredQuestion>(question);
                        break;
                    case InterviewQuestionType.SingleLinkedToList:
                        result = Map<InterviewSingleOptionQuestion>(question, res =>
                        {
                            res.Options = GetOptionsLinkedToListQuestion(callerInterview, identity,
                                (question.GetAsInterviewTreeSingleOptionLinkedToListQuestion()).LinkedSourceId).ToList();
                        });
                        break;
                    case InterviewQuestionType.Text:
                        {
                            InterviewTreeQuestion textQuestion = callerInterview.GetQuestion(identity);
                            result = autoMapper.Map<InterviewTextQuestion>(textQuestion);
                            var textQuestionMask = questionnaire.GetTextQuestionMask(identity.Id);
                            if (!string.IsNullOrEmpty(textQuestionMask))
                            {
                                ((InterviewTextQuestion)result).Mask = textQuestionMask;
                            }
                        }
                        break;
                    case InterviewQuestionType.Integer:
                        {
                            InterviewTreeQuestion integerQuestion = callerInterview.GetQuestion(identity);
                            var interviewIntegerQuestion = autoMapper.Map<InterviewIntegerQuestion>(integerQuestion);
                            var callerQuestionnaire = questionnaire;

                            interviewIntegerQuestion.UseFormatting = callerQuestionnaire.ShouldUseFormatting(identity.Id);
                            var isRosterSize = callerQuestionnaire.ShouldQuestionSpecifyRosterSize(identity.Id);
                            interviewIntegerQuestion.IsRosterSize = isRosterSize;

                            if (isRosterSize)
                            {
                                var isRosterSizeOfLongRoster = callerQuestionnaire.IsQuestionIsRosterSizeForLongRoster(identity.Id);
                                interviewIntegerQuestion.AnswerMaxValue = isRosterSizeOfLongRoster ? Constants.MaxLongRosterRowCount : Constants.MaxRosterRowCount;
                            }

                            result = interviewIntegerQuestion;
                        }
                        break;
                    case InterviewQuestionType.Double:
                        {
                            InterviewTreeQuestion textQuestion = callerInterview.GetQuestion(identity);
                            var interviewDoubleQuestion = autoMapper.Map<InterviewDoubleQuestion>(textQuestion);
                            var callerQuestionnaire = questionnaire;
                            interviewDoubleQuestion.CountOfDecimalPlaces = callerQuestionnaire.GetCountOfDecimalPlacesAllowedByQuestion(identity.Id);
                            interviewDoubleQuestion.UseFormatting = callerQuestionnaire.ShouldUseFormatting(identity.Id);
                            result = interviewDoubleQuestion;
                        }
                        break;
                    case InterviewQuestionType.MultiFixedOption:
                        {
                            result = autoMapper.Map<InterviewMutliOptionQuestion>(question);

                            var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                            var typedResult = (InterviewMutliOptionQuestion)result;
                            typedResult.Options = options;
                            var callerQuestionnaire = questionnaire;
                            typedResult.Ordered = callerQuestionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                            typedResult.MaxSelectedAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                            typedResult.IsRosterSize = callerQuestionnaire.ShouldQuestionSpecifyRosterSize(identity.Id);
                        }
                        break;
                    case InterviewQuestionType.MultiLinkedToList:
                        result = Map<InterviewMutliOptionQuestion>(question, res =>
                        {
                            res.Options = GetOptionsLinkedToListQuestion(callerInterview, identity,
                                (question.GetAsInterviewTreeMultiOptionLinkedToListQuestion()).LinkedSourceId).ToList();
                        });
                        break;
                    case InterviewQuestionType.DateTime:
                        result = autoMapper.Map<InterviewDateQuestion>(question);
                        break;
                    case InterviewQuestionType.TextList:
                        {
                            result = autoMapper.Map<InterviewTextListQuestion>(question);
                            var typedResult = (InterviewTextListQuestion)result;
                            var callerQuestionnaire = questionnaire;
                            typedResult.MaxAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id) ?? 200;
                            typedResult.IsRosterSize = callerQuestionnaire.ShouldQuestionSpecifyRosterSize(identity.Id);
                        }
                        break;
                    case InterviewQuestionType.YesNo:
                        {
                            var interviewYesNoQuestion = autoMapper.Map<InterviewYesNoQuestion>(question);
                            var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                            interviewYesNoQuestion.Options = options;
                            var callerQuestionnaire = questionnaire;
                            interviewYesNoQuestion.Ordered = callerQuestionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                            interviewYesNoQuestion.MaxSelectedAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                            interviewYesNoQuestion.IsRosterSize = callerQuestionnaire.ShouldQuestionSpecifyRosterSize(identity.Id);

                            result = interviewYesNoQuestion;
                        }
                        break;
                    case InterviewQuestionType.Gps:
                        result = autoMapper.Map<InterviewGpsQuestion>(question);
                        break;
                    case InterviewQuestionType.SingleLinkedOption:
                        result = Map<InterviewLinkedSingleQuestion>(question, res =>
                        {
                            res.Options = GetLinkedOptionsForLinkedQuestion(callerInterview, identity, question.AsLinked.Options).ToList();
                        });
                        break;
                    case InterviewQuestionType.MultiLinkedOption:
                        result = Map<InterviewLinkedMultiQuestion>(question, res =>
                        {
                            res.Options = GetLinkedOptionsForLinkedQuestion(callerInterview, identity, question.AsLinked.Options).ToList();
                            res.Ordered = questionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                            res.MaxSelectedAnswersCount = questionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                        });
                        break;
                    case InterviewQuestionType.Multimedia:
                        result = Map<InterviewMultimediaQuestion>(question);
                        break;
                    case InterviewQuestionType.QRBarcode:
                        InterviewTreeQuestion barcodeQuestion = callerInterview.GetQuestion(identity);
                        result = autoMapper.Map<InterviewBarcodeQuestion>(barcodeQuestion);
                        break;
                    case InterviewQuestionType.Audio:
                        InterviewTreeQuestion audioQuestion = callerInterview.GetQuestion(identity);
                        result = autoMapper.Map<InterviewAudioQuestion>(audioQuestion);
                        break;
                }

                PutValidationMessages(result.Validity, callerInterview, identity);
                PutInstructions(result, identity, questionnaire);
                ApplyDisablement(result, identity, questionnaire);
                ApplyReviewState(result, question, callerInterview, isReviewMode);
                result.Comments = GetComments(question, callerInterview);

                return result;
            }

            InterviewTreeStaticText staticText = callerInterview.GetStaticText(identity);
            if (staticText != null)
            {
                InterviewStaticText result = autoMapper.Map<InterviewTreeStaticText, InterviewStaticText>(staticText, map =>
                {
                    map.AfterMap((text, interviewStaticText) =>
                    {
                        var callerQuestionnaire = questionnaire;
                        var attachment = callerQuestionnaire.GetAttachmentForEntity(identity.Id);
                        if (attachment != null)
                        {
                            interviewStaticText.AttachmentContent = attachment.ContentId;
                        }
                    });
                });

                ApplyDisablement(result, identity, questionnaire);
                PutValidationMessages(result.Validity, callerInterview, identity);
                return result;
            }

            InterviewTreeGroup group = callerInterview.GetGroup(identity);
            if (group != null)
            {
                var result = autoMapper.Map<InterviewGroupOrRosterInstance>(group);

                ApplyDisablement(result, identity, questionnaire);
                ApplyGroupStateData(result, group, isReviewMode);
                ApplyGroupValidity(result.Validity, group, isReviewMode);
                return result;
            }

            InterviewTreeRoster roster = callerInterview.GetRoster(identity);
            if (roster != null)
            {
                var result = autoMapper.Map<InterviewGroupOrRosterInstance>(roster);

                ApplyDisablement(result, identity, questionnaire);
                ApplyGroupStateData(result, roster, isReviewMode);
                ApplyGroupValidity(result.Validity, group, isReviewMode);
                return result;
            }

            return null;
        }

        private T Map<T>(InterviewTreeQuestion question, Action<T> afterMap = null)
        {
            return autoMapper.Map<InterviewTreeQuestion, T>(question,
                opts => opts.AfterMap((treeQuestion, target) => afterMap?.Invoke(target)));
        }

        private void PutValidationMessages(Validity validity, IStatefulInterview callerInterview, Identity identity)
        {
            validity.Messages = callerInterview.GetFailedValidationMessages(identity, Strings.Error)
                .ToArray();
        }

        private void ApplyDisablement(InterviewEntity result, Identity identity, IQuestionnaire questionnaire)
        {
            if (result.IsDisabled)
            {
                result.Title = HtmlRemovalRegex.Replace(result.Title, string.Empty);
            }

            result.HideIfDisabled = questionnaire.ShouldBeHiddenIfDisabled(identity.Id);
        }

        private void ApplyReviewState(GenericQuestion result, InterviewTreeQuestion question, IStatefulInterview callerInterview, bool isReviewMode)
        {
            if (!isReviewMode)
            {
                result.AcceptAnswer = true;
                return;
            }

            result.AcceptAnswer = question.IsSupervisors &&
                                  callerInterview.Status < InterviewStatus.ApprovedByHeadquarters &&
                                  (authorizedUser.IsSupervisor && !authorizedUser.IsObserving);
        }

        private void PutInstructions(GenericQuestion result, Identity id, IQuestionnaire questionnaire)
        {

            result.Instructions = questionnaire.GetQuestionInstruction(id.Id);
            result.HideInstructions = questionnaire.GetHideInstructions(id.Id);
        }

        private static IEnumerable<LinkedOption> GetLinkedOptionsForLinkedQuestion(IStatefulInterview callerInterview,
            Identity identity, List<RosterVector> options) => options.Select(x => new LinkedOption
        {
            Value = x.ToString(),
            RosterVector = x,
            Title = callerInterview.GetLinkedOptionTitle(identity, x)
        });

        private static IEnumerable<CategoricalOption> GetOptionsLinkedToListQuestion(IStatefulInterview callerInterview,
            Identity identity, Guid linkedSourceId)
        {
            var listQuestion = callerInterview.FindQuestionInQuestionBranch(linkedSourceId, identity);

            if ((listQuestion == null) || listQuestion.IsDisabled() || listQuestion.GetAsInterviewTreeTextListQuestion().GetAnswer()?.Rows == null)
                return new List<CategoricalOption>();

            return new List<CategoricalOption>(listQuestion.GetAsInterviewTreeTextListQuestion().GetAnswer().Rows.Select(x => new CategoricalOption
            {
                Value = x.Value,
                Title = x.Text
            }));
        }

        public Comment[] GetComments(InterviewTreeQuestion question, IStatefulInterview statefulInterview)
        {
            return question.AnswerComments.Select(ac
                    => new Comment
                    {
                        Text = ac.Comment,
                        IsOwnComment = ac.UserId == statefulInterview.CurrentResponsibleId,
                        UserRole = ac.UserRole,
                        CommentTimeUtc = ac.CommentTime
                    })
                .ToArray();
        }

        private void ApplyGroupValidity(Validity validity, InterviewTreeGroup treeGroup, bool isReviewMode)
        {
            validity.IsValid = !HasQuestionsWithInvalidAnswers(treeGroup);
        }

        private void ApplyGroupStateData(InterviewGroupOrRosterInstance group, InterviewTreeGroup treeGroup, bool isReviewMode)
        {
            group.StatisticsByAnswersAndSubsections = GetGroupStatisticsByAnswersAndSubsections(treeGroup, isReviewMode);
            group.StatisticsByInvalidAnswers = GetGroupStatisticsByInvalidAnswers(treeGroup);
            group.Status = GetGroupStatus(treeGroup, isReviewMode).ToString();
        }

        private static bool HasQuestionsWithInvalidAnswers(InterviewTreeGroup group)
            => group.CountEnabledInvalidQuestionsAndStaticTexts() > 0;

        private static GroupStatus GetGroupStatus(InterviewTreeGroup group, bool isReviewMode)
        {
            if (group.HasUnansweredQuestions() || HasSubgroupsWithUnansweredQuestions(group, isReviewMode))
                return group.CountEnabledAnsweredQuestions() > 0 ? GroupStatus.Started : GroupStatus.NotStarted;

            return GroupStatus.Completed;
        }

        private static bool HasSubgroupsWithUnansweredQuestions(InterviewTreeGroup group, bool isReviewMode)
            => group.Children
                .OfType<InterviewTreeGroup>()
                .Where(subGroup => !subGroup.IsDisabled())
                .Any(subGroup => GetGroupStatus(subGroup, isReviewMode) != GroupStatus.Completed || HasQuestionsWithInvalidAnswers(subGroup));

        private string GetGroupStatisticsByInvalidAnswers(InterviewTreeGroup group)
            => HasQuestionsWithInvalidAnswers(group) ? GetInformationByInvalidAnswers(group) : null;

        private static string GetGroupStatisticsByAnswersAndSubsections(InterviewTreeGroup group, bool isReviewMode)
        {
            switch (GetGroupStatus(@group, isReviewMode))
            {
                case GroupStatus.NotStarted:
                    return Headquarters.Resources.WebInterview.Interview_Group_Status_NotStarted;

                case GroupStatus.Started:
                    return string.Format(Headquarters.Resources.WebInterview.Interview_Group_Status_StartedIncompleteFormat, GetInformationByQuestionsAndAnswers(group));

                case GroupStatus.Completed:
                    return string.Format(Headquarters.Resources.WebInterview.Interview_Group_Status_CompletedFormat, GetInformationByQuestionsAndAnswers(group));
            }

            return null;
        }

        private static string GetInformationByQuestionsAndAnswers(InterviewTreeGroup group)
        {
            var subGroupsText = GetInformationBySubgroups(group);
            var enabledAnsweredQuestionsCount = group.CountEnabledAnsweredQuestions();

            return $@"{(enabledAnsweredQuestionsCount == 1 ?
                Headquarters.Resources.WebInterview.Interview_Group_AnsweredQuestions_One :
                string.Format(Headquarters.Resources.WebInterview.Interview_Group_AnsweredQuestions_Many, enabledAnsweredQuestionsCount))}, {subGroupsText}";
        }

        private static string GetInformationByInvalidAnswers(InterviewTreeGroup group)
        {
            var countEnabledInvalidQuestionsAndStaticTexts = group.CountEnabledInvalidQuestionsAndStaticTexts();

            return countEnabledInvalidQuestionsAndStaticTexts == 1
                ? Headquarters.Resources.WebInterview.Interview_Group_InvalidAnswers_One
                : string.Format(Headquarters.Resources.WebInterview.Interview_Group_InvalidAnswers_ManyFormat, 
                    countEnabledInvalidQuestionsAndStaticTexts);
        }

        private static string GetInformationBySubgroups(InterviewTreeGroup group)
        {
            var subGroupsCount = group.Children.OfType<InterviewTreeGroup>().Count();
            switch (subGroupsCount)
            {
                case 0:
                    return Headquarters.Resources.WebInterview.Interview_Group_Subgroups_Zero;

                case 1:
                    return Headquarters.Resources.WebInterview.Interview_Group_Subgroups_One;

                default:
                    return string.Format(Headquarters.Resources.WebInterview.Interview_Group_Subgroups_ManyFormat, subGroupsCount);
            }
        }
    }
}