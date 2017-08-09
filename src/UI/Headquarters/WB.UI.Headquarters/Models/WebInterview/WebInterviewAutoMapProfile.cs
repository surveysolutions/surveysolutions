using System.Linq;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

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
                 .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.BrowserReadyText))
                 .ForMember(x => x.Validity, opts => opts.MapFrom(x => x))
                 .ForMember(x => x.IsAnswered, opts => opts.MapFrom(x => x.IsAnswered()))
                 .ForMember(x => x.IsDisabled, opts => opts.MapFrom(x => x.IsDisabled()));

            this.CreateMap<InterviewTreeQuestion, StubEntity>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>();

            this.CreateMap<InterviewTreeQuestion, InterviewTextQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsTextAnswer()));

            this.CreateMap<InterviewTreeQuestion, InterviewBarcodeQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsQRBarcodeAnswer()));

            this.CreateMap<InterviewTreeQuestion, InterviewAudioQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsAudioAnswer()!=null
                    ? (long)x.GetAsAudioAnswer().Length.TotalMilliseconds
                    : (long?)null));

            this.CreateMap<InterviewTreeQuestion, InterviewSingleOptionQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x =>
                    x.IsLinkedToListQuestion
                        ? x.GetAsSingleLinkedToListAnswer().SelectedValue
                        : x.GetAsSingleFixedOptionAnswer().SelectedValue));
            
            this.CreateMap<InterviewTreeQuestion, InterviewFilteredQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts =>
                {
                    opts.PreCondition(x => x.IsAnswered());
                    opts.MapFrom(x => GetSingleFixedOptionAnswerAsDropdownItem(x));
                });

            this.CreateMap<InterviewTreeQuestion, InterviewLinkedSingleQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsSingleLinkedOptionAnswer().SelectedValue));

            this.CreateMap<InterviewTreeQuestion, InterviewLinkedMultiQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsMultiLinkedOptionAnswer().CheckedValues));

            this.CreateMap<InterviewTreeQuestion, InterviewMutliOptionQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x =>
                    x.IsLinkedToListQuestion
                        ? x.GetAsMultiLinkedToListAnswer().CheckedValues
                        : x.GetAsMultiFixedOptionAnswer().CheckedValues));

            this.CreateMap<CheckedYesNoAnswerOption, InterviewYesNoAnswer>();

            this.CreateMap<InterviewTreeQuestion, InterviewYesNoQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsYesNoAnswer().CheckedOptions));
            this.CreateMap<InterviewTreeQuestion, InterviewIntegerQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsIntegerAnswer().Value));
            this.CreateMap<InterviewTreeQuestion, InterviewDoubleQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsDoubleAnswer().Value));

            this.CreateMap<InterviewTreeQuestion, InterviewDateQuestion>()
               .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
               .ForMember(x => x.IsTimestamp, opts => opts.MapFrom(x => ((InterviewTreeDateTimeQuestion)x.InterviewQuestion).IsTimestamp))
               .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsDateTimeAnswer().Value));

            this.CreateMap<TextListAnswerRow, TextListAnswerRowDto>()
                .ForMember(x => x.Text, opts => opts.MapFrom(x => x.Text))
                .ForMember(x => x.Value, opts => opts.MapFrom(x => x.Value));

            this.CreateMap<InterviewTreeQuestion, InterviewTextListQuestion>()
               .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
               .ForMember(x => x.Rows, opts => opts.MapFrom(x => x.GetAsTextListAnswer().Rows));

            this.CreateMap<GeoPosition, GpsAnswer>()
                .ForMember(x => x.Latitude, opts => opts.MapFrom(x => x.Latitude))
                .ForMember(x => x.Longitude, opts => opts.MapFrom(x => x.Longitude))
                .ForMember(x => x.Accuracy, opts => opts.MapFrom(x => x.Accuracy))
                .ForMember(x => x.Altitude, opts => opts.MapFrom(x => x.Altitude))
                .ForMember(x => x.Timestamp, opts => opts.MapFrom(x => x.Timestamp.ToUnixTimeMilliseconds()));

            this.CreateMap<InterviewTreeQuestion, InterviewGpsQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsGpsAnswer().Value));

            this.CreateMap<InterviewTreeStaticText, Validity>()
                .ForMember(x => x.IsValid, opts => opts.MapFrom(x => x.IsValid));

            this.CreateMap<InterviewTreeStaticText, InterviewEntity>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
                .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.BrowserReadyText))
                .ForMember(x => x.IsDisabled, opts => opts.MapFrom(x => x.IsDisabled()));

            this.CreateMap<InterviewTreeStaticText, InterviewStaticText>()
                .IncludeBase<InterviewTreeStaticText, InterviewEntity>()
                .ForMember(x => x.Validity, opts => opts.MapFrom(x => x));

            this.CreateMap<InterviewTreeGroup, InterviewEntity>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
                .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.BrowserReadyText))
                .ForMember(x => x.IsDisabled, opts => opts.MapFrom(x => x.IsDisabled()));

            this.CreateMap<InterviewTreeRoster, InterviewEntity>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
                .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.BrowserReadyText))
                .ForMember(x => x.IsDisabled, opts => opts.MapFrom(x => x.IsDisabled()));

            this.CreateMap<InterviewTreeGroup, InterviewGroupOrRosterInstance>()
                .IncludeBase<InterviewTreeGroup, InterviewEntity>()
                .ForMember(x => x.StatisticsByAnswersAndSubsections, opts => opts.MapFrom(x => GetGroupStatisticsByAnswersAndSubsections(x)))
                .ForMember(x => x.StatisticsByInvalidAnswers, opts => opts.MapFrom(x => GetGroupStatisticsByInvalidAnswers(x)))
                .ForMember(x => x.Status, opts => opts.MapFrom(x => GetGroupStatus(x).ToString()))
                .ForMember(x => x.Validity, opts => opts.MapFrom(x => x))
                .ForMember(x => x.IsRoster, opts => opts.MapFrom(x => x is InterviewTreeRoster));

            this.CreateMap<InterviewTreeGroup, Validity>()
                .ForMember(x => x.IsValid, opts => opts.MapFrom(x => !HasQuestionsWithInvalidAnswers(x)));

            this.CreateMap<InterviewTreeRoster, InterviewGroupOrRosterInstance>()
                .IncludeBase<InterviewTreeRoster, InterviewEntity>()
                .ForMember(x => x.StatisticsByAnswersAndSubsections, opts => opts.MapFrom(x => GetGroupStatisticsByAnswersAndSubsections(x)))
                .ForMember(x => x.StatisticsByInvalidAnswers, opts => opts.MapFrom(x => GetGroupStatisticsByInvalidAnswers(x)))
                .ForMember(x => x.Status, opts => opts.MapFrom(x => GetGroupStatus(x).ToString()))
                .ForMember(x => x.Validity, opts => opts.MapFrom(x => x))
                .ForMember(x => x.RosterTitle, opts => opts.MapFrom(x => x.RosterTitle))
                .ForMember(x => x.IsRoster, opts => opts.MapFrom(x => true));

            this.CreateMap<InterviewTreeGroup, SidebarPanel>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
                .ForMember(x => x.State, opts => opts.MapFrom(x => GetGroupStatus(x).ToString()))
                .ForMember(x => x.ParentId, opts => opts.MapFrom(x => x.Parent == null ? null : x.Parent.Identity)) // automapper do not allow null propagation in expressions
                .ForMember(x => x.HasChildren, opts => opts.MapFrom(x => x.Children.OfType<InterviewTreeGroup>().Any(c => !c.IsDisabled())))
                .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.BrowserReadyText))
                .ForMember(x => x.RosterTitle, opts =>
                {
                    opts.Condition(x => x is InterviewTreeRoster);
                    opts.MapFrom(x => (x as InterviewTreeRoster).RosterTitle);
                })
                .ForMember(x => x.Validity, opts => opts.MapFrom(x => x))
                .ForMember(x => x.IsRoster, opts => opts.MapFrom(x => x is InterviewTreeRoster)); 

            this.CreateMap<InterviewTreeQuestion, InterviewMultimediaQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => $@"?interviewId={x.Tree.InterviewId}&questionId={x.Identity}&filename={x.GetAsMultimediaAnswer().FileName}"));
        }

        private static DropdownItem GetSingleFixedOptionAnswerAsDropdownItem(InterviewTreeQuestion question)
        {
            return new DropdownItem(question.GetAsSingleFixedOptionAnswer().SelectedValue, question.GetAnswerAsString());
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