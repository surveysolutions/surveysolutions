using AutoMapper;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Models.Api
{
    public class AssignmentProfile : Profile
    {
        public AssignmentProfile()
        {
            this.CreateMap<Assignment, AssignmentApiDocument>()
                .ForMember(x => x.Id, opts => opts.MapFrom(x => x.Id.ToString()))
                .ForMember(x => x.Quantity, opts => opts.MapFrom(x => x.Quantity))
                .ForMember(x => x.QuestionnaireId, opts => opts.MapFrom(x => x.QuestionnaireId));
        }
    }
}