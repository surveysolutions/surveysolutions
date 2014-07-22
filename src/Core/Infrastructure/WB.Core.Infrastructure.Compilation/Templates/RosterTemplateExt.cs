namespace WB.Core.Infrastructure.Compilation.Templates
{
    public partial class RosterTemplate
    {
        protected RosterTemplateModel RosterTemplateModel { set; get; }
    
        public RosterTemplate(RosterTemplateModel model)
        {
            this.RosterTemplateModel = model;

        }
    }
}
