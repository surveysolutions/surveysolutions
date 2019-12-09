using System;
using System.Linq;
using System.Web;
using AutoMapper;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Enumerator.Native.WebInterview.Models
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
                .ForMember(x => x.IsDisabled, opts => opts.MapFrom(x => x.IsDisabled()))
                .ForMember(x => x.Instructions, opts => opts.MapFrom(x => x.Instructions.BrowserReadyText));

            this.CreateMap<InterviewTreeQuestion, StubEntity>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>();

            this.CreateMap<InterviewTreeQuestion, InterviewTextQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsInterviewTreeTextQuestion().GetAnswer()));

            this.CreateMap<InterviewTreeQuestion, InterviewBarcodeQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsInterviewTreeQRBarcodeQuestion().GetAnswer()));

            this.CreateMap<InterviewTreeQuestion, InterviewAreaQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(_ => _.Answer, opts =>
                {
                    opts.PreCondition(x => x.IsAnswered());
                    opts.MapFrom(x => ToGeometryAnswer(x.GetAsInterviewTreeAreaQuestion().GetAnswer().Value));
                });

            this.CreateMap<InterviewTreeQuestion, InterviewAudioQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Filename, opts => opts.MapFrom(x => x.GetAsInterviewTreeAudioQuestion().GetAnswer() != null
                        ? x.GetAsInterviewTreeAudioQuestion().GetAnswer().FileName
                        : null))
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsInterviewTreeAudioQuestion().GetAnswer()!=null
                    ? (long)x.GetAsInterviewTreeAudioQuestion().GetAnswer().Length.TotalMilliseconds
                    : (long?)null));

            this.CreateMap<InterviewTreeQuestion, InterviewSingleOptionQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x =>
                    x.IsLinkedToListQuestion
                        ? x.GetAsInterviewTreeSingleOptionLinkedToListQuestion().GetAnswer().SelectedValue
                        : x.GetAsInterviewTreeSingleOptionQuestion().GetAnswer().SelectedValue));
            
            this.CreateMap<InterviewTreeQuestion, InterviewFilteredQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts =>
                {
                    opts.PreCondition(x => x.IsAnswered());
                    opts.MapFrom(x => GetSingleFixedOptionAnswerAsDropdownItem(x));
                });

            this.CreateMap<InterviewTreeQuestion, InterviewLinkedSingleQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsInterviewTreeSingleLinkedToRosterQuestion().GetAnswer().SelectedValue));

            this.CreateMap<RosterVector, RosterVector>()
                .ConstructUsing(m => new RosterVector(m));

            this.CreateMap<InterviewTreeQuestion, InterviewLinkedMultiQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsInterviewTreeMultiLinkedToRosterQuestion().GetAnswer().CheckedValues));

            this.CreateMap<InterviewTreeQuestion, InterviewMutliOptionQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x =>
                    x.IsLinkedToListQuestion
                        ? x.GetAsInterviewTreeMultiOptionLinkedToListQuestion().GetAnswer().CheckedValues
                        : x.GetAsInterviewTreeMultiOptionQuestion().GetAnswer().CheckedValues));

            this.CreateMap<CheckedYesNoAnswerOption, InterviewYesNoAnswer>();

            this.CreateMap<InterviewTreeQuestion, InterviewYesNoQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsInterviewTreeYesNoQuestion().GetAnswer().CheckedOptions));
            this.CreateMap<InterviewTreeQuestion, InterviewIntegerQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsInterviewTreeIntegerQuestion().GetAnswer().Value));
            this.CreateMap<InterviewTreeQuestion, InterviewDoubleQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsInterviewTreeDoubleQuestion().GetAnswer().Value));

            this.CreateMap<InterviewTreeQuestion, InterviewDateQuestion>()
               .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
               .ForMember(x => x.IsTimestamp, opts => opts.MapFrom(x => (x.GetAsInterviewTreeDateTimeQuestion()).IsTimestamp))
               .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsInterviewTreeDateTimeQuestion().GetAnswer().Value));

            this.CreateMap<TextListAnswerRow, TextListAnswerRowDto>()
                .ForMember(x => x.Text, opts => opts.MapFrom(x => x.Text))
                .ForMember(x => x.Value, opts => opts.MapFrom(x => x.Value));

            this.CreateMap<InterviewTreeQuestion, InterviewTextListQuestion>()
               .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
               .ForMember(x => x.Rows, opts => opts.MapFrom(x => x.GetAsInterviewTreeTextListQuestion().GetAnswer().Rows));

            this.CreateMap<GeoPosition, GpsAnswer>()
                .ForMember(x => x.Latitude, opts => opts.MapFrom(x => x.Latitude))
                .ForMember(x => x.Longitude, opts => opts.MapFrom(x => x.Longitude))
                .ForMember(x => x.Accuracy, opts => opts.MapFrom(x => x.Accuracy))
                .ForMember(x => x.Altitude, opts => opts.MapFrom(x => x.Altitude))
                .ForMember(x => x.Timestamp, opts => opts.MapFrom(x => x.Timestamp.ToUnixTimeMilliseconds()));

            this.CreateMap<InterviewTreeQuestion, InterviewGpsQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.GetAsInterviewTreeGpsQuestion().GetAnswer().Value));

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
                .ForMember(x => x.IsRoster, opts => opts.MapFrom(x => x is InterviewTreeRoster));
            
            this.CreateMap<InterviewTreeRoster, InterviewGroupOrRosterInstance>()
                .IncludeBase<InterviewTreeRoster, InterviewEntity>()
                .ForMember(x => x.RosterTitle, opts => opts.MapFrom(x => x.RosterTitle))
                .ForMember(x => x.IsRoster, opts => opts.MapFrom(x => true));

            this.CreateMap<InterviewTreeGroup, SidebarPanel>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
                .ForMember(x => x.ParentId, opts => opts.MapFrom(x => x.Parent == null ? null : x.Parent.Identity)) // automapper do not allow null propagation in expressions
                .ForMember(x => x.HasChildren, opts => opts.MapFrom(x => x.Children.OfType<InterviewTreeGroup>().Any(c => !c.IsDisabled())))
                .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.BrowserReadyText))
                .ForMember(x => x.RosterTitle, opts =>
                {
                    opts.Condition(x => x is InterviewTreeRoster);
                    opts.MapFrom(x => HttpUtility.HtmlEncode((x as InterviewTreeRoster).RosterTitle));
                })
                .ForMember(x => x.IsRoster, opts => opts.MapFrom(x => x is InterviewTreeRoster)); 

            this.CreateMap<InterviewTreeQuestion, InterviewMultimediaQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.AnswerTimeUtc, opts => opts.MapFrom(x => x.GetAsInterviewTreeMultimediaQuestion().GetAnswer().AnswerTimeUtc))
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => $@"?interviewId={x.Tree.InterviewId}&questionId={x.Identity}&filename={x.GetAsInterviewTreeMultimediaQuestion().GetAnswer().FileName}"));
        }

        private static InterviewGeometryAnswer ToGeometryAnswer(Area area) => new InterviewGeometryAnswer
        {
            SelectedPoints = area.Coordinates.Split(';').Select(x =>
                new GeoLocation(double.Parse(x.Split(',')[0]), double.Parse(x.Split(',')[1]), 0, 0)).ToArray(),
            Length = area.Length,
            Area = area.AreaSize
        };

        private static DropdownItem GetSingleFixedOptionAnswerAsDropdownItem(InterviewTreeQuestion question)
        {
            return new DropdownItem(question.GetAsInterviewTreeSingleOptionQuestion().GetAnswer().SelectedValue, question.GetAnswerAsString());
        }
    }
}
