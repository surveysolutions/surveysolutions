using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
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
        private readonly IWebNavigationService webNavigationService;
        private readonly ISubstitutionTextFactory substitutionTextFactory;
        private static readonly Regex HtmlRemovalRegex = new Regex(Constants.HtmlRemovalPattern, RegexOptions.Compiled);

        public WebInterviewInterviewEntityFactory(IMapper autoMapper,
            IEnumeratorGroupStateCalculationStrategy enumeratorGroupStateCalculationStrategy,
            ISupervisorGroupStateCalculationStrategy supervisorGroupStateCalculationStrategy,
            IWebNavigationService webNavigationService,
            ISubstitutionTextFactory substitutionTextFactory)
        {
            this.autoMapper = autoMapper;
            this.enumeratorGroupStateCalculationStrategy = enumeratorGroupStateCalculationStrategy;
            this.supervisorGroupStateCalculationStrategy = supervisorGroupStateCalculationStrategy;
            this.webNavigationService = webNavigationService;
            this.substitutionTextFactory = substitutionTextFactory;
        }

        public Sidebar GetSidebarChildSectionsOf(string currentSectionId,
            IStatefulInterview interview,
            IQuestionnaire questionnaire,
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
                var children = parentId == null || parentId == "null"
                    ? interview.GetEnabledSections()
                    : interview.GetGroup(Identity.Parse(parentId))?.Children
                        .OfType<InterviewTreeGroup>().Where(g => !g.IsDisabled());

                children = children.Where(e => !questionnaire.IsCustomViewRoster(e.Identity.Id));

                foreach (var child in children ?? Array.Empty<InterviewTreeGroup>())
                {
                    var sidebar = this.autoMapper.Map<InterviewTreeGroup, SidebarPanel>(child, SidebarMapOptions);
                    result.Groups.Add(sidebar);
                }

                void SidebarMapOptions(IMappingOperationOptions<InterviewTreeGroup, SidebarPanel> opts)
                {
                    opts.AfterMap((g, sidebarPanel) =>
                    {
                        sidebarPanel.Status = this.CalculateSimpleStatus(g, isReviewMode, interview, questionnaire);

                        this.ApplyValidity(sidebarPanel.Validity, sidebarPanel.Status);
                        sidebarPanel.Collapsed = !visibleSections.Contains(g.Identity);
                        sidebarPanel.Current = visibleSections.Contains(g.Identity);
                        sidebarPanel.HasChildren = g.Children.OfType<InterviewTreeGroup>().Any(c => 
                            !c.IsDisabled() 
                            && !questionnaire.IsCustomViewRoster(c.Identity.Id));
                    });
                }
            }

            return result;
        }

        public InterviewEntity GetEntityDetails(string id, IStatefulInterview callerInterview, IQuestionnaire questionnaire, bool isReviewMode)
        {
            var identity = Identity.Parse(id);

            InterviewTreeQuestion question = callerInterview.GetQuestion(identity);

            var webLinksVirtualDirectory = WebLinksVirtualDirectory(isReviewMode);

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
                                res.Options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200, null);
                            });
                        }
                        break;
                    case InterviewQuestionType.Cascading:
                        var canBeShownAsList = questionnaire.ShowCascadingAsList(identity.Id);
                        if (canBeShownAsList)
                        {
                            var threshold = questionnaire.GetCascadingAsListThreshold(identity.Id) ?? Constants.DefaultCascadingAsListThreshold;
                            if (!callerInterview.DoesCascadingQuestionHaveMoreOptionsThanThreshold(identity, threshold))
                            {
                                var parentCascadingQuestion = question.GetAsInterviewTreeCascadingQuestion().GetCascadingParentQuestion();
                                if (parentCascadingQuestion.IsAnswered())
                                {
                                    result = this.Map<InterviewSingleOptionQuestion>(question, res =>
                                        {
                                            res.Options = callerInterview.GetTopFilteredOptionsForQuestion(identity, parentCascadingQuestion.GetAnswer().SelectedValue, null, threshold, null);
                                        });
                                    break;
                                }
                            }
                        }

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
                            interviewIntegerQuestion.Options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200, null);

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
                            interviewDoubleQuestion.Options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200, null);
                            result = interviewDoubleQuestion;
                        }
                        break;
                    case InterviewQuestionType.MultiFixedOption:
                        {
                            var callerQuestionnaire = questionnaire;

                            var typedResult = this.autoMapper.Map<InterviewMutliOptionQuestion>(question);

                            var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200, null);
                            var answeredOptions = questionnaire.GetCategoricalMultiOptionsByValues(question.Identity.Id, typedResult.Answer);

                            typedResult.Options = options.Union(answeredOptions).Distinct().ToList();
                            typedResult.Ordered = callerQuestionnaire.ShouldQuestionRecordAnswersOrder(identity.Id);
                            typedResult.MaxSelectedAnswersCount = callerQuestionnaire.GetMaxSelectedAnswerOptions(identity.Id);
                            typedResult.IsRosterSize = callerQuestionnaire.IsRosterSizeQuestion(identity.Id);
                            typedResult.ProtectedAnswer = typedResult.Answer
                                .Where(i => question.IsAnswerProtected(i))
                                .ToArray();

                            result = typedResult;
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
                            var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200, null);
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

                this.PutValidationMessages(result.Validity, callerInterview, identity, questionnaire, isReviewMode);
                this.PutHideInstructions(result, identity, questionnaire);
                this.ApplyDisablement(result, identity, questionnaire);
                this.ApplyReviewState(result, question, callerInterview, isReviewMode);
                result.Comments = this.GetComments(question);

                result.Instructions = this.webNavigationService.MakeNavigationLinks(result.Instructions, identity, questionnaire, callerInterview, webLinksVirtualDirectory);
                result.Title = this.webNavigationService.MakeNavigationLinks(result.Title, identity, questionnaire, callerInterview, webLinksVirtualDirectory);

                return result;
            }

            InterviewTreeStaticText staticText = callerInterview.GetStaticText(identity);
            if (staticText != null)
            {
                InterviewStaticText result = this.autoMapper.Map<InterviewTreeStaticText, InterviewStaticText>(staticText);

                var attachment = questionnaire.GetAttachmentForEntity(identity.Id);
                result.AttachmentContent = attachment?.ContentId;

                result.Title = this.webNavigationService.MakeNavigationLinks(result.Title, identity, questionnaire, callerInterview, webLinksVirtualDirectory);

                this.ApplyDisablement(result, identity, questionnaire);
                this.PutValidationMessages(result.Validity, callerInterview, identity, questionnaire, isReviewMode);
                return result;
            }

            InterviewTreeGroup group = callerInterview.GetGroup(identity);
            if (group != null)
            {
                var result = this.autoMapper.Map<InterviewGroupOrRosterInstance>(group);

                this.ApplyDisablement(result, identity, questionnaire);
                this.ApplyGroupStateData(result, group, callerInterview, isReviewMode, questionnaire);
                this.ApplyValidity(result.Validity, result.Status);
                return result;
            }

            InterviewTreeRoster roster = callerInterview.GetRoster(identity);
            if (roster != null)
            {
                return GetRosterInstanceEntity(callerInterview, questionnaire, isReviewMode, roster, identity);
            }

            if (questionnaire.IsTableRoster(identity.Id) || questionnaire.IsMatrixRoster(identity.Id))
            {
                var parentGroupId = questionnaire.GetParentGroup(identity.Id);
                var parentGroup = callerInterview.GetGroup(new Identity(parentGroupId.Value, identity.RosterVector));

                string CreateTitleWithSubstitutionsAndMarkdownAndNavLinks(Identity questionIdentity, string text)
                {
                    var substitutionText = this.substitutionTextFactory.CreateText(questionIdentity, text, questionnaire);
                    substitutionText.ReplaceSubstitutions(parentGroup.Tree);

                    return this.webNavigationService.MakeNavigationLinks(
                        substitutionText?.BrowserReadyText, questionIdentity, questionnaire, callerInterview, webLinksVirtualDirectory);
                }

                var tableRosterInstances = parentGroup.Children
                    .Where(c => c.Identity.Id == identity.Id)
                    .Cast<InterviewTreeRoster>()
                    .Select(ri => new TableRosterInstance()
                    {
                        Id = ri.Identity.ToString(),
                        RosterVector = ri.Identity.RosterVector.ToString(),
                        RosterTitle = CreateTitleWithSubstitutionsAndMarkdownAndNavLinks(ri.Identity, ri.RosterTitle),
                        Status = this.CalculateSimpleStatus(ri, isReviewMode, callerInterview, questionnaire),
                    }).ToArray();

                tableRosterInstances.ForEach(rosterInstance =>
                {
                    this.ApplyDisablement(rosterInstance, identity, questionnaire);
                    this.ApplyValidity(rosterInstance.Validity, rosterInstance.Status);
                });


                return new TableOrMatrixRoster()
                {
                    Id = id,
                    Title =  CreateTitleWithSubstitutionsAndMarkdownAndNavLinks(identity, questionnaire.GetGroupTitle(identity.Id)),
                    Questions = questionnaire.GetChildQuestions(identity.Id)
                        .Select(questionId =>
                        {
                            var qIdentity = Identity.Create(questionId, identity.RosterVector);

                            return new TableOrMatrixRosterQuestionReference()
                            {
                                Id = questionId.FormatGuid(),
                                Title = CreateTitleWithSubstitutionsAndMarkdownAndNavLinks(qIdentity, questionnaire.GetQuestionTitle(questionId)),
                                Instruction = CreateTitleWithSubstitutionsAndMarkdownAndNavLinks(qIdentity, questionnaire.GetQuestionInstruction(questionId)),
                                EntityType = GetEntityType(qIdentity, questionnaire, callerInterview, isReviewMode).ToString(),
                                Options = questionnaire.IsMatrixRoster(identity.Id)
                                    ? questionnaire.GetOptionsForQuestion(questionId, null, null, new int[0]).ToArray()
                                    : null
                            };
                        }).ToArray(),
                    Instances = tableRosterInstances
                };
            }

            return null;
        }

        private InterviewGroupOrRosterInstance GetRosterInstanceEntity(IStatefulInterview callerInterview, IQuestionnaire questionnaire,
            bool isReviewMode, InterviewTreeRoster roster, Identity identity)
        {
            var result = this.autoMapper.Map<InterviewGroupOrRosterInstance>(roster);

            this.ApplyDisablement(result, identity, questionnaire);
            this.ApplyGroupStateData(result, roster, callerInterview, isReviewMode, questionnaire);
            this.ApplyValidity(result.Validity, result.Status);
            return result;
        }

        private T Map<T>(InterviewTreeQuestion question, Action<T> afterMap = null)
        {
            return this.autoMapper.Map<InterviewTreeQuestion, T>(question,
                opts => opts.AfterMap((treeQuestion, target) => afterMap?.Invoke(target)));
        }

        private void PutValidationMessages(Validity validity, IStatefulInterview callerInterview, Identity identity,
            IQuestionnaire questionnaire, bool isReview)
        {
            validity.Messages = callerInterview.GetFailedValidationMessages(identity, Resources.WebInterview.Error)
                .Select(x => this.webNavigationService.MakeNavigationLinks(x, identity, questionnaire, callerInterview, WebLinksVirtualDirectory(isReview))).ToArray();

            validity.Warnings = callerInterview.GetFailedWarningMessages(identity, Resources.WebInterview.Warning)
                .Select(x => this.webNavigationService.MakeNavigationLinks(x, identity, questionnaire, callerInterview, WebLinksVirtualDirectory(isReview))).ToArray();
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
                    CommentTimeUtc = ac.CommentTime,
                    Id = ac.Id,
                    Resolved = ac.Resolved
                })
                .ToArray();
        }

        public void ApplyValidity(Validity validity, GroupStatus status)
        {
            validity.IsValid = !(status == GroupStatus.StartedInvalid || status == GroupStatus.CompletedInvalid);
        }

        private void ApplyGroupStateData(InterviewGroupOrRosterInstance @group, InterviewTreeGroup treeGroup,
            IStatefulInterview callerInterview, bool isReviewMode, IQuestionnaire questionnaire)
        {
            group.Status = this.CalculateSimpleStatus(treeGroup, isReviewMode, callerInterview, questionnaire);
        }

        public GroupStatus CalculateSimpleStatus(InterviewTreeGroup group, bool isReviewMode, IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            if (isReviewMode)
            {
                return this.supervisorGroupStateCalculationStrategy.CalculateDetailedStatus(group.Identity, interview, questionnaire);
            }

            return this.enumeratorGroupStateCalculationStrategy.CalculateDetailedStatus(@group.Identity, interview, questionnaire);
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

        public Identity GetUIParent(IStatefulInterview interview, IQuestionnaire questionnaire, Identity identity)
        {
            var parent = interview.GetParentGroup(identity);
            while (parent != null && questionnaire.IsCustomViewRoster(parent.Id))
            {
                parent = interview.GetParentGroup(parent);
            }
            return parent;
        }

        public InterviewEntityType GetEntityType(Identity identity, IQuestionnaire callerQuestionnaire,
            IStatefulInterview interview, bool isReviewMode)
        {
            var entityId = identity.Id;

            if (callerQuestionnaire.IsVariable(entityId)) return InterviewEntityType.Unsupported;

            if (callerQuestionnaire.HasGroup(entityId) || callerQuestionnaire.IsRosterGroup(entityId))
            {
                if (callerQuestionnaire.IsFlatRoster(entityId))
                    return InterviewEntityType.GroupTitle;
                if (callerQuestionnaire.IsTableRoster(entityId))
                    return InterviewEntityType.TableRoster;
                if (callerQuestionnaire.IsMatrixRoster(entityId))
                    return InterviewEntityType.MatrixRoster;
                return InterviewEntityType.Group;
            }

            if (callerQuestionnaire.IsStaticText(entityId)) return InterviewEntityType.StaticText;

            switch (callerQuestionnaire.GetQuestionType(entityId))
            {
                case QuestionType.DateTime:
                    return InterviewEntityType.DateTime;
                case QuestionType.GpsCoordinates:
                    return InterviewEntityType.Gps;
                case QuestionType.Multimedia:
                    return InterviewEntityType.Multimedia; // InterviewEntityType.Multimedia;
                case QuestionType.MultyOption:
                    if (callerQuestionnaire.IsQuestionFilteredCombobox(entityId))
                    {
                        return InterviewEntityType.MultiCombobox;
                    }
                    if (callerQuestionnaire.IsLinkedToListQuestion(entityId))
                        return InterviewEntityType.CategoricalMulti;
                    if (callerQuestionnaire.IsQuestionLinked(entityId)
                        || callerQuestionnaire.IsQuestionLinkedToRoster(entityId))
                        return InterviewEntityType.LinkedMulti;
                    return callerQuestionnaire.IsQuestionYesNo(entityId)
                        ? InterviewEntityType.CategoricalYesNo
                        : InterviewEntityType.CategoricalMulti;
                case QuestionType.SingleOption:
                    if (callerQuestionnaire.IsLinkedToListQuestion(entityId))
                        return InterviewEntityType.CategoricalSingle;
                    if (callerQuestionnaire.IsQuestionLinked(entityId)
                        || callerQuestionnaire.IsQuestionLinkedToRoster(entityId))
                        return InterviewEntityType.LinkedSingle;
                    if (callerQuestionnaire.IsQuestionCascading(entityId))
                    {
                        if (callerQuestionnaire.ShowCascadingAsList(entityId))
                        {
                            var threshold = callerQuestionnaire.GetCascadingAsListThreshold(entityId) ?? Constants.DefaultCascadingAsListThreshold;

                            if (!interview.DoesCascadingQuestionHaveMoreOptionsThanThreshold(identity, threshold))
                            {
                                return InterviewEntityType.CategoricalSingle;
                            }

                            return InterviewEntityType.Combobox;
                        }

                        return InterviewEntityType.Combobox;
                    }
                    if (callerQuestionnaire.IsQuestionFilteredCombobox(entityId))
                        return InterviewEntityType.Combobox;
                    else
                        return InterviewEntityType.CategoricalSingle;
                case QuestionType.Numeric:
                    return callerQuestionnaire.IsQuestionInteger(entityId)
                        ? InterviewEntityType.Integer
                        : InterviewEntityType.Double;
                case QuestionType.Text:
                    return InterviewEntityType.TextQuestion;
                case QuestionType.TextList:
                    return InterviewEntityType.TextList;
                case QuestionType.QRBarcode:
                    return InterviewEntityType.QRBarcode;
                case QuestionType.Audio:
                    return InterviewEntityType.Audio;
                case QuestionType.Area:
                    return isReviewMode ? InterviewEntityType.Area : InterviewEntityType.Unsupported;
                default:
                    return InterviewEntityType.Unsupported;
            }
        }

        protected virtual string WebLinksVirtualDirectory(bool isReview) => "WebTester/Interview";
    }
}
