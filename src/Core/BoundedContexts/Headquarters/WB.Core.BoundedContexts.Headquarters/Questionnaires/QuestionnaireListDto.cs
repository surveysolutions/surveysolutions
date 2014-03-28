using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires
{
    public class QuestionnaireListDto
    {
        public QuestionnaireListDto()
        {
            this.Items = new List<QuestionnaireDto>();
        }

        public List<QuestionnaireDto> Items { get; set; }

        public int Total { get; set; }
    }
}