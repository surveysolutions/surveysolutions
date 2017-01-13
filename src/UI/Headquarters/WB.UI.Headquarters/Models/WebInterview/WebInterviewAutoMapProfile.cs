using AutoMapper;
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
            this.CreateMap<InterviewTreeQuestion, InterviewIntegerQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.AsInteger.GetAnswer().Value));
            this.CreateMap<InterviewTreeQuestion, InterviewDoubleQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.AsDouble.GetAnswer().Value));

            this.CreateMap<InterviewTreeStaticText, Validity>()
                .ForMember(x => x.IsValid, opts => opts.MapFrom(x => x.IsValid));

            this.CreateMap<InterviewTreeStaticText, InterviewEntity>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
                .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.Text))
                .ForMember(x => x.IsDisabled, opts => opts.MapFrom(x => x.IsDisabled()));

            this.CreateMap<InterviewTreeStaticText, InterviewStaticText>()
                .IncludeBase<InterviewTreeStaticText, InterviewEntity>()
                .ForMember(x => x.Validity, opts => opts.MapFrom(x => x));
        }
    }
}