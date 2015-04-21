using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels
{
    public class SingleOptionQuestionModel : BaseQuestionModel
    {
        public Guid? CascadeFromQuestionId { get; set; }

        public bool? IsFilteredCombobox { get; set; }

        public List<OptionModel> Options { get; set; }
    }
}