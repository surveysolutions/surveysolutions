namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditGroupView
    {
        public NewEditGroupView(GroupDetailsView @group, Breadcrumb[] breadcrumbs)
        {
            Group = @group;
            Breadcrumbs = breadcrumbs;
        }

        public GroupDetailsView Group { get; set; }
        public Breadcrumb[] Breadcrumbs { get; set; }
    }
}
