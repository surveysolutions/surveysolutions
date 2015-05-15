using System;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class FeaturedQuestionItem
    {
        protected FeaturedQuestionItem()
        {
        }

        public FeaturedQuestionItem(Guid id, string title, string caption)
        {
            Id = id;
            Title = title;
            Caption = caption;
        }

        public virtual Guid Id { get; protected set; }
        public virtual string Title { get; protected set; }
        public virtual string Caption { get; protected set; }
    }
}
