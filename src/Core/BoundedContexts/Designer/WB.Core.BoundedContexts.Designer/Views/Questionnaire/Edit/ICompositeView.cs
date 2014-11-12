using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public interface ICompositeView
    {
        Guid Id { get; set; }

        string Title { get; set; }
    }
}