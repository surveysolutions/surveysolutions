using System;
using System.Collections.Generic;
using System.Linq;

using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;

namespace WB.UI.Designer.Views.Questionnaire
{
    public class EditQuestionnaireView
    {
        public EditQuestionnaireView(QuestionnaireDocument source)
        {
            CreatedBy = source.CreatedBy;
            CreationDate = source.CreationDate;
            LastEntryDate = source.LastEntryDate;
            PublicKey = source.PublicKey;
            IsPublic = source.IsPublic;
            Title = source.Title;
            Children  = source.Children.Cast<IGroup>().Select(@group => new GroupView(source, @group)).ToList();
        }

        public IEnumerable<ICompositeView> Children  { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastEntryDate { get; set; }

        public Guid? Parent { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public bool IsPublic { get; set; }
    }
}

