using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Flurl.Http.Testing;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.UI.Headquarters.Models.WebInterview;
using InterviewStaticText = WB.UI.Headquarters.Models.WebInterview.InterviewStaticText;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview
    {
        public SectionData GetPrefilledQuestions()
        {
            var questions = this.GetCallerQuestionnaire()
                .GetPrefilledQuestions()
                .Select(x => new InterviewEntityWithType
                {
                    Identity = Identity.Create(x, RosterVector.Empty).ToString(),
                    EntityType = this.GetEntityType(x).ToString()
                })
                .ToArray();

            return new SectionData
            {
                Id = PrefilledSectionStub.Id,
                Type = PrefilledSectionStub.Type,
                Status = PrefilledSectionStub.Status,
                Entities = questions
            };
        }

        public static readonly SectionData PrefilledSectionStub = new SectionData
        {
            Type = "Prefilled",
            Status = SimpleGroupStatus.Other,
            Id = "C001D00D-5ECC-BEEF-CA4E-B00B1E54A111"
        };

        public InterviewDetails GetInterviewSections()
        {
            var statefulInterview = this.GetCallerInterview();

            var sections = this.GetCallerQuestionnaire().GetAllSections().Select(s => new SectionData
            {
               Id = s.FormatGuid(),
               Type = "Section",
               Status = CalculateSimpleStatus(Identity.Create(s, RosterVector.Empty), statefulInterview),
               Title = statefulInterview.GetGroup(Identity.Create(s, RosterVector.Empty)).Title.Text
            });

            sections = new[] {PrefilledSectionStub}.Union(sections);

            return new InterviewDetails
            {
                Sections = sections.ToArray()
            };
        }

        public SectionData GetSectionDetails(string sectionId)
        {
            if (sectionId == null) throw new ArgumentNullException(nameof(sectionId));

            if (sectionId == PrefilledSectionStub.Id)
            {
                return this.GetPrefilledQuestions();
            }

            Identity secitonIdentity = Identity.Parse(sectionId);
            var statefulInterview = this.GetCallerInterview();
            var ids = statefulInterview.GetUnderlyingInterviewerEntities(secitonIdentity);

            var entities = ids.Select(x => new InterviewEntityWithType
            {
                Identity = x.ToString(),
                EntityType = this.GetEntityType(x.Id).ToString()
            }).ToArray();

            return new SectionData
            {
                Breadcrumbs = GetBreadcrumbs(secitonIdentity, statefulInterview),
                Id = sectionId,
                Type = "Section",
                Status = CalculateSimpleStatus(secitonIdentity, statefulInterview),
                Entities = entities,
                Title = statefulInterview.GetGroup(secitonIdentity).Title.Text
            };
        }

        private Breadcrumb[] GetBreadcrumbs(Identity group, IStatefulInterview statefulInterview)
        {
            var callerQuestionnaire = this.GetCallerQuestionnaire();
            ReadOnlyCollection<Guid> parentIds = callerQuestionnaire.GetParentsStartingFromTop(group.Id);

            var breadCrumbs = new List<Breadcrumb>();
            int metRosters = 0;

            foreach (Guid parentId in parentIds)
            {
                if (callerQuestionnaire.IsRosterGroup(parentId))
                {
                    metRosters++;
                    var itemRosterVector = group.RosterVector.Shrink(metRosters);
                    var itemIdentity = new Identity(parentId, itemRosterVector);
                    var breadCrumb = new Breadcrumb {Title = statefulInterview.GetGroup(itemIdentity).Title.Text };

                    breadCrumbs.Add(breadCrumb);
                }
                else
                {
                    var itemIdentity = new Identity(parentId, group.RosterVector.Shrink(metRosters));
                    var breadCrumb = new Breadcrumb { Title = statefulInterview.GetGroup(itemIdentity).Title.Text };

                    breadCrumbs.Add(breadCrumb);
                }
            }

            return breadCrumbs.ToArray();
        }

        private static SimpleGroupStatus CalculateSimpleStatus(Identity group, IStatefulInterview interview)
        {
            if (interview.HasEnabledInvalidQuestionsAndStaticTexts(group))
                return SimpleGroupStatus.Invalid;

            if (interview.HasUnansweredQuestions(group))
                return SimpleGroupStatus.Other;

            bool isSomeSubgroupNotCompleted = interview
                .GetEnabledSubgroups(group)
                .Select(subgroup => CalculateSimpleStatus(subgroup, interview))
                .Any(status => status != SimpleGroupStatus.Completed);

            if (isSomeSubgroupNotCompleted)
                return SimpleGroupStatus.Other;

            return SimpleGroupStatus.Completed;
        }

        public InterviewEntity GetEntityDetails(string id)
        {
            var identity = Identity.Parse(id);
            var callerInterview = this.GetCallerInterview();

            InterviewTreeQuestion question = callerInterview.GetQuestion(identity);
            if (question != null)
            {
                GenericQuestion result = new StubEntity { Id = id };

                if (question.IsSingleFixedOption)
                {
                    result = this.autoMapper.Map<InterviewSingleOptionQuestion>(question);

                    var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                    ((InterviewSingleOptionQuestion)result).Options = options;
                }
                else if (question.IsText)
                {
                    InterviewTreeQuestion textQuestion = callerInterview.GetQuestion(identity);
                    result = this.autoMapper.Map<InterviewTextQuestion>(textQuestion);
                    var textQuestionMask = this.GetCallerQuestionnaire().GetTextQuestionMask(identity.Id);
                    if (!string.IsNullOrEmpty(textQuestionMask))
                    {
                        ((InterviewTextQuestion)result).Mask = textQuestionMask;
                    }
                }
                else if (question.IsInteger)
                {
                    InterviewTreeQuestion integerQuestion = callerInterview.GetQuestion(identity);
                    var interviewIntegerQuestion = this.autoMapper.Map<InterviewIntegerQuestion>(integerQuestion);
                    var callerQuestionnaire = this.GetCallerQuestionnaire();

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
                else if (question.IsDouble)
                {
                    InterviewTreeQuestion textQuestion = callerInterview.GetQuestion(identity);
                    var interviewDoubleQuestion = this.autoMapper.Map<InterviewDoubleQuestion>(textQuestion);
                    var callerQuestionnaire = this.GetCallerQuestionnaire();
                    interviewDoubleQuestion.CountOfDecimalPlaces = callerQuestionnaire.GetCountOfDecimalPlacesAllowedByQuestion(identity.Id);
                    interviewDoubleQuestion.UseFormatting = callerQuestionnaire.ShouldUseFormatting(identity.Id);
                    result = interviewDoubleQuestion;
                }
                else if (question.IsMultiFixedOption)
                {
                    result = this.autoMapper.Map<InterviewMutliOptionQuestion>(question);

                    var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                    var typedResult = (InterviewMutliOptionQuestion)result;
                    typedResult.Options = options;
                    typedResult.Ordered = this.GetCallerQuestionnaire().ShouldQuestionRecordAnswersOrder(identity.Id);
                    typedResult.MaxSelectedAnswersCount = this.GetCallerQuestionnaire().GetMaxSelectedAnswerOptions(identity.Id);
                }

                this.PutValidationMessages(result.Validity, callerInterview, identity);
                this.PutInstructions(result, identity);
                this.PutHideIfDisabled(result, identity);

                return result;
            }

            InterviewTreeStaticText staticText = callerInterview.GetStaticText(identity);
            if (staticText != null)
            {
                InterviewStaticText result = new InterviewStaticText() { Id = id };
                result = this.autoMapper.Map<InterviewStaticText>(staticText);

                var callerQuestionnaire = this.GetCallerQuestionnaire();
                var attachment = callerQuestionnaire.GetAttachmentForEntity(identity.Id);
                if (attachment != null)
                {
                    result.AttachmentContent = attachment.ContentId;
                }

                this.PutHideIfDisabled(result, identity);
                this.PutValidationMessages(result.Validity, callerInterview, identity);

                return result;
            }

            return null;
        }

        private void PutValidationMessages(Validity validity, IStatefulInterview callerInterview, Identity identity)
        {
            validity.Messages = callerInterview.GetFailedValidationMessages(identity).ToArray();
        }

        private void PutHideIfDisabled(InterviewEntity result, Identity identity)
        {
            result.HideIfDisabled = this.GetCallerQuestionnaire().ShouldBeHiddenIfDisabled(identity.Id);
        }

        private void PutInstructions(GenericQuestion result, Identity id)
        {
            var callerQuestionnaire = this.GetCallerQuestionnaire();

            result.Instructions = callerQuestionnaire.GetQuestionInstruction(id.Id);
            result.HideInstructions = callerQuestionnaire.GetHideInstructions(id.Id);
        }

        private InterviewEntityType GetEntityType(Guid entityId)
        {
            var callerQuestionnaire = this.GetCallerQuestionnaire();

            if (callerQuestionnaire.IsVariable(entityId)) return InterviewEntityType.Unsupported;
            if (callerQuestionnaire.HasGroup(entityId)) return InterviewEntityType.Group;
            if (callerQuestionnaire.IsRosterGroup(entityId)) return InterviewEntityType.RosterInstance;
            if (callerQuestionnaire.IsStaticText(entityId)) return InterviewEntityType.StaticText;

            switch (callerQuestionnaire.GetQuestionType(entityId))
            {
                case QuestionType.DateTime:
                    return InterviewEntityType.DateTime;
                case QuestionType.GpsCoordinates:
                    return InterviewEntityType.Gps;
                case QuestionType.Multimedia:
                    return InterviewEntityType.Multimedia;
                case QuestionType.MultyOption:
                    return InterviewEntityType.CategoricalMulti;
                case QuestionType.SingleOption:
                    return InterviewEntityType.CategoricalSingle;
                case QuestionType.Numeric:
                    return callerQuestionnaire.IsQuestionInteger(entityId)
                        ? InterviewEntityType.Integer
                        : InterviewEntityType.Double;
                case QuestionType.Text:
                    return InterviewEntityType.TextQuestion;
                default:
                    return InterviewEntityType.Unsupported;
            }
        }
    }
}