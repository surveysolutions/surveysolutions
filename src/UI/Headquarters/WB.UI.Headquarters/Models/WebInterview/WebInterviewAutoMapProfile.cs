using AutoMapper;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class WebInterviewAutoMapProfile : Profile
    {
        public WebInterviewAutoMapProfile()
        {
             this.CreateMap<InterviewTreeQuestion, GenericQuestion>()
              .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Identity))
              .ForMember(x => x.Title, opts => opts.MapFrom(x => x.Title.Text));

            this.CreateMap<InterviewTreeQuestion, InterviewTextQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>();
            this.CreateMap<InterviewTreeQuestion, InterviewSingleOptionQuestion>()
                .IncludeBase<InterviewTreeQuestion, GenericQuestion>();
        }
    }
}