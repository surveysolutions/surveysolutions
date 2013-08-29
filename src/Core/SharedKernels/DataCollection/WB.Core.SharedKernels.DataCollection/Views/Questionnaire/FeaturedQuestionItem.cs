using System;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class FeaturedQuestionItem
    {
        public FeaturedQuestionItem(Guid id, string title, string caption)
        {
            Id = id;
            Title = title;
            Caption = caption;
        }

        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string Caption { get; private set; }
    }
}
