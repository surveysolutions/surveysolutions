using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public struct DropdownEntityView
    {
        public bool IsSectionPlaceHolder;
        public string Id;
        public string Title;
        public string Breadcrumbs;
        public string Type;
        public string VarName;
        public string QuestionType;

        public override string ToString()
        {
            return (this.IsSectionPlaceHolder? "+--" : $"`--{this.Breadcrumbs}.") + $"{this.Title} ({this.VarName})";
        }
    }
}