using Main.Core.Entities.Composite;

namespace Main.Core.Entities.SubEntities
{
    public interface IStaticText : IComposite, IConditional, IValidatable
    {
        string Text { get; set; }
        string AttachmentName { get; set; }
    }
}
