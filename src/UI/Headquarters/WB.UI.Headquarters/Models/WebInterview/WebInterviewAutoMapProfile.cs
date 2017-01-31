using System;
using System.Linq;
using AutoMapper;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class WebInterviewAutoMapProfile : Profile
    {
        public WebInterviewAutoMapProfile()
        {
            this.CreateMap<InterviewTreeQuestion, Validity>()
                .ForMember(x => x.IsValid, opts => opts.MapFrom(x => x.IsValid));

            this.CreateMap<InterviewTreeQuestion, GenericQuestion>()
                 .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
                 .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.Text))
                 .ForMember(x => x.Validity, opts => opts.MapFrom(x => x))
                 .ForMember(x => x.IsAnswered, opts => opts.MapFrom(x => x.IsAnswered()))
                 .ForMember(x => x.IsDisabled, opts => opts.MapFrom(x => x.IsDisabled()));

            this.CreateMap<InterviewTreeQuestion, InterviewTextQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.AsText.GetAnswer()));
            this.CreateMap<InterviewTreeQuestion, InterviewSingleOptionQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.AsSingleFixedOption.GetAnswer().SelectedValue));
            this.CreateMap<InterviewTreeQuestion, InterviewMutliOptionQuestion>()
               .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
               .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.AsMultiFixedOption.GetAnswer().CheckedValues));
            this.CreateMap<CheckedYesNoAnswerOption, InterviewYesNoAnswer>();
            this.CreateMap<InterviewTreeQuestion, InterviewYesNoQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.AsYesNo.GetAnswer().CheckedOptions));
            this.CreateMap<InterviewTreeQuestion, InterviewIntegerQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.AsInteger.GetAnswer().Value));
            this.CreateMap<InterviewTreeQuestion, InterviewDoubleQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.AsDouble.GetAnswer().Value));

            this.CreateMap<InterviewTreeQuestion, InterviewDateQuestion>()
               .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
               .ForMember(x => x.IsTimestamp, opts => opts.MapFrom(x => x.AsDateTime.IsTimestamp))
               .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.AsDateTime.GetAnswer().Value));

            this.CreateMap<Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers.TextListAnswerRow, TextListAnswerRow>()
                .ForMember(x => x.Text, opts => opts.MapFrom(x => x.Text))
                .ForMember(x => x.Value, opts => opts.MapFrom(x => x.Value));
            this.CreateMap<InterviewTreeQuestion, InterviewTextListQuestion>()
               .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
               .ForMember(x => x.Rows, opts => opts.MapFrom(x => x.AsTextList.GetAnswer().Rows));

            this.CreateMap<InterviewTreeStaticText, Validity>()
                .ForMember(x => x.IsValid, opts => opts.MapFrom(x => x.IsValid));

            this.CreateMap<InterviewTreeStaticText, InterviewEntity>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
                .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.Text))
                .ForMember(x => x.IsDisabled, opts => opts.MapFrom(x => x.IsDisabled()));

            this.CreateMap<InterviewTreeStaticText, InterviewStaticText>()
                .IncludeBase<InterviewTreeStaticText, InterviewEntity>()
                .ForMember(x => x.Validity, opts => opts.MapFrom(x => x));

            this.CreateMap<InterviewTreeGroup, InterviewEntity>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
                .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.Text))
                .ForMember(x => x.IsDisabled, opts => opts.MapFrom(x => x.IsDisabled()));

            this.CreateMap<InterviewTreeRoster, InterviewEntity>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
                .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.Text))
                .ForMember(x => x.IsDisabled, opts => opts.MapFrom(x => x.IsDisabled()));

            this.CreateMap<InterviewTreeGroup, InterviewGroupOrRosterInstance>()
                .IncludeBase<InterviewTreeGroup, InterviewEntity>()
                .ForMember(x => x.StatisticsByAnswersAndSubsections, opts => opts.MapFrom(x => GetGroupStatisticsByAnswersAndSubsections(x)))
                .ForMember(x => x.StatisticsByInvalidAnswers, opts => opts.MapFrom(x => GetGroupStatisticsByInvalidAnswers(x)))
                .ForMember(x => x.Status, opts => opts.MapFrom(x => GetGroupStatus(x).ToString()))
                .ForMember(x => x.Validity, opts => opts.MapFrom(x => x));

            this.CreateMap<InterviewTreeGroup, Validity>()
                .ForMember(x => x.IsValid, opts => opts.MapFrom(x => !HasQuestionsWithInvalidAnswers(x)));

            this.CreateMap<InterviewTreeRoster, InterviewGroupOrRosterInstance>()
                .IncludeBase<InterviewTreeRoster, InterviewEntity>()
                .ForMember(x => x.StatisticsByAnswersAndSubsections, opts => opts.MapFrom(x => GetGroupStatisticsByAnswersAndSubsections(x)))
                .ForMember(x => x.StatisticsByInvalidAnswers, opts => opts.MapFrom(x => GetGroupStatisticsByInvalidAnswers(x)))
                .ForMember(x => x.Status, opts => opts.MapFrom(x => GetGroupStatus(x).ToString()))
                .ForMember(x => x.Validity, opts => opts.MapFrom(x => x))
                .ForMember(x => x.RosterTitle, opts => opts.MapFrom(x => x.RosterTitle));

            this.CreateMap<InterviewTreeGroup, SidebarPanel>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
                .ForMember(x => x.State, opts => opts.MapFrom(x => GetGroupStatus(x).ToString()))
                .ForMember(x => x.ParentId, opts => opts.MapFrom(x => x.Parent == null ? null : x.Parent.Identity)) // automapper do not allow null propagation in expressions
                .ForMember(x => x.HasChildrens, opts => opts.MapFrom(x => x.Children.OfType<InterviewTreeGroup>().Any()))
                .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.Text))
                .ForMember(x => x.Validity, opts => opts.MapFrom(x => x));
        }

        private static bool HasQuestionsWithInvalidAnswers(InterviewTreeGroup group)
            => group.CountEnabledInvalidQuestionsAndStaticTexts() > 0;

        private static GroupStatus GetGroupStatus(InterviewTreeGroup group)
        {
            if (group.HasUnansweredQuestions() || HasSubgroupsWithUnansweredQuestions(@group))
                return group.CountEnabledAnsweredQuestions() > 0 ? GroupStatus.Started : GroupStatus.NotStarted;

            return GroupStatus.Completed;
        }

        private static bool HasSubgroupsWithUnansweredQuestions(InterviewTreeGroup @group)
            => @group.Children
                .OfType<InterviewTreeGroup>()
                .Where(subGroup => !subGroup.IsDisabled())
                .Any(subGroup => GetGroupStatus(subGroup) != GroupStatus.Completed || HasQuestionsWithInvalidAnswers(subGroup));

        private string GetGroupStatisticsByInvalidAnswers(InterviewTreeGroup group)
            => HasQuestionsWithInvalidAnswers(@group) ? GetInformationByInvalidAnswers(@group) : null;

        private static string GetGroupStatisticsByAnswersAndSubsections(InterviewTreeGroup group)
        {
            switch (GetGroupStatus(group))
            {
                case GroupStatus.NotStarted:
                    return Resources.WebInterview.Interview_Group_Status_NotStarted;

                case GroupStatus.Started:
                    return string.Format(Resources.WebInterview.Interview_Group_Status_StartedIncompleteFormat, GetInformationByQuestionsAndAnswers(group));

                case GroupStatus.Completed:
                    return string.Format(Resources.WebInterview.Interview_Group_Status_CompletedFormat, GetInformationByQuestionsAndAnswers(group));
            }

            return null;
        }

        private static string GetInformationByQuestionsAndAnswers(InterviewTreeGroup group)
        {
            var subGroupsText = GetInformationBySubgroups(group);
            var enabledAnsweredQuestionsCount = @group.CountEnabledAnsweredQuestions();

            return $@"{(enabledAnsweredQuestionsCount == 1 ?
                Resources.WebInterview.Interview_Group_AnsweredQuestions_One :
                string.Format(Resources.WebInterview.Interview_Group_AnsweredQuestions_Many, enabledAnsweredQuestionsCount))}, {subGroupsText}";
        }

        private static string GetInformationByInvalidAnswers(InterviewTreeGroup group)
        {
            var countEnabledInvalidQuestionsAndStaticTexts = @group.CountEnabledInvalidQuestionsAndStaticTexts();

            return countEnabledInvalidQuestionsAndStaticTexts == 1
                ? Resources.WebInterview.Interview_Group_InvalidAnswers_One
                : string.Format(Resources.WebInterview.Interview_Group_InvalidAnswers_ManyFormat, countEnabledInvalidQuestionsAndStaticTexts);
        }

        private static string GetInformationBySubgroups(InterviewTreeGroup group)
        {
            var subGroupsCount = @group.Children.OfType<InterviewTreeGroup>().Count();
            switch (subGroupsCount)
            {
                case 0:
                    return Resources.WebInterview.Interview_Group_Subgroups_Zero;

                case 1:
                    return Resources.WebInterview.Interview_Group_Subgroups_One;

                default:
                    return string.Format(Resources.WebInterview.Interview_Group_Subgroups_ManyFormat, subGroupsCount);
            }
        }
    }
}