using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class QuestionnaireListItem
    {
        public Guid Id;
        public string Title;

        public QuestionnaireListItem(Guid id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}
