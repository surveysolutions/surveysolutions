using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.WebInterview.Models;

namespace WB.Enumerator.Native.WebInterview.Services
{
    public class WebInterviewInterviewEntityFactory : IWebInterviewInterviewEntityFactory
    {
        private readonly IMapper autoMapper;
        private readonly IEnumeratorGroupStateCalculationStrategy enumeratorGroupStateCalculationStrategy;
        private readonly ISupervisorGroupStateCalculationStrategy supervisorGroupStateCalculationStrategy;
        private static readonly Regex HtmlRemovalRegex = new Regex(Constants.HtmlRemovalPattern, RegexOptions.Compiled);

        public WebInterviewInterviewEntityFactory(IMapper autoMapper,
            IEnumeratorGroupStateCalculationStrategy enumeratorGroupStateCalculationStrategy,
            ISupervisorGroupStateCalculationStrategy supervisorGroupStateCalculationStrategy)
        {
            this.autoMapper = autoMapper;
            this.enumeratorGroupStateCalculationStrategy = enumeratorGroupStateCalculationStrategy;
            this.supervisorGroupStateCalculationStrategy = supervisorGroupStateCalculationStrategy;
        }

        public Sidebar GetSidebarChildSectionsOf(string currentSectionId, 
            IStatefulInterview interview, 
            string[] sectionIds, 
            bool isReviewMode)
        {
            Sidebar result = new Sidebar();
            HashSet<Identity> visibleSections = new HashSet<Identity>();

            if (currentSectionId != null)
            {
                var currentOpenSection = interview.GetGroup(Identity.Parse(currentSectionId));

                //roster instance could be removed
                if (currentOpenSection != null)
                {
                    var shownPanels = currentOpenSection.Parents.Union(new[] { currentOpenSection });
                    visibleSections = new HashSet<Identity>(shownPanels.Select(p => p.Identity));
                }
            }

            foreach (var parentId in sectionIds.Distinct())
            {
                var children = parentId == null
                    ? interview.GetEnabledSections()
                    : interview.GetGroup(Identity.Parse(parentId))?.Children
                        .OfType<InterviewTreeGroup>().Where(g => !g.IsDisabled());

                foreach (var child in children ?? Array.Empty<InterviewTreeGroup>())
                {
                    var sidebar = this.autoMapper.Map<InterviewTreeGroup, SidebarPanel>(child, SidebarMapOptions);
                    result.Groups.Add(sidebar);
                }

                void SidebarMapOptions(IMappingOperationOptions<InterviewTreeGroup, SidebarPanel> opts)
                {
                    opts.AfterMap((g, sidebarPanel) =>
                    {
                        this.ApplyValidity(sidebarPanel.Validity, g, interview, isReviewMode);
                        sidebarPanel.Status = this.CalculateSimpleStatus(g, isReviewMode, interview);
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
                GenericQuestion result;

                switch (question.InterviewQuestionType)
                {
                    case InterviewQuestionType.SingleFixedOption:
                        if (questionnaire.IsQuestionFilteredCombobox(identity.Id))
                        {
                            result = this.Map<InterviewFilteredQuestion>(question);
                        }
                        else
                        {
                            result = this.Map<InterviewSingleOptionQuestion>(question, res =>
                            {
                                res.Options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                            });
                        }
                        break;
                    case InterviewQuestionType.Cascading:
                        result = this.Map<InterviewFilteredQuestion>(question);
                        break;
                    case InterviewQuestionType.SingleLinkedToList:
                        result = this.Map<InterviewSingleOptionQuestion>(question, res =>
                        {
                            res.Options = GetOptionsLinkedToListQuestion(callerInterview, identity,
                                (question.GetAsInterviewTreeSingleOptionLinkedToListQuestion()).LinkedSourceId).ToList();
                        });
                        break;
                    case InterviewQuestionType.Text:
                        {
                            result = this.autoMapper.Map<InterviewTextQuestion>(question);
                            var textQuestionMask = questionnaire.GetTextQuestionMask(identity.Id);
                            if (!string.IsNullOrEmpty(textQuestionMask))
                            {
                                ((InterviewTextQuestion)result).Mask = textQuestionMask;
                            }
                        }
                        break;
                    case InterviewQuestionType.Integer:
                        {
                            var interviewIntegerQuestion = this.autoMapper.Map<InterviewIntegerQuestion>(question);
                            var callerQuestionnaire = questionnaire;

                            interviewIntegerQuestion.UseFormatting = callerQuestionnaire.ShouldUseFormatting(identity.Id);
                            var isRosterSize = callerQuestionnaire.IsRosterSizeQuestion(identity.Id);
                            interviewIntegerQuestion.IsRosterSize = isRosterSize;

                            if (isRosterSize)
                            {
                                var isRosterSizeOfLongRoster = callerQuestionnaire.IsQuestionIsRosterSizeForLongRoster(identity.Id);
                                interviewIntegerQuestion.AnswerMaxValue = isRosterSizeOfLongRoster ? Constants.MaxLongRosterRowCount : Constants.MaxRosterRowCount;
                            }
                            interviewIntegerQuestion.Options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);

                            if (interviewIntegerQuestion.Answer.HasValue)
                            {
                                var hasProtectedAnswer = question.HasProtectedAnswer();
                                interviewIntegerQuestion.IsProtected = hasProtectedAnswer;

                                if (hasProtectedAnswer)
                                    interviewIntegerQuestion.ProtectedAnswer = question.GetAsInterviewTreeIntegerQuestion().ProtectedAnswer.Value;
                            }

                            result = interviewIntegerQuestion;
                        }
                        break;
                    case InterviewQuestionType.Double:
                        {
                            var interviewDoubleQuestion = this.autoMapper.Map<InterviewDoubleQuestion>(question);
                            var callerQuestionnaire = questionnaire;
                            interviewDoubleQuestion.CountOfDecimalPlaces = callerQuestionnaire.GetCountOfDecimalPlacesAllowedByQuestion(identity.Id);
                            interviewDoubleQuestion.UseFormatting = callerQuestionnaire.ShouldUseFormatting(identity.Id);
                            interviewDoubleQuestion.Options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                            result = interviewDoubleQuestion;
                        }
                        break;
                    case InterviewQuestionType.MultiFixedOption:
                        {
                            result = this.autoMapper.Map<InterviewMutliOptionQuestion>(question);

                            var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                            var typedResult = (InterviewMutliOptionQuestion)result;
                            typedResult.Options = options;
                            var callerQuestionnaire = questionnaire;
                            typedResult.Ordered = callerQuestionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                            typedResult.MaxSelectedAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                            typedResult.IsRosterSize = callerQuestionnaire.IsRosterSizeQuestion(identity.Id);
                            typedResult.ProtectedAnswer = typedResult.Answer
                                .Where(i => question.IsAnswerProtected(i))
                                .ToArray();
                        }
                        break;
                    case InterviewQuestionType.MultiLinkedOption:
                        result = this.Map<InterviewLinkedMultiQuestion>(question, res =>
                        {
                            res.Options = GetLinkedOptionsForLinkedQuestion(callerInterview, identity, question.AsLinked.Options).ToList();
                            res.Ordered = questionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                            res.MaxSelectedAnswersCount = questionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                        });
                        break;
                    case InterviewQuestionType.MultiLinkedToList:
                        result = this.Map<InterviewMutliOptionQuestion>(question, res =>
                        {
                            res.Options = GetOptionsLinkedToListQuestion(callerInterview, identity, question.GetAsInterviewTreeMultiOptionLinkedToListQuestion().LinkedSourceId).ToList();
                            var callerQuestionnaire = questionnaire;
                            res.Ordered = callerQuestionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                            res.MaxSelectedAnswersCount = questionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                        });
                        break;
                    case InterviewQuestionType.DateTime:
                        result = this.Map<InterviewDateQuestion>(question, res =>
                        {
                            res.DefaultDate = questionnaire.GetDefaultDateForDateQuestion(identity.Id);
                        });
                        break;
                    case InterviewQuestionType.TextList:
                        {
                            result = this.autoMapper.Map<InterviewTextListQuestion>(question);
                            var typedResult = (InterviewTextListQuestion)result;
                            var callerQuestionnaire = questionnaire;
                            typedResult.MaxAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id) ?? 200;
                            typedResult.IsRosterSize = callerQuestionnaire.IsRosterSizeQuestion(identity.Id);
                            foreach (var textListAnswerRowDto in typedResult.Rows)
                            {
                                textListAnswerRowDto.IsProtected = question.IsAnswerProtected(textListAnswerRowDto.Value);
                            }
                        }
                        break;
                    case InterviewQuestionType.YesNo:
                        {
                            var interviewYesNoQuestion = this.autoMapper.Map<InterviewYesNoQuestion>(question);
                            var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                            interviewYesNoQuestion.Options = options;
                            var callerQuestionnaire = questionnaire;
                            interviewYesNoQuestion.Ordered = callerQuestionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                            interviewYesNoQuestion.MaxSelectedAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                            interviewYesNoQuestion.IsRosterSize = callerQuestionnaire.IsRosterSizeQuestion(identity.Id);
                            foreach (var answerRowDto in interviewYesNoQuestion.Answer)
                            {
                                answerRowDto.IsProtected = question.IsAnswerProtected(answerRowDto.Value);
                            }

                            result = interviewYesNoQuestion;
                        }
                        break;
                    case InterviewQuestionType.Gps:
                        result = this.autoMapper.Map<InterviewGpsQuestion>(question);
                        break;
                    case InterviewQuestionType.SingleLinkedOption:
                        result = this.Map<InterviewLinkedSingleQuestion>(question, res =>
                        {
                            res.Options = GetLinkedOptionsForLinkedQuestion(callerInterview, identity, question.AsLinked.Options).ToList();
                        });
                        break;
                   
                    case InterviewQuestionType.Multimedia:
                        result = this.Map<InterviewMultimediaQuestion>(question);
                        break;
                    case InterviewQuestionType.QRBarcode:
                        result = this.autoMapper.Map<InterviewBarcodeQuestion>(question);
                        break;
                    case InterviewQuestionType.Audio:
                        result = this.autoMapper.Map<InterviewAudioQuestion>(question);
                        break;
                    default:
                        result = this.Map<StubEntity>(question);
                        break;
                    case InterviewQuestionType.Area:
                        result = this.Map<InterviewAreaQuestion>(question, res =>
                            {
                                res.Type = questionnaire.GetQuestionByVariable(question.VariableName).Properties.GeometryType;
                            });
                        break;
                }

                this.PutValidationMessages(result.Validity, callerInterview, identity);
                this.PutHideInstructions(result, identity, questionnaire);
                this.ApplyDisablement(result, identity, questionnaire);
                this.ApplyReviewState(result, question, callerInterview, isReviewMode);
                result.Comments = this.GetComments(question);

                return result;
            }

            InterviewTreeStaticText staticText = callerInterview.GetStaticText(identity);
            if (staticText != null)
            {
                InterviewStaticText result = this.autoMapper.Map<InterviewTreeStaticText, InterviewStaticText>(staticText);

                var attachment = questionnaire.GetAttachmentForEntity(identity.Id);
                result.AttachmentContent = attachment.ContentId;

                this.ApplyDisablement(result, identity, questionnaire);
                this.PutValidationMessages(result.Validity, callerInterview, identity);
                return result;
            }

            InterviewTreeGroup group = callerInterview.GetGroup(identity);
            if (group != null)
            {
                var result = this.autoMapper.Map<InterviewGroupOrRosterInstance>(group);

                this.ApplyDisablement(result, identity, questionnaire);
                this.ApplyGroupStateData(result, group, callerInterview, isReviewMode);
                this.ApplyValidity(result.Validity, group, callerInterview, isReviewMode);
                return result;
            }

            InterviewTreeRoster roster = callerInterview.GetRoster(identity);
            if (roster != null)
            {
                var result = this.autoMapper.Map<InterviewGroupOrRosterInstance>(roster);

                this.ApplyDisablement(result, identity, questionnaire);
                this.ApplyGroupStateData(result, roster, callerInterview, isReviewMode);
                this.ApplyValidity(result.Validity, roster, callerInterview, isReviewMode);
                return result;
            }

            return null;
        }

        private T Map<T>(InterviewTreeQuestion question, Action<T> afterMap = null)
        {
            return this.autoMapper.Map<InterviewTreeQuestion, T>(question,
                opts => opts.AfterMap((treeQuestion, target) => afterMap?.Invoke(target)));
        }

        private void PutValidationMessages(Validity validity, IStatefulInterview callerInterview, Identity identity)
        {
            validity.Messages = callerInterview.GetFailedValidationMessages(identity, Resources.WebInterview.Error).ToArray();
            validity.Warnings = callerInterview.GetFailedWarningMessages(identity, Resources.WebInterview.Warning).ToArray();
        }

        private void ApplyDisablement(InterviewEntity result, Identity identity, IQuestionnaire questionnaire)
        {
            if (result.IsDisabled)
            {
                result.Title = HtmlRemovalRegex.Replace(result.Title, string.Empty);
            }

            result.HideIfDisabled = questionnaire.ShouldBeHiddenIfDisabled(identity.Id);
        }

        protected virtual void ApplyReviewState(GenericQuestion result, InterviewTreeQuestion question, IStatefulInterview callerInterview, bool isReviewMode)
        {
            result.AcceptAnswer = true;
        }

        private void PutHideInstructions(GenericQuestion result, Identity id, IQuestionnaire questionnaire)
        {
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

            if (listQuestion == null || listQuestion.IsDisabled() || listQuestion.GetAsInterviewTreeTextListQuestion().GetAnswer()?.Rows == null)
                return new List<CategoricalOption>();

            return new List<CategoricalOption>(listQuestion.GetAsInterviewTreeTextListQuestion().GetAnswer().Rows.Select(x => new CategoricalOption
            {
                Value = x.Value,
                Title = x.Text
            }));
        }

        protected virtual Comment[] GetComments(InterviewTreeQuestion question)
        {
            return question.AnswerComments.Select(
                ac => new Comment
                {
                    Text = ac.Comment,
                    IsOwnComment = true,
                    UserRole = ac.UserRole,
                    CommentTimeUtc = ac.CommentTime
                })
                .ToArray();
        }

        public void ApplyValidity(Validity validity, InterviewTreeGroup group, IStatefulInterview callerInterview,
            bool isReviewMode)
        {
            validity.IsValid = !HasQuestionsWithInvalidAnswers(group, isReviewMode);
        }

        private void ApplyGroupStateData(InterviewGroupOrRosterInstance @group, InterviewTreeGroup treeGroup,
            IStatefulInterview callerInterview, bool isReviewMode)
        {
            group.Stats = this.GetGroupStatistics(treeGroup, isReviewMode);
            group.Status = this.CalculateSimpleStatus(treeGroup, isReviewMode, callerInterview);
        }

        private static bool HasQuestionsWithInvalidAnswers(InterviewTreeGroup group, bool isReviewMode) =>
            isReviewMode
                ? group.CountEnabledInvalidQuestionsAndStaticTextsForSupervisor() > 0
                : group.CountEnabledInvalidQuestionsAndStaticTexts() > 0;

        private static bool HasUnansweredQuestions(InterviewTreeGroup group, bool isReviewMode) =>
            isReviewMode
                ? group.HasUnansweredQuestionsForSupervisor()
                : group.HasUnansweredQuestions();

        private static int CountEnabledAnsweredQuestions(InterviewTreeGroup group, bool isReviewMode) =>
            isReviewMode
                ? group.CountEnabledAnsweredQuestionsForSupervisor()
                : group.CountEnabledAnsweredQuestions();

        private static int CountEnabledInvalidQuestionsAndStaticTexts(InterviewTreeGroup group, bool isReviewMode) =>
            isReviewMode
                ? group.CountEnabledInvalidQuestionsAndStaticTextsForSupervisor()
                : group.CountEnabledInvalidQuestionsAndStaticTexts();
        
        public GroupStatus CalculateSimpleStatus(InterviewTreeGroup group, bool isReviewMode, IStatefulInterview interview)
        {
            if (isReviewMode)
            {
                return this.supervisorGroupStateCalculationStrategy.CalculateDetailedStatus(group.Identity, interview);
            }

            return this.enumeratorGroupStateCalculationStrategy.CalculateDetailedStatus(@group.Identity, interview);
        }

        public GroupStatus GetInterviewSimpleStatus(IStatefulInterview interview, bool isReviewMode)
        {
            if (InvalidEntities() > 0)
                return GroupStatus.StartedInvalid;

            return ActiveQuestions() == AnsweredQuestions()
                ? GroupStatus.Completed
                : GroupStatus.NotStarted;

            int InvalidEntities() => isReviewMode
                ? interview.CountInvalidEntitiesInInterviewForSupervisor()
                : interview.CountInvalidEntitiesInInterview();

            int ActiveQuestions() => isReviewMode
                ? interview.CountActiveQuestionsInInterviewForSupervisor()
                : interview.CountActiveQuestionsInInterview();

            int AnsweredQuestions() => isReviewMode
                ? interview.CountActiveAnsweredQuestionsInInterviewForSupervisor()
                : interview.CountActiveAnsweredQuestionsInInterview();
        }

        private InterviewGroupOrRosterInstance.AnswersStats GetGroupStatistics(InterviewTreeGroup group, bool isReviewMode)
        {
            var stats = new InterviewGroupOrRosterInstance.AnswersStats
            {
                AnsweredCount = CountEnabledAnsweredQuestions(group, isReviewMode),
                HasUnanswered = HasUnansweredQuestions(group, isReviewMode),
                InvalidCount = CountEnabledInvalidQuestionsAndStaticTexts(group, isReviewMode),
                SubSectionsCount = group.Children.OfType<InterviewTreeGroup>().Count()
            };

            return stats;
        }
    }
}
