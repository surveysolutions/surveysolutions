namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class RosterTemplate
    {
        protected RosterTemplateModel Model { private set; get; }

        public RosterTemplate(RosterTemplateModel model)
        {
            this.Model = model;
        }
    }
}
