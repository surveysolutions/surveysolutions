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
                //.ForMember(x => x.Messages, opts => opts.MapFrom(x => x.ValidationMessages));//FailedValidations

            this.CreateMap<InterviewTreeQuestion, GenericQuestion>()
                 .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
                 .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.Text))
                 .ForMember(x => x.Validity, opts => opts.MapFrom(x => x))
                 .ForMember(x => x.IsAnswered, opts => opts.MapFrom(x => x.IsAnswered()));

            this.CreateMap<InterviewTreeQuestion, InterviewTextQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>();
            this.CreateMap<InterviewTreeQuestion, InterviewSingleOptionQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.AsSingleFixedOption.GetAnswer().SelectedValue));
        }
    }
}