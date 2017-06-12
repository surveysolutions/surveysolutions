using AutoMapper;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Models.Api
{
    public class AssignmentProfile : Profile
    {
        public AssignmentProfile()
        {
            this.CreateMap<Assignment, AssignmentApiView>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Id.ToString()))
                .ForMember(x => x.Quantity, opts => opts.MapFrom(x => x.Quantity))
                .ForMember(x => x.QuestionnaireId, opts => opts.MapFrom(x => x.QuestionnaireId))
                .ForMember(x => x.IdentifyingData, opts => opts.MapFrom(x => x.IdentifyingData))
                .ForMember(x => x.InterviewsCount, opts => opts.MapFrom(x => x.InterviewSummaries.Count));

            this.CreateMap<IdentifyingAnswer, AssignmentApiView.IdentifyingAnswer>()
                .ForMember(x => x.Answer, opts => opts.MapFrom(x => x.Answer))
                .ForMember(x => x.Identity, opts => opts.MapFrom(x => x.Identity));
        }
    }
}