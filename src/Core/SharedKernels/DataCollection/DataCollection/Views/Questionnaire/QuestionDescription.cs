using System;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionDescription
    {
        public QuestionDescription(Guid id, string title, string caption)
        {
            this.Id = id;
            this.Title = title;
            this.Caption = caption;
        }

        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string Caption { get; private set; }
    }
}