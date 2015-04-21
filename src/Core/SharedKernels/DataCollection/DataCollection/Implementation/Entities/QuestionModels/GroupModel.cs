using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels
{
    public class GroupModel
    {
        public GroupModel()
        {
            this.Children = new List<QuestionnaireReferenceModel>();
        }

        public Guid Id { get; set; }

        public string Title { get; set; }

        public List<QuestionnaireReferenceModel> Children { get; set; }
    }
}