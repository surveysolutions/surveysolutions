namespace WB.Core.Infrastructure.Compilation.Templates
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
