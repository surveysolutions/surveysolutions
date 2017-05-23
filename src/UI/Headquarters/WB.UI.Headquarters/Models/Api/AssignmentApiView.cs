using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Models.Api
{
    public class AssignmentApiView
    {
        public string Id { get; set; }
        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public int? Capacity { get; set; }
        public List<IdentifyingAnswerApiView> IdentifyingData { get; set; }
    }
}